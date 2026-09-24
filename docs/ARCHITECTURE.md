# InternTrack Architecture

This document describes the high-level architecture of the InternTrack backend and explains the responsibilities of each project layer.

InternTrack uses a layered architecture to separate API concerns, business logic, shared models, data access, infrastructure-specific implementations, and automated tests.

---

## Architecture Overview

The backend solution is organized into the following projects:

- `InternTrack.Api`
- `InternTrack.Business`
- `InternTrack.Core`
- `InternTrack.DataAccess`
- `InternTrack.Infrastructure`
- `InternTrack.Tests`

The general request flow is:

    Client
      |
      v
    InternTrack.Api
      |
      v
    InternTrack.Business
      |
      v
    InternTrack.DataAccess
      |
      v
    Database

Shared models, DTOs, constants, and helpers are provided by:

    InternTrack.Core

Infrastructure-specific implementations are provided by:

    InternTrack.Infrastructure

Automated tests are located in:

    InternTrack.Tests

---

## InternTrack.Api

`InternTrack.Api` is the entry point of the backend application.

It is responsible for receiving HTTP requests, validating authentication and authorization requirements, passing operations to the Business layer, and returning HTTP responses.

Main responsibilities include:

- Controllers
- Middleware
- API helpers
- API response conventions
- Authentication configuration
- Authorization configuration
- Dependency injection configuration
- CORS configuration
- Rate limiting
- Swagger configuration
- Application startup configuration
- HTTP response handling

Controllers should remain relatively thin.

They are mainly responsible for:

- Reading route parameters
- Reading request DTOs
- Reading authenticated user information
- Calling the appropriate service
- Mapping service results to HTTP responses

Business rules should not be implemented directly inside controllers.

Repeated Swagger response metadata is centralized through `InternTrackApiConventions`.

This reduces repeated `ProducesResponseType` declarations while preserving response documentation.

Runtime conversion of business results into HTTP responses is handled through `ServiceResultMapper`.

---

## InternTrack.Business

`InternTrack.Business` contains the main application and business logic.

This layer is responsible for deciding whether an operation is allowed and how it should be processed.

Main responsibilities include:

- Service implementations
- Service interfaces
- Business rules
- Authorization-related business checks
- Validation logic
- Password hashing
- Service result structures
- Logging abstraction
- Coordination between repositories

Examples of business rules handled in this layer include:

- Preventing unauthorized task operations
- Restricting task status transitions
- Preventing overdue tasks from being started or completed
- Preventing inactive Intern accounts from authenticating
- Validating related Department records
- Validating related Intern records
- Preventing duplicate data
- Applying soft delete rules
- Applying reactivation rules

The Business layer communicates with repositories instead of accessing the database directly.

Business service interfaces inherit from `IScopedService`.

This marker interface allows application services to be discovered and registered automatically with scoped lifetime.

---

## InternTrack.Core

`InternTrack.Core` contains shared application structures that can be used by other layers.

Main responsibilities include:

- Domain models
- DTOs
- Shared constants
- Role constants
- Task status constants
- Task priority constants
- Shared helpers

Examples include:

- User models
- Intern models
- Department models
- Task models
- Authentication DTOs
- Intern DTOs
- Department DTOs
- Task DTOs
- `RoleHelper`

Role interpretation is centralized through `RoleHelper`.

Instead of scattering role string comparisons throughout services, role interpretation is performed in one project-owned helper.

This keeps role-related intent clearer and reduces repeated comparison logic.

This layer does not contain database access or HTTP-specific behavior.

Its purpose is to provide shared structures that represent the application's data and common definitions.

---

## InternTrack.DataAccess

`InternTrack.DataAccess` is responsible for persistence and database communication.

The project uses Entity Framework Core with SQLite.

Main responsibilities include:

- Entity Framework Core DbContext
- Repository interfaces
- Repository implementations
- Query definitions
- Database migrations
- Entity persistence

