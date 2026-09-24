# InternTrack Setup Guide

This document explains how to configure and run the InternTrack backend and frontend in a local development environment.

InternTrack consists of two separate applications:

- Backend: ASP.NET Core Web API
- Frontend: React, TypeScript, and Vite

Both applications should be configured and started separately.

---

## Prerequisites

Before running the project, make sure the following tools are installed:

- .NET SDK
- Node.js
- npm
- Git
- SQLite support
- Visual Studio Code or another compatible IDE

Recommended tools:

- Postman or Swagger for API testing
- GitHub for source control

---

# Backend Setup

The backend is developed using ASP.NET Core Web API.

Main backend technologies include:

- C#
- ASP.NET Core
- Entity Framework Core
- SQLite
- JWT
- xUnit
- Moq

---

## 1. Clone the Backend Repository

Clone the backend repository:

    git clone <backend-repository-url>

Navigate to the backend project directory:

    cd InternTrack-Backend

---

## 2. Restore Dependencies

Restore NuGet packages:

    dotnet restore

---

## 3. Configure Application Settings

The application uses configuration sections such as:

- ConnectionStrings
- Jwt
- Cors
- Logging

Sensitive configuration values should not be committed directly to source control.

Examples of sensitive values include:

- JWT secret key
- Production connection strings
- Other environment-specific secrets

Use environment variables or .NET User Secrets for sensitive values.

---

## 4. Configure JWT Settings

The application requires JWT configuration values.

Typical settings include:

- Key
- Issuer
- Audience
- AccessTokenMinutes
- RefreshTokenDays

Example configuration structure:

    Jwt:
      Key: <your-secret-key>
      Issuer: InternTrack
      Audience: InternTrackClient
      AccessTokenMinutes: 15
      RefreshTokenDays: 7

The real secret key should not be committed to the repository.

For local development, sensitive values can be supplied through environment variables or .NET User Secrets.

---

## 5. Configure Database Connection

InternTrack currently uses SQLite.

The connection string is defined using:

    ConnectionStrings:DefaultConnection

Example:

    Data Source=interntrack.db

The exact database filename may vary depending on the local configuration.

---

## 6. Apply Database Migrations

Entity Framework Core migrations are used to manage database schema changes.

Apply existing migrations with:

    dotnet ef database update --project InternTrack.DataAccess --startup-project InternTrack.Api

If the Entity Framework CLI is not installed, install it with:

    dotnet tool install --global dotnet-ef

If it is already installed but needs to be updated:

    dotnet tool update --global dotnet-ef

---

## 7. Initial Database State

Application startup does not automatically insert Department or other business data.

There is no generic `DbSeeder` in the current application flow.

A fresh database can therefore start without Department records.

Departments are created explicitly by an authorized Admin or HR user through:

    POST /api/departments

Registration requires an existing active Department.

This keeps business-data creation inside the normal application flow instead of startup initialization.

---

## 8. Build the Backend

Build the solution:

    dotnet build

The build should complete without errors before the API is started.

---

## 9. Run the Backend

Start the API with:

    dotnet run --project InternTrack.Api

The application will use the URLs configured by ASP.NET Core.

Check the terminal output to see the active local API address.

---

## 10. Swagger

Swagger is available in the development environment.

After starting the API, open the Swagger URL displayed by the application.

Swagger can be used to:

- View available endpoints
- Inspect request and response models
- Review documented HTTP status codes
- Test API requests
- Review authentication requirements

Repeated response metadata is centralized through `InternTrackApiConventions`.

This keeps controller actions cleaner while preserving Swagger response documentation.

---

## 11. Dependency Injection

Business services and DataAccess repositories are registered automatically.

Marker interfaces are used for scoped dependencies:

- `IScopedService`
- `IScopedRepository`

The API discovers matching implementations and registers them with scoped lifetime.

Infrastructure-specific services such as token generation and application logging can remain explicitly registered.

