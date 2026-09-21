# InternTrack Backend

InternTrack Backend is the server-side application of InternTrack, an internship and task management system developed with ASP.NET Core Web API.

## Features

- JWT-based authentication
- HttpOnly cookie-based access and refresh token handling
- Refresh token rotation
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
- Unit tests with xUnit and Moq

## Tech Stack

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQLite
- JWT
- xUnit
- Moq

## Architecture

The backend follows a layered architecture:

```text
InternTrack.Api
InternTrack.Business
InternTrack.Core
InternTrack.DataAccess
InternTrack.Infrastructure
InternTrack.Tests
```

### InternTrack.Api

Contains:

- Controllers
- Middleware
- API helpers
- Application configuration
- Authentication and authorization pipeline
- Rate limiting
- CORS configuration
- Swagger configuration

### InternTrack.Business

Contains:

- Business services
- Service interfaces
- Business rules
- ServiceResult structures
- Password hashing utilities

### InternTrack.Core

Contains:

- Models
- DTOs
- Shared constants
- Role constants
- Task status constants
- Task priority constants

### InternTrack.DataAccess

Contains:

- Entity Framework Core DbContext
- Repository interfaces
- Repository implementations
- Database migrations
- Database seeding

### InternTrack.Infrastructure

Contains infrastructure-specific implementations such as JWT token generation.

### InternTrack.Tests

Contains unit tests for:

- Authentication
- Departments
- Interns
- Tasks
- Dashboard statistics

## Authentication

InternTrack uses JWT-based authentication.

The application uses:

- Short-lived access tokens
- Refresh tokens
- HttpOnly cookies
- Refresh token rotation
- Refresh token revocation
- SHA-256 refresh token hashing

Inactive Intern accounts are prevented from logging in or refreshing an existing session.

## User Roles

### Admin

Admin users can:

- Manage interns
- Manage departments
- Manage tasks
- Activate and deactivate records
- Restore inactive records
- Access inactive records where permitted

### HR

HR users can:

- Manage active interns
- Manage active departments
- Manage permitted task operations
- Edit active records

HR users cannot perform Admin-only restore or deactivation operations.

### Intern

Intern users can:

- Access their own internship information
- View assigned tasks
- Update permitted task statuses
- Create or manage tasks according to business rules
- Delete tasks according to assigned permissions

## Soft Delete

InternTrack supports soft delete for:

- Departments
- Interns
- Tasks

Inactive records are excluded through Entity Framework Core query filters.

Admin users can restore records when related business rules are satisfied.

For example:

- An Intern can only be restored if the related Department is active.
- A Task can only be restored if the assigned Intern is active.

## Task Rules

Task operations include business rules such as:

- Interns cannot update tasks assigned to another Intern.
- Overdue tasks cannot be started or completed by Intern users.
- Intern task status transitions are restricted.
- HR users cannot start or complete overdue tasks without first updating the due date.
- HR users cannot delete tasks.
- Admin users can manage active and inactive tasks.
- Completed task timestamps are managed automatically.

## Security

The backend includes several security measures:

- JWT issuer and audience validation
- HttpOnly cookies
- Secure cookies outside development
- Refresh token hashing
- Refresh token rotation and revocation
- Role-based authorization
- Rate limiting for authentication endpoints
- Configurable CORS origins
- HTTPS redirection outside development
- Secrets kept outside source-controlled application settings

## Rate Limiting

Authentication endpoints use rate limiting.

Examples include:

- Login request limits
- Registration request limits
- Refresh token request limits

When the configured limit is exceeded, the API returns HTTP 429.

## Database

InternTrack currently uses SQLite with Entity Framework Core.

Database configuration is defined through:

```text
ConnectionStrings:DefaultConnection
```

Entity Framework Core migrations are used to manage schema changes.

## Development

Restore dependencies:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build
```

Run the API:

```bash
dotnet run --project InternTrack.Api
```

Run tests:

```bash
dotnet test
```

## Configuration

Important configuration sections include:

```text
ConnectionStrings
Jwt
Cors
Logging
```

The JWT secret key should not be committed to source control.

Use environment variables or .NET User Secrets for sensitive values.

## Testing

The backend includes unit tests for critical business and authorization rules.

Run all tests with:

```bash
dotnet test
```

Current test coverage includes:

- Login validation
- Inactive Intern authentication rules
- Refresh token validation and rotation
- Department soft delete and restore
- Intern soft delete and restore
- Task update authorization
- Overdue task rules
- Task deletion authorization
- Task restore rules
- Dashboard statistics

## Frontend

The frontend is maintained in a separate repository and is developed with:

- React
- TypeScript
- Vite
- Axios
- React Router

## Project Status

InternTrack Backend v1 is functionally complete.

The current version includes authentication, authorization, soft delete, task management, internship management, department management, dashboard statistics, security hardening, and automated tests.