The Business layer communicates with the DataAccess layer through repository interfaces.

Repository interfaces inherit from `IScopedRepository`.

This marker interface allows repository implementations to be discovered and registered automatically with scoped lifetime.

Application startup does not populate business data.

There is no generic database seeder or predefined Department list.

Departments are created explicitly through the application flow using:

    POST /api/departments

The corresponding Business operation is handled through `DepartmentService.AddAsync`.

A fresh database therefore starts without predefined Department records.

---

## Query Organization

Database query responsibilities are organized separately from business rules.

For Department name validation:

    DepartmentService
        |
        v
    DepartmentRepository
        |
        v
    DepartmentQueries
        |
        v
    Entity Framework Core
        |
        v
    SQLite

`DepartmentQueries` defines the reusable query logic.

`DepartmentRepository` executes the query.

`DepartmentService` interprets the result and applies the business rule.

The previous boolean-oriented:

    NameExistsAsync

approach was replaced with:

    GetByNameIncludingInactiveAsync

This operation returns the matching Department or `null`.

Duplicate-name business rules therefore remain inside the Business layer instead of being expressed as repository-specific business decisions.

Department name matching:

- Includes inactive Departments
- Ignores leading and trailing whitespace
- Normalizes case
- Supports excluding the current Department during update

Entity Framework Core LINQ is currently used for this query.

Raw SQL is not required for the current implementation.

---

## InternTrack.Infrastructure

`InternTrack.Infrastructure` contains implementations that depend on technical or infrastructure-specific concerns.

Current examples include:

- JWT access token generation
- Refresh token generation
- Application logging implementation

Infrastructure-specific interfaces can be defined outside this layer while their concrete implementations remain here.

Examples include:

- `ITokenService` → JWT token implementation
- `IAppLogger` → `ConsoleAppLogger`

Separating these implementations from the Business layer helps keep business logic independent from technical implementation details.

---

## Dependency Injection

InternTrack uses automatic dependency registration for Business services and DataAccess repositories.

The main marker interfaces are:

- `IScopedService`
- `IScopedRepository`

The API scans the related assemblies and finds concrete implementations for interfaces that inherit from these marker interfaces.

Matching implementations are registered automatically with scoped lifetime.

This removes the need to manually register every service and repository with individual statements such as:

    AddScoped<IService, Service>()

Infrastructure-specific services that are outside this scanning structure remain explicitly registered where appropriate.

Examples include:

- `ITokenService`
- `IAppLogger`

The automatic registration structure is implemented through the API dependency injection extension.

Startup validation also helps detect missing or ambiguous implementations.

---

## InternTrack.Tests

`InternTrack.Tests` contains automated tests for the backend.

The project uses:

- xUnit
- Moq
- SQLite-backed persistence tests where appropriate

Current tested areas include:

- Authentication
- Login rules
- Refresh token behavior
- Password changes
- Profile updates
- Avatar updates
- Departments
- Interns
- Tasks
- Task status transitions
- Soft delete
- Reactivation operations
- Dashboard statistics
- Role-based authorization rules
- Centralized role interpretation
- Dependency injection registration
- Custom logging
- Exception handling
- Department query behavior
- Persistence behavior
- Validation scenarios
- Edge cases

The current test suite contains:

    243 passing tests
    0 failing tests

Repositories and other dependencies are mocked where appropriate so that Business rules can be tested independently.

SQLite-backed tests are also used where real persistence and query behavior need to be verified.

---

## Dependency Flow

The main dependency flow can be represented as:

    InternTrack.Api
        |
        v
    InternTrack.Business
        |
        v
    InternTrack.DataAccess

`InternTrack.Core` provides shared models, DTOs, constants, and helpers used by multiple layers.

`InternTrack.Infrastructure` provides infrastructure-specific implementations used by the application.

`InternTrack.Tests` references the required application projects for automated testing.

---

## Request Flow Example

