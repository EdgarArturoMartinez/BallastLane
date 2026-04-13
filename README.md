# Ballastlane — Task Management API

[![CI](https://github.com/EdgarArturoMartinez/BallastLane/actions/workflows/ci.yml/badge.svg)](https://github.com/EdgarArturoMartinez/BallastLane/actions/workflows/ci.yml)

Full-stack task management application built as a .NET Technical Interview exercise.


**Stack:** ASP.NET Core 8 · ADO.NET · SQL Server 2022 · React 19 + Vite 8 · Docker Compose

**Version:** 0.2.0

**Architecture:** Hexagonal (Ports & Adapters) / Clean Architecture

See the user stories for the project here: [User Stories](docs/User-Stories.md)



---

## Run Locally (Docker — recommended, zero prerequisites)

```bash
# Clone and start the full stack (SQL Server + API + Frontend)
git clone <repo-url>
cd Ballastlane
docker compose up --build -d
```

One-step start (recommended for interview/demo)

You can start the full stack with a single, canonical command from the repository root. This builds the images, starts services and leaves the stack running:

```powershell
docker compose up --build -d
```

After the command completes, the services should be reachable at:

- Frontend: `http://localhost:5173`
- API (Swagger): `http://localhost:5000`
- Demo credentials: `demo` / `Demo@12345`

| Service | URL |
|---------|-----|
| Frontend | http://localhost:5173 |
| API (Swagger) | http://localhost:5000 |
| SQL Server | localhost:14330 |}

**Demo credentials (pre-seeded):** `demo` / `Demo@12345`

To stop:
```bash
docker compose down
```

---

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login, returns JWT |
| GET | `/api/tasks` | JWT | List own tasks |
| POST | `/api/tasks` | JWT | Create task |
| GET | `/api/tasks/{id}` | JWT | Get task by ID |
| PUT | `/api/tasks/{id}` | JWT | Update task |
| DELETE | `/api/tasks/{id}` | JWT | Delete task |
| GET | `/api/tasks/public/stats` | No | Public stats (no auth) |
| GET | `/health` | No | Health check |
| GET | `/api/audit` | JWT | Returns audit entries (protected) |

---

## Architecture

```
Driving Adapters       →  API Controllers, React SPA
Input Ports            →  ITaskService, IAuthService
Application Services   →  TaskService, AuthService
Domain Model           →  TaskItem, User, Value Objects, Business Rules
Output Ports           →  ITaskRepository, IUserRepository, IPasswordHasher, IJwtTokenGenerator
Driven Adapters        →  ADO.NET (SqlServer), PBKDF2 hasher, JWT generator
```

---

## GenAI Development Approach

**Tool used:** GitHub Copilot (Agent Mode / Claude Sonnet 4.6)

**What was validated and corrected:**

| AI Output | Human Validation | Correction made |
|-----------|------------------|-----------------|
| `node:18` in Web Dockerfile | Panel would get build error — Vite 8 requires Node ≥ 20.19 | Changed to `node:22-alpine` |
| SQL Server 2022 healthcheck used `mssql-tools` path | Path changed to `mssql-tools18` in 2022 image; containers never became healthy | Fixed path + added `-C` flag |
| `DbMigrator` connected directly to `BallastlaneDb` | Fresh container fails — DB doesn't exist yet | Added `EnsureDatabaseExistsAsync()` via `master` first |
| `0001_init.sql` created `__Migrations` table | Migrator creates it programmatically — conflict on `docker compose up` | Removed duplicate DDL from SQL script |
| No `nginx.conf` for the web container | React SPA returned 404 on browser refresh; `/api` calls had no proxy | Created `nginx.conf` with `try_files` + `proxy_pass` |
| Architecture selection not deliberated | AI defaulted to layered architecture; 4 options were evaluated in a decision matrix | Hexagonal chosen after explicit trade-off analysis |
| No logging strategy in initial proposal | Clean Architecture requires cross-cutting concerns to be designed explicitly | Added Serilog with `ILogger<T>` abstraction across all layers (Domain excluded) |

**Critical thinking demonstrated:**
- Every architectural choice (ADO.NET, Hexagonal, Docker Compose) was challenged before adoption — not accepted blindly.
- AI-generated Docker configuration had 5 runtime bugs that only surfaced during `docker compose up --build` — all caught and fixed through systematic validation.
- The human enforced production constraint: "The domain layer must never log" — AI had logging in domain entities in early drafts.
- Prompt engineering was iterative: initial scaffolding prompt was refined to add specific constraints (PBKDF2, numbered migration runner, `ITaskOwnershipValidator`) after reviewing the first output.

- **Human Decisions:**
    - **Drag-and-drop UX:** Implemented drag-and-drop in the Kanban `TasksPage` to persist status changes; UX handlers added to the frontend so state and API remain consistent (see [web/ballastlane-web/src/pages/TasksPage.tsx](web/ballastlane-web/src/pages/TasksPage.tsx)).
    - **Frontend layout:** Switched to a Tailwind-based Kanban layout (Login, Header, Tasks) — human-chosen for clarity and presentation.
    - **Encoding & editor hygiene:** Fixed stray encoding artifacts (e.g., `Â·`, `â€¦`), added `charset utf-8` to `nginx.conf` and repository `.editorconfig`/`.gitattributes`.
    - **API Docs on Login:** Added an "API Docs" (Swagger) button to the Login page; link configurable via `VITE_API_URL`.
    - **Preserve architecture:** Arturo refused invasive changes that would violate the Hexagonal / Clean Architecture; only minimal, well-scoped adapters added.
    - **Audit write instrumentation:** Approved `sql/migrations/0003_audit.sql`, `IAuditRepository.InsertAsync`, and write instrumentation in `TasksController` and `AuthService.Register` so create/update/delete actions produce audit rows. Verified seed + audit inserts locally.
    - **Audit UI & API features:** Show both server and user-local timestamps in audit UI, implemented server + client pagination for audit endpoints, and store `OldValues`/`NewValues` JSON in `Audits`.
    - **Drag-and-drop persistence decision:** Persist status changes via API, not just local state — chosen for consistency and auditability.

- **Data-access decision (in-depth evaluation):**
    - **Alternatives considered:** ADO.NET (SqlClient), RepoDB/OrmLite (micro-ORMs), Npgsql/Sqlite drivers, MongoDB, custom micro-ORM.
    - **Final choice:** `ADO.NET` + SQL Server. Rationale: aligns with exercise spirit (no EF/Dapper), demonstrates low-level skills (parameterized queries, connection management), and fits Hexagonal adapter pattern (infrastructure-only concern).
    - **Rejected options:** RepoDB/OrmLite (violates spirit), MongoDB (relational semantics implied by exercise), custom micro-ORM (over-engineering).

- **User-story and requirements discipline:**
    - Converted one-line AI stories into enterprise-quality user stories: HU numbering, Process/Subprocess, full data model fields, codified business rules (RN-XX), and Gherkin acceptance scenarios. Split into distinct user stories when appropriate (Tasks vs Auth).

- **Logging strategy (gap found & fixed):**
    - **Human found missing coverage:** AI initially omitted a logging strategy.
    - **Decision:** Use `ILogger<T>` (built-in) for structured logs; mention Serilog as an easy production upgrade. Domain layer remains pure — no logging in entities; application/infrastructure layers handle logging and map `DomainException` to responses.

- **Docker & deployment (reviewer empathy):**
    - **Problem:** LocalDB/Windows-only assumption is brittle for reviewers.
    - **Decision:** Primary delivery via `docker compose` (cross-platform, seeded DB), with manual/local instructions as a fallback. This minimizes friction for interviewers and ensures deterministic demos.

- **Database initialization strategy (hybrid, concrete):**
    - **Chosen approach:** Numbered SQL migration files stored in `sql/migrations/` + C# orchestrator (`DbMigrator`) that:
        - Connects to `master` and ensures database exists (`EnsureDatabaseExistsAsync()`).
        - Applies migration scripts idempotently.
        - Records applied scripts in `__Migrations`.
    - **Rationale:** Scripts remain reviewable/run-manually; C# runner makes Docker-first startup robust (cold-start safe).

- **Copilot / checkpoint policy:**
    - `.github/copilot-instructions.md` is the repo-level policy; `docs/Arthur_Checkpoint_Exploration.md` is the canonical checkpoint for resuming sessions. Human must ask the assistant to re-read if modified mid-session.

- **Notable divergences the human corrected (runtime/config bugs discovered):**
    - Wrong web base image: `node:18-alpine` → changed to `node:22-alpine` (Vite compatibility).
    - SQL Server healthcheck: used wrong `sqlcmd` path and missed `-C` flag for 2022 image; caused container to remain unhealthy.
    - Cold-start migration: `DbMigrator` originally connected directly to `BallastlaneDb` (didn't exist) → added `EnsureDatabaseExistsAsync()` via `master`.
    - Duplicate DDL: `0001_init.sql` attempted to create `__Migrations` while migrator also creates it; removed duplicate.
    - Missing `nginx.conf`: SPA refresh broke; added `try_files` + `/api` proxy rules.
    - CI badge link used wrong GitHub username — fixed to avoid broken README badges.
    - Over-ambitious hardening proposed by AI (full OWASP/Snyk) was scoped down by human to essential headers + `docker-compose.prod.yml`.

- **Implementation phase — AI strengths & failures:**
    - **What AI got right:** Domain entities (value objects), application services and ports, ADO.NET parameterization patterns, xUnit test scaffolding, JWT + PBKDF2 design, correlation-id middleware, React SPA layout scaffolding.
    - **What AI got wrong (required human fixes):** runtime Docker bugs above, CI link, over-specified hardening, hand-waved DB init flow.
    - **Validation workflow used:** iterate — scaffold → `dotnet build` → `dotnet test` → `docker compose up --build -d` → `docker compose ps` → `docker compose logs` → browser smoke tests / API smoke tests. This surfaced 5 silent runtime bugs that code review alone did not show.

- **Prompt engineering insight (practical):**
    - Generic prompts produced incomplete scaffolds. Adding explicit constraints (ADO.NET only, PBKDF2, numbered migration runner, `ITaskOwnershipValidator`, TDD) produced architecturally correct output faster.

---

## Run Locally (manual — requires .NET 8 SDK + SQL Server)

Set secrets via environment variables or a local `.env` file (copy `.env.example` and edit values).

Example using a local `.env` (recommended):

```bash
# Copy the example and edit the values (do not commit your .env)
cp .env.example .env
# Edit .env and set SA_PASSWORD and JWT_SECRET (and optionally SQLSERVER_INTEGRATION_CONNSTR)
```

Example (Linux / macOS) using environment variables directly:

```bash
export SA_PASSWORD="<your-sa-password>"
export JWT_SECRET="<a-secret-key-of-at-least-32-chars>"
export SQLSERVER_INTEGRATION_CONNSTR="Server=localhost,1433;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True;"

# Run API (applies migrations + seed on startup)
dotnet run --project src/Ballastlane.Api

# Run frontend (separate shell)
cd web/ballastlane-web && npm install && npm run dev
```

Windows (PowerShell) example:

```powershell
$env:SA_PASSWORD = "<your-sa-password>"
$env:JWT_SECRET = "<a-secret-key-of-at-least-32-chars>"
$env:SQLSERVER_INTEGRATION_CONNSTR = "Server=localhost,1433;User Id=sa;Password=$env:SA_PASSWORD;TrustServerCertificate=True;"

dotnet run --project src/Ballastlane.Api
```

---

## Tests

```bash
dotnet test Ballastlane.sln
```

137 tests across 4 layers: Domain · Application · Infrastructure · API

---


### Quick-start with Swagger

1. Open `http://localhost:5000` (or the URL set via `VITE_API_URL`).
2. Call `POST /api/auth/login` with the following body:

```json
{"username":"demo","password":"Demo@12345"}
```

3. Copy the `token` field from the response.
4. Click the **Authorize** button (padlock icon) in the top-right corner of Swagger and enter:

```
Bearer <your_token>
```

5. Call `GET /api/tasks` — it will now return your tasks.

2) PowerShell (login + get tasks)

```powershell
$r = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method Post -ContentType 'application/json' -Body '{"username":"demo","password":"Demo@12345"}'
$token = $r.token
Invoke-RestMethod -Uri 'http://localhost:5000/api/tasks' -Headers @{ Authorization = "Bearer $token" }
```

3) curl (login + get tasks)

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d '{"username":"demo","password":"Demo@12345"}' | jq -r .token)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/tasks
```

Note: if your API is hosted at a different URL, replace `http://localhost:5000` with the value of `VITE_API_URL`.
```
## CI

GitHub Actions runs on every push/PR to `main`, `dev`, and `qa`:
- `dotnet restore`
- `dotnet build --configuration Release`
- `dotnet test --configuration Release`

