# Ballastlane — Task Management API

[![CI](https://github.com/arturo-martinez/Ballastlane/actions/workflows/ci.yml/badge.svg)](https://github.com/arturo-martinez/Ballastlane/actions/workflows/ci.yml)

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
