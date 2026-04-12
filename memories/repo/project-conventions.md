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

- **Tasks table** (`sql/migrations/0001_init.sql`):
	- `Id` : UNIQUEIDENTIFIER PK DEFAULT NEWID()
	- `Title` : NVARCHAR(200) NOT NULL
	- `Description` : NVARCHAR(2000) NOT NULL DEFAULT ''
	- `Status` : NVARCHAR(50) NOT NULL DEFAULT 'Todo' — values: `'Todo'`, `'InProgress'`, `'Done'` (NOT an INT enum)
	- `DueDate` : DATETIME2 NULL (nullable — handle DBNull in SqlDataReader)
	- `OwnerUserId` : UNIQUEIDENTIFIER NOT NULL FK→Users.Id — **column is `OwnerUserId`, not `UserId`**
	- Index: `IX_Tasks_OwnerUserId`

- **Users table** (`sql/migrations/0001_init.sql`):
	- `Id` : UNIQUEIDENTIFIER PK DEFAULT NEWID()
	- `Username` : NVARCHAR(100) NOT NULL UNIQUE
	- `Email` : NVARCHAR(320) NOT NULL UNIQUE
	- `PasswordHash` : NVARCHAR(512) NOT NULL
	- `Salt` : NVARCHAR(128) NOT NULL (stored separately from PasswordHash)
	- `Role` : NVARCHAR(50) NOT NULL DEFAULT 'User' — values: `'User'`, `'Admin'`

- **Migration tracking**: `__Migrations` table auto-created by `DbMigrator` (not in SQL files).
- **Demo seed** (`sql/migrations/0002_seed.sql`): user `demo@ballastlane.dev`, pwd `Demo@12345`, role `Admin`.

## Business Rules (short)

- `Title` required, 3–200 chars.
- `DueDate` for new tasks must be today or future; nullable in DB.
- Ownership enforced via `OwnerUserId` from JWT claims (never trust client for owner).
- `Status` stored as NVARCHAR: filter with `WHERE Status = 'Todo'` (not `Status = 0`).
- Passwords always hashed with BCrypt + Salt; `Salt` stored separately in Users table. Never log raw passwords.

## CI / PR / Review

- PR size: prefer <200 lines, include unit tests for new behavior.
- CI must run `dotnet build` and `dotnet test` on PRs.
- Add minimal integration test for Auth and primary Task CRUD flows.

## Current Project State (Progress & Next Steps)

- Phase: Phases 0–11 tracked; Phases 0–10 completed; Phase 11 (documentation) in progress; Phase 12 (E2E) pending.
- Completed work: solution scaffold, Domain/Application/Infrastructure layers, ADO.NET repositories, DbMigrator/seeder, API controllers (Auth + Tasks + Users), professional React+Vite+Tailwind frontend (Kanban UI, animated modals, responsive), tests (92 passing), Docker Compose stack, CI workflow, security hardening.
- Recent changes (2026-04-12): Frontend professional redesign — Tailwind CSS + react-hot-toast + @heroicons/react; Kanban TasksPage, animated TaskModal, responsive Header, split-panel Login/Register. User approved visually at http://localhost:5173.
- Current blocking step: None — stack fully operational (frontend http://localhost:5173, API http://localhost:5000).
- Next technical steps:
	1. E2E tests with Playwright (pending user confirmation).
	2. Create PR for frontend redesign branch.
	3. Final release tag + README polish (Phase 11 completion).

Notes:
- ADO.NET repositories and DB migrations are already implemented earlier in the project — they are not pending work.
- This memory is the canonical short summary the assistant will read to resume the project; update it if conventions or phase change.
- Assistant resume behavior: the assistant SHOULD read `/.github/copilot-instructions.md`, this file (`/memories/repo/project-conventions.md`) and `/docs/DAILY_CONTEXT.md` on session start and use them as the canonical, minimal context. `Arthur_Checkpoint_Exploration.md` is an extended history file and should only be read when explicitly requested by the user.

## Useful file references

- Checkpoint document: `Arthur_Checkpoint_Exploration.md` (contains user stories, data model, acceptance criteria).

## Maintenance

- Keep this memory short. When conventions change, update this file and create a one-line note in `DAILY_CONTEXT.md` (see guidance).

## EOD Notes

- 2026-04-11: Applied minimal Phase 10 hardening (security headers, HSTS in prod), added `docker-compose.prod.yml`, fixed CI badge in README, added GenAI implementation narrative. Changes committed to `building-solution-webapi` and pushed.
- 2026-04-12: Frontend professional redesign (Tailwind Kanban, animated modals, responsive header). Fixed actual DB schema in all docs (Users has Salt+Role; Tasks uses OwnerUserId; Status is NVARCHAR). Deleted `docs/12_PHASE_ROADMAP.md` — phases consolidated into checkpoint. User approved UI.

## Contacts

- Maintainer / Candidate: Arturo Martínez (use repo PRs/discussions for changes)
