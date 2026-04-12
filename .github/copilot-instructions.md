# copilot-instructions.md

## Purpose
This file provides short project guidelines that Copilot should consider automatically when assisting in this repository. Keep it concise and up to date.

## Project Conventions (summary)
- Architecture: Hexagonal / Clean Architecture (Ports & Adapters)
- Data access: ADO.NET only (NO EF, NO Dapper, NO MediatR)
- DB: SQL Server (use parameterized queries)
- Layer order: Domain → Application → Infrastructure ← API (dependency rule points inward)
- All passwords hashed with BCrypt.Net-Next; JWT Bearer tokens for auth

## Database Schema (actual — `sql/migrations/0001_init.sql`)
- **Users**: `Id` (PK), `Username` NVARCHAR(100) UNIQUE, `Email` NVARCHAR(320) UNIQUE, `PasswordHash` NVARCHAR(512), `Salt` NVARCHAR(128), `Role` NVARCHAR(50) DEFAULT `'User'`
- **Tasks**: `Id` (PK), `Title` NVARCHAR(200), `Description` NVARCHAR(2000), `Status` NVARCHAR(50) DEFAULT `'Todo'` (values: `'Todo'`,`'InProgress'`,`'Done'`), `DueDate` DATETIME2 NULL, `OwnerUserId` UNIQUEIDENTIFIER FK→Users.Id
- ⚠️ FK column is **`OwnerUserId`** (not `UserId`). Status is **NVARCHAR** (not INT).
- Migration tracking: `__Migrations` table auto-created by `DbMigrator` (not in SQL files)
- Demo seed (`0002_seed.sql`): user `demo@ballastlane.dev`, role `Admin`, pwd `Demo@12345`

- **Audits**: `Id` UNIQUEIDENTIFIER (PK), `Entity` NVARCHAR(100), `EntityId` NVARCHAR(100), `Action` NVARCHAR(50), `UserId` UNIQUEIDENTIFIER NULL, `Username` NVARCHAR(200) NULL, `OldValues` NVARCHAR(MAX) NULL, `NewValues` NVARCHAR(MAX) NULL, `CreatedAt` DATETIME2 DEFAULT SYSUTCDATETIME(). Migration `0003_audit.sql` creates this table. The codebase includes a read adapter (`IAuditRepository` / `AdoAuditRepository`) and write instrumentation (TasksController, AuthService) that inserts audit rows on create/update/delete.

## Frontend Stack
- React 19 + Vite + TypeScript 6
- Tailwind CSS 3.4 + react-hot-toast 2.4 + @heroicons/react 2.0
- Config: `web/ballastlane-web/tailwind.config.cjs`, `postcss.config.cjs`
- Dockerfile: `npm install --no-audit` (no npm ci — no package-lock committed)
- nginx proxies `/api/*` → API container (no CORS needed)

## Common Commands
- Build: `dotnet build`
- Test: `dotnet test`
- Run (full stack): `docker compose up --build -d`
- Frontend dev (local): `cd web/ballastlane-web && npm run dev`
- URLs: API `http://localhost:5000` | Frontend `http://localhost:5173`
- Demo login: `demo` / `Demo@12345`

## Testing Policy
- Use `xUnit` + `Moq`. Prefer TDD: write failing tests first, then implement.

## Code Review
- Keep PRs small (<200 lines).
- Add unit tests for new behavior and update documentation when introducing design changes.

## Model / Escalation Policy
- Default model for routine tasks: GPT-5 mini
- Escalate to Claude Sonnet 4.6 / GPT-5.4 for deep architectural analysis
- Reserve Claude Opus 4.6 for critical emergencies only

## Onboarding & Daily Flow
## Onboarding & Daily Flow
- Primary checkpoint: `docs/Arthur_Checkpoint_Exploration.md` — canonical project checkpoint and decisions.
- At session start the assistant should read `/.github/copilot-instructions.md` and `docs/Arthur_Checkpoint_Exploration.md` to resume state.
- EOD: update `docs/Arthur_Checkpoint_Exploration.md` with a short 6–12 line summary containing: 1) What I did today 2) Files changed 3) Decisions made 4) Blockers 5) Next tasks. Commit and push the update when finishing the day.

## How to resume work the next day
1. Update `docs/Arthur_Checkpoint_Exploration.md` with the short summary before finishing the day.
2. If conventions changed, update `docs/Arthur_Checkpoint_Exploration.md` with a one-line note and version.
3. When you restart the assistant, ask: "Resume project using `docs/Arthur_Checkpoint_Exploration.md`. Show a 5-line plan to continue." The assistant will read that file and produce a short plan.

## End-of-day update template
Use this exact structure for concise onboarding the next day:

```
EOD Update:
- Completed: (3 bullets)
- Files changed: (file paths)
- Decisions: (1–2 short bullets)
- Blockers: (1 short bullet)
- Next: (top 3 tasks for tomorrow)
```

Example start message for Copilot:

"Start: read `/.github/copilot-instructions.md` and `docs/Arthur_Checkpoint_Exploration.md`. Summarize in 5 lines and generate a precise to-do list for today (3 tasks)."

## Notes
- Keep instructions short. Do not put secrets in this file.

## EOD Update & Next-Session Checklist
Follow this short EOD routine to make next-day resumption frictionless.

EOD routine (must be completed before ending your work session):
1. Update `docs/Arthur_Checkpoint_Exploration.md` with a 6–12 line summary using the EOD template.
2. If you changed architecture or conventions, add a one-line note in `docs/Arthur_Checkpoint_Exploration.md` and increment the version in the checkpoint.
3. Add a short checkpoint entry describing decisions and blockers.
4. Commit and push all changes to your working branch.

Next-session start steps (what to do when you return):
1. Pull the latest branch: `git pull origin <branch>`.
2. Open `docs/Arthur_Checkpoint_Exploration.md` to load the agreed conventions and yesterday's context.
3. Ask the assistant: "Resume project using `docs/Arthur_Checkpoint_Exploration.md`. Show a 5-line plan to continue." The assistant will read that file and produce a focused plan.
4. Run quick verification: `dotnet build` and `dotnet test` (or `docker-compose up --build -d` if using Docker). Report failures in your EOD summary in the checkpoint and create a tiny ticket in your issue tracker.

Keep this file short; updates should be additive and versioned with commits.

## Assistant Resume Behavior
- On session start the assistant SHOULD read `/.github/copilot-instructions.md` and `docs/Arthur_Checkpoint_Exploration.md` to resume state. Do NOT require the user to paste large files or the full checkpoint document.
- `Arthur_Checkpoint_Exploration.md` is an extended checkpoint/history file and SHOULD only be read when explicitly requested (e.g., "Read checkpoint history").
- Use the session start single-line prompt (documented in `docs/Arthur_Checkpoint_Exploration.md`) or a short instruction to the assistant to re-read the checkpoint file when needed.
