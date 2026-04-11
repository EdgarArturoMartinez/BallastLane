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

## Current Project State (Exploration & Structuring)

- Phase: Exploration / design. No production code yet; architecture proposals evaluated.
- Decided constraints: Clean Architecture / Hexagonal; ADO.NET; SQL Server; TDD. Task & Auth modules defined in HU-01 and HU-02.
- Next technical steps: scaffold solution, add Domain and Application projects, add basic DTOs and interfaces, implement simple ADO.NET repository and migration scripts.

## Useful file references

- Checkpoint document: `Arthur_Checkpoint_Exploration.md` (contains user stories, data model, acceptance criteria).

## Maintenance

- Keep this memory short. When conventions change, update this file and create a one-line note in `DAILY_CONTEXT.md` (see guidance).

## Contacts

- Maintainer / Candidate: Arturo Martínez (use repo PRs/discussions for changes)
