# Arthur Checkpoint — Exploration Phase

**Candidate:** Arturo Martínez | Senior Software Engineer (.NET / React / Angular)  
**Position:** Ballast Lane — .NET Technical Interview  
**Date:** April 10, 2026  

NOTE FOR ASSISTANT RESUME: The assistant should first read `/.github/copilot-instructions.md` to resume state. `Arthur_Checkpoint_Exploration.md` is extended history and should only be read when explicitly requested by the user.

---

## 1. Exercise Deep Analysis

### 1.1 Core Requirements Breakdown

| Area | Requirement | Criticality |
|------|------------|-------------|
| **Architecture** | Must follow **Clean Architecture** principles | 🔴 High — Explicit evaluation criterion |
| **Testing** | TDD methodology, unit tests for all layers | 🔴 High — Explicit evaluation criterion |
| **ORM Restriction** | ❌ No Entity Framework, ❌ No Dapper, ❌ No MediatR | 🔴 High — Hard constraint |
| **Data Layer** | Custom data access with CRUD operations | 🔴 High — Core deliverable |
| **Auth** | User registration, login, authorized/non-authorized endpoints | 🔴 High — Security requirement |
| **API** | ASP.NET Web API with proper HTTP verbs, params, return values | 🔴 High — Core deliverable |
| **Business Logic** | Independent layer with validation & business rules | 🟡 Medium — Must be decoupled |
| **Frontend** | React/Vue/etc., responsive, CRUD, structured code | 🟡 Medium — Full-stack proof |
| **GenAI** | Prompt engineering demo, validate AI output, edge cases | 🟡 Medium — Show critical thinking |
| **Database** | At least 2 tables: one for domain data, one for users | 🟢 Standard |
| **Seeded Data** | Demo credentials and sample data | 🟢 Standard |
| **README** | Setup instructions + documentation | 🟢 Standard |

### 1.2 Hidden Complexity & Traps

1. **No EF / No Dapper / No MediatR** — This is the single most impactful constraint. It forces:
   - Raw **ADO.NET** (`SqlConnection`, `SqlCommand`, `SqlDataReader`) or a hand-rolled micro data-access abstraction.
   - A **custom command/query dispatching** mechanism if using CQRS (since MediatR is banned).
   - Manual **parameterized queries** (must avoid SQL injection — OWASP Top 10).
   - Manual **connection lifecycle management** (`using` statements, connection pooling awareness).

2. **"A second API"** — The wording says _"a second API should include endpoints for user creation, user login, and authorized and non-authorized endpoints."_ This likely means:
   - A separate logical API grouping (e.g., `AuthController` vs `TasksController`), not necessarily a second running service.
   - Demonstrates understanding of **API separation of concerns**.

3. **User Story** — The candidate must **create their own user story**. This is an opportunity to show product thinking (not just code). A task management system fits well given the GenAI section references it explicitly.

4. **Presentation** — This is a live code review. Architecture decisions must be **defensible**. Every choice needs a "why."

5. **Clean Architecture evaluation** — They will check:
   - Domain/entities have **zero** framework dependencies.
   - Dependencies point **inward** (Dependency Rule).
   - Infrastructure is **pluggable** (could swap SQL Server for MongoDB without touching business logic).

### 1.3 User Stories (Formal Definition)

Given the GenAI section explicitly mentions a **task management system** with fields `title`, `description`, `status`, `due_date`, we align the entire project around this domain. The user stories below follow enterprise-grade conventions: structured description, field specification, business rules (RN-XX), and Gherkin acceptance criteria — the same format used in production Azure DevOps backlogs.

---

#### HU-01: Task Management (CRUD)

**Process:** Task Lifecycle Management  
**Subprocess:** Creation, Consultation, Modification, and Deletion of Tasks  

**As a:** Registered and authenticated user  
**I want to:** Create, view, update, and delete my personal tasks with title, description, status, and due date  
**So that:** I can organize and track my work efficiently, ensuring each task has clear ownership, deadlines, and status visibility  

**Functional Description**

The system must provide a Task Management module where authenticated users can manage their own tasks. Each task is owned by the user who created it; no cross-user task visibility is allowed. Tasks must support lifecycle transitions through defined statuses. The module must enforce data integrity, ownership isolation, and validation rules at the business logic layer — independent of the API and data layers.

**Data Model — Task Entity**

| # | Field | Type | Description |
|---|-------|------|-------------|
| 1 | `Id` | UNIQUEIDENTIFIER (PK) | System-generated unique identifier |
| 2 | `Title` | NVARCHAR(200), NOT NULL | Short descriptive name of the task |
| 3 | `Description` | NVARCHAR(2000), NULL | Detailed description or notes |
| 4 | `Status` | INT, NOT NULL | Task lifecycle: `Todo = 0`, `InProgress = 1`, `Done = 2` |
| 5 | `DueDate` | DATETIME2, NOT NULL | Target completion date |
| 6 | `UserId` | UNIQUEIDENTIFIER (FK), NOT NULL | Owner — references `Users.Id` |
| 7 | `CreatedAt` | DATETIME2 | Auto-generated on creation (UTC) |
| 8 | `UpdatedAt` | DATETIME2 | Auto-updated on modification (UTC) |

**Key Functionalities**

- **Ownership Isolation:** Users can only access (read/update/delete) tasks they own. The system must filter by `UserId` derived from the JWT token, never from client input.
- **Status Transitions:** Status changes must be validated (e.g., cannot move from `Done` back to `Todo` without business justification — configurable).
- **Soft Validation:** `DueDate` should be in the future for new tasks; updates allow past dates for historical tracking.
- **Pagination & Filtering:** Task listing should support status filtering and basic pagination for scalability.

**Business Rules**

| Code | Rule |
|------|------|
| RN-01 | `Title` is mandatory and must be between 3 and 200 characters. |
| RN-02 | `DueDate` must be a valid date. For new tasks, it must be today or in the future. |
| RN-03 | `Status` must be one of the defined values: `Todo (0)`, `InProgress (1)`, `Done (2)`. Invalid values are rejected. |
| RN-04 | A user can only view, update, or delete tasks where `UserId` matches the authenticated user's ID from the JWT claim. |
| RN-05 | Tasks cannot be deleted physically; they must support logical deletion or direct removal depending on business decision (for this MVP: hard delete is acceptable). |
| RN-06 | `UpdatedAt` must be automatically set to `GETUTCDATE()` on every modification. |
| RN-07 | All task operations (Create, Update, Delete) must be performed only by authenticated users with a valid JWT Bearer token. |

**Acceptance Criteria (Gherkin)**

```gherkin
Scenario 1 — Create a new task
  Given the user is authenticated with a valid JWT token
  When the user sends a POST request to /api/tasks with title "Review PR", description "Check unit tests", status "Todo", and dueDate "2026-04-20"
  Then the system should create the task and return HTTP 201 with the task details
  And the task's UserId must match the authenticated user's ID

Scenario 2 — List only own tasks
  Given the user is authenticated
  And there are tasks belonging to the user and tasks belonging to other users
  When the user sends a GET request to /api/tasks
  Then the system should return only tasks where UserId matches the authenticated user
  And tasks from other users must NOT be visible

Scenario 3 — Update a task
  Given the user is authenticated and owns task with Id "abc-123"
  When the user sends a PUT request to /api/tasks/abc-123 with updated title "Review PR v2"
  Then the system should update the task and return HTTP 200
  And the UpdatedAt field must reflect the current UTC time

Scenario 4 — Prevent updating another user's task
  Given the user is authenticated
  And task with Id "xyz-789" belongs to a different user
  When the user sends a PUT request to /api/tasks/xyz-789
  Then the system should return HTTP 403 Forbidden

Scenario 5 — Delete a task
  Given the user is authenticated and owns task with Id "abc-123"
  When the user sends a DELETE request to /api/tasks/abc-123
  Then the system should delete the task and return HTTP 204

Scenario 6 — Validation errors on create
  Given the user is authenticated
  When the user sends a POST request to /api/tasks with an empty title
  Then the system should return HTTP 400 with validation error "Title is required"

Scenario 7 — Due date validation
  Given the user is authenticated
  When the user sends a POST request to /api/tasks with dueDate "2020-01-01" (past date)
  Then the system should return HTTP 400 with validation error "DueDate must be today or in the future"
```

---

#### HU-02: User Authentication & Registration

**Process:** Identity & Access Management  
**Subprocess:** User Registration and JWT Authentication  

**As a:** Visitor (unauthenticated)  
**I want to:** Register a new account and log in to receive a JWT token  
**So that:** I can access protected endpoints and manage my tasks securely  

**Functional Description**

The system must provide an authentication module with registration and login endpoints. User passwords must be hashed using BCrypt before storage — plaintext passwords must never be persisted or logged. On successful login, the system issues a JWT Bearer token containing the user's ID and username as claims. The token must have a configurable expiration.

**Data Model — User Entity**

| # | Field | Type | Description |
|---|-------|------|-------------|
| 1 | `Id` | UNIQUEIDENTIFIER (PK) | System-generated unique identifier |
| 2 | `Username` | NVARCHAR(100), NOT NULL, UNIQUE | User's display name and login identifier |
| 3 | `Email` | NVARCHAR(200), NOT NULL, UNIQUE | User's email address |
| 4 | `PasswordHash` | NVARCHAR(500), NOT NULL | BCrypt hash of the password |
| 5 | `CreatedAt` | DATETIME2 | Auto-generated on registration (UTC) |

**Business Rules**

| Code | Rule |
|------|------|
| RN-08 | `Username` must be unique, between 3 and 100 characters, alphanumeric. |
| RN-09 | `Email` must be unique and in valid email format. |
| RN-10 | Password must meet minimum strength: at least 8 characters, one uppercase, one number. |
| RN-11 | Passwords must be hashed with BCrypt before storage. Plaintext passwords must never be stored or logged. |
| RN-12 | Login must validate credentials and return a JWT token on success, or HTTP 401 on failure. Error messages must not reveal whether the username or password was incorrect (security best practice). |
| RN-13 | JWT tokens must include `UserId` and `Username` claims, with a configurable expiration (default: 60 minutes). |
| RN-14 | Non-authorized endpoints (e.g., `GET /api/tasks/public/stats`) must be accessible without authentication. |

**Acceptance Criteria (Gherkin)**

