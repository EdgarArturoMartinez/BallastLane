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

Interviewers can start the full stack with a single, canonical command from the repository root. This builds the images, starts services and leaves the stack running:

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

## Run Locally (manual — requires .NET 8 SDK + SQL Server)

```bash
# 1. Set connection string
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BallastlaneDb;User Id=sa;Password=<your-sa-password>;TrustServerCertificate=True;"
export Jwt__Secret="<a-secret-key-of-at-least-32-chars>"
export Jwt__Issuer="ballastlane-api"
export Jwt__Audience="ballastlane-client"

# 2. Run API (applies migrations + seed on startup)
dotnet run --project src/Ballastlane.Api

# 3. Run frontend (separate shell)
cd web/ballastlane-web && npm install && npm run dev
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

### Configurar la URL del API para la UI (opcional)

El botón "API Docs" en la pantalla de login apunta por defecto a `http://localhost:5000`. Puedes sobrescribir esa URL en desarrollo con la variable de entorno `VITE_API_URL` (útil si el API corre en otro host/puerto).

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

También puedes crear un archivo `.env` o `.env.local` dentro de `web/ballastlane-web` con la línea:
```
VITE_API_URL=http://localhost:5000
```

Esto hará que la UI use la URL indicada para abrir Swagger desde el botón "API Docs" en la pantalla de login.

---

### Uso rápido de Swagger y ejemplos de API

Si quieres probar rápidamente los endpoints y cómo autorizarte, aquí hay pasos y ejemplos útiles.

1) Usar Swagger UI

- Abre `http://localhost:5000` (o la URL que hayas configurado con `VITE_API_URL`).
- Ejecuta `POST /api/auth/login` con el body JSON:

```json
{"username":"demo","password":"Demo@12345"}
```

- Copia el campo `token` de la respuesta.
- Pulsa el botón **Authorize** (candado) en la esquina superior derecha de Swagger y pega:

```
Bearer <tu_token>
```

- Ejecuta `GET /api/tasks` desde Swagger — ahora devolverá tus tareas.

2) PowerShell (login + obtener tasks)

```powershell
$r = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method Post -ContentType 'application/json' -Body '{"username":"demo","password":"Demo@12345"}'
$token = $r.token
Invoke-RestMethod -Uri 'http://localhost:5000/api/tasks' -Headers @{ Authorization = "Bearer $token" }
```

3) curl (login + obtener tasks)

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d '{"username":"demo","password":"Demo@12345"}' | jq -r .token)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/tasks
```

Nota: si tu API está en otra URL, sustituye `http://localhost:5000` por el valor de `VITE_API_URL`.


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