A typical request follows this sequence:

    1. Client sends HTTP request

    2. Controller receives the request

    3. Authentication and authorization are checked

    4. Controller reads request data and current user information

    5. Controller calls a Business service

    6. Business service validates business rules

    7. Business service calls repository methods when required

    8. Repository executes the required Entity Framework Core query

    9. Entity Framework Core communicates with SQLite

    10. Result returns to the Business service

    11. Business service creates a ServiceResult

    12. ServiceResultMapper converts the result into an HTTP response

    13. Response is returned to the client

Swagger/OpenAPI response metadata for controller actions is described through `InternTrackApiConventions`.

---

## Controller and Service Separation

Controllers and services have different responsibilities.

### Controllers

Controllers are responsible for API-related concerns.

Examples:

- Receiving HTTP requests
- Reading authenticated user information
- Reading route values
- Reading DTOs
- Applying authorization attributes
- Calling Business services
- Returning HTTP responses

Repeated response metadata is kept outside individual controller actions through API conventions.

### Services

Services are responsible for application behavior.

Examples:

- Applying business rules
- Performing validation
- Checking permissions
- Coordinating repository operations
- Returning `ServiceResult` values
- Recording important application events through `IAppLogger`

This separation keeps controllers simpler and makes Business rules easier to test.

---

## Repository Pattern

InternTrack uses the Repository pattern to separate business logic from database access.

Instead of accessing Entity Framework Core directly from services, services use repository interfaces.

General structure:

    Controller
        |
        v
    Service
        |
        v
    Repository Interface
        |
        v
    Repository Implementation
        |
        v
    Query Definition / Entity Framework Core
        |
        v
    SQLite

Benefits of this structure include:

- Better separation of concerns
- Easier unit testing
- Reduced database coupling
- Cleaner service code
- Explicit query responsibility
- Easier replacement or modification of persistence logic

Repositories are responsible for persistence and retrieval.

Business decisions remain in services.

---

## ServiceResult Pattern

The Business layer uses `ServiceResult` structures to represent operation results.

This allows services to return information such as:

- Success
- Validation errors
- Not found results
- Forbidden operations
- Conflict results
- Result data

Controllers do not need to repeat mappings for every result type.

`ServiceResultMapper` centrally maps Business results to HTTP responses.

Examples:

    Success          -> 200 OK
    ValidationError  -> 400 Bad Request
    NotFound         -> 404 Not Found
    Forbidden        -> 403 Forbidden
    Conflict         -> 409 Conflict

Create operations can map successful results to:

    201 Created

Deactivation operations can map successful results to:

    204 No Content

This keeps HTTP response mapping separate from Business logic.

---

## API Response Conventions

Controller response metadata is centralized through:

    InternTrackApiConventions

The convention structure defines the documented response status codes for groups of controller operations.

Examples include:

- Authentication responses
- Department operations
- Intern operations
- Task operations
- Dashboard responses
- Create responses
- Update responses
- Deactivation responses
- Reactivation responses

Assembly-level `ApiConventionType` registration in `ApiConventionRegistration.cs` supplies response metadata by action name. Five actions use explicit `ApiConventionMethod` overrides where their response sets differ from a matching prefix convention: Task creation/update, Department deactivation, and Auth avatar/profile updates.

This reduces repeated response declarations while keeping Swagger/OpenAPI documentation available.

The responsibilities remain separate:

    InternTrackApiConventions
        -> Swagger / response metadata

    ServiceResultMapper
        -> Runtime HTTP result mapping

    Controller
        -> Request handling and service invocation

---

## DTO Usage

DTOs are used to transfer data between API requests, services, and responses.

Examples include:

- Registration data
- Login data
- Intern creation data
- Intern update data
- Department creation data
- Task creation data
- Task update data
- Profile update data
- Password change data

DTOs help prevent API requests from directly modifying entity models.

They also provide a clear contract for incoming and outgoing data.

---

## Authentication Architecture