```gherkin
Scenario 1 — Register a new user
  Given a visitor sends a POST request to /api/auth/register with username "arturo", email "arturo@mail.com", and password "Secure123"
  When the system processes the registration
  Then the system should create the user and return HTTP 201
  And the password stored in the database must be a BCrypt hash, not plaintext

Scenario 2 — Prevent duplicate registration
  Given a user with username "arturo" already exists
  When a visitor sends a POST request to /api/auth/register with the same username
  Then the system should return HTTP 409 Conflict

Scenario 3 — Login with valid credentials
  Given a registered user with username "arturo" and password "Secure123"
  When the user sends a POST request to /api/auth/login with correct credentials
  Then the system should return HTTP 200 with a valid JWT token
  And the token must contain UserId and Username claims

Scenario 4 — Login with invalid credentials
  Given a registered user with username "arturo"
  When the user sends a POST request to /api/auth/login with incorrect password
  Then the system should return HTTP 401 Unauthorized
  And the error message must not reveal whether the username or password was wrong

Scenario 5 — Access protected endpoint without token
  Given the user does NOT include an Authorization header
  When the user sends a GET request to /api/tasks
  Then the system should return HTTP 401 Unauthorized

Scenario 6 — Access public endpoint without token
  Given the user does NOT include an Authorization header
  When the user sends a GET request to /api/tasks/public/stats
  Then the system should return HTTP 200 with public data
```

---

## 2. Four Architecture Proposals

### Architecture 1: Classic Clean Architecture (Onion Model) with ADO.NET

```
┌─────────────────────────────────────────────────┐
│                 Presentation                     │
│         (ASP.NET Web API Controllers)            │
│         (React SPA Frontend)                     │
├─────────────────────────────────────────────────┤
│              Application Layer                   │
│      (Use Cases / Application Services)          │
│      (DTOs, Validators, Interfaces)              │
├─────────────────────────────────────────────────┤
│               Domain Layer                       │
│       (Entities, Value Objects, Enums)            │
│       (Domain Services, Business Rules)          │
├─────────────────────────────────────────────────┤
│            Infrastructure Layer                  │
│    (ADO.NET Repositories, SQL Server)            │
│    (JWT Auth Provider, Config)                   │
└─────────────────────────────────────────────────┘
```

**Project Structure:**
```
src/
├── TaskManager.Domain/            # Entities, interfaces, domain logic
├── TaskManager.Application/       # Use cases, DTOs, validation, service interfaces
├── TaskManager.Infrastructure/    # ADO.NET repos, JWT, SQL scripts
├── TaskManager.API/               # Controllers, middleware, DI config
├── TaskManager.Frontend/          # React app
tests/
├── TaskManager.Domain.Tests/
├── TaskManager.Application.Tests/
├── TaskManager.Infrastructure.Tests/
├── TaskManager.API.Tests/
```

**Pros:**
- ✅ Directly maps to the "Clean Architecture" evaluation criterion — easy to explain.
- ✅ Well-known pattern; interviewer expectations are aligned.
- ✅ Clear separation of concerns: Domain has zero dependencies.
- ✅ Dependency Rule is obvious: all arrows point inward.
- ✅ ADO.NET repositories are isolated in Infrastructure — could swap to MongoDB.
- ✅ Simple to test: mock repository interfaces in Application layer tests.

**Cons:**
- ⚠️ Can feel "over-layered" for a simple CRUD app.
- ⚠️ Mapping between layers (Entity → DTO → ViewModel) can be verbose.
- ⚠️ Boilerplate-heavy without MediatR to dispatch use cases.

**Best for:** Demonstrating textbook Clean Architecture knowledge. Safe, orthodox choice.

---

### Architecture 2: Vertical Slice Architecture with Feature Folders

```
┌──────────────────────────────────────────────────────┐
│                     Features/                         │
│  ┌──────────┐  ┌──────────┐  ┌────────────────────┐  │
│  │  Tasks/   │  │  Auth/   │  │  Users/            │  │
│  │ Create/   │  │ Login/   │  │  Register/         │  │
│  │  Read/    │  │ Token/   │  │  Profile/          │  │
│  │ Update/   │  │          │  │                    │  │
│  │ Delete/   │  │          │  │                    │  │
│  └──────────┘  └──────────┘  └────────────────────┘  │
│                                                       │
│  Each slice contains:                                 │
│  [Request] → [Handler] → [Validator] → [Repository]  │
├──────────────────────────────────────────────────────┤
│              Shared Infrastructure                    │
│     (ADO.NET DbContext, JWT, Middleware)              │
└──────────────────────────────────────────────────────┘
```

**Pros:**
- ✅ Each feature is self-contained — easy to navigate and understand.
- ✅ Minimal coupling between features.
- ✅ Adding new features doesn't affect existing ones.
- ✅ Natural fit for CRUD operations (each operation is one slice).

**Cons:**
- ❌ **Does NOT directly map to "Clean Architecture"** — the exercise explicitly requires it.
- ⚠️ Without MediatR, dispatching to handlers requires a custom pipeline.
- ⚠️ Shared domain logic becomes awkward (where does cross-feature validation live?).
- ⚠️ Interviewers expecting Onion/Hexagonal may not recognize this as "Clean Architecture."

**Best for:** Real-world productivity, but **risky for this interview** because the exercise explicitly says "Clean Architecture principles."

---

### Architecture 3: CQRS with Custom Dispatcher (No MediatR)

```
┌─────────────────────────────────────────────────────┐
│                  API Controllers                     │
│            (Thin, dispatch only)                     │
├───────────────────┬─────────────────────────────────┤
│   Command Side    │          Query Side              │
│                   │                                  │
│  CreateTaskCmd    │     GetTasksQuery                │
│  UpdateTaskCmd    │     GetTaskByIdQuery             │
│  DeleteTaskCmd    │                                  │
│  RegisterUserCmd  │     Custom IQueryDispatcher      │
│  LoginUserCmd     │                                  │
│                   │                                  │
│  Custom           │                                  │
│  ICommandDispatcher                                  │
├───────────────────┴─────────────────────────────────┤
│               Domain Layer                           │
│        (Entities, Business Rules)                    │
├─────────────────────────────────────────────────────┤
│            Infrastructure Layer                      │
│   (ADO.NET Read Repos, Write Repos, JWT)             │
└─────────────────────────────────────────────────────┘
```

**Custom Dispatcher Example (replacing MediatR):**
```csharp
public interface ICommandHandler<TCommand, TResult> { Task<TResult> HandleAsync(TCommand command); }
public interface IQueryHandler<TQuery, TResult> { Task<TResult> HandleAsync(TQuery query); }

public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _provider;
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = _provider.GetRequiredService(handlerType);
        return await handler.HandleAsync((dynamic)command);
    }
}
```

**Pros:**
- ✅ Shows advanced architectural thinking — goes beyond basic CRUD layering.
- ✅ Read and write paths can be optimized independently.
- ✅ Custom dispatcher demonstrates deep understanding of DI and generics.
- ✅ Still respects Clean Architecture layers (Domain → Application → Infrastructure).
- ✅ Directly addresses the "no MediatR" constraint with a custom solution.

**Cons:**
- ⚠️ Over-engineered for a simple CRUD — may seem like showing off.
- ⚠️ Custom dispatcher adds code surface area and potential bugs.
- ⚠️ More complex to test (dispatcher registration, handler resolution).
- ⚠️ Interview panel might question why CQRS for a small app.

**Best for:** Demonstrating advanced .NET skills, but the interviewer might see it as unnecessary complexity.

---

### Architecture 4: Hexagonal Architecture (Ports & Adapters) with Clean Architecture Layers

```
┌──────────────────────────────────────────────────────────┐
│                    Driving Adapters                       │
│              (REST API Controllers)                       │
│              (React Frontend)                             │
│              (Integration Test Harness)                   │
├──────────────────────────────────────────────────────────┤
│                    Input Ports                            │
│          (ITaskService, IAuthService)                     │
│          Application Services implement these             │
├──────────────────────────────────────────────────────────┤
│                   Application Core                        │
│  ┌────────────────────────────────────────────────────┐   │
│  │              Domain Model                          │   │
│  │   TaskItem, User, Status (Value Object)            │   │
│  │   Business rules, validation, domain services      │   │
│  └────────────────────────────────────────────────────┘   │
│  ┌────────────────────────────────────────────────────┐   │
│  │          Application Services                       │  │
│  │   TaskService, AuthService                          │  │
│  │   Orchestrate domain + output ports                 │  │
│  └────────────────────────────────────────────────────┘   │
├──────────────────────────────────────────────────────────┤
│                   Output Ports                            │
│         (ITaskRepository, IUserRepository)                │
│         (IPasswordHasher, ITokenGenerator)                │
├──────────────────────────────────────────────────────────┤
│                   Driven Adapters                         │
│        (SqlServerTaskRepository — ADO.NET)                │
│        (SqlServerUserRepository — ADO.NET)                │
│        (BcryptPasswordHasher)                             │
│        (JwtTokenGenerator)                                │
└──────────────────────────────────────────────────────────┘
```

**Project Structure:**
```
src/
├── TaskManager.Domain/              # Entities, value objects, domain rules
│   ├── Entities/
│   │   ├── TaskItem.cs
│   │   └── User.cs
│   ├── ValueObjects/
│   │   └── TaskStatus.cs
│   └── Exceptions/
│       └── DomainException.cs
│
├── TaskManager.Application/         # Ports (interfaces) + Application Services
│   ├── Ports/
│   │   ├── Input/
│   │   │   ├── ITaskService.cs
│   │   │   └── IAuthService.cs
│   │   └── Output/
│   │       ├── ITaskRepository.cs
│   │       ├── IUserRepository.cs
│   │       ├── IPasswordHasher.cs
│   │       └── ITokenGenerator.cs
│   ├── Services/
│   │   ├── TaskService.cs
│   │   └── AuthService.cs
│   └── DTOs/
│       ├── TaskDto.cs
│       ├── CreateTaskRequest.cs
│       └── LoginRequest.cs
│
├── TaskManager.Infrastructure/      # Driven Adapters (Output Port Implementations)
│   ├── Persistence/
│   │   ├── SqlServerTaskRepository.cs
│   │   ├── SqlServerUserRepository.cs
│   │   └── DatabaseInitializer.cs          # Programmatic: CREATE IF NOT EXISTS + seed
│   ├── Security/
│   │   ├── BcryptPasswordHasher.cs
│   │   └── JwtTokenGenerator.cs
│   └── Scripts/                            # Raw SQL files (versioned, reviewable)
│       ├── 001_CreateDatabase.sql          # CREATE DATABASE IF NOT EXISTS
│       ├── 002_CreateUsersTable.sql         # Users table DDL
│       ├── 003_CreateTasksTable.sql         # Tasks table DDL + FK
│       └── 004_SeedData.sql                # Demo user + sample tasks
│
├── TaskManager.API/                 # Driving Adapter (REST)
│   ├── Controllers/
│   │   ├── TasksController.cs
│   │   └── AuthController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
│
├── TaskManager.Frontend/            # Driving Adapter (UI)
│   └── (React App)
│
tests/
├── TaskManager.Domain.Tests/
├── TaskManager.Application.Tests/
├── TaskManager.Infrastructure.Tests/
└── TaskManager.API.Tests/
```

