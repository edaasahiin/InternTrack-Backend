# InternTrack Backend

InternTrack Backend is the server-side application of **InternTrack**, an internship and task management system developed with ASP.NET Core Web API.

The backend provides authentication, authorization, internship management, department management, task management, dashboard statistics, soft delete and restore operations, and business-rule validation.

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
- Soft delete and restore support
- Active / inactive record management
- Dashboard statistics
- Business-rule validation
- Global exception handling
- Structured logging
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
- Authentication and authorization pipeline
- Rate limiting configuration
- CORS configuration
- Swagger configuration
- Application startup configuration

### InternTrack.Business

Contains the main business logic of the application.

Includes:

- Business services
- Service interfaces
- Business rules
- Validation logic
- ServiceResult structures
- Password hashing utilities

### InternTrack.Core

Contains shared application models and data structures.

Includes:

- Models
- DTOs
- Shared constants
- Role constants
- Task status constants
- Task priority constants

### InternTrack.DataAccess

Responsible for database communication and persistence.

Contains:

- Entity Framework Core DbContext
- Repository interfaces
- Repository implementations
- Database migrations
- Database seeding

### InternTrack.Infrastructure

Contains infrastructure-specific implementations.

Currently includes functionality such as:

- JWT access token generation
- Refresh token generation

### InternTrack.Tests

Contains unit tests for the business layer.

Main tested areas include:

- Authentication
- Departments
- Interns
- Tasks
- Dashboard statistics

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
- Restore inactive records
- Perform administrative task operations

### HR

HR users can:

- Manage active interns
- Manage active departments
- Manage permitted task operations
- Edit active records

HR users cannot perform operations that are restricted to Admin users, such as some restore and deactivation operations.

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

---

## Soft Delete and Restore

InternTrack supports soft delete for:

- Departments
- Interns
- Tasks

Soft-deleted records remain in the database but are marked as inactive.

Entity Framework Core query filters are used to exclude inactive records from normal queries.

Admin users can access or restore inactive records where permitted.

Restore operations also include related business-rule checks.

Examples:

- An Intern can only be restored if the related Department is active.
- A Task can only be restored if the assigned Intern is active.

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

Task transition rules are validated consistently for different roles at the business layer.

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

The DataAccess layer also includes database seeding functionality for initial application data.

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

The backend includes automated unit tests for critical business rules, authorization scenarios, authentication flows, and validation cases.

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
- Department creation and updates
- Department soft delete and restore
- Intern creation and updates
- Intern soft delete and restore
- Task update authorization
- Task status transitions
- Overdue task restrictions
- Task deletion authorization
- Task restore rules
- Dashboard statistics
- Missing related entity scenarios
- Validation edge cases

Current test status:

    144 tests passed
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

### Architecture Documentation

File:

    docs/Architecture.md

Contains:

- Layered architecture overview
- Responsibilities of each project
- Controller and service separation
- Repository pattern
- ServiceResult pattern
- DTO usage
- Authentication architecture
- Authorization architecture
- Soft delete and restore flow
- Dashboard architecture
- Error handling
- Logging
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

## Project Status

InternTrack Backend v1 is functionally complete.

The current version includes:

- Authentication and authorization
- Role-based access control
- Internship management
- Department management
- Task management
- Soft delete and restore operations
- Dashboard statistics
- Refresh token management
- Security improvements
- Business-rule validation
- Automated unit testing

The project is currently in the **documentation, final review, and deployment preparation phase**.