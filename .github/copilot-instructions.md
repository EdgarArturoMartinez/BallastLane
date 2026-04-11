# copilot-instructions.md

## Purpose
This file provides short project guidelines that Copilot should consider automatically when assisting in this repository. Keep it concise and up to date.

## Project Conventions (summary)
- Architecture: Hexagonal / Clean Architecture (Ports & Adapters)
- Data access: ADO.NET only (NO EF, NO Dapper, NO MediatR)
- DB: SQL Server (use parameterized queries)

## Common Commands
- Build: `dotnet build`
- Test: `dotnet test`
- Run (local backend): `docker-compose up --build`

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