**Pros:**
- ✅ **Best fit for this exercise** — it IS Clean Architecture with explicit port/adapter terminology.
- ✅ Ports & Adapters make the "no EF/Dapper" constraint a non-issue — the adapter is just one implementation.
- ✅ Extremely testable: mock output ports for unit tests, swap adapters for integration tests.
- ✅ Domain is 100% framework-free (no ASP.NET references, no SQL references).
- ✅ Aligns with Arturo's resume: Clean Architecture, Microservices, SOLID — this is the natural expression.
- ✅ Easy to explain in presentation: "Here are my ports, here are my adapters."
- ✅ Shows maturity: not just layers, but explicit contracts between layers.
- ✅ Frontend (React) is just another driving adapter — consistent mental model.

**Cons:**
- ⚠️ Slightly more interfaces than classic Onion — but each one is justified.
- ⚠️ Requires discipline to keep domain pure.

**Best for:** This exact interview. Clean, defensible, impressive, and practical.

---

## 3. Architecture Comparison Matrix

| Criterion | 1. Classic Onion | 2. Vertical Slice | 3. CQRS Custom | 4. Hexagonal (Ports & Adapters) |
|-----------|:---:|:---:|:---:|:---:|
| Clean Architecture alignment | ⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Testability (TDD fit) | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| No EF/Dapper adaptation | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| No MediatR adaptation | ⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| Presentation clarity | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Implementation speed | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |
| Avoids over-engineering | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |
| Shows senior expertise | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

---

## 4. Final Architecture Decision

### ✅ CHOSEN: Architecture 4 — Hexagonal (Ports & Adapters) with Clean Architecture Layers

### Rationale

**Why Hexagonal over Classic Onion (Architecture 1)?**
Both are valid Clean Architecture implementations. Hexagonal adds the explicit concept of **Ports** (interfaces that define contracts) and **Adapters** (implementations that plug in). This vocabulary is more precise and gives Arturo better language during the presentation. Instead of saying "I have repositories," he says "I defined output ports for persistence and implemented SQL Server adapters using raw ADO.NET — if I needed MongoDB tomorrow, I'd write a new adapter without touching a single line in Application or Domain."

**Why not Vertical Slice (Architecture 2)?**
The exercise **explicitly requires Clean Architecture**. Vertical Slice is a modern alternative, but an interviewer evaluating "Clean Architecture adherence" may not consider it compliant. Too risky.

**Why not CQRS (Architecture 3)?**
The custom dispatcher is impressive but introduces complexity that isn't justified for a simple CRUD app. Interviewers may see it as over-engineering. However, elements of CQRS thinking (separate read/write DTOs) can be incorporated subtly into Architecture 4.

**Why this is the best match for Arturo's profile:**
- 16+ years of .NET experience → Hexagonal shows mature architectural thinking, not just framework knowledge.
- Clean Architecture + SOLID listed on resume → This architecture is the purest expression of those principles.
- Microservices background → Ports & Adapters is how services communicate in microservice ecosystems.
- Azure / Cloud-native experience → Adapter pattern is how cloud services are abstracted.
- GenAI integration experience → The presentation can reference how AI helped validate the architecture.

---

## 5. Technical Implementation Strategy (Preview)

### Containerization: Docker Compose (Full Stack)
The entire application will run via `docker-compose up` — a single command that starts SQL Server, the .NET API, and the React frontend. This eliminates the "works on my machine" problem and ensures interviewers can run the project without installing SQL Server, .NET SDK, or Node.js locally.

```yaml
# docker-compose.yml (preview)
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=TaskManager_Dev#2026
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/lib/mssql/data

  api:
    build: ./src/TaskManager.API
    depends_on:
      - sqlserver
    ports:
      - "5000:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TaskManagerDb;User Id=sa;Password=TaskManager_Dev#2026;TrustServerCertificate=true

  frontend:
    build: ./src/TaskManager.Frontend
    depends_on:
      - api
    ports:
      - "3000:3000"

volumes:
  sqldata:
```

- **Database auto-initialization:** The API runs `DatabaseInitializer.cs` on startup — it reads the numbered SQL scripts from `Infrastructure/Scripts/`, executes them in order via ADO.NET, and skips already-applied scripts. This is a **hybrid approach**: SQL files are versioned and human-readable in the repo, but execution is automated programmatically.
- **Scripts folder structure:**
  - `001_CreateDatabase.sql` — Creates the database if it doesn't exist
  - `002_CreateUsersTable.sql` — Users table DDL
  - `003_CreateTasksTable.sql` — Tasks table DDL with FK to Users
  - `004_SeedData.sql` — Inserts demo user (`demo / Demo1234!`) and 5 sample tasks
- **Seeded credentials:** `demo / Demo1234!` pre-loaded for immediate login.
- **README instructions:** Both `docker-compose up` (zero-install) and manual setup (for developers who prefer local tools).

### Database: SQL Server (Containerized via Docker)

> ⚠️ **Actual schema** from `sql/migrations/0001_init.sql` — supersedes any earlier conceptual schema in this document.

```sql
-- Users table
CREATE TABLE Users (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Username     NVARCHAR(100)    NOT NULL,
    Email        NVARCHAR(320)    NOT NULL,
    PasswordHash NVARCHAR(512)    NOT NULL,
    Salt         NVARCHAR(128)    NOT NULL,
    Role         NVARCHAR(50)     NOT NULL DEFAULT 'User',  -- values: 'User', 'Admin'
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email    UNIQUE (Email)
);

-- Tasks table
CREATE TABLE Tasks (
    Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title       NVARCHAR(200)    NOT NULL,
    Description NVARCHAR(2000)   NOT NULL DEFAULT '',
    Status      NVARCHAR(50)     NOT NULL DEFAULT 'Todo',  -- values: 'Todo', 'InProgress', 'Done'
    DueDate     DATETIME2        NULL,
    OwnerUserId UNIQUEIDENTIFIER NOT NULL,  -- FK column is OwnerUserId, not UserId
    CONSTRAINT FK_Tasks_Users FOREIGN KEY (OwnerUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Tasks_OwnerUserId ON Tasks(OwnerUserId);

-- __Migrations table (auto-created by DbMigrator — not in SQL files)
-- ScriptName NVARCHAR(200) PK, AppliedAt DATETIME2
```

**Key schema facts (critical for ADO.NET queries):**
- Task owner column is `OwnerUserId` — never `UserId`.
- `Status` is `NVARCHAR(50)`, not INT — filter with `WHERE Status = 'Todo'`.
- `DueDate` is nullable — handle `DBNull` in SqlDataReader.
- Users have `Salt` (separate from PasswordHash) and `Role` columns.
- No `CreatedAt`/`UpdatedAt` in Tasks; no `CreatedAt` in Users in this migration.

**Migration files:**
- `sql/migrations/0001_init.sql` — schema (Users + Tasks tables)
- `sql/migrations/0002_seed.sql` — demo user (`demo` / `Demo@12345`, role: `Admin`, email: `demo@ballastlane.dev`) + 3 sample tasks

### Data Access: Raw ADO.NET with Parameterized Queries
- `SqlConnection` with `using` statements for proper disposal.
- `SqlCommand` with `SqlParameter` for SQL injection prevention.
- `SqlDataReader` for read operations, `ExecuteNonQueryAsync` for write operations.
- Connection string from `IConfiguration` (appsettings.json).

### Authentication: JWT Bearer Tokens
- BCrypt for password hashing (via `BCrypt.Net-Next` NuGet).
- JWT token generation with claims (UserId, Username).
- `[Authorize]` attribute on protected endpoints.
- One public endpoint (e.g., `GET /api/tasks/public/stats`) to demonstrate non-authorized access.

### Logging: Built-in `ILogger<T>` (Structured Logging)
- `ILogger<T>` injected via constructor DI in Controllers, Application Services, and Infrastructure Adapters.
- **No logging in Domain layer** — domain stays pure.
- Log levels: `Debug` (flow tracing), `Information` (business events), `Warning` (failed lookups, auth failures), `Error` (exceptions).
- Structured message templates: `_logger.LogInformation("Task {TaskId} created by {UserId}", id, userId)`.
- Console sink for development; mention Serilog + Application Insights as production upgrade path.
- **Never log sensitive data:** passwords, JWT tokens, PII.

### Frontend: React with TypeScript
- Vite for tooling.
- Axios for HTTP calls to the API.
- React Router for navigation.
- Simple, clean component structure.
- Responsive layout (CSS Grid/Flexbox or a minimal UI library).

### Testing Strategy (TDD)
| Layer | Test Type | Tools | What to test |
|-------|-----------|-------|--------------|
| Domain | Unit | xUnit | Entity validation, business rules, value objects |
| Application | Unit | xUnit + Moq | Service logic with mocked output ports |
| Infrastructure | Integration | xUnit + TestContainers or LocalDB | ADO.NET repos against real DB |
| API | Integration | xUnit + WebApplicationFactory | Controller endpoints, auth flow |

---

## 6. Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| Raw ADO.NET is verbose and error-prone | Create a thin `DbHelper` class for connection/command management (not an ORM, just a helper) |
| SQL injection via string concatenation | Enforce parameterized queries only; code review every SQL string |
| JWT token security | Short expiration, proper secret key management, HTTPS enforcement |
| Time management (scope creep) | MVP first: CRUD + Auth working end-to-end, then polish |
| Frontend quality vs. backend focus | Use a component library (e.g., Material UI) for quick professional look |
| Presentation nerves | Prepare talking points for each architectural decision |
| Interviewer can't run the project (no SQL Server, wrong .NET version) | Docker Compose: `docker-compose up` runs everything — zero local dependencies required |
| Docker not installed on reviewer's machine | README provides both paths: Docker (recommended) and manual setup with LocalDB fallback |