This means new Business services and repositories that follow the marker-interface convention do not require a separate manual `AddScoped` entry.

---

## 12. Application Logging

The application uses a project-owned logging abstraction.

Main components include:

- `IAppLogger`
- `ConsoleAppLogger`

Business services do not depend on `ILogger<T>`.

Important application events are written through the custom logger.

Sensitive values such as passwords, password hashes, access tokens, refresh tokens, and secrets should not be logged.

---

## 13. Run Backend Tests

Run the automated tests with:

    dotnet test

The current backend test suite contains:

    243 tests passed
    0 tests failed

The tests cover areas such as:

- Authentication
- Authorization
- Role handling
- Departments
- Interns
- Tasks
- Dashboard statistics
- Soft delete
- Reactivation operations
- Refresh tokens
- Task status transitions
- Dependency injection registration
- Custom application logging
- Department query behavior
- Persistence behavior
- Validation scenarios
- Edge cases

---

# Frontend Setup

The frontend is developed using:

- React
- TypeScript
- Vite
- Axios
- React Router

The frontend is maintained in a separate repository.

---

## 1. Clone the Frontend Repository

Clone the frontend repository:

    git clone <frontend-repository-url>

Navigate to the frontend project:

    cd InternTrack-Frontend

---

## 2. Install Dependencies

Install frontend dependencies with:

    npm install

---

## 3. Configure Backend API Address

The frontend communicates with the backend using an environment-based API address.

Create or update:

    .env.local

Example:

    VITE_API_BASE_URL=http://localhost:5053/api

The backend port must match the local backend address.

The repository also contains an example environment configuration that can be used as a reference.

Real environment-specific values should not be committed when they contain sensitive or machine-specific configuration.

The frontend validates that `VITE_API_BASE_URL` is configured before creating API requests.

---

## 4. Cookie-Based Requests

InternTrack uses HttpOnly cookies for authentication.

The Axios client is configured with credential support so authentication cookies can be sent with API requests.

This is important for:

- Login
- Authenticated API requests
- Refresh token operations
- Logout

The backend CORS configuration must also allow the frontend development origin and credentials.

---

## 5. Start the Frontend

Start the Vite development server:

    npm run dev

The terminal will display the local frontend address.

Open this address in a browser.

---

## 6. Run Type Checking

Run TypeScript validation with:

    npm run typecheck

This checks the project for TypeScript errors.

---

## 7. Run ESLint

Run code-quality checks with:

    npm run lint

This checks the frontend source code for linting problems.

---

## 8. Create Production Build

Create a production build with:

    npm run build

The build should complete successfully before deployment.

The generated production files are written to the Vite build output directory:

    dist/

---

# Recommended Local Startup Order

When running the full application locally, use the following order:

    1. Configure backend settings

    2. Apply database migrations

    3. Start the backend API

    4. Confirm Swagger is available

    5. Create any required initial Department records

    6. Configure VITE_API_BASE_URL

    7. Start the frontend

    8. Open the frontend application

    9. Test authentication and application operations

---

# Local Development Flow

A typical local request flow is:

    Browser
        |
        v
    React Frontend
        |
        v
    Axios Request
        |
        v
    ASP.NET Core API
        |
        v
    Business Layer
        |
        v
    DataAccess Layer
        |
        v
    SQLite Database

Authentication cookies are handled between the browser and the backend.

---

# CORS Configuration

The backend uses configurable CORS settings.

The frontend development address must be included in the allowed origins.

Example local origins can include:

    http://localhost:5173

    http://localhost:5174

The exact frontend port may vary.

Because InternTrack uses cookie-based authentication, the backend CORS configuration must allow credentials.

Production origins should be configured separately from local development origins.

---

# Authentication Configuration

InternTrack uses:

- JWT access tokens
- Refresh tokens
- HttpOnly cookies
- Refresh token rotation
- Refresh token revocation
- Centralized role interpretation

