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
- Primary memory: `/memories/repo/project-conventions.md` — canonical conventions and facts.
- DAILY_CONTEXT.md: create one at repo root or `/docs/DAILY_CONTEXT.md` each day with a short 6–12 line summary containing:
	1. What I did today (bullet list)
	2. Files changed (paths)
	3. Decisions made (short)
	4. Blockers (short)
	5. What I will do tomorrow (top 3 tasks)

- Example DAILY_CONTEXT.md:

```
Summary: Scaffolding solution and domain models completed.
Files changed: src/Ballastlane.Domain/*, src/Ballastlane.Application/*
Decisions: Use Hexagonal architecture; ADO.NET for DB access.
Blocked: Need DB migration script and sample connection string for local dev.
Tomorrow: 1) Implement TaskRepository (ADO.NET) 2) Add Auth endpoints 3) Seed sample users
```

## How to resume work the next day
1. Update `/docs/DAILY_CONTEXT.md` with the short summary before finishing the day.
2. If conventions changed, update `/memories/repo/project-conventions.md` with a one-line note and version.
3. When you restart Copilot, ask: "Resume project using `/memories/repo/project-conventions.md` and `DAILY_CONTEXT.md`. Show a 5-line plan to continue." The agent will read the memory and DAILY_CONTEXT and produce a short plan.

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

"Start: read `/memories/repo/project-conventions.md` and `DAILY_CONTEXT.md`. Summarize in 5 lines and generate a precise to-do list for today (3 tasks)."

## Related memories
- /memories/repo/project-conventions.md

## Notes
- Keep instructions short. Do not put secrets in this file.

## EOD Update & Next-Session Checklist
Follow this exact short EOD routine to make next-day resumption frictionless.

EOD routine (must be completed before ending your work session):
- Update `/docs/DAILY_CONTEXT.md` with a 6–12 line summary using the EOD template.
- If you changed architecture or conventions, update `/memories/repo/project-conventions.md` with a one-line note and an incremental version.
- Add a short checkpoint entry in `Arthur_Checkpoint_Exploration.md` describing decisions and blockers.
- Commit and push all changes to your working branch.

Next-session start steps (what to do when you return):
1. Pull the latest branch: `git pull origin <branch>`.
2. Open `/memories/repo/project-conventions.md` and `/docs/DAILY_CONTEXT.md` to load the agreed conventions and yesterday's context.
3. Ask the agent: "Resume project using `/memories/repo/project-conventions.md` and `/docs/DAILY_CONTEXT.md`. Show a 5-line plan to continue." The agent will read those files and produce a focused plan.
4. Run quick verification: `dotnet build` and `dotnet test` (or `docker-compose up --build -d` if using Docker). Report failures immediately in `DAILY_CONTEXT.md` and create a tiny ticket in your issue tracker.

Keep this file short; updates should be additive and versioned with commits.

## Assistant Resume Behavior
- On session start the assistant SHOULD read `/memories/repo/project-conventions.md` and `/docs/DAILY_CONTEXT.md` to resume state. Do NOT require the user to paste large files or the full checkpoint document.
- `Arthur_Checkpoint_Exploration.md` is an extended checkpoint/history file and SHOULD only be read when explicitly requested (e.g., "Read checkpoint history").
- Use the session start single-line prompt (documented in `docs/DAILY_CONTEXT.md`) to resume. At end-of-day use the single-line EOD prompt to trigger auto-sync, commit and push of the docs.