---

## 7. Presentation Talking Points (Architecture Defense)

1. **"Why Hexagonal?"** → "It's the most explicit expression of Clean Architecture. Ports define what I need, adapters provide how. The domain knows nothing about SQL or HTTP."

2. **"Why raw ADO.NET?"** → "The constraint removed EF and Dapper. ADO.NET is the foundation they're built on. It shows I understand what ORMs abstract away: connections, commands, readers, parameterization."

3. **"Why not MediatR replacement?"** → "For this scope, direct service injection is cleaner. A custom dispatcher would be over-engineering. I kept CQRS thinking in my DTOs (separate Create/Update requests from query responses) without the pipeline complexity."

4. **"How does TDD work here?"** → "I wrote port interfaces first (the contract), then tests against those contracts, then the implementation. Red-Green-Refactor at every layer."

5. **"Why React?"** → "Industry standard, my resume shows React experience, and it pairs naturally with a REST API. TypeScript adds type safety that mirrors C#'s strong typing."

6. **"Why `ILogger<T>` and not Serilog?"** → "ILogger<T> is built into ASP.NET Core — zero dependencies. In production I'd add Serilog as a provider for richer sinks (Application Insights, Seq), but the abstraction stays the same. The domain layer never logs directly — that's a Clean Architecture boundary I enforced."

7. **"Why Docker Compose?"** → "I assumed the reviewers may not have SQL Server installed locally, or may be on Mac/Linux. Docker Compose lets you run the entire stack — SQL Server, API, and frontend — with a single `docker-compose up`. Zero prerequisites beyond Docker itself. It also proves I think about DevOps and deployment, not just code."

---

---

## 8. AI And Human Decisions

> This section documents the real discussions between Arturo (the developer) and the AI assistant during the exploration phase. These are not AI-generated outputs accepted blindly — they are deliberated decisions where the human challenged, validated, and redirected the AI's proposals.

### Recent Human Decisions (2026-04-12)

The following decisions were explicitly chosen by Arturo after direct discussion with the AI. They record human judgement and distinguish developer decisions from AI suggestions.

- **Drag-and-drop for tasks:** Arturo decided to add drag-and-drop behavior to task cards so users can change task status by dragging cards between columns. Implementation notes: drag handlers were added to `web/ballastlane-web/src/pages/TasksPage.tsx` to persist status changes via the Tasks API and provide immediate visual feedback.
- **Frontend layout change:** Arturo altered the basic frontend layout originally proposed by the AI, approving a Tailwind-based Kanban redesign (Login, Header, Tasks pages) to improve clarity and presentation.
- **Encoding fixes:** Arturo identified encoding issues (for example `Â·` and `â€¦`) in the UI and directed the fixes: replace escaped sequences in source files, add `charset utf-8` to `web/ballastlane-web/nginx.conf`, and add `.editorconfig` and `.gitattributes` to the repository. The web image was rebuilt and verified.
- **API Docs button:** Arturo requested an "API Docs" (Swagger) button on the Login page so reviewers can quickly open and test API endpoints. The button was implemented and the link was made configurable via `VITE_API_URL`.
- **Preserve repository architecture:** Arturo declined to accept additional files, folders, or scripts suggested by the AI that would have altered or complicated the project's Hexagonal/Clean architecture. Preference was given to minimal, well-scoped changes that preserve reviewability and the existing repository structure.

- **Audit write instrumentation:** Arturo approved adding write-side audit instrumentation so create/update/delete operations are recorded. Implementation notes: added `sql/migrations/0003_audit.sql` (creates `Audits` table), extended `IAuditRepository` with `InsertAsync`, implemented `AdoAuditRepository.InsertAsync`, and added audit inserts in `TasksController` (Create/Update/Delete) and `AuthService.Register`. Verified locally: the API applies migrations at startup and creating a task as `demo` produces an `Audits` row with `NewValues` JSON. The audit feature provides a simple forensic trail for Tasks and Users changes.

- **Human decisions by Arturo:**
  - **Server vs. local timestamps:** Arturo approved showing both the server timestamp and the user's local timestamp in the audit UI to improve forensic clarity; the UI displays server time, user local time, and timezone indicators/flags.
  - **Server-side and client-side pagination:** Arturo requested backend and frontend pagination for audit records to ensure scalability; paging was implemented in `IAuditRepository.ListPagedAsync` and consumed by the UI.
  - **Audit strategy:** Arturo defined and approved the audit strategy: persist `OldValues` and `NewValues` JSON in `Audits` rows, expose metadata endpoints, and provide paged endpoints for browsing and filtering.
  - **Drag-and-drop UX decision:** Arturo approved adding drag-and-drop to `TasksPage` with persisted status changes in the API to provide an intuitive workflow and immediate visual feedback.

These entries document the human-driven decisions and provide short rationales to aid reviewers during the interview. They are the definitive record of choices made on 2026-04-12.

---

### Context files & Copilot integration (Architectural decision)

- `.github/copilot-instructions.md`: repo-level instructions. Purpose: short, authoritative policy the assistant can consult. Place concise, high-signal rules here.

- `docs/Arthur_Checkpoint_Exploration.md`: canonical checkpoint and project facts. Keep this file up to date with decisions, phase status and short EOD summaries.

Notes on auto-loading and usage:
- The assistant may consult `/.github/copilot-instructions.md` and `docs/Arthur_Checkpoint_Exploration.md` when requested. If you edit these files mid-session, ask the assistant to re-read them explicitly.
- Keep these files short (50–200 lines) and focused so they are easy to read and reference.

Why this is an architectural decision: it formalizes how we provide context to AI helpers — separating permanent policy and short daily context — so the assistant behaves predictably and reviewers can reproduce the same environment.


### Discussion 1: Data Access — Is ADO.NET the Only Option? (Human-Initiated)

**Arturo's question:** _"The exercise bans Entity Framework, Dapper, and MediatR. But are there other data access tools we could use instead of raw ADO.NET? I want to make sure we're not choosing ADO.NET just because it's the first thing that comes to mind."_

**Alternatives Evaluated:**

| Tool | Type | Banned? | Viability Assessment |
|------|------|---------|---------------------|
| **ADO.NET** (SqlConnection, SqlCommand, SqlDataReader) | Raw data access provider | ❌ Not banned | ✅ Built into .NET runtime. Zero external dependencies. Full control. |
| **RepoDB** | Micro-ORM | Not explicitly banned | ⚠️ Gray area. It's an ORM — the exercise spirit is to avoid ORMs. An interviewer could challenge this as "using another Dapper." |
| **ServiceStack.OrmLite** | Micro-ORM | Not explicitly banned | ⚠️ Same gray area as RepoDB. Also adds a significant dependency. |
| **Npgsql** (raw, no EF) | ADO.NET provider for PostgreSQL | ❌ Not banned | ✅ Viable if using PostgreSQL instead of SQL Server. Same raw access pattern as ADO.NET. |
| **Microsoft.Data.Sqlite** | ADO.NET provider for SQLite | ❌ Not banned | ✅ Viable for lightweight/embedded DB. But SQLite lacks some SQL Server features (stored procs, schemas). |
| **MongoDB.Driver** | NoSQL native driver | ❌ Not banned | ✅ Viable. Document-based. No SQL needed. But the exercise mentions "tables" and "primary keys" — relational is implied. |
| **LiteDB** | Embedded NoSQL | ❌ Not banned | ⚠️ Too simple. Doesn't demonstrate meaningful data layer skills. |
| **Custom micro-ORM** (hand-built) | Abstraction over ADO.NET | ❌ Not banned | ⚠️ Could be impressive, but risks over-engineering and detracts from business logic. |

**Final Decision (Human + AI consensus):** **ADO.NET with SQL Server**

**Arturo's justification:**
1. **Intent of the constraint:** The exercise bans EF, Dapper, and MediatR to test whether candidates understand what these tools abstract away. Using another micro-ORM like RepoDB would technically comply but violate the spirit of the constraint. The interviewers want to see raw data access — connection management, parameterized queries, reader mapping.

2. **SQL Server is the natural .NET pair:** The exercise says ".NET C#, ASP.NET MVC, Web API." SQL Server is the ecosystem database. Using MongoDB or SQLite would add an unnecessary "why?" to the presentation without architectural benefit.

3. **ADO.NET demonstrates foundational knowledge:** At 16+ years of .NET experience, showing that you understand `SqlConnection`, `SqlParameter`, `SqlDataReader`, connection pooling, and `using` disposal patterns proves you know what EF abstracts. This is exactly what the interviewers want to verify.

4. **Security by default — parameterized queries:** ADO.NET forces you to explicitly write parameterized queries (`@Title`, `@UserId`), which demonstrates OWASP awareness. ORMs hide this, and the interview is testing whether you'd write safe SQL on your own.

5. **Clean Architecture alignment:** In our Hexagonal Architecture, ADO.NET is just an **adapter implementation** behind an output port (`ITaskRepository`). The choice of ADO.NET vs. MongoDB vs. anything else is an infrastructure decision — the domain and application layers never know. This is the whole point of Ports & Adapters. During the presentation, Arturo can say: _"If you asked me to switch to PostgreSQL tomorrow, I'd write a new Npgsql adapter implementing the same port interface — zero changes to domain or application."_

**What we explicitly rejected and why:**
- **RepoDB / OrmLite:** Technically not banned, but using a micro-ORM to circumvent the ORM restriction would look like a loophole exploit, not a demonstration of skill.
- **MongoDB:** The exercise mentions "database with tables" and "primary key" — relational semantics. MongoDB is valid but adds unnecessary justification burden.
- **Custom micro-ORM:** Building a mini Dapper would be impressive but distracts from the actual business requirements. We're demonstrating architecture, not reinventing the wheel.

---

### Discussion 2: User Story Depth — Enterprise Standards (Human-Initiated)

**Arturo's input:** _"The initial AI-generated user story was too generic — just a one-liner with bullet-point acceptance criteria. In real Azure DevOps projects, we structure user stories with field specifications, business rules (RN-XX codes), Gherkin acceptance criteria, and functional descriptions. I shared a reference document (DevopsClasificacionArancelaria.md) from a real enterprise project to show the level of detail expected."_

