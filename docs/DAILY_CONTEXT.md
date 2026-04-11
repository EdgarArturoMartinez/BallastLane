# DAILY_CONTEXT.md

Summary: 
- What I did today: (3–6 short bullets)
- Files changed: (list workspace-relative paths)
- Decisions made: (1–2 short bullets)
- Blockers: (1 short bullet)
- Next (top 3 tasks): (1) (2) (3)

---

EOD Update (fill before finishing work):
- Completed: (3 bullets)
- Files changed: (file paths)
- Decisions: (1–2 short bullets)
- Blockers: (1 short bullet)
- Next: (top 3 tasks for tomorrow)

Quick start for next session:
1. `git pull origin <branch>`
2. Open [memories/repo/project-conventions.md](memories/repo/project-conventions.md) and [docs/DAILY_CONTEXT.md](docs/DAILY_CONTEXT.md)
3. Ask the assistant: "Resume project using `/memories/repo/project-conventions.md` and `/docs/DAILY_CONTEXT.md`. Show a 5-line plan to continue."
4. Run `dotnet build` and `dotnet test` (or `docker-compose up --build -d` if using Docker)

Example (fill-in):
Summary: Frontend built; Dockerfiles added; tests green locally.
- What I did today: 1) Completed frontend build 2) Added Dockerfiles 3) Added docker-compose and scripts
- Files changed: src/Ballastlane.Api/Dockerfile, web/ballastlane-web/Dockerfile, docker-compose.yml
- Decisions made: Use SQL Server container; ADO.NET adapters as infrastructure
- Blockers: Need to run Docker after restarting machine
- Next: 1) Start docker-compose and validate services 2) Add CI pipeline 3) Update README with Docker run instructions
