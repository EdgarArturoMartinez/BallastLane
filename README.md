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

| Service | URL |
|---------|-----|
| Frontend | http://localhost:5173 |
| API (Swagger) | http://localhost:5000 |
| SQL Server | localhost:1433 |

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