**What changed based on Arturo's feedback:**
- Added **formal HU-XX numbering** (HU-01, HU-02) matching enterprise backlog conventions.
- Added **Process / Subprocess** headers for traceability.
- Added **Data Model tables** with field-by-field specification (type, constraints, description).
- Added **Business Rules (RN-01 through RN-14)** with codified rules — the same format used in production DevOps backlogs.
- Replaced simple bullet criteria with **Gherkin scenarios** (Given/When/Then) that are testable and unambiguous.
- Added **Functional Description** paragraphs explaining the business context, not just technical requirements.
- Split into two user stories (Tasks + Auth) because they are distinct business capabilities.

**Why this matters for the interview:**
- Shows Arturo doesn't just code — he **thinks in product terms**.
- Gherkin scenarios map directly to test cases (TDD alignment).
- Business rules (RN-XX) demonstrate the discipline to formalize constraints before coding.
- The interviewer can see that the user story drove the architecture, not the other way around.

---

### Discussion 4: Logging Strategy — A Gap Detected by the Human (Human-Initiated)

**Arturo's observation:** _"I reviewed the entire checkpoint document and realized there is no logging strategy anywhere — not in the technical implementation, not in the architecture, not in the risk mitigation. Should we add one? Would it be a bonus? Is it already implied somewhere?"_

**AI's honest answer:** No, logging was not contemplated. The AI focused on the explicit exercise requirements (CRUD, Auth, Clean Architecture, TDD) and missed this cross-cutting concern entirely. This is precisely the kind of gap that human review catches.

**Analysis: Is Logging Required by the Exercise?**

The exercise document does **not** explicitly require logging. However:

| Evaluation Criterion (from exercise) | Logging Relevance |
|--------------------------------------|-------------------|
| **Clean Architecture** | ✅ Logging is a classic cross-cutting concern — how you integrate it reveals architectural maturity (does it violate layer boundaries?) |
| **Code quality / best practices** | ✅ Production-grade code always has structured logging. Its absence would be noticed. |
| **Functionality without errors** | ✅ Logging helps trace issues during the live demo. If something fails, logs tell the story. |
| **Presentation** | ✅ Mentioning logging strategy shows operational awareness beyond "it works on my machine." |

**Verdict: Not required, but a high-value bonus that demonstrates production thinking.**

**Logging Strategy Chosen: Built-in `ILogger<T>` with Structured Logging**

| Option | Description | Decision |
|--------|-------------|----------|
| **`ILogger<T>`** (Microsoft.Extensions.Logging) | Built into ASP.NET Core. Zero extra dependencies. DI-friendly. | ✅ **Chosen** |

| **Serilog** | Popular structured logging library. Sinks for file, Seq, Elasticsearch. | ⚠️ Powerful but adds dependency. Could be mentioned as "production upgrade" in presentation. |
| **NLog** | Mature logging framework. XML config-heavy. | ❌ Rejected — config overhead not justified for this scope. |
| **Console.WriteLine** | Quick and dirty. | ❌ Rejected — not professional. No structure, no levels, no DI. |

**Why `ILogger<T>` is the right choice:**

1. **Zero external dependencies:** It's part of `Microsoft.Extensions.Logging`, already included in ASP.NET Core. No extra NuGet packages = cleaner project.
2. **Clean Architecture compliance:** `ILogger<T>` is injected via constructor DI. The Application and Domain layers can log through the abstraction without knowing the implementation (console, file, cloud). This is the Ports & Adapters pattern applied to logging.
3. **Structured logging built-in:** Supports message templates with named parameters: `_logger.LogInformation("Task {TaskId} created by user {UserId}", task.Id, userId)` — these are queryable in production.
4. **Log levels aligned with use cases:**

| Level | When to Use | Example in This Project |
|-------|-------------|------------------------|
| `LogDebug` | Detailed flow for development | `"Entering CreateTask with title {Title}"` |
| `LogInformation` | Key business events | `"Task {TaskId} created successfully"`, `"User {Username} logged in"` |
| `LogWarning` | Recoverable issues | `"Login failed for username {Username}"`, `"Task {TaskId} not found"` |
| `LogError` | Exceptions and failures | `"Database connection failed: {Error}"`, `"Unhandled exception in TaskService"` |

5. **Presentation bonus:** During the demo, Arturo can show the console output with structured logs, demonstrating that the application is observable. If asked about production readiness, he can say: _"In production, I'd swap the console sink for Serilog with Azure Application Insights or Seq — zero code changes, just configuration. The `ILogger<T>` abstraction makes this a deployment decision, not a code decision."_

**Where Logging Lives in the Hexagonal Architecture:**
 
---

## EOD Update (2026-04-11)

- Completed: Updated repo-level agent instructions, added daily-context template, and recorded project conventions in repo memory.
- Files changed: [.github/copilot-instructions.md](.github/copilot-instructions.md), Arthur_Checkpoint_Exploration.md
- Decisions: Keep repository docs in English; follow EOD routine and memory usage for consistent agent resumption.
- Blockers: User finishing Docker Desktop restart before starting containers.
- Next: 1) User confirms Docker ready 2) Start `docker-compose up --build -d` to validate stack 3) Add CI pipeline (GitHub Actions)

### Next-session checklist (what to run first)
1. Pull latest branch: `git pull origin <branch>`
2. Open [docs/Arthur_Checkpoint_Exploration.md](docs/Arthur_Checkpoint_Exploration.md) to review the current state.
3. Ask the assistant: "Resume project using `docs/Arthur_Checkpoint_Exploration.md`. Show a 5-line plan to continue."
4. If Docker Desktop was restarted, start the stack:
  ```powershell
  docker-compose up --build -d
  docker-compose ps
  docker-compose logs -f
  ```
5. If you prefer local dev without Docker, run `dotnet build` and `dotnet test` locally.

```
┌──────────────────────────────────────────────────┐
│  API Layer (Controllers)                         │
│  → Log: Request received, response codes         │
│  → Log: Authentication events                    │
├──────────────────────────────────────────────────┤
│  Application Layer (Services)                    │
│  → Log: Business operations (create, update...)  │
│  → Log: Validation failures                      │
│  → Log: Warnings (not found, unauthorized)       │
├──────────────────────────────────────────────────┤
│  Infrastructure Layer (Adapters)                 │
│  → Log: Database connection events               │
│  → Log: Query execution (debug level)            │
│  → Log: Errors (connection failures, timeouts)   │
└──────────────────────────────────────────────────┘

Domain Layer: NO logging. Domain entities are pure.
Logging never leaks into the domain — it stays in
Application and Infrastructure layers.
```

**Key architectural decision:** The Domain layer does **NOT** log. Domain entities and value objects are pure business logic with no infrastructure concerns. If a domain rule fails, it throws a `DomainException` — the Application layer catches it, logs it, and translates it to an appropriate HTTP response. This preserves the Clean Architecture dependency rule.

**What we will NOT do:**
- ❌ No logging of sensitive data (passwords, tokens, PII).
- ❌ No `try/catch` with `Console.WriteLine` — only `ILogger<T>` through DI.
- ❌ No logging in Domain entities — keeps domain pure.
- ❌ No over-logging — only meaningful business events and errors.

---

### Discussion 5: Docker & Deployment Strategy — Thinking About the Reviewer's Experience (Human-Initiated)

**Arturo's concern:** _"What about Docker? The interviewers will need to install and run the solution on their machines. But they may not have SQL Server, or they could be on Mac/Linux. How do we handle this? Should we containerize everything?"_

**AI's assessment:** The original checkpoint assumed SQL Server LocalDB (Windows-only, requires Visual Studio). This is a risky assumption — the interviewers could be on any OS, and requiring manual SQL Server installation creates friction that could hurt the first impression before they even see the code.

**Options Evaluated:**

| Strategy | Pros | Cons | Decision |
|----------|------|------|----------|
| **1. Docker Compose (full stack)** | One command runs everything. Cross-platform. No local installs. | Requires Docker installed. Image download ~1.5GB first time. | ✅ **Chosen as primary path** |
| **2. Docker only for SQL Server** | Lighter than full compose. API runs locally with hot-reload. | Reviewer still needs .NET SDK + Node.js locally. | ✅ **Included as dev alternative** |
| **3. SQLite fallback (no Docker at all)** | Zero dependencies. Embedded DB. | Different SQL dialect. Can't demo SQL Server-specific features. Feels like a compromise. | ⚠️ Considered but rejected |
| **4. LocalDB only** | Comes with Visual Studio. No Docker needed. | Windows-only. Fails on Mac/Linux. Some VS editions don't include it. | ❌ Rejected as sole option |
| **5. Cloud-hosted DB (Azure SQL)** | No local DB needed at all. | Costs money. Requires internet. Connection string exposes credentials. | ❌ Rejected — security and cost concerns |

**Final Decision (Human + AI consensus):** **Docker Compose as primary + Manual setup with LocalDB as fallback**

**Arturo's justification:**

1. **Reviewer empathy:** The first thing an interviewer does is clone the repo and try to run it. If they hit a "SQL Server not found" error, the project starts with a negative impression. `docker-compose up` eliminates this entirely — one command, everything works.

2. **Cross-platform:** SQL Server runs in Docker on Windows, Mac (Intel & Apple Silicon via Rosetta), and Linux. No assumptions about the reviewer's OS.

3. **Demonstrates DevOps awareness:** The exercise evaluation includes "best practices." Containerization is a modern best practice. It shows Arturo thinks about the full delivery pipeline, not just the code. This aligns with his resume (Azure, CI/CD, Docker, DevOps).

4. **Seeded data guaranteed:** The `DatabaseInitializer` runs inside the API container at startup. It creates tables if they don't exist and seeds demo users + tasks. The reviewer opens the app and sees data immediately — no manual SQL scripts to run.

5. **Reproducibility:** "Works on my machine" is not acceptable for a senior engineer's deliverable. Docker ensures deterministic environments.

**What the README will offer — two paths:**

```
## Quick Start (Recommended — requires only Docker)

1. Clone the repository
2. Run: docker-compose up --build
3. Open: http://localhost:3000 (Frontend)
4. API: http://localhost:5000/swagger
5. Login: demo / Demo1234!

## Manual Setup (for development without Docker)

Prerequisites: .NET 8 SDK, Node.js 18+, SQL Server (LocalDB or full)
1. Update connection string in appsettings.Development.json
2. Run API: dotnet run --project src/TaskManager.API
3. Run Frontend: cd src/TaskManager.Frontend && npm install && npm run dev
4. Database is auto-created on first API startup
```