The authentication flow uses:

- JWT access tokens
- Refresh tokens
- HttpOnly cookies
- Refresh token hashing
- Refresh token rotation
- Refresh token revocation

General login flow:

    User submits login request
        |
        v
    AuthController
        |
        v
    AuthService
        |
        v
    User validation
        |
        v
    Role and Intern profile validation
        |
        v
    Access token generation
        |
        v
    Refresh token generation
        |
        v
    Refresh token persistence
        |
        v
    HttpOnly cookies
        |
        v
    Authenticated session

The access token is short-lived.

The refresh token is used to renew the session without requiring the user to log in again.

---

## Role Handling

Role interpretation is centralized through `RoleHelper`.

Role-related `StringComparison` operations are not scattered through Business services.

The helper provides clearly named role checks such as:

- Admin claim interpretation
- HR claim interpretation
- Intern claim interpretation
- Authentication-specific Intern role interpretation

Authentication distinguishes between:

- A user who does not require an Intern profile
- An Intern user whose Intern profile is missing
- An Intern user whose Intern profile exists but is inactive

Non-Intern users do not require an Intern profile.

Intern users with missing or inactive Intern profiles are rejected according to the existing authentication rules.

This structure replaces the previous combined `IsInactiveIntern` helper.

---

## Refresh Token Flow

The refresh process follows this general structure:

    Client sends refresh request
        |
        v
    Refresh token read from HttpOnly cookie
        |
        v
    Stored token is validated
        |
        v
    User and account status are validated
        |
        v
    Existing refresh token is revoked
        |
        v
    New access token is generated
        |
        v
    New refresh token is generated
        |
        v
    New refresh token is stored
        |
        v
    New cookies are returned

This process provides refresh token rotation.

If the refresh operation fails, authentication cookies can be cleared.

---

## Authorization Architecture

Authorization is enforced at more than one level.

### API-Level Authorization

Controllers use authorization attributes such as:

- `[Authorize]`
- `[Authorize(Roles = Roles.Admin)]`
- `[Authorize(Roles = Roles.AdminOrHR)]`
- `[AllowAnonymous]`

These attributes control access to endpoints.

### Business-Level Authorization

Additional checks are performed inside services.

Examples include:

- Ensuring an Intern only accesses permitted Intern data
- Ensuring an Intern only modifies permitted tasks
- Applying task status transition rules
- Restricting overdue task operations
- Applying role-specific deactivation and reactivation rules

This means frontend restrictions are not relied on as the main security boundary.

---

## Soft Delete Architecture

Departments, Interns, and Tasks support soft delete.

Instead of permanently deleting records, entities are marked as inactive.

Service and repository methods name this action explicitly:

- `DeactivateDepartmentAsync`
- `DeactivateInternAsync`
- `DeactivateTaskAsync`

The controller actions use corresponding names without the `Async` suffix.

Existing HTTP DELETE routes remain unchanged.

General flow:

    Delete request
        |
        v
    Business rule validation
        |
        v
    IsActive = false
        |
        v
    Record remains in database

Entity Framework Core query filters are used to exclude inactive records from normal queries.

Admin-specific operations can retrieve inactive records when required.

---

## Reactivation Architecture

Reactivation operations reverse soft delete when related business rules are satisfied.

The existing `/restore` routes remain unchanged for API compatibility.

| Entity | Service and Repository Method | Controller Action |
| --- | --- | --- |
| Department | `ReactivateDepartmentAsync` | `ReactivateDepartment` |
| Intern | `ReactivateInternAsync` | `ReactivateIntern` |
| Task | `ReactivateTaskAsync` | `ReactivateTask` |

Examples include:

- An Intern cannot be reactivated if the related Department is inactive.
- A Task cannot be reactivated if the assigned Intern is inactive.

General flow:

    Restore request
        |
        v
    Record lookup
        |
        v
    Related entity validation
        |
        v
    Business rule validation
        |
        v
    IsActive = true
        |
        v
    Record reactivated

