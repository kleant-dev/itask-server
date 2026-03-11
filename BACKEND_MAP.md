# Slender Backend – Architecture Map

This document describes how the backend is wired: layers, request flow, and where each piece is registered so you can continue development without technical debt.

---

## 1. Layer Overview

| Layer        | Project        | Responsibility |
|-------------|----------------|----------------|
| **API**     | slender-server.API | HTTP endpoints, auth, validation, OpenAPI. No business logic. |
| **Application** | slender-server.Application | Contracts (service interfaces, DTOs, validators), shared models (Result, pagination, sorting), and app-level helpers (PaginationService, SortingService, DataShapingService). No persistence. |
| **Domain**  | slender-server.Domain | Entities, enums, repository interfaces, `IUnitOfWork`. No dependencies on other layers. |
| **Infrastructure** | slender-server.Infra | Implementations: all repositories, all services, DbContext, auth (AuthService, UserContext, TokenProvider). References Application + Domain. |

**Reference direction:** API → Application + Infra → Application → Domain. Domain does not reference anyone.

---

## 2. Request Flow (uniform pattern)

Every write request follows this path:

```
HTTP Request
  → Controller (API)
      → Validator (FluentValidation, optional)
      → IUserContext.GetRequiredUserIdAsync() for auth
      → I*Service.*Async(...)
  → Service (Infra)
      → Permission / membership checks (e.g. workspace member)
      → I*Repository.*Async(...)  and/or  IRepository<T>.*Async(...)
      → IUnitOfWork.SaveChangesAsync()   // single commit for the request
  → Repository (Infra)
      → ApplicationDbContext (DbSet<T>)
  → ApplicationDbContext implements IUnitOfWork
      → SaveChangesAsync() persists all staged changes
  → Response: Result<T> or Result → Controller → HTTP
```

**Rules:**

- Controllers never touch repositories or DbContext.
- Services orchestrate repos and call `IUnitOfWork.SaveChangesAsync()` once per operation (or per transaction).
- Repositories only stage changes (Add/Update/Delete); they do **not** call `SaveChangesAsync`.
- All update/delete staging uses the same contract: `UpdateAsync(entity, ct)` and `DeleteAsync(entity, ct)` from `IRepository<T>`.

---

## 3. Unit of Work

- **Interface:** `Domain.Interfaces.IUnitOfWork` — only `Task<int> SaveChangesAsync(CancellationToken ct)`.
- **Implementation:** `Infra.Database.ApplicationDbContext` implements `IUnitOfWork`.
- **Registration:** `IUnitOfWork` is resolved as the same instance as `ApplicationDbContext` (per request):  
  `builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());`

All repositories use the same `ApplicationDbContext` instance in a request, so one `SaveChangesAsync()` commits every change made by the services in that request.

---

## 4. Repositories

**Base contract (Domain):** `IRepository<T>`