**Why we rejected SQLite as fallback:**
Arturo considered having a `--use-sqlite` flag for zero-dependency runs. We decided against it because:
- It would require a second adapter implementation (`SqliteTaskRepository`) — more code surface, more tests, more maintenance.
- The Hexagonal Architecture **could** support it (just another adapter behind the port), but the effort-to-value ratio doesn't justify it for this exercise.
- However, this is a great **presentation talking point**: _"My architecture supports swapping SQL Server for SQLite by writing a new adapter — I didn't implement it because it wasn't in scope, but the port/adapter design makes it trivial."_

---

### Discussion 6: Database Initialization Strategy — Scripts, Code, or Both? (Human-Initiated)

**Arturo's question:** _"So we said Docker handles everything, but how exactly does the database get created and seeded? Do we need a `db/` folder with SQL scripts? Does Docker run them? Or does the C# code handle it? I want to understand the full flow from `docker-compose up` to 'the app is ready with data.'"_

**AI clarification:** The previous checkpoint mentioned `DatabaseInitializer.cs` and `seed.sql` but never explained the actual initialization flow. This is a gap — the mechanism was hand-waved. The human is right to push for a concrete design.

**Three Approaches Evaluated:**

| Approach | How It Works | Pros | Cons | Decision |
|----------|-------------|------|------|----------|
| **1. Raw SQL scripts in Docker `entrypoint`** | Mount `.sql` files into the SQL Server container's `/docker-entrypoint-initdb.d/` folder. SQL Server image auto-executes them on first start. | Standard Docker pattern. DBA-friendly. Scripts are reviewable. | SQL Server's official Docker image does NOT support `initdb.d` like PostgreSQL/MySQL do. Would require a custom entrypoint script (`entrypoint.sh` + `sqlcmd`). Extra complexity. | ⚠️ Rejected — SQL Server Docker doesn't natively support auto-init scripts |
| **2. Fully programmatic in C# (`DatabaseInitializer.cs`)** | The .NET API checks on startup if the DB/tables exist. If not, it creates them using ADO.NET `ExecuteNonQueryAsync` with inline SQL strings. | Zero extra files. Everything in C#. Clean for small schemas. | SQL is embedded as C# strings — hard to read, hard to review, can't be run independently by a DBA. Mixes infrastructure concerns. | ⚠️ Viable but not ideal for reviewability |
| **3. Hybrid: SQL script files + C# orchestrator** | SQL files live in `Infrastructure/Scripts/` (numbered: `001_CreateDatabase.sql`, `002_CreateUsersTable.sql`, etc.). `DatabaseInitializer.cs` reads them in order and executes via ADO.NET. Idempotent — checks before applying. | Best of both worlds: SQL is reviewable, C# handles execution. Scripts can be run manually too. Shows DB versioning awareness. | Slightly more file management. | ✅ **Chosen** |

**Final Decision (Human + AI consensus):** **Hybrid — SQL script files + C# `DatabaseInitializer` orchestrator**

**The complete flow from `docker compose up` to "app is ready":**

```
1. docker compose up --build -d
   │
   ├── [db container] SQL Server 2022 starts on port 1433
   │
   ├── [api container] .NET 8 API starts
   │   └── Program.cs → DbMigrator.MigrateAsync()
   │       ├── Step 1: Connect to master DB, CREATE DATABASE IF NOT EXISTS
   │       ├── Step 2: Execute sql/migrations/0001_init.sql  (Users + Tasks tables)
   │       ├── Step 3: Execute sql/migrations/0002_seed.sql  (demo user + sample tasks)
   │       └── Each script recorded in __Migrations (idempotent re-runs)
   │
   └── [web container] nginx:alpine serves React SPA
       └── /api/* proxied to api container (no CORS needed)
```

**Result:** http://localhost:5173 → login with `demo` / `Demo@12345` → full Kanban task UI.

---

## 9. Development Phases Roadmap

> Single source of truth for all project phases. `docs/12_PHASE_ROADMAP.md` has been deleted — this section is the canonical reference.

| # | Phase | Status | Date |
|---|-------|--------|------|
| 0 | Solution Scaffold | ✅ Done | 2026-04-09 |
| 1 | Domain Layer (entities, value objects, exceptions) | ✅ Done | 2026-04-09 |
| 2 | Application Layer (ports, services, DTOs) | ✅ Done | 2026-04-09 |
| 3 | Infrastructure (ADO.NET repos, DbMigrator, security) | ✅ Done | 2026-04-10 |
| 4 | API Layer (controllers, middleware, auth) | ✅ Done | 2026-04-10 |
| 5 | Frontend SPA — Initial (React + Vite + TypeScript) | ✅ Done | 2026-04-10 |
| 6 | Tests (xUnit + Moq; 92 tests passing) | ✅ Done | 2026-04-10 |
| 7 | Containerization (Docker Compose — db + api + web) | ✅ Done | 2026-04-11 |
| 8 | Frontend UI Professional Redesign (Tailwind, Kanban, Animations) | ✅ Done | 2026-04-12 |
| 9 | CI Pipeline (GitHub Actions — build + test on PRs) | ✅ Done | 2026-04-11 |
| 10 | Hardening (security headers, HSTS, prod compose) | ✅ Done (minimal) | 2026-04-11 |
| 11 | Documentation & Release (README, demo creds, release) | 🔄 In Progress | 2026-04-12 |
| 12 | E2E Tests (Playwright or Cypress) | ⬜ Pending | TBD |

### Phase Summaries

**Phase 0 — Solution Scaffold**
- `Ballastlane.sln` with 4 src projects (Domain, Application, Infrastructure, Api) + 4 test projects.
- Folders: `src/`, `tests/`, `sql/migrations/`, `web/`, `scripts/`, `docs/`, `memories/`.

**Phase 1 — Domain Layer**
- `TaskItem`: Id, Title, Description, Status (TaskStatus enum), DueDate, OwnerUserId + business validation.
- `User`: Id, Username, Email, PasswordHash, Salt, Role fields.
- `TaskStatus` value object: `Todo`, `InProgress`, `Done`.
- `DomainException`. Zero external dependencies — no framework references.

**Phase 2 — Application Layer**
- Output ports: `ITaskRepository`, `IUserRepository`, `IPasswordHasher`, `ITokenGenerator`.
- Input ports: `ITaskService`, `IAuthService`.
- Services: `TaskService`, `AuthService`.
- DTOs: `TaskDto`, `CreateTaskRequest`, `UpdateTaskRequest`, `LoginRequest`, `RegisterRequest`, `LoginResponse`.

**Phase 3 — Infrastructure Layer**
- `SqlServerTaskRepository` — ADO.NET; filters by `OwnerUserId`; Status stored as NVARCHAR.
- `SqlServerUserRepository` — ADO.NET; stores `PasswordHash` + `Salt`; reads `Role`.
- `DbMigrator` — reads `sql/migrations/*.sql` in alphabetical order; tracks in `__Migrations`; creates DB via master connection if missing.
- `BcryptPasswordHasher` (BCrypt.Net-Next), `JwtTokenGenerator` (JWT with UserId + Username + Role claims).

**Phase 4 — API Layer**
- `AuthController`: `POST /api/auth/register`, `POST /api/auth/login`.
- `TasksController`: full CRUD — all `[Authorize]`.
- `UsersController`: `GET /api/users/me`.
- `GlobalExceptionMiddleware`: domain exceptions → HTTP status codes.
- `CorrelationIdMiddleware`: `X-Correlation-Id` on all responses.

**Phase 5 — Frontend SPA (Initial)**
- React 19 + Vite + TypeScript. React Router, Axios clients, `AuthContext` (JWT storage + refresh).
- Basic pages: LoginPage, RegisterPage, TasksPage (table), TaskModal.

**Phase 6 — Tests**
- 92 unit/integration tests passing locally.
- Coverage: Domain entities, Application services (Moq), Infrastructure security, API controllers (WebApplicationFactory + fake services).

**Phase 7 — Containerization** _(2026-04-11)_
- `web/ballastlane-web/nginx.conf`: `try_files` SPA routing + `proxy_pass /api` → API container.
- `web/ballastlane-web/Dockerfile`: Node 22-alpine build + nginx:alpine serve.
- `docker-compose.yml`: 3 services (db SQL Server 2022, api, web) with healthchecks.
- `DbMigrator.EnsureDatabaseExistsAsync()`: creates DB via master connection before running scripts.
- Validated: `docker compose up --build -d` → all containers healthy.

**Phase 8 — Frontend UI Professional Redesign** _(2026-04-12)_
- Added: Tailwind CSS 3.4, react-hot-toast 2.4, @heroicons/react 2.0.
- `tailwind.config.cjs` + `postcss.config.cjs`; `index.css` stripped to Tailwind directives.
- **LoginPage**: split-panel (brand gradient left / form right), password toggle, loading spinner.
- **RegisterPage**: mirrors LoginPage design.
- **Header**: sticky top-0 z-40, avatar with initials, role badge, mobile hamburger + slide-down menu.
- **TasksPage**: Kanban 3-column (To Do / In Progress / Done), task count + progress bar, loading skeleton, overdue date in red.
- **TaskModal**: fade+slide-in animation (requestAnimationFrame), radio status selector (edit mode) / dropdown (create mode), saving spinner, Escape-to-close, click-backdrop-to-close.
- **User approved** at http://localhost:5173.

**Phase 9 — CI Pipeline** _(2026-04-11)_
- `.github/workflows/ci.yml`: `dotnet build` + `dotnet test` on push/PR.

**Phase 10 — Hardening (minimal)** _(2026-04-11)_
- Security headers: HSTS, X-Content-Type-Options, X-Frame-Options, CSP in `Program.cs`.
- `docker-compose.prod.yml`: non-root user, read-only filesystem.

**Phase 11 — Documentation & Release** _(in progress)_
- README updated with Run Locally (Docker) + manual setup + GenAI narrative.
- Demo: `demo` / `Demo@12345` (email: `demo@ballastlane.dev`, role: `Admin`).
- Endpoints: API `http://localhost:5000` | Frontend `http://localhost:5173`.

**Phase 12 — E2E Tests** _(pending)_
- Playwright (preferred) or Cypress for login → tasks CRUD flow.
- Pending user confirmation before implementation.

---

## EOD Update (2026-04-12)

