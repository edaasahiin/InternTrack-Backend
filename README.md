# InternTrack Backend

InternTrack Backend is the server-side application of **InternTrack**, an internship and task management system developed with ASP.NET Core Web API.

The backend provides authentication, authorization, internship management, department management, task management, dashboard statistics, soft delete and reactivation operations, and business-rule validation.

---

## Features

- JWT-based authentication
- HttpOnly cookie-based access and refresh token handling
- Refresh token rotation
- Refresh token revocation
- SHA-256 refresh token hashing
- Role-based authorization
- Admin, HR, and Intern roles
- Internship management
- Department management
- Task management
- Soft delete and reactivation support
- Active / inactive record management
- Dashboard statistics
- Business-rule validation
- Global exception handling
- Project-owned application logging
- Centralized role handling
- Automatic scoped dependency registration
- Query-oriented DataAccess structure
- Centralized API response conventions
- CORS configuration
- Rate limiting
- Swagger support in development
- Unit testing with xUnit and Moq

---

## Tech Stack

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQLite
- JWT
- xUnit
- Moq

---

## Architecture

The backend follows a layered architecture.

Main projects:

- `InternTrack.Api`
- `InternTrack.Business`
- `InternTrack.Core`
- `InternTrack.DataAccess`
- `InternTrack.Infrastructure`
- `InternTrack.Tests`

### InternTrack.Api

Responsible for the API layer and application configuration.

Contains:

- Controllers
- Middleware
- API helpers
- API conventions
- Dependency injection extensions
- Authentication and authorization pipeline
- Rate limiting configuration
- CORS configuration
- Swagger configuration
- Application startup configuration

Repeated response metadata is centralized through `InternTrackApiConventions`, which keeps controllers cleaner while preserving Swagger response documentation.

### InternTrack.Business

Contains the main business logic of the application.

Includes:

- Business services
- Service interfaces
- Business rules
- Validation logic
- ServiceResult structures
- Password hashing utilities
- Application logging abstraction

Business service interfaces inherit from `IScopedService`, allowing scoped registrations to be discovered automatically.

### InternTrack.Core

Contains shared application models and data structures.

Includes:

- Models
- DTOs
- Shared constants
- Role constants
- Task status constants
- Task priority constants
- Shared helpers

Role interpretation is centralized through `RoleHelper` instead of scattering role string comparisons across business services.

### InternTrack.DataAccess

Responsible for database communication and persistence.

Contains:

- Entity Framework Core DbContext
- Repository interfaces
- Repository implementations
- Query definitions
- Database migrations

Repository interfaces inherit from `IScopedRepository`, allowing them to be automatically registered through the dependency injection setup.

Database queries that require explicit reusable filtering can be placed under the `Queries` structure. For example, department name lookup is defined in `DepartmentQueries`, executed by `DepartmentRepository`, and interpreted as a business rule by `DepartmentService`.

### InternTrack.Infrastructure

Contains infrastructure-specific implementations.

Current responsibilities include:

- JWT access token generation
- Refresh token generation
- Custom application logging implementation

`ConsoleAppLogger` implements the project-owned `IAppLogger` abstraction and writes structured application events without depending on `ILogger<T>` inside application services.

### InternTrack.Tests

Contains automated tests for:

- Authentication
- Authorization
- Departments
- Interns
- Tasks
- Dashboard statistics
- Dependency injection
- Role handling
- Logging
- Persistence behavior
- API-related behavior
- Validation and edge cases

---

## Dependency Injection

InternTrack uses automatic scoped registration for Business services and DataAccess repositories.

The application uses marker interfaces:

- `IScopedService`
- `IScopedRepository`

The API scans the related assemblies, discovers matching interfaces and implementations, and registers them with scoped lifetime.

This removes the need to manually add a separate `AddScoped<I..., ...>()` statement for every Business service and repository.

Infrastructure-specific dependencies, such as `ITokenService` and `IAppLogger`, remain explicitly registered where appropriate.

---

## Authentication

InternTrack uses JWT-based authentication.

The authentication system includes:

- Short-lived access tokens
- Refresh tokens
- HttpOnly cookies
- Refresh token rotation
- Refresh token revocation
- SHA-256 refresh token hashing

Inactive Intern accounts are prevented from logging in or refreshing an existing session.

Refresh tokens are rotated when a session is renewed. The previous token is revoked and a new refresh token is generated.

Intern authentication checks explicitly distinguish between:

- Users who do not require an Intern profile
- Intern users with a missing Intern profile
- Intern users with an inactive Intern profile

Role interpretation is centralized through `RoleHelper`.

---

## User Roles

The system supports three roles:

- Admin
- HR
- Intern

### Admin

Admin users can:

- Manage interns
- Manage departments
- Manage tasks
- Access permitted inactive records
- Deactivate records
- Reactivate inactive records
- Perform administrative task operations

### HR

HR users can:

- Manage active interns
- Manage active departments
- Manage permitted task operations
- Edit active records

HR users cannot perform operations that are restricted to Admin users, such as some reactivation and deactivation operations.

### Intern

Intern users can:

- Access their own internship information
- View assigned tasks
- Update permitted task statuses
- Perform task operations according to defined business rules

