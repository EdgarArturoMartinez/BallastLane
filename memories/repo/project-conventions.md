## Project Conventions (Ballast Lane)

- Architecture: Hexagonal / Clean Architecture (Ports & Adapters). The canonical layer separation is: API → Application → Domain → Infrastructure.
- Chosen project structure (recommended):
	- `src/TaskManager.Domain/`
	- `src/TaskManager.Application/`
	- `src/TaskManager.Infrastructure/`
	- `src/TaskManager.API/`
	- `src/TaskManager.Frontend/` (React)

- ORM / Data access: **ADO.NET only** (NO Entity Framework, NO Dapper, NO MediatR). Use `SqlConnection` + `SqlCommand` + `SqlDataReader` and `SqlParameter` for parameterized queries.
- Database: **SQL Server**. Connection strings and secrets must come from environment variables or secret store (never checked into repo). Use `CONN_STRING` or `ConnectionStrings__Default` env var.
- Authentication: JWT Bearer tokens. Passwords hashed with BCrypt.
- Testing: `xUnit` + `Moq`. Follow TDD — start with failing unit tests for new behavior.
- Build & Run (local):
	- Build: `dotnet build`
	- Test: `dotnet test`
	- Run locally (backend): `docker-compose up --build` (when docker-compose is provided)
	- Frontend dev: use `npm run dev` or `pnpm` depending on chosen package manager (specify in frontend README)
- Lint / Formatting: use repository `.editorconfig` and `dotnet format` for C#. Add a pre-commit hook to run formatting and tests fast checks.
- Logging: use `ILogger<T>` in Infrastructure and API layers. **Do not** scatter logging inside Domain entities.
- Secrets: Use environment variables or a secrets manager. Never put secrets in memory files or `copilot-instructions.md`.

## Domain Facts (entities & rules)

- Task entity fields (reference):
	- `Id` : UNIQUEIDENTIFIER (PK)
	- `Title` : NVARCHAR(200) NOT NULL
	- `Description` : NVARCHAR(2000) NULL
	- `Status` : INT NOT NULL (Todo=0, InProgress=1, Done=2)
	- `DueDate` : DATETIME2 NOT NULL
	- `UserId` : UNIQUEIDENTIFIER FK NOT NULL
	- `CreatedAt`, `UpdatedAt` : DATETIME2 (UTC)

- User entity fields (reference):
	- `Id` : UNIQUEIDENTIFIER (PK)
	- `Username` : NVARCHAR(100) NOT NULL, UNIQUE
	- `Email` : NVARCHAR(200) NOT NULL, UNIQUE
	- `PasswordHash` : NVARCHAR(500) NOT NULL (BCrypt)
	- `CreatedAt` : DATETIME2 (UTC)

## Business Rules (short)

- `Title` required, 3–200 chars.
- `DueDate` for new tasks must be today or future.
- Ownership enforced via `UserId` from JWT claims (never trust client for owner).
- Passwords always hashed with BCrypt; do not log raw passwords.

## CI / PR / Review

- PR size: prefer <200 lines, include unit tests for new behavior.
- CI must run `dotnet build` and `dotnet test` on PRs.
- Add minimal integration test for Auth and primary Task CRUD flows.

## Current Project State (Progress & Next Steps)

- Phase: Implementation — Phases 0–6 completed; Phase 7 (containerization) scaffolded.
- Completed work: solution scaffold, Domain/Application/Application DTOs and ports, Infrastructure ADO.NET repositories, Db migrator/seeder, API controllers, React+Vite frontend, tests (unit + integration) all green locally (92 tests passing).
- Recent changes: Dockerfiles for API and frontend, `docker-compose.yml`, `.dockerignore` files and `scripts/dev-up.ps1` / `scripts/dev-down.ps1` added and committed to branch `building-solution-webapi`.
- Current blocking step: Docker Desktop restart required on your machine before running `docker-compose up` to validate the stack.
- Next technical steps (recommended):
	1. Restart Docker Desktop and run `docker-compose up --build -d` to validate containers (db → api → web).
	2. Verify runtime behaviour and logs; fix any environment-specific issues.
	3. Add minimal CI (GitHub Actions) to run `dotnet build` and `dotnet test` on PRs.
	4. Add a short README `Run Locally` section with exact env var guidance and seeded demo credentials.

Notes:
- ADO.NET repositories and DB migrations are already implemented earlier in the project — they are not pending work.
- This memory is the canonical short summary the assistant will read to resume the project; update it if conventions or phase change.
- Assistant resume behavior: the assistant SHOULD read this file and `/docs/DAILY_CONTEXT.md` on session start and use them as the canonical, minimal context. `Arthur_Checkpoint_Exploration.md` is an extended history file and should only be read when explicitly requested by the user.

## Useful file references

- Checkpoint document: `Arthur_Checkpoint_Exploration.md` (contains user stories, data model, acceptance criteria).

## Maintenance

- Keep this memory short. When conventions change, update this file and create a one-line note in `DAILY_CONTEXT.md` (see guidance).

## Contacts

- Maintainer / Candidate: Arturo Martínez (use repo PRs/discussions for changes)
