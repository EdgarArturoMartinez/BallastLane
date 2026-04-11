# DAILY_CONTEXT.md


Summary: Completed frontend and backend work; added Docker scaffold and EOD docs.
- What I did today: 1) Finalized React+Vite frontend build 2) Added Dockerfiles and `docker-compose.yml` 3) Created EOD / memory files and updated repo conventions
- Files changed: src/Ballastlane.Api/Dockerfile, web/ballastlane-web/Dockerfile, docker-compose.yml, .github/copilot-instructions.md, docs/DAILY_CONTEXT.md, memories/repo/project-conventions.md, Arthur_Checkpoint_Exploration.md
- Decisions made: Keep repo docs in English; canonical conventions saved to `/memories/repo/project-conventions.md`; prefer Docker compose for demo runs
- Blockers: Docker Desktop requires a system restart before starting containers on this machine
- Next (top 3 tasks): 1) Restart Docker Desktop and run `docker-compose up --build -d` to validate stack 2) Review runtime logs and fix any environment-specific issues 3) Add a minimal GitHub Actions workflow to run `dotnet build` and `dotnet test`

---

EOD Update (fill before finishing work):
- Completed: (3 bullets)
- Files changed: (file paths)
- Decisions: (1–2 short bullets)
- Blockers: (1 short bullet)
- Next: (top 3 tasks for tomorrow)

Quick start for next session (exact steps to paste to the assistant):
1. `git pull origin building-solution-webapi`
2. Open [memories/repo/project-conventions.md](memories/repo/project-conventions.md) and [docs/DAILY_CONTEXT.md](docs/DAILY_CONTEXT.md) to review state
3. Paste this exact single-line instruction into the chat with the assistant (copy/paste exactly):

Start: read /.github/copilot-instructions.md, /memories/repo/project-conventions.md and /docs/DAILY_CONTEXT.md. Summarize in 5 lines and generate a precise to-do list for today (3 tasks).

4. At end of day, paste this exact single-line EOD instruction (copy/paste exactly) to trigger document sync + commit + push:

EOD: ensure /.github/copilot-instructions.md, /docs/DAILY_CONTEXT.md and /memories/repo/project-conventions.md reflect today's work; commit with message "docs: EOD update" and push.

4. After the assistant returns the plan, if Docker Desktop was restarted, run:
```powershell
docker-compose up --build -d
docker-compose ps
docker-compose logs -f
```

Example (fill-in):
Summary: Frontend built; Dockerfiles added; tests green locally.
- What I did today: 1) Completed frontend build 2) Added Dockerfiles 3) Added docker-compose and scripts
- Files changed: src/Ballastlane.Api/Dockerfile, web/ballastlane-web/Dockerfile, docker-compose.yml
- Decisions made: Use SQL Server container; ADO.NET adapters as infrastructure
- Blockers: Need to run Docker after restarting machine
- Next: 1) Start docker-compose and validate services 2) Add CI pipeline 3) Update README with Docker run instructions