- Completed: 1) Professional frontend UI redesign (Tailwind Kanban layout, animated modals, responsive header, toast notifications — **user approved**); 2) Fixed Docker build errors from component duplication and CSS syntax issues; 3) Consolidated development phases into checkpoint (deleted `docs/12_PHASE_ROADMAP.md`); 4) Updated all context docs to reflect actual DB schema.
- Files changed: `web/ballastlane-web/src/pages/LoginPage.tsx`, `RegisterPage.tsx`, `TasksPage.tsx`, `web/ballastlane-web/src/components/Header.tsx`, `TaskModal.tsx`, `web/ballastlane-web/src/index.css`, `web/ballastlane-web/tailwind.config.cjs`, `web/ballastlane-web/postcss.config.cjs`, `web/ballastlane-web/package.json`, `web/ballastlane-web/Dockerfile`, `docs/Arthur_Checkpoint_Exploration.md`, `.github/copilot-instructions.md` (deleted `docs/12_PHASE_ROADMAP.md`).
- Decisions: Frontend redesign approved by user; phases now tracked inside checkpoint (single source of truth); actual DB schema documented (Users has `Salt`+`Role`; Tasks uses `OwnerUserId`; `Status` is NVARCHAR).
- Blockers: None — full stack running and UI redesign approved.
- Next: 1) E2E tests with Playwright (pending user confirmation) 2) Create PR for frontend redesign 3) Final release tag + README polish.
   │       │   └── IF NOT EXISTS (SELECT * FROM sys.databases
   │       │       WHERE name = 'TaskManagerDb')
   │       │       CREATE DATABASE TaskManagerDb
   │       │
   │       ├── Step 3: Switch connection to TaskManagerDb
   │       │
   │       ├── Step 4: Execute 002_CreateUsersTable.sql
   │       │   └── IF NOT EXISTS (SELECT * FROM sys.tables
   │       │       WHERE name = 'Users') CREATE TABLE Users (...)
   │       │
   │       ├── Step 5: Execute 003_CreateTasksTable.sql
   │       │   └── IF NOT EXISTS ... CREATE TABLE Tasks (...)
   │       │
   │       └── Step 6: Execute 004_SeedData.sql
   │           └── IF NOT EXISTS (SELECT 1 FROM Users
   │               WHERE Username = 'demo')
   │               INSERT demo user + 5 sample tasks
   │
   └── [frontend container] starts React dev server on port 3000
       └── Proxies API calls to http://api:8080

✅ App ready: http://localhost:3000 | Login: demo / Demo1234!
```

**Why the hybrid approach is the best choice:**

1. **Reviewability:** Interviewers can open the `Scripts/` folder and read the SQL directly. They don't need to hunt through C# string literals to understand the schema. This is important — they will **review your database design**.

2. **Idempotent by design:** Every script uses `IF NOT EXISTS` guards. Running `docker-compose up` multiple times doesn't break anything. The `DatabaseInitializer` can be called on every startup safely.

3. **Retry logic for container ordering:** Docker Compose `depends_on` only waits for the container to **start**, not for SQL Server to be **ready to accept connections** (~10-15 seconds). The `DatabaseInitializer` implements a retry loop:
   ```csharp
   // Pseudocode for the retry pattern
   int maxRetries = 10;
   for (int i = 0; i < maxRetries; i++)
   {
       try {
           await connection.OpenAsync();
           break; // SQL Server is ready
       }
       catch (SqlException) {
           await Task.Delay(TimeSpan.FromSeconds(3 * (i + 1))); // Exponential backoff
       }
   }
   ```

4. **Numbered script convention:** `001_`, `002_`, `003_` ensures execution order is explicit and deterministic. This mirrors professional DB migration tools (FluentMigrator, DbUp). The interviewers will recognize the pattern.

5. **Manual execution path:** If a reviewer prefers not to use Docker, they can run the SQL scripts manually against their local SQL Server instance using SSMS, Azure Data Studio, or `sqlcmd`. The scripts stand alone — no C# dependency for the SQL itself.

6. **Seed data is a separate script:** `004_SeedData.sql` is isolated from schema creation. This is intentional — in production, you'd never seed demo data. Keeping it in a separate numbered file makes it obvious and removable.

**What `DatabaseInitializer.cs` looks like conceptually:**

```csharp
public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseInitializer> _logger;

    public async Task InitializeAsync()
    {
        // 1. Wait for SQL Server to be ready (retry with backoff)
        await WaitForSqlServerAsync();

        // 2. Read and execute scripts in order
        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");
        var scripts = Directory.GetFiles(scriptsPath, "*.sql")
                               .OrderBy(f => f);  // 001_, 002_, 003_...

        foreach (var script in scripts)
        {
            var sql = await File.ReadAllTextAsync(script);
            await ExecuteNonQueryAsync(sql);
            _logger.LogInformation("Executed: {Script}", Path.GetFileName(script));
        }
    }
}
```


**What we explicitly decided:**
 - ✅ SQL scripts are **embedded as content files** in the Infrastructure project (copied to output on build).
 - ✅ Scripts use `IF NOT EXISTS` — safe for repeated execution.
 - ✅ Seed script hashes the demo password with BCrypt **at build time** (pre-computed hash in SQL, not plaintext).
 - ✅ The `DatabaseInitializer` is called in `Program.cs` before the API starts listening.
 - ❌ No EF Migrations — they're not needed and would violate the "no EF" constraint.
 - ❌ No Docker `entrypoint` scripts — keeps SQL Server container vanilla (official image, no custom Dockerfile).

---

### Discussion 7: Copilot instructions and memories (Human-Initiated)


**Arturo's question:** _"How does Copilot consume files like `.github/copilot-instructions.md`? Do I need to ask the agent to read them every session or are they auto-loaded? Why place them in `.github/`?"_

**Summary of outcome:**
- `.github/copilot-instructions.md` is a repo-level instruction file intended to be consulted by Copilot in supported clients; placing it in `.github/` follows GitHub conventions and improves discoverability by tools and reviewers.
- `docs/Arthur_Checkpoint_Exploration.md` serves as the canonical checkpoint containing project decisions and short EOD summaries; keep it up to date for session resumption.
- Auto-loading behavior depends on the client/version: some Copilot/Agent clients load repo instructions at conversation start or when the agent detects relevance; if you edit these files mid-session, ask the assistant to re-read them explicitly.

**What to say in the interview:**
- "I maintain a short `.github/copilot-instructions.md` for repo-level policy and `docs/Arthur_Checkpoint_Exploration.md` as the canonical checkpoint. This lets the assistant read only high-signal files and avoids re-sending large context in prompts."

**Practical notes:**
- Keep these files concise and focused — Copilot prioritizes short, high-signal documents. Avoid storing secrets.
- If you ever doubt the assistant is using the latest file, prompt: `Please re-read .github/copilot-instructions.md and docs/Arthur_Checkpoint_Exploration.md`.


### Discussion 8: Decisions Log — What the AI Proposed vs. What the Human Decided

| Topic | AI's Initial Proposal | Arturo's Decision | Rationale |
|-------|----------------------|-------------------|-----------|
| Data access tool | ADO.NET (assumed) | ADO.NET (validated after evaluating 7 alternatives) | Not assumed — deliberated. Spirit of the constraint matters. |
| User story format | One-liner + bullet acceptance criteria | Full enterprise format with Gherkin + Business Rules | AI lacked context on real DevOps standards; human provided reference file |
| Architecture | 4 options presented equally | Hexagonal chosen after matrix comparison | Human validated the trade-offs and confirmed alignment with resume profile |
| Database choice | SQL Server (assumed) | SQL Server (confirmed after considering MongoDB, SQLite) | Relational semantics in exercise wording; .NET ecosystem alignment |
| Number of user stories | 1 combined story | 2 separate stories (Tasks + Auth) | Separation of concerns — auth is a distinct business capability |
| Gherkin scenarios | Not included initially | Added 13 scenarios across 2 user stories | Human insisted on testable acceptance criteria tied to TDD approach |
| Logging strategy | ❌ Not contemplated by AI | `ILogger<T>` with structured logging across all layers (except Domain) | Human detected the gap. AI had zero logging in the entire document. Proves human review value. |
| Docker / deployment | LocalDB assumed (Windows-only) | Docker Compose full stack + manual fallback | Human anticipated reviewer's environment. AI assumed local SQL Server. Cross-platform and zero-friction setup. |
| DB initialization | Hand-waved (`DatabaseInitializer.cs` + `seed.sql` with no details) | Hybrid: numbered SQL scripts in `Scripts/` folder + C# orchestrator with retry logic | Human asked "how exactly does it work?" — AI had no concrete flow. Led to full startup sequence design. |
| Docker build image (web) | `node:18-alpine` | `node:22-alpine` | Vite 8 requires Node ≥ 20.19; `node:18` caused `CustomEvent is not defined` at runtime. AI used an outdated base image. |
| SQL Server 2022 healthcheck | `mssql-tools` path + no `-C` flag | `mssql-tools18` path + `-C` flag + `retries:15` | SQL Server 2022 image ships sqlcmd under `mssql-tools18`. Without `-C` the TLS cert is rejected. Container stayed unhealthy forever; API never started. Human ran `docker compose ps` and traced the cascade failure. |
| DB migrator startup | `DbMigrator` opened `BallastlaneDb` directly | Added `EnsureDatabaseExistsAsync()` connecting via `master` first | Fresh container has no `BallastlaneDb` — `Login failed` on first run. AI never considered the cold-start scenario. Human caught it from the API container logs. |
| `__Migrations` table creation | `0001_init.sql` contained `CREATE TABLE __Migrations` | Removed duplicate DDL from SQL script | `DbMigrator.EnsureMigrationsTableAsync()` creates the table programmatically, then the SQL script tried to create it again. Human identified the conflict from the migration error log. |
| nginx config for web container | No `nginx.conf` at all | Created `nginx.conf` with SPA `try_files` + `/api` proxy | React SPA on page refresh returned nginx 404 (no fallback route). API calls from React to `/api` had no upstream target. Both bugs invisible until validating the browser in Docker. |
| CI badge URL in README | `github.com/arturo-martinez/Ballastlane` (wrong user/repo) | `github.com/EdgarArturoMartinez/BallastLane` | AI guessed a plausible URL that did not match the actual GitHub repo. Human verified via `git remote get-url origin`. |
| Security hardening scope | Proposed full OWASP/Snyk/integration test suite as "Phase 10" | Reverted to only essential headers (HSTS, X-Content-Type-Options, CSP, X-Frame-Options) + prod docker-compose | Human re-read the exercise requirements and correctly identified that hardening beyond the basics was out of scope. AI had over-engineered the phase. |

---

### Key Takeaway for Interviewers

This exploration phase demonstrates that the AI was used as a **collaborative thinking partner**, not as a code generator. At every stage:

1. **The human challenged AI assumptions** — e.g., "Is ADO.NET really the only option?"
2. **The human provided real-world context** — e.g., Azure DevOps user story format from production projects.
3. **The human made the final architectural decision** — the AI presented 4 options; the human evaluated and chose.
4. **The human elevated quality standards** — pushing from generic acceptance criteria to enterprise Gherkin specifications.
5. **The human caught AI blind spots** — logging was completely absent from the AI's entire document. The human identified the gap, and together we defined a production-grade strategy that respects Clean Architecture boundaries.
6. **The human anticipated the reviewer's experience** — the AI assumed LocalDB (Windows-only). The human asked: "What if they don't have SQL Server?" This led to Docker Compose as the primary delivery mechanism — showing operational maturity beyond just writing code.
7. **The human demanded implementation specifics** — the AI hand-waved database initialization ("DatabaseInitializer runs on startup"). The human asked: "How exactly? Scripts? Code? Both?" This forced a concrete design: numbered SQL files + C# orchestrator with retry logic. The AI had no answer until the human pushed.
8. **The human validated the running system end-to-end** — AI-generated Docker configuration had 5 silent runtime bugs (wrong Node version, wrong sqlcmd path, missing DB creation, duplicate DDL, missing nginx config). None were visible in code review; all only surfaced when running `docker compose up --build`. The human drove the validation loop: run → read logs → isolate → fix → re-run.
9. **The human kept the AI in scope** — When AI proposed a full OWASP/Snyk/integration-test hardening phase, the human re-read the exercise requirements and correctly stopped the over-engineering. The correct response was: add minimal security headers and a `docker-compose.prod.yml` — nothing more.
10. **The human caught a silent documentation bug** — The AI generated a CI badge URL with a wrong GitHub username (`arturo-martinez` instead of `EdgarArturoMartinez`). The badge would have shown a broken image in the README during the presentation. Human verified the URL against `git remote get-url origin`.

This is how GenAI tools should be used in professional software development: augmenting domain expertise, not replacing critical thinking.

---

### Discussion 9: Implementation Phase — AI-Assisted Development with Human-Driven Validation

> The previous discussions (1–8) covered the exploration and architecture phase. This discussion documents the **implementation phase** — how AI generated code, and how the human systematically validated, corrected, and steered the output.

**Tool used:** GitHub Copilot with Agent Mode (Claude Sonnet 4.6)

**Phase covered:** Phases 0–10 (Solution scaffold → Domain → Application → Infrastructure → API → Frontend → Tests → Docker → CI → Hardening)

#### What AI did well (accepted without major changes)

| Component | AI quality | Notes |
|-----------|-----------|-------|
| Domain entities (`TaskItem`, `User`, `Email`) | ✅ High | Clean value object pattern, immutability, `DomainException` — matched intended design |
| Application services (`TaskService`, `AuthService`) | ✅ High | Ports correctly injected; no infrastructure leakage into application layer |
| ADO.NET repositories (`AdoTaskRepository`, `AdoUserRepository`) | ✅ High | Parameterized queries from first draft; correct `using` disposal; OWASP-compliant |
| xUnit test structure (all 4 layers) | ✅ High | 92 tests; `WebApplicationFactory` integration wiring; Moq usage correct |
| JWT + PBKDF2 password hasher | ✅ High | Correct `SymmetricSecurityKey`, `ClockSkew=Zero`, PBKDF2 with salt |
| Serilog integration + `CorrelationIdMiddleware` | ✅ High | Structured logging; clean middleware chain; domain layer stays log-free |
| React SPA (login, register, tasks CRUD) | ✅ High | Clean component structure; `AuthContext` with JWT; responsive layout |

#### What AI got wrong (required human correction)

| Issue | What AI produced | What was wrong | What human did |
|-------|-----------------|----------------|----------------|
| Web container base image | `node:18-alpine` | Vite 8 requires Node ≥ 20.19; `CustomEvent` not defined at runtime | Changed to `node:22-alpine` |
| SQL Server healthcheck | `mssql-tools` path, no `-C` | 2022 image uses `mssql-tools18`; TLS cert rejected without `-C`; containers never reached `healthy` | Fixed path + flag + raised `retries` to 15 |
| `DbMigrator` cold start | Connected directly to `BallastlaneDb` | DB doesn't exist on fresh container — `Login failed` | Added `EnsureDatabaseExistsAsync()` via `master` connection |
| `0001_init.sql` | Contained `CREATE TABLE __Migrations` | Duplicate — migrator creates it programmatically first | Removed the DDL from the SQL script |
| `nginx.conf` | Not generated | SPA page refresh → 404; `/api` proxy missing | Created `nginx.conf` from scratch: `try_files` + `proxy_pass` |
| CI badge URL | `github.com/arturo-martinez/Ballastlane` | Wrong username and casing — badge would show broken in README | Corrected to `github.com/EdgarArturoMartinez/BallastLane` |
| Phase 10 scope | Full OWASP/Snyk/integration tests | Out of scope for the exercise | Human re-read requirements and scoped down to essential security headers only |

#### The validation workflow used

```
AI generates code → Human reviews for architectural fit
         ↓