Reactivation operations are mainly restricted to Admin users.

---

## Other Business Method Names

`IInternService.CreateInternWithAccountAsync` creates both an Intern and its linked User account.

`InternController.CreateInternWithAccount` calls it through the existing:

    POST /api/interns

route.

Repository `AddAsync` methods retain their persistence meaning.

Ordinary methods such as:

- `AddAsync`
- `UpdateAsync`
- `GetAllAsync`
- `GetByIdAsync`

remain where the owning interface and parameters already make the operation clear.

`IncludingInactive` query variants retain their explicit meaning.

`UserRepository.DeleteAsync` still represents permanent deletion and is therefore not renamed to deactivation.

Validation, authorization, persistence boundaries, and response mapping remain unaffected by these naming improvements.

---

## Dashboard Architecture

Dashboard statistics are generated according to the authenticated user's ID and role.

This allows different users to receive different dashboard data.

For example:

- Admin users can receive global statistics.
- HR users can receive permitted management statistics.
- Intern users can receive statistics related to their own data.

The controller passes the authenticated user's ID and role to the Dashboard service.

The service then applies the required Business rules before returning statistics.

---

## Error Handling

The application includes global exception handling.

Expected Business errors are generally returned through `ServiceResult`.

Examples include:

- Validation errors
- Not found results
- Conflict results
- Forbidden operations

Unexpected application errors are handled centrally through middleware.

This prevents repeated exception-handling logic inside individual controllers.

Unexpected failures are also recorded through the application logging abstraction.

---

## Logging

InternTrack uses a project-owned application logging abstraction.

The main components are:

- `IAppLogger`
- `ConsoleAppLogger`

Business services do not depend on `ILogger<T>`.

`ConsoleAppLogger` writes structured application events to standard output.

Examples of logged events include:

- Login success and failure
- Refresh failures
- Inactive-account attempts
- Password changes
- Deactivation operations
- Reactivation operations
- Business-rule violations
- Unexpected application failures

Sensitive values are not logged.

Examples include:

- Passwords
- Password hashes
- Access tokens
- Refresh tokens
- Secret keys
- Request contents

Unexpected failures are logged centrally by the exception-handling middleware.

This keeps logging behavior explicit and independent from core Business rules.

---

## Frontend and Backend Communication

The frontend is maintained in a separate repository.

The frontend uses:

- React
- TypeScript
- Vite
- Axios
- React Router

General application flow:

    React Frontend
        |
        v
    Axios HTTP Request
        |
        v
    ASP.NET Core API
        |
        v
    Business Layer
        |
        v
    Repository Layer
        |
        v
    SQLite Database

Authentication cookies are sent with API requests when required.

The frontend uses an environment-based API address through:

    VITE_API_BASE_URL

Credential-based requests allow HttpOnly authentication cookies to be included with API requests.

---

## Architecture Goals

The main goals of the current architecture are:

- Separation of concerns
- Maintainable code
- Testable business logic
- Clear authorization rules
- Reduced duplication
- Independent database access
- Explicit query organization
- Centralized role handling
- Centralized response metadata
- Reusable application structures
- Easier future development

The architecture allows each layer to focus on a specific responsibility while keeping critical Business rules centralized in the Business layer.

---

## Project Status

The current backend architecture is functionally complete for InternTrack v1.

The project currently includes:

- Layered architecture
- Repository pattern
- Query-oriented DataAccess structure
- Service layer
- DTO-based API communication
- ServiceResult pattern
- Centralized API response conventions
- Automatic dependency registration
- Role-based authorization
- Centralized role handling
- Business-rule validation
- JWT authentication
- Refresh token rotation
- Soft delete and reactivation
- Global exception handling
- Project-owned structured logging
- Automated unit testing

Current automated test status:

    243 tests passed
    0 tests failed

The project is currently in the documentation, final review, and deployment preparation phase.