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

Shared models and DTOs are provided by:

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
- Authentication configuration
- Authorization configuration
- Dependency injection
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
- Applying restore rules

The Business layer communicates with repositories instead of accessing the database directly.

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

Examples include:

- User models
- Intern models
- Department models
- Task models
- Authentication DTOs
- Intern DTOs
- Department DTOs
- Task DTOs

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
- Database queries
- Database migrations
- Database seeding
- Entity persistence

The Business layer communicates with the DataAccess layer through repository interfaces.

This helps separate business logic from database-specific operations.

Examples of repository responsibilities include:

- Retrieving entities by ID
- Retrieving entities by email
- Checking whether records exist
- Adding records
- Updating records
- Retrieving active records
- Retrieving inactive records when required

---

## InternTrack.Infrastructure

`InternTrack.Infrastructure` contains implementations that depend on technical or infrastructure-specific concerns.

Examples include:

- JWT access token generation
- Refresh token generation

Separating these implementations from the Business layer helps keep business logic independent from technical implementation details.

The Business layer can depend on interfaces while the Infrastructure layer provides the concrete implementation.

---

## InternTrack.Tests

`InternTrack.Tests` contains automated unit tests for the backend.

The project uses:

- xUnit
- Moq

The tests mainly focus on the Business layer.

Current tested areas include:

- Authentication
- Login rules
- Refresh token behavior
- Password changes
- Profile updates
- Departments
- Interns
- Tasks
- Task status transitions
- Soft delete
- Restore operations
- Dashboard statistics
- Role-based authorization rules
- Validation scenarios
- Edge cases

The current test suite contains:

    144 passing tests
    0 failing tests

Repositories and other dependencies are mocked where appropriate so that business rules can be tested independently.

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

`InternTrack.Core` provides shared models, DTOs, and constants used by multiple layers.

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
    8. Repository communicates with Entity Framework Core
    9. Entity Framework Core communicates with SQLite
    10. Result returns to the Business service
    11. Business service creates a ServiceResult
    12. Controller maps the result to an HTTP response
    13. Response is returned to the client

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
- Returning HTTP status codes
- Applying authorization attributes

### Services

Services are responsible for application behavior.

Examples:

- Applying business rules
- Performing validation
- Checking permissions
- Coordinating repository operations
- Returning ServiceResult values

This separation keeps controllers simpler and makes business rules easier to test.

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
    Entity Framework Core
        |
        v
    SQLite

Benefits of this structure include:

- Better separation of concerns
- Easier unit testing
- Reduced database coupling
- Cleaner service code
- Easier replacement or modification of persistence logic

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

Controllers can then convert these results into appropriate HTTP responses.

This keeps HTTP response mapping separate from most business logic.

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
- Applying role-specific delete and restore rules

This means frontend restrictions are not relied on as the main security boundary.

---

## Soft Delete Architecture

Departments, Interns, and Tasks support soft delete.

Instead of permanently deleting records, entities are marked as inactive.

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

## Restore Architecture

Restore operations reverse soft delete when related business rules are satisfied.

Examples include:

- An Intern cannot be restored if the related Department is inactive.
- A Task cannot be restored if the assigned Intern is inactive.

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
    Record restored

Restore operations are mainly restricted to Admin users.

---

## Dashboard Architecture

Dashboard statistics are generated according to the authenticated user's ID and role.

This allows different users to receive different dashboard data.

For example:

- Admin users can receive global statistics.
- HR users can receive permitted management statistics.
- Intern users can receive statistics related to their own data.

The controller passes the authenticated user's ID and role to the Dashboard service.

The service then applies the required business rules before returning statistics.

---

## Error Handling

The application includes global exception handling.

This helps provide consistent behavior when unexpected exceptions occur.

Expected business errors are generally returned through `ServiceResult`.

Examples include:

- Validation errors
- Not found results
- Conflict results
- Forbidden operations

Unexpected application errors can be handled through middleware.

This prevents repeated exception-handling logic inside individual controllers.

---

## Logging

Structured logging is used in the application.

Logging can be used for important operations such as:

- Authentication events
- Business-rule failures
- Service errors
- Unexpected exceptions

Logging helps with debugging and application monitoring without changing the main business logic.

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

---

## Architecture Goals

The main goals of the current architecture are:

- Separation of concerns
- Maintainable code
- Testable business logic
- Clear authorization rules
- Reduced duplication
- Independent database access
- Reusable application structures
- Easier future development

The architecture allows each layer to focus on a specific responsibility while keeping critical business rules centralized in the Business layer.

---

## Project Status

The current backend architecture is functionally complete for InternTrack v1.

The project currently includes:

- Layered architecture
- Repository pattern
- Service layer
- DTO-based API communication
- ServiceResult pattern
- Role-based authorization
- Business-rule validation
- JWT authentication
- Refresh token rotation
- Soft delete and restore
- Global exception handling
- Structured logging
- Automated unit testing

The project is currently in the documentation, final review, and deployment preparation phase.