Intern users cannot manage records that belong to other interns unless explicitly permitted by the business rules.

---

## Business Rules

Business rules are implemented in the Business layer instead of relying only on frontend restrictions.

This ensures that the API protects application rules even when requests are sent directly to backend endpoints.

Examples include:

- Users cannot perform unauthorized operations based on their role.
- Interns cannot update tasks assigned to another Intern.
- Invalid task status transitions are rejected.
- Overdue task restrictions are enforced at the service level.
- Duplicate data is validated before records are created or updated.
- Related entities are validated before operations are completed.
- Inactive Intern accounts cannot log in or refresh sessions.
- Reactivation operations validate related active records.

---

## Soft Delete and Reactivation

InternTrack supports soft delete for:

- Departments
- Interns
- Tasks

Soft-deleted records remain in the database but are marked as inactive.

Internal methods use explicit business-oriented names:

- `DeactivateDepartmentAsync`
- `DeactivateInternAsync`
- `DeactivateTaskAsync`
- `ReactivateDepartmentAsync`
- `ReactivateInternAsync`
- `ReactivateTaskAsync`

Intern creation uses:

- `CreateInternWithAccountAsync`

to make linked User account creation explicit.

Existing HTTP DELETE and PATCH `/restore` routes remain unchanged.

Entity Framework Core query filters are used to exclude inactive records from normal queries.

Admin users can access or reactivate inactive records where permitted.

Examples:

- An Intern can only be reactivated if the related Department is active.
- A Task can only be reactivated if the assigned Intern is active.

---

## Task Rules

Task operations include several business rules.

Examples include:

- Interns cannot update tasks assigned to another Intern.
- Overdue tasks cannot be started or completed by Intern users.
- Task status transitions are restricted.
- HR users cannot start or complete overdue tasks without first updating the due date.
- HR users cannot delete tasks.
- Admin users can manage active and inactive tasks where permitted.
- Completed task timestamps are managed automatically.

Task transition rules are validated consistently for different roles at the Business layer.

---

## Data Access Queries

Data access responsibilities are separated clearly.

For department name validation:

1. `DepartmentQueries` defines the query.
2. `DepartmentRepository` executes the query.
3. `DepartmentService` applies the duplicate-name business rule.

The previous `NameExistsAsync` approach was replaced with:

    GetByNameIncludingInactiveAsync

This returns the matching Department record or `null` instead of exposing a boolean business-oriented helper from the repository.

Department name checks:

- Include inactive departments
- Ignore leading and trailing whitespace
- Normalize case
- Support excluding the current Department during updates

Business decisions remain in the Business layer while query execution remains in DataAccess.

---

## Application Logging

InternTrack uses a project-owned logging abstraction.

The logging structure includes:

- `IAppLogger`
- `ConsoleAppLogger`

Application services do not depend on `ILogger<T>`.

The custom logger writes structured application events and is used for important operations such as:

- Login success and failure
- Refresh token failures
- Inactive account attempts
- Password changes
- Deactivation operations
- Reactivation operations
- Business-rule violations
- Unexpected application failures

Sensitive values are not logged.

Examples of excluded data include:

- Passwords
- Password hashes
- JWT values
- Refresh tokens
- Secret keys
- Request contents
- Sensitive user information

Unexpected failures are logged centrally by the exception-handling middleware.

---

## API Response Conventions

Repeated API response metadata is centralized using `InternTrackApiConventions`.

This reduces repeated `ProducesResponseType` declarations in controllers while preserving Swagger/OpenAPI response documentation.

Controllers remain focused on:

- Reading request data
- Reading authenticated user information
- Calling services
- Mapping service results

Runtime Business result mapping remains centralized through `ServiceResultMapper`.

Examples include:

- `NotFound` → `404 Not Found`
- `ValidationError` → `400 Bad Request`
- `Conflict` → `409 Conflict`
- `Forbidden` → `403 Forbidden`
- Create operations → `201 Created`
- Deactivation operations → `204 No Content`

---

## Security

The backend includes several security-related measures.

### Authentication Security

- JWT issuer validation
- JWT audience validation
- Access token validation
- HttpOnly cookies
- Secure cookies outside development
- Refresh token hashing
- Refresh token rotation
- Refresh token revocation

### Authorization

- Role-based authorization
- Admin, HR, and Intern role separation
- Centralized role interpretation
- Business-layer permission checks

### API Security

- Rate limiting for authentication endpoints
- Configurable CORS origins
- HTTPS redirection outside development
- Sensitive values kept outside source-controlled configuration files

The JWT secret key should never be committed to source control.

Environment variables or .NET User Secrets should be used for sensitive configuration values.

---

## Rate Limiting

Authentication-related endpoints use rate limiting.

Examples include:

- Login request limits
- Registration request limits
- Refresh token request limits

When the configured request limit is exceeded, the API may return:

    HTTP 429 Too Many Requests

---

## Database

InternTrack currently uses **SQLite** with Entity Framework Core.

Database configuration is defined through:

    ConnectionStrings:DefaultConnection

Entity Framework Core migrations are used to manage database schema changes.