Access token configuration includes:

    Jwt:AccessTokenMinutes

Refresh token configuration includes:

    Jwt:RefreshTokenDays

The refresh token is sent only to authentication-related backend routes because of its configured cookie path.

Role interpretation is centralized through `RoleHelper`.

Intern accounts additionally require an existing active Intern profile.

---

# Development Environment

In development mode:

- Swagger is available.
- Secure cookies can be disabled for local HTTP development.
- Local frontend origins can be allowed through CORS.
- Environment-specific values can be provided through local configuration.

Outside development:

- Secure cookies should be enabled.
- HTTPS should be used.
- Production CORS origins should be configured.
- Sensitive secrets should be provided through secure configuration sources.
- Swagger is not intended to be enabled by the current production configuration unless explicitly configured.

---

# Verification Checklist

Before considering the local setup complete, verify the following:

- Backend dependencies restored successfully
- Database configuration is valid
- Database migrations applied successfully
- Required Department records created when needed
- Backend build succeeds
- Backend API starts successfully
- Swagger opens successfully
- Backend tests pass: 238 passed, 0 failed
- Frontend dependencies installed successfully
- `VITE_API_BASE_URL` points to the correct backend
- Frontend development server starts successfully
- TypeScript checks pass
- ESLint checks pass
- Frontend production build succeeds
- Login works
- Authenticated requests work
- Refresh flow works
- Logout works
- Dashboard loads successfully
- Department operations work according to role
- Intern operations work according to role
- Task operations work according to role

---

# Useful Commands

Backend:

    dotnet restore

    dotnet build

    dotnet run --project InternTrack.Api

    dotnet test

Database:

    dotnet ef database update --project InternTrack.DataAccess --startup-project InternTrack.Api

Frontend:

    npm install

    npm run dev

    npm run typecheck

    npm run lint

    npm run build

---

# Troubleshooting

## Backend Does Not Start

Check:

- .NET SDK installation
- Application configuration
- Database connection string
- JWT configuration
- Missing environment variables
- Port conflicts

Also check whether the required JWT secret value has been configured outside source-controlled settings.

---

## Database Errors

Check:

- SQLite database configuration
- Existing migrations
- Entity Framework CLI installation

Try:

    dotnet ef database update --project InternTrack.DataAccess --startup-project InternTrack.Api

Remember that a fresh database does not automatically contain Department records.

This is expected behavior in the current architecture.

---

## Frontend Cannot Reach Backend

Check:

- Backend is running
- `VITE_API_BASE_URL` is correct
- Backend CORS configuration allows the frontend origin
- Browser requests include credentials where required
- Backend port matches frontend configuration

A browser network error can occur simply because the backend API is not currently running.

---

## Authentication Does Not Work

Check:

- Backend API is running
- Cookie credentials are enabled
- CORS settings are correct
- JWT settings are configured
- Access and refresh token lifetimes are valid
- Browser cookies are not blocked
- The Intern account has a related active Intern profile when applicable
- The required Department exists and is active

---

## Dependency Injection Errors

If application startup reports a missing or ambiguous service implementation, check:

- The service interface inherits from `IScopedService`
- The repository interface inherits from `IScopedRepository`
- A matching concrete implementation exists
- Multiple unintended implementations have not been created

Infrastructure-specific dependencies should be checked separately if they use explicit registration.

---

## Build Errors

Backend:

    dotnet build

Frontend:

    npm run typecheck

    npm run lint

    npm run build

Review terminal output for the specific error before making changes.

---

# Setup Status

The project is configured as two separate applications:

- InternTrack Backend
- InternTrack Frontend

The backend provides:

- API endpoints
- Authentication and authorization
- Business rules
- Persistence
- Automatic scoped dependency registration
- Custom application logging
- Centralized API response conventions
- Automated tests

The frontend provides:

- User interface
- Routing
- Authentication state
- Environment-based API communication

Both applications must be configured correctly for the complete InternTrack system to work.