dotnet build (0 errors, 0 warnings)
         ↓
dotnet test (92 tests green)
         ↓
docker compose up --build -d
         ↓
docker compose ps → check service health
         ↓
docker compose logs api → read startup sequence
         ↓
HTTP smoke tests:
  GET /health → 200
  POST /api/auth/login → JWT token
  GET /api/tasks (with token) → 3 seeded tasks
  Browser: http://localhost:5173 → SPA loads, login works
         ↓
If any step fails → isolate root cause → fix → restart loop
```

**Total Docker bugs found through this loop:** 5  
**All 5 were invisible in code review — only surfaced at runtime.**

#### Prompt engineering example — iterative refinement

**First prompt (too generic):**
> "Create a .NET Clean Architecture task management API with SQL Server."

**What was missing from the output:**
- No migration runner (assumed EF Migrations)
- No PBKDF2 (used BCrypt which adds a dependency)
- No `ITaskOwnershipValidator` — tasks were not user-scoped properly
- No `EnsureDatabaseExistsAsync` — migrator assumed DB existed

**Refined prompt (what produced the final implementation):**
> "You are a senior .NET architect. Scaffold a production-quality ASP.NET Core 8 Web API following Hexagonal Architecture. Constraints: ADO.NET only (no EF, Dapper, MediatR), SQL Server with parameterized queries, xUnit + Moq TDD (failing test first), JWT + PBKDF2 (no BCrypt dependency), numbered SQL migration runner that first creates the DB via a `master` connection, task ownership enforcement (user can only read/modify their own tasks). Projects: Domain / Application / Infrastructure / API / 4 test projects."

**Lesson:** The constraint specification in the prompt is the most valuable investment. Vague prompts require multiple correction rounds. Specific constraints (with rationale) produce output that is architecturally correct from the first draft.

---

### Discussion 10: Panel Q&A Preparation — GenAI Fluency Questions

> This discussion prepares specific answers to the most likely panel questions about GenAI tool usage.

**Q: "What GenAI tool did you use and how?"**
> I used GitHub Copilot in Agent Mode (Claude Sonnet 4.6) throughout the entire project — from architecture exploration to code generation to Docker debugging. I used it as a pair programmer: I drove the requirements and constraints; Copilot generated implementation drafts that I validated, challenged, and corrected. Every architectural decision was made after explicit evaluation of alternatives, not by accepting the AI's first suggestion.

**Q: "Can you show me the prompt you used to generate the API?"**
> _(Point to the prompt in Discussion 9 or the README GenAI section)_
> The key discipline was constraint-first prompting: instead of asking for a generic API, I specified every constraint upfront — ADO.NET only, no BCrypt, numbered migration runner, task ownership enforcement. This reduced correction rounds from many to almost none for the core logic.

**Q: "How did you validate the AI's output?"**
> I used a 4-gate validation loop: (1) `dotnet build` — 0 errors/warnings; (2) `dotnet test` — all 92 tests green; (3) `docker compose up --build` + log inspection; (4) HTTP smoke tests — health, login, tasks. The Docker validation step caught 5 bugs that were invisible in code review: wrong Node version, wrong sqlcmd path, missing DB creation, duplicate DDL, missing nginx config. None would have been caught by just reading the code.

**Q: "Did you have to correct or improve the AI output? Give an example."**
> Yes — multiple times. Most impactful: the AI generated a `DbMigrator` that connected directly to `BallastlaneDb` on startup. This failed silently on a fresh container because the database hadn't been created yet. I diagnosed it from the API container logs (`Login failed for user 'sa'`), traced it to the cold-start scenario the AI had never considered, and added an `EnsureDatabaseExistsAsync()` method that first connects via `master`, creates the DB if missing, then proceeds with migrations. Small fix, fundamental reliability improvement.

**Q: "How did you handle edge cases?"**
> Three specific examples: (1) **Task ownership** — added `ITaskOwnershipValidator` to ensure a user can't read or modify another user's tasks — the AI's initial draft had no ownership check; (2) **JWT `ClockSkew=Zero`** — set explicitly so tokens expire exactly when expected, not 5 minutes later (the ASP.NET default); (3) **Migration idempotency** — every SQL script uses `IF NOT EXISTS` guards, so `docker compose up` can be run multiple times without errors.

**Q: "What would you do differently if you had more time?"**
> I'd add a PostgreSQL adapter behind the same `ITaskRepository` port to demonstrate that the Hexagonal Architecture truly allows swapping the database without touching domain or application code. I'd also add integration tests using Testcontainers (SQL Server in a Docker container during CI). Both are architectural capabilities that exist now — just not implemented because they weren't in scope.

**Q: "Is this project production-ready?"**
> It's production-structured, not production-deployed. The architecture separates concerns correctly; secrets come from environment variables; SQL uses parameterized queries; passwords use PBKDF2 with salt; JWT has zero clock skew; CI runs on every PR. What's missing for true production: TLS termination, secrets management (Azure Key Vault / AWS Secrets Manager), rate limiting, distributed tracing, and horizontal scaling configuration. I intentionally kept those out of scope to match the exercise requirements — but I can design any of them because the port/adapter boundaries are already in place.

---

*Phases 0–10 implemented and validated. All 92 tests passing. Full Docker stack operational. CI green. Branch `building-solution-webapi` merged to `main` via `dev` → `qa` → `main` PRs.*
