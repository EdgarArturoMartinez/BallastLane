# Ballastlane — Task Management API

[![CI](https://github.com/EdgarArturoMartinez/BallastLane/actions/workflows/ci.yml/badge.svg)](https://github.com/EdgarArturoMartinez/BallastLane/actions/workflows/ci.yml)

Full-stack task management application built as a .NET Technical Interview exercise.

**Stack:** ASP.NET Core 8 · ADO.NET · SQL Server 2022 · React 19 + Vite 8 · Docker Compose

**Architecture:** Hexagonal (Ports & Adapters) / Clean Architecture

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

92 tests across 4 layers: Domain · Application · Infrastructure · API

---

## E2E (Playwright)

End-to-end smoke tests are scaffolded using Playwright inside the frontend folder.

Quick steps (frontend):

```bash
cd web/ballastlane-web
npm install
# Run Playwright tests (expects the frontend to be served at http://localhost:5173)
npm run test:e2e
# Open the Playwright HTML report
npx playwright show-report
```

Notes:
- The scaffold includes `playwright.config.ts` and a minimal smoke test at `web/ballastlane-web/e2e/specs/smoke.spec.ts`.
- Tests may require the API at `http://localhost:5000` if flows perform backend interactions; start the stack with `docker compose up --build -d` before running e2e if needed.

---

### Configure the API URL for the UI (optional)

The "API Docs" button on the login screen points to `http://localhost:5000` by default. Override it in development with the `VITE_API_URL` environment variable (useful when the API runs on a different host or port).

Windows (PowerShell):
```powershell
$env:VITE_API_URL='http://localhost:5000'
cd web/ballastlane-web
npm install
npm run dev
```

macOS / Linux (bash):
```bash
export VITE_API_URL='http://localhost:5000'
cd web/ballastlane-web
npm install
npm run dev
```

You can also create a `.env` or `.env.local` file inside `web/ballastlane-web` with:
```
VITE_API_URL=http://localhost:5000
```

This tells the UI which URL to open when the user clicks "API Docs" on the login screen.

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


## Release

Prepare a release tag and push it to the remote. Do not tag until your working tree has the desired changes and you are ready to publish.

Suggested (manual) steps to create an annotated tag locally and push it to GitHub:

```bash
# Bump versions / ensure working tree is ready
git add -A
git commit -m "chore(release): prepare v0.1.0"    # run only when ready
git tag -a v0.1.0 -m "release: v0.1.0"
git push origin v0.1.0
```



## CI

GitHub Actions runs on every push/PR to `main`, `dev`, and `qa`:
- `dotnet restore`
- `dotnet build --configuration Release`
- `dotnet test --configuration Release`

---

## GenAI Development Approach

**Tool used:** GitHub Copilot (Agent Mode / Claude Sonnet 4.6)

**Primary prompt template used to scaffold the solution:**

```
You are a senior .NET architect. Scaffold a production-quality ASP.NET Core 8 Web API
following Hexagonal Architecture (Ports & Adapters / Clean Architecture).

Constraints:
- ADO.NET only — NO Entity Framework, NO Dapper, NO MediatR
- SQL Server with parameterized queries (OWASP compliance)
- xUnit + Moq for all layers — TDD approach (failing test first)
- JWT Bearer authentication with PBKDF2 password hashing
- The domain must include: TaskItem (title, description, status, due_date),
  User, Email value object, DomainException
- Expose: POST /api/auth/register, POST /api/auth/login,
  full CRUD /api/tasks, GET /api/tasks/public/stats (no auth)
- Projects: Domain / Application / Infrastructure / API / Tests (4 test projects)

Generate the solution structure with all interfaces (ports), implementations
(adapters), DTOs, and a numbered SQL migration runner.
```

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
