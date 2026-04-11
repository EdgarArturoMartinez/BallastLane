# copilot-instructions.md

## Purpose
Este archivo proporciona las pautas de proyecto que Copilot debe considerar automáticamente al ayudar en este repositorio. Manténlo corto y actualizado.

## Project Conventions (resumen)
- Architecture: Hexagonal / Clean Architecture (Ports & Adapters)
- Data access: ADO.NET only (NO EF, NO Dapper, NO MediatR)
- DB: SQL Server (parameterized queries)

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
- Default model for routine tasks: GPT-5 mini (0×)
- Escalar a Claude Sonnet 4.6 / GPT-5.4 (1×) para razonamiento profundo o análisis arquitectónico
- Reservar Claude Opus 4.6 (3×) para emergencias críticas solamente

## Onboarding & Daily Flow (how Copilot should be re-fed each day)
- Primary memory: `/memories/repo/project-conventions.md` — contains the canonical conventions and facts.
- DAILY_CONTEXT.md: create one at repo root or `/docs/DAILY_CONTEXT.md` each day with a short 6–12 line summary of:
	1. What I did today (bullet list of completed items)
 2. Files changed (paths)
 3. Decisions made (very short)
 4. What is blocked (short)
 5. What I will do tomorrow (top 3 tasks)

- Example DAILY_CONTEXT.md (short):

```
Summary: Scaffolding solution and domain models completed.
Files changed: src/TaskManager.Domain/*, src/TaskManager.Application/*
Decisions: Use Hexagonal architecture; ADO.NET for DB access.
Blocked: Need DB migration script and sample connection string for local dev.
Tomorrow: 1) Implement TaskRepository (ADO.NET) 2) Add Auth endpoints 3) Seed sample users
```

## How to resume the exact point tomorrow (recommended steps for you)
1. Update `/docs/DAILY_CONTEXT.md` with the short summary above before ending the day.
2. If any conventions changed, update `/memories/repo/project-conventions.md` with a one-line note and version (e.g., `v0.2 — changed password policy`).
3. When you reopen Copilot the next day, ask: "Resume project using `/memories/repo/project-conventions.md` and `DAILY_CONTEXT.md`; show a 5-line plan to continue." The agent will read the memory and DAILY_CONTEXT without costing premium requests.

## End-of-day update template (what to tell the assistant)
- Use this exact structure for concise automatic onboarding:

```
EOD Update:
- Completed: (3 bullets)
- Files changed: (file paths)
- Decisions: (1–2 short bullets)
- Blockers: (1 short bullet)
- Next: (top 3 tasks for tomorrow)
```

Example message you can send to Copilot when starting the next day:

"Start: read `/memories/repo/project-conventions.md` and `DAILY_CONTEXT.md`. Summarize in 5 lines and generate a precise to-do list for today (3 tasks)." 

## Related memories
- /memories/repo/project-conventions.md

## Notes
- Keep instructions short. Avoid embedding secrets here.