- `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `GetPagedAsync`.

**Concrete repository interfaces** (all in `Domain.Interfaces`) extend or use this where it fits:

| Interface | Implementation (Infra) | Notes |
|-----------|-------------------------|------|
| `IUserRepository` | UserRepository | User + identity lookup |
| `IWorkspaceRepository` | WorkspaceRepository | Extends `IRepository<Workspace>`, + slug, members, role |
| `IWorkspaceMemberRepository` | WorkspaceMemberRepository | Extends `IRepository<WorkspaceMember>`, + member/role helpers |
| `IWorkspaceInviteRepository` | WorkspaceInviteRepository | Extends `IRepository<WorkspaceInvite>`, + token, pending invites |
| `IProjectRepository` | ProjectRepository | Extends `IRepository<Project>`, + by workspace, members, task counts |
| `IProjectMemberRepository` | ProjectMemberRepository | Project membership |
| `ITaskRepository` | TaskRepository | Extends `IRepository<Task>`, + by project/assignee, subtasks, counts |
| `ILabelRepository` | LabelRepository | Extends `IRepository<Label>`, + by workspace, name, usage |

**Generic registration:** `IRepository<>` → `Repository<>` for any entity that only needs the base CRUD + paging (e.g. Notification, ActivityLog, CalendarEvent when used via `IRepository<T>`).

**Update/delete uniformity:** All repositories use the base `UpdateAsync` and `DeleteAsync` from `IRepository<T>`; there are no custom `void Update(...)` or `void Remove(...)` methods. Services always call `await repo.UpdateAsync(entity, ct)` or `await repo.DeleteAsync(entity, ct)`, then `await unitOfWork.SaveChangesAsync(ct)`.

---

## 5. Services

**Interfaces** live in `Application.Interfaces.Services`. **Implementations** live in `Infra.Services`.

| Service interface | Implementation | Main dependencies |
|-------------------|----------------|--------------------|
| IAuthService | AuthService (Infra.Auth) | Identity, UserManager, ITokenProvider, IUserRepository |
| IUserContext | UserContext | IHttpContextAccessor, IUserRepository |
| IUserService | UserService | IUserRepository |
| IWorkspaceService | WorkspaceService | IWorkspaceRepository, IWorkspaceMemberRepository, IWorkspaceInviteRepository, IUserRepository, IUnitOfWork, IPaginationService, ISortingService |
| IProjectService | ProjectService | IProjectRepository, IWorkspaceRepository, IWorkspaceMemberRepository, IUnitOfWork, IPaginationService, ISortingService |
| ITaskService | TaskService | ITaskRepository, IWorkspaceMemberRepository, IUnitOfWork, ISortingService, IPaginationService, IUserContext |
| ILabelService | LabelService | ILabelRepository, IWorkspaceMemberRepository, IUnitOfWork |
| INotificationService | NotificationService | IRepository&lt;Notification&gt;, IUnitOfWork |
| IActivityLogService | ActivityLogService | IRepository&lt;ActivityLog&gt;, IWorkspaceMemberRepository, IUnitOfWork |

**Application-only (no persistence):**  
IPaginationService → PaginationService, IDataShapingService → DataShapingService, ISortingService → SortingService, ILinkService → LinkService (API).

---

## 6. Controllers and Routes

All under `slender-server.API.Controllers`, `[Authorize]` unless noted.

| Controller | Base route | Main actions |
|------------|-----------|--------------|
| AuthController | `api/v1/auth` | Login, register, refresh (auth flows) |
| UsersController | `api/v1/users` | GET/PATCH/DELETE `me` |
| WorkspacesController | `api/v1/workspaces` | CRUD workspaces, members, invites (e.g. `GET /`, `GET /{id}`, `POST /{id}/members`, `POST /invites/accept`) |
| ProjectsController | `api/v1/` | `POST workspaces/{workspaceId}/projects`, `GET workspaces/{workspaceId}/projects`, `GET/PATCH projects/{projectId}`, `POST projects/{projectId}/archive` |
| TasksController | `api/v1/` | `GET workspaces/{workspaceId}/tasks`, `GET/POST/PATCH/DELETE tasks/{taskId}` |

Labels, Notifications, and ActivityLog have services registered but **no controllers yet**; add controllers that call the existing services and follow the same pattern (IUserContext, validators, Result handling).

---

## 7. Dependency Injection (where things are registered)

**API (Program.cs):**  
`AddApiServices()` → `AddApplicationServices()` → `AddInfrastructureServices()` → `AddAuthenticationServices()` → `AddDatabase()` → `AddErrorHandling()` → `AddCorsPolicy()`.

- **AddApiServices:** Controllers, API versioning, OpenAPI (with document transformer for title/version/description), response caching, ILinkService.
- **AddApplicationServices:** FluentValidation from Application assembly, IPaginationService, IDataShapingService, ISortingService, and all `SortMappingDefinition<TDto, TEntity>` for sorting.
- **AddAuthenticationServices:** Identity, JWT + Google auth, IAuthService.
- **AddDatabase:** ApplicationDbContext, ApplicationIdentityDbContext (with Npgsql + snake_case).
- **AddInfrastructureServices:**
  - `IRepository<>` → `Repository<>`
  - IUserContext, IAuthService, ITokenProvider
  - All *Service interfaces → *Service implementations (Workspace, User, Task, Project, Label, Notification, ActivityLog)
  - All *Repository interfaces → *Repository implementations
  - IUnitOfWork → ApplicationDbContext

No duplicate registrations: IUserContext, IAuthService, and ISortingService are registered only once (in Infra for context/auth, in Application for sorting).

---

## 8. OpenAPI / Frontend schema

- OpenAPI is configured in **AddApiServices** with a document transformer that sets **Title** (“Slender API”), **Version** (“1.0”), and **Description** for frontend use.
- In **Development**, `app.MapOpenApi()` is called in Program.cs so the OpenAPI JSON is exposed (e.g. at `/openapi/v1.json` or the default endpoint depending on your host).
- Use this endpoint to generate frontend types and client code (e.g. OpenAPI Generator, NSwag, or similar).

---

## 9. Conventions to keep (avoiding technical debt)

1. **New resource:** Add entity in Domain; optional dedicated `I*Repository` in Domain (or use `IRepository<T>`); add DTOs and `I*Service` in Application; implement repository and service in Infra; register in AddInfrastructureServices; add controller in API that uses the service and returns `Result<T>` / `Result`.
2. **Persistence:** Services call repositories then **one** `await unitOfWork.SaveChangesAsync(ct)` per logical operation. Repositories never call SaveChanges.
3. **Updates/deletes:** Always use `await repo.UpdateAsync(entity, ct)` or `await repo.DeleteAsync(entity, ct)` from `IRepository<T>` (no custom void Update/Remove).
4. **Auth:** Controllers use `IUserContext.GetRequiredUserIdAsync(ct)` and pass `userId` into services; services enforce workspace/project membership as needed.
5. **Validation:** Use FluentValidation for request DTOs; call `ValidateAndThrowAsync` in the controller before calling the service.
6. **Responses:** Prefer `Result<T>` / `Result` from Application.Models.Common and map to 200/201/400/404 in the controller.

Following this map keeps the backend consistent and easy to extend.
