# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

GotFired ("İşten Atıldım Destek") collects dismissal (wrongful termination) cases. Applicants submit a case anonymously and staff reply. Originally .NET Framework 4.6.1 (2016–2018), it was migrated in place to **.NET 10** (ASP.NET Core, EF Core 10, ASP.NET Core Identity) and runs in a Linux container. User-facing text is Turkish.

## Commands

```sh
docker compose up --build          # API :8080 + SQL Server :1433 + Mailpit :8025; seeds dev-admin/admin123
dotnet build GotFired.slnx
dotnet tool restore                # dotnet-ef from dotnet-tools.json
dotnet ef migrations add <Name> --project GotFired.DAL --startup-project GotFired.DAL   # DB from $GOTFIRED_CONNECTION
dotnet ef migrations has-pending-model-changes --project GotFired.DAL --startup-project GotFired.DAL
dotnet test --project tests/GotFired.Model.Tests/GotFired.Model.Tests.csproj       # domain unit tests; no database, about 1 s
dotnet test --project tests/GotFired.Api.Tests/GotFired.Api.Tests.csproj           # integration tests; needs Docker (Testcontainers)
dotnet test --project tests/GotFired.Api.Tests/GotFired.Api.Tests.csproj --filter-method "*Tampered*"   # one test
docker run --rm --network gotfired_default -v "$PWD/tests/e2e":/e2e -w /e2e -e BASE_URL=http://api:8080 -e MAILPIT_URL=http://mailpit:8025 \
  mcr.microsoft.com/playwright/python:v1.65.2-noble sh -c "pip install -q --break-system-packages playwright==1.65.2 && python ui_test.py"   # browser test vs compose
```

`tests/GotFired.Api.Tests` is the regression net. It holds behaviour tests at the HTTP boundary, run in-process against SQL Server 2022 and Mailpit in Testcontainers. Its database is built like production on cutover day: EF6 fixture with Turkish collation → level 160 → upgrade script → `migrations.sql`. A schema-parity test compares it with a migrations-only database. Tests share the database and run in parallel, so each test creates its own uniquely named data. When running inside a container, set `TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal` and mount the Docker socket. Its defaults target a DB built from `db/tests/legacy-ef6-schema.sql` + `db/upgrade-legacy-database.sql` (users `legacy-admin`/`legacy-user`, password `legacy-pass`). Set `MAILPIT_URL` to also check outgoing mail.

Configuration lives in `appsettings*.json`, with production values in env vars: `ConnectionStrings__GotFired`, `Jwt__Key` (≥32 bytes), `Smtp__*`. `Seed:Enabled` runs migrations plus `DevDataSeeder` on startup (Development only).

## Architecture

Project references flow Api → DAL → Model. Versions are central in `Directory.Packages.props`, and the target framework is set in `Directory.Build.props`. Nullable is off because the code predates it.

- **Model**: the domain model (no framework dependencies).
  - **`DismissalCase` is the aggregate root.** State has private setters. Its comments, tags and supporters are read-only collections over private lists. Every state change is a method that enforces the workflow:
    - `Submit` (factory): a new case is always Pending.
    - `AnswerByStaff`: Answered, a staff comment, and a new link token replacing the old one.
    - `ReplyAsApplicant`: requires a valid unexpired token; Pending, and a comment under the applicant's name.
    - `MarkAnswered`, `UpdateNotes`, `Evaluate` (sets tags exactly), `AddTag` (idempotent), `RemoveTag`.
  - **Owned entities:** `Comment`, `DismissalCaseTag` and `DismissalCaseSupportedBy` have internal constructors, so only the aggregate creates them.
  - **Other entities:** `Applicant` and `AnswerTemplate` have factories (and `Edit`). Reference data (cities, tags, reasons) is plain data.
  - **The link rule** exists once, as `IsAccessibleWith` (in memory, constant-time) and as `AccessibleWith` (a query expression). A unit test keeps the two in agreement.
  - **`BusinessRuleException`** is the domain's rule-violation exception.
  - **API shapes and enums:** API shapes are in `ViewModels/*` and `Dtos.cs`; enums are stored as `tinyint` (`Enums/Enums.cs`).
