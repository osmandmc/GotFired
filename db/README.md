# Moving the database to SQL Server 2022

The production database moves from the old SQL Server 2008 R2 (Plesk) to a new SQL Server 2022 server. Every step below was tested in this order on a restore of the 2025-08 backup.

## Steps

1. **Stop writes on the old site** (maintenance page), so no applications arrive during the move.
2. **Back up the old database:**
   ```sql
   BACKUP DATABASE gotfired TO DISK = 'gotfired-cutover.bak' WITH COPY_ONLY, INIT;
   ```
3. **Restore it on SQL Server 2022.** Use `RESTORE FILELISTONLY` first to get the logical names; they are `gotfired` and `gotfired_log`.
   ```sql
   RESTORE DATABASE gotfired FROM DISK = '/path/gotfired-cutover.bak'
   WITH MOVE 'gotfired' TO '/var/opt/mssql/data/gotfired.mdf',
        MOVE 'gotfired_log' TO '/var/opt/mssql/data/gotfired_log.ldf', RECOVERY;
   ```
   SQL Server upgrades the file format automatically. The database keeps its `Turkish_CI_AS` collation, which is expected and handled.
4. **Raise the compatibility level.** A restore keeps level 100, and the app refuses to start below the level it is configured for (`Database:CompatibilityLevel`, default 160).
   ```sql
   ALTER DATABASE gotfired SET COMPATIBILITY_LEVEL = 160;
   ```
5. **Run the upgrade script** (idempotent; it records the EF Core `Baseline` migration):
   ```sh
   sqlcmd -S <server> -d gotfired -U <admin> -i db/upgrade-legacy-database.sql -b
   ```
   It ends by listing user names with non-ASCII characters; the 2025-08 backup had none.
6. **Apply the EF Core migrations** that came after the baseline (idempotent; `-I` is needed for the filtered index):
   ```sh
   sqlcmd -S <server> -d gotfired -U <admin> -I -i db/migrations.sql -b
   ```
   Regenerate after adding a migration: `dotnet ef migrations script 20261118213655_Baseline --idempotent --project GotFired.DAL --startup-project GotFired.DAL -o db/migrations.sql`, then remove the UTF-8 BOM (sqlcmd rejects it on stdin).
7. **Create an app login** with only `db_datareader` and `db_datawriter`, not `sa`. Future EF Core migrations run separately with a DDL-capable login.
8. **Deploy the container** with these environment variables:

   | Variable | Value |
   |---|---|
   | `ConnectionStrings__GotFired` | `Server=…;Database=gotfired;User ID=…;Password=…;Encrypt=True` (add `TrustServerCertificate=True` only for a self-signed certificate) |
   | `Jwt__Key` | random, at least 32 bytes, e.g. `openssl rand -base64 48` |
   | `Smtp__Password` | the **rotated** SMTP password |
   | `Smtp__ExceptionRecipient` | who receives error emails |
   | `Smtp__PublicBaseUrl` | public site URL, used in the applicant email link |
   | `ApplicantLinks__Lifetime` | how long an emailed applicant link works (default `90.00:00:00`) |
| `Identity__PasswordHashCompatibilityV2` | `true` while a rollback must stay possible (see below) |

9. **Check it:** log in with a real account and open a case. Run `tests/e2e/ui_test.py` only against a staging copy, never against production, because it writes test data.

## Rollback

The old database on the 2008 R2 server is untouched by this procedure, so rolling back to it loses anything written since the move. To roll back *with* that data, point the old app at the new database. The upgrade only adds columns and tables, so the old app keeps working. This requires `Identity__PasswordHashCompatibilityV2=true` from the start: otherwise every user who logged in to the new app gets a v3 password hash, which the old app cannot verify. Remove the setting once rollback is no longer needed.

## Local copy of a backup

Loads a backup into the `docker compose` stack. The data stays in the `sqldata` volume; it contains personal data, so remove it with `docker compose down -v` when done. Outgoing email goes to Mailpit (http://localhost:8025), never to applicants.

```sh
docker compose up -d --wait sql                     # SQL only, so the API does not create an empty database first
unzip db/<backup>.zip -d /tmp/gotfired-bak
docker compose cp /tmp/gotfired-bak/<backup> sql:/var/opt/mssql/gotfired.bak && rm -rf /tmp/gotfired-bak
docker compose exec -u root sql chown mssql /var/opt/mssql/gotfired.bak
docker compose exec sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'Dev_Passw0rd!' -b -Q "
  RESTORE DATABASE gotfired FROM DISK='/var/opt/mssql/gotfired.bak'
  WITH MOVE 'gotfired' TO '/var/opt/mssql/data/gotfired.mdf', MOVE 'gotfired_log' TO '/var/opt/mssql/data/gotfired_log.ldf', REPLACE;
  ALTER DATABASE gotfired SET COMPATIBILITY_LEVEL = 160;"
docker compose exec -T sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'Dev_Passw0rd!' -d gotfired -b < db/upgrade-legacy-database.sql
docker compose exec -T sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'Dev_Passw0rd!' -d gotfired -I -b < db/migrations.sql
docker compose exec sql rm /var/opt/mssql/gotfired.bak
docker compose up -d --build                        # API + Mailpit; log in as dev-admin / admin123
```

## Files

- `upgrade-legacy-database.sql`: the one-time upgrade above.
- `migrations.sql`: idempotent script of the EF Core migrations after `Baseline`.
- `tests/legacy-ef6-schema.sql`: a test fixture that reproduces the old schema (Turkish collation, compatibility level 100, the missing SGK foreign key, duplicate Guids).
- `tests/schema-snapshot.sql`: one line per column, key and index. Diff the output from two databases to check parity; the only expected difference is `AspNetUsers.LockoutEndDateUtc`, which is kept for rollback.