Application startup does not automatically insert business data.

An authorized Admin or HR user creates departments explicitly through:

    POST /api/departments

A fresh database therefore starts without predefined departments.

Registration requires an existing active Department.

Department creation rejects duplicate names, including names belonging to inactive departments.

Existing inactive records should be explicitly reactivated instead of being recreated.

---

## Configuration

Important configuration sections include:

- `ConnectionStrings`
- `Jwt`
- `Cors`
- `Logging`

Sensitive values such as JWT secret keys should not be stored directly in source-controlled application settings.

Use:

- Environment variables
- .NET User Secrets

for sensitive configuration values.

---

## Development

### Restore Dependencies

    dotnet restore

### Build the Solution

    dotnet build

### Run the API

    dotnet run --project InternTrack.Api

### Run Unit Tests

    dotnet test

---

## Testing

The backend includes automated tests for critical business rules, authorization scenarios, authentication flows, infrastructure behavior, persistence behavior, and validation cases.

Tests are implemented using:

- xUnit
- Moq

Run all tests with:

    dotnet test

Current test coverage includes:

- Login validation
- Invalid password scenarios
- Inactive Intern authentication rules
- Intern accounts without related Intern profiles
- Role handling
- Refresh token validation
- Refresh token expiration
- Refresh token rotation
- Refresh token revocation
- Logout behavior
- Password change scenarios
- Profile updates
- Avatar updates
- Email validation and normalization
- Role-based authorization
- Dependency injection registration
- Scoped service lifetimes
- Custom application logging
- Exception logging
- Department creation and updates
- Department duplicate-name persistence checks
- Department deactivation and reactivation
- Intern creation and updates
- Intern deactivation and reactivation
- Task update authorization
- Task status transitions
- Overdue task restrictions
- Task deletion authorization
- Task reactivation rules
- Dashboard statistics
- Missing related entity scenarios
- Validation edge cases

Current test status:

    238 tests passed
    0 tests failed

---

## Frontend

The frontend is maintained in a separate repository.

It is developed using:

- React
- TypeScript
- Vite
- Axios
- React Router

The frontend communicates with the backend API and uses cookie-based authentication for access and refresh token handling.

---

## Documentation

Additional project documentation is available in the `docs` directory.

### API Documentation

File:

    docs/API.md

Contains:

- Authentication endpoints
- Dashboard endpoint
- Department endpoints
- Intern endpoints
- Task endpoints
- Role-based access information
- Authentication cookie behavior
- Rate limiting information
- API security notes
- API response conventions

### Architecture Documentation

File:

    docs/ARCHITECTURE.md

Contains:

- Layered architecture overview
- Responsibilities of each project
- Controller and service separation
- Automatic dependency registration
- Repository pattern
- Query organization
- ServiceResult pattern
- DTO usage
- Authentication architecture
- Authorization architecture
- Soft delete and reactivation flow
- Dashboard architecture
- Error handling
- Custom logging
- Frontend and backend communication

### Authentication Documentation

File:

    docs/AUTHENTICATION.md

Contains:

- Login flow
- Access token handling
- Refresh token handling
- Refresh token rotation
- Refresh token revocation
- HttpOnly cookie configuration
- Current user flow
- Password change flow
- Profile update flow
- Avatar update flow
- Logout flow
- Inactive Intern handling
- Role-based authorization
- Rate limiting
- Authentication security summary

### Role Handling Documentation

File:

    docs/ROLE_HANDLING.md

Contains:

- Centralized role interpretation
- Admin, HR, and Intern role checks
- Authentication-specific Intern role handling
- Role comparison behavior
- Intern profile validation

### Data Access Query Documentation

File:

    docs/DATA_ACCESS_QUERIES.md

Contains:

- Query organization
- Repository responsibilities
- Department query flow
- Duplicate-name validation structure
- Query and business-rule separation

### Setup Guide

File:

    docs/SETUP.md

Contains:

- Backend setup
- Frontend setup
- Dependency installation
- Database migration steps
- JWT configuration
- Local development configuration
- CORS configuration
- Development startup order
- Build and test commands
- Verification checklist
- Troubleshooting information

### Deployment Guide

File:

    docs/DEPLOYMENT.md

Contains:

- Production configuration requirements
- Backend environment variables
- JWT deployment configuration
- CORS configuration
- Authentication cookie settings
- HTTPS requirements
- Rate limiting behavior
- Database migration notes
- Backend build and publish steps
- Frontend environment configuration
- Production API URL setup
- Frontend build verification
- Source control security checks
- Deployment verification checklist
- Smoke test checklist
- Connection troubleshooting
- Final deployment flow

---

## Project Status

InternTrack Backend v1 is functionally complete.

The current version includes:

- Authentication and authorization
- Role-based access control
- Centralized role handling
- Internship management
- Department management
- Task management
- Soft delete and reactivation operations
- Dashboard statistics
- Refresh token management
- Security improvements
- Business-rule validation
- Automatic dependency registration
- Query-oriented DataAccess organization
- Custom application logging
- Centralized API response conventions
- Automated unit testing

The project is currently in the **documentation, final review, and deployment preparation phase**.