- **DAL**: `GotFiredDbContext : IdentityDbContext<ApplicationUser>` (`Identity/ApplicationUser.cs`), its migrations, and `DevDataSeeder`. `SaveChanges` stamps the audit columns, including a new string `RowVersion`. `RowVersion` is an EF concurrency token, so a stale update fails atomically (`DbUpdateConcurrencyException`). There are no repository or unit-of-work wrappers: the `DbContext` is the unit of work.
- **Api**: vertical slices with CQRS and no mediator. Each use case is one file under `Features/<area>/`: a command (writes) or a query (reads), its handler (`ICommandHandler`/`IQueryHandler` in `Infrastructure/Cqrs.cs`, registered by scanning), and its minimal-API endpoint. `Features/Endpoints.cs` lists every route and its authorization policy. Commands load the aggregate, call one domain method and save, and are wrapped in `ConcurrencyRetry`. Queries project straight to response records with `AsNoTracking`. Shared response shapes live in `Features/Shared`.
  - **REST API** under `/api/v1`: `cases` (with `comments`, `notes`, `appeal-state` and `tags` sub-resources), `application-form`, `applications`, `applicant/case` (token in the `X-Applicant-Token` header), `answer-templates`, `users`, `roles`, `categories` and `tags`. `/token` is the OAuth2 password grant used by `login.html`.
  - **Status codes:** 201 with `Location`, 202 for a submitted application, 204, 404. Errors are Problem Details: 400 for validation, 409 for a conflict left after retrying, 412/428 for template `If-Match`, 422 for a `BusinessRuleException`, 500 for anything else (which is emailed by `EmailExceptionHandler`).
  - **JSON** is System.Text.Json with PascalCase names (the pages read `data.FullName`) and numbers accepted as strings.

### Database: mapped onto a pre-existing EF6 schema

The EF Core model deliberately reproduces the schema created by EF6 + ASP.NET Identity 2. Keep it that way, because production data lives in it:
- pluralized table names (`DismissalCases`, `SupportedBies`, …), including the misspelled `DismissalCaseSupprtedBies` (the C# class is `DismissalCaseSupportedBy`)
- `datetime` (not `datetime2`)
- `nvarchar(128)` Identity keys
- EF6-style constraint names (`PK_dbo.X`, `FK_dbo.X_dbo.Y_Col`, `IX_Col`), set in `GotFiredDbContext.cs` → `ApplyEf6ConstraintNames`.

Existing databases are upgraded once with `db/upgrade-legacy-database.sql`. It adds the Core Identity columns and tables and marks the `Baseline` migration as applied. Fresh databases get the same schema from the `Baseline` migration. To check parity, diff the output of `db/tests/schema-snapshot.sql` between the two. The only expected difference is `AspNetUsers.LockoutEndDateUtc`, which is kept for rollback to the old app.

Production is moving from SQL Server 2008 R2 to a new SQL Server 2022; the cutover runbook is [db/README.md](db/README.md). Facts from the 2025-08 backup that the code and scripts depend on:
- The database collation is **`Turkish_CI_AS`**, and a restore keeps it. The upgrade script therefore upper-cases normalized names under a Latin collation; otherwise `i`→`İ` breaks logins.
- The backup is at compatibility level **100**. The runbook raises it to 160. EF generates SQL for `Database:CompatibilityLevel` (default 160), and startup fails fast if the database's level is lower.
- `DismissalCases.Guid` is not unique (74 old double-submitted cases share 31 Guids). `userview/{guid}` shows the newest case with the merged conversation, and `AddDismissalCase` ignores a repeated Guid.

Identity v2 password hashes still verify and are rehashed to v3 at login. While rollback to the .NET Framework app must stay possible, set `Identity:PasswordHashCompatibilityV2=true`.

### Applicant links

Applicants reach only `userView.html?guid=<token>`, from the email sent when staff answer:
- The token is 256 random bits (`ApplicantAccessToken`). The database stores only its SHA-256 hash in `DismissalCases.AccessTokenHash`, with `AccessTokenExpiresAt` (`ApplicantLinks:Lifetime`, default 90 days).
- Every staff answer issues a new token, which invalidates the previous one.
- `userview` and `useranswer` resolve the case from the token. They ignore the case ID and name sent from the browser and never return staff `Notes`.
- The case `Guid` is only a double-submit key. Old Guid links stopped working with this change.
- The parameter is still called `guid` so `userView.html` works unchanged.

### Front end: AngularJS 1.x in `GotFired.Api/wwwroot`, no build step

The admin SPA lives in `index.html` + `app/`; all API calls go through the `$resource` definitions in `app/service.js`. The public pages are `apply.html`, `userView.html` and `login.html`.
- **`apply.html`** posts a form-encoded body to `POST /applications` and alerts the `errors` from a 400 Problem Details.
- **`userView.html`** reads the token from the URL fragment (`#token=`) and sends it in `X-Applicant-Token`.
- **Template editing** sends the `ETag` from `GET` back as `If-Match` (via `$http` in `answerTemplate.js`).
- **The interceptor** in `app/app.js` redirects to `/login.html` on 401 and shows Problem Details `detail`/`title`/`errors` as toasts.
- **Static files:** the Web SDK excludes `bower_components` from publish by default, so `GotFired.Api.csproj` includes it explicitly (the mask plugin). Linux file names are case-sensitive.
