# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

GotFired ("İşten Atıldım Destek") collects dismissal (wrongful termination) cases. Applicants submit a case anonymously and staff reply. Originally .NET Framework 4.6.1 (2016–2018), it was migrated in place to **.NET 10** (ASP.NET Core, EF Core 10, ASP.NET Core Identity) and runs in a Linux container. User-facing text is Turkish.

## Commands

```sh
docker compose up --build          # API :8080 + SQL Server :1433 + Mailpit :8025; seeds admin/admin123 on first start
dotnet build GotFired.slnx
dotnet tool restore                # dotnet-ef from dotnet-tools.json
dotnet ef migrations add <Name> --project GotFired.DAL --startup-project GotFired.DAL   # DB from $GOTFIRED_CONNECTION
dotnet ef migrations has-pending-model-changes --project GotFired.DAL --startup-project GotFired.DAL
python3 tests/api_smoke_test.py [base_url] [admin] [pw] [user] [pw]   # end-to-end contract test, writes data
```

There are no unit tests. `tests/api_smoke_test.py` is the regression net for the HTTP contract. Its defaults target a DB built from `db/tests/legacy-ef6-schema.sql` + `db/upgrade-legacy-database.sql` (users `legacy-admin`/`legacy-user`, password `legacy-pass`). Set `MAILPIT_URL` to also check outgoing mail.

Configuration lives in `appsettings*.json`, with production values in env vars: `ConnectionStrings__GotFired`, `Jwt__Key` (≥32 bytes), `Smtp__*`. `Seed:Enabled` runs migrations plus `DevDataSeeder` on startup (Development only).

## Architecture

Project references flow Api → Business → DAL → Model. Versions are central in `Directory.Packages.props`, and the target framework is set in `Directory.Build.props`. Nullable is off because the code predates it.

- **Model**: EF entities (`Entities/DismissalCase/*`, derived from `BaseEntity`), enums stored as `tinyint` (`Enums/Enums.cs`, `[Display]` names are served to the UI via `EnumHelper.GetEnumLookups<T>()`), view models, repository interfaces, `Identity/ApplicationUser`.
- **DAL**: `GotFiredDbContext : IdentityDbContext<ApplicationUser>`. `UnitOfWork` (scoped) lazily creates repositories over the shared context. `Save()` stamps audit fields and a **string `RowVersion`** that `BaseRepository.Update` uses as a hand-rolled optimistic-concurrency check. `AuthRepository` wraps `UserManager`/`RoleManager` (async).
- **Business**: `*Business` classes plus `SmtpManager` (MailKit, `SmtpOptions`), all injected through DI. The case/parameter code is synchronous; the auth code is async.
- **Api**: `Program.cs` holds all wiring. Controllers use attribute routes under `api/v1/<controller>/...` and `[Authorize(Roles=...)]`. `/token` (`Auth/TokenEndpoint.cs`) replaces the old OWIN OAuth password grant and issues JWTs that carry `role` claims. `EmailExceptionHandler` emails unhandled exceptions.

### Database: mapped onto a pre-existing EF6 schema

The EF Core model deliberately reproduces the schema created by EF6 + ASP.NET Identity 2. Keep it that way, because production data lives in it:
- pluralized table names (`DismissalCases`, `SupportedBies`, …)
- `datetime` (not `datetime2`)
- `nvarchar(128)` Identity keys
- EF6-style constraint names (`PK_dbo.X`, `FK_dbo.X_dbo.Y_Col`, `IX_Col`), set in `DBContext.cs` → `ApplyEf6ConstraintNames`.

Existing databases are upgraded once with `db/upgrade-legacy-database.sql`. It adds the Core Identity columns and tables and marks the `Baseline` migration as applied. Fresh databases get the same schema from the `Baseline` migration. To check parity, diff the output of `db/tests/schema-snapshot.sql` between the two. The only expected difference is `AspNetUsers.LockoutEndDateUtc`, which is kept for rollback to the old app.

Identity v2 password hashes still verify and are rehashed to v3 at login. While rollback to the .NET Framework app must stay possible, set `Identity:PasswordHashCompatibilityV2=true`.

### Front end: AngularJS 1.x in `GotFired.Api/wwwroot`, no build step

The admin SPA lives in `index.html` + `app/`. The public pages are `apply.html`, `userView.html` and `login.html`. **The API contract must stay Web API 2 compatible:**
- Newtonsoft JSON with PascalCase property names. Anonymous objects keep their lowercase keys (`data`, `paging`, `guid`).
- `Ok("text")` returns a JSON string, because `StringOutputFormatter` is removed.
- `login.html` posts `/token` as `text/plain`. `apply` and `useranswer` receive jQuery form posts (`[FromForm]`); everything else is JSON.
- Errors use the shapes `{Message}` and `{Message, ModelState}` (read by `apply.html`).
- Automatic model-state 400s are off; controllers check `ModelState` themselves.
- The SPA interceptor in `app/app.js` redirects to `/login.html` on 401 and shows `StatusMessage` toasts on 412.
