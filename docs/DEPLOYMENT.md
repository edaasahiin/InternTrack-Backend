# InternTrack Deployment Guide

This document describes the deployment preparation steps for the InternTrack backend and frontend applications.

InternTrack consists of two separate applications:

- Backend: ASP.NET Core Web API
- Frontend: React, TypeScript, and Vite

The purpose of this guide is to summarize the configuration, security, build, environment, and verification steps required before deployment.

---

## Deployment Overview

The general deployment flow is:

    1. Configure production environment variables

    2. Configure database connection

    3. Configure JWT settings

    4. Configure allowed CORS origins

    5. Restore and build the backend

    6. Run backend tests

    7. Apply database migrations

    8. Start or deploy the backend

    9. Configure the frontend API URL

    10. Run frontend checks

    11. Build the frontend

    12. Deploy the frontend build

    13. Perform smoke tests

    14. Complete final verification

---

## Backend Production Configuration

The backend uses configuration values from `appsettings.json`, environment variables, and other supported .NET configuration sources.

Important configuration sections include:

- `ConnectionStrings`
- `Jwt`
- `Cors`
- `Logging`

Sensitive values should not be stored directly in source-controlled configuration files.

Production-specific values should be supplied by the deployment environment.

---

## Backend Environment Variables

A production environment should provide values such as:

    ConnectionStrings__DefaultConnection=Data Source=interntrack.db

    Jwt__Key=<secure-jwt-secret>

    Jwt__Issuer=InternTrack.Api

    Jwt__Audience=InternTrack.Frontend

    Jwt__AccessTokenMinutes=15

    Jwt__RefreshTokenDays=7

    Cors__AllowedOrigins__0=https://frontend-domain.example.com

Additional origins can be configured using additional indexes:

    Cors__AllowedOrigins__1=https://another-domain.example.com

The actual production values should be configured according to the selected hosting environment.

---

## JWT Configuration

The backend requires JWT configuration values.

Required settings include:

- Key
- Issuer
- Audience
- Access token lifetime
- Refresh token lifetime

The JWT secret key must not be committed to source control.

The application validates that required JWT configuration is available during startup.

JWT validation includes:

- Issuer validation
- Audience validation
- Lifetime validation
- Signing key validation

The current token validation configuration also uses:

    ClockSkew = TimeSpan.Zero

This prevents additional expiration tolerance from being applied.

---

## CORS Configuration

The backend reads allowed frontend origins from configuration.

Example production value:

    Cors__AllowedOrigins__0=https://frontend-domain.example.com

The application uses credential-enabled CORS because authentication is based on HttpOnly cookies.

The backend CORS policy includes:

- Configured origins
- Any request header
- Any HTTP method
- Credentials

Production origins should match the real frontend deployment address.

Local development addresses should not be used as the only production CORS configuration.

---

## Authentication Cookies

The application uses two authentication cookies:

- `accessToken`
- `refreshToken`

Both cookies use:

- HttpOnly
- SameSite Lax

Outside development, cookies are also configured as:

    Secure = true

This means production deployment should use HTTPS.

Access token cookie path:

    /api

Refresh token cookie path:

    /api/auth

The restricted paths reduce unnecessary cookie transmission.

Legacy root-path token cookies can also be cleared during authentication cleanup.

---

## HTTPS

HTTPS redirection is enabled outside the development environment.

Production deployment should provide HTTPS correctly through the selected hosting platform, reverse proxy, or web server.

Secure cookies depend on HTTPS being available.

---

## Rate Limiting

Authentication endpoints use rate limiting.

Current policies include:

- Login policy
- Registration policy
- Refresh policy

Current limits include:

### Login

    5 requests per minute per IP address

### Registration

    3 requests per 10 minutes per IP address

### Refresh

    20 requests per minute per IP address

When the configured limit is exceeded, the API returns:

    HTTP 429 Too Many Requests

Response metadata for these rate-limited endpoints is documented through the centralized API convention structure.

---

## Reverse Proxy Note

Rate limiting currently uses the remote IP address as the partition key.

If the application is deployed behind a reverse proxy, load balancer, or similar infrastructure, forwarded header configuration may be required so that the backend can identify the real client IP address.

This should be reviewed according to the selected hosting environment.

---

## Database

InternTrack currently uses SQLite with Entity Framework Core.

The connection string is configured using:

    ConnectionStrings__DefaultConnection

Before deployment, verify that:

- The connection string is valid.
- The deployment environment has write access to the SQLite database location.
- Database migrations are applied.
- Database files are stored in a persistent location.

---

## Database Migrations

Apply existing Entity Framework Core migrations before starting the production application when required.

Example command:

    dotnet ef database update --project InternTrack.DataAccess --startup-project InternTrack.Api

Migration execution should be verified before the application begins serving production traffic.

---

## Initial Department Setup

The application does not insert business data on startup.

There is no generic `DbSeeder` in the current application flow.

Existing Departments are preserved, and a fresh database starts without predefined Departments.

After migrations are applied and an authorized Admin or HR account is available through the selected account-provisioning process, create the required Departments using:

    POST /api/departments

Example request body:

    {
      "name": "Software"
    }

At least one active Department is required before Intern registration can use that Department.

Department creation is handled through the existing Business and DataAccess flow.

Duplicate Department names are rejected, including names belonging to inactive records.

Matching:

- Includes inactive Departments
- Ignores leading and trailing whitespace
- Normalizes case
- Supports update exclusion

Inactive Departments should be explicitly reactivated instead of recreated.

The previous predefined Department names are not required system records.

---

## Dependency Injection

Business services and DataAccess repositories are registered automatically using marker interfaces.

Main marker interfaces include:

- `IScopedService`
- `IScopedRepository`

The API scans the relevant assemblies and registers matching implementations with scoped lifetime.

This avoids maintaining a separate manual `AddScoped` entry for each Business service and repository.

Infrastructure-specific dependencies remain explicitly registered where appropriate.

Examples include:

- `ITokenService`
- `IAppLogger`

Startup validation helps detect missing or ambiguous implementations.

---

## Application Logging

The application uses a project-owned logging abstraction.

The Business layer depends on:

- `IAppLogger`

The Infrastructure layer provides:

- `ConsoleAppLogger`

Business services do not depend on `ILogger<T>`.

The custom logger records structured application events such as:

- Authentication events
- Refresh failures
- Inactive-account attempts
- Password changes
- Deactivation operations
- Reactivation operations
- Business-rule violations
- Unexpected application failures

Sensitive values should not be logged.

Examples include:

- Passwords
- Password hashes
- Access tokens
- Refresh tokens
- JWT secret keys
- Request contents

Unexpected failures are logged centrally by the exception-handling middleware.

---

## API Response Conventions

Repeated response metadata is centralized through:

    InternTrackApiConventions

Assembly-level `ApiConventionType` registration supplies conventions by action name. Five explicit `ApiConventionMethod` overrides resolve overlaps for Task creation/update, Department deactivation, and Auth avatar/profile updates.

This reduces repeated `ProducesResponseType` declarations while preserving Swagger/OpenAPI response documentation.

Runtime Business result mapping continues to be handled by:

    ServiceResultMapper

Examples include:

    ValidationError -> 400 Bad Request

    NotFound -> 404 Not Found

    Forbidden -> 403 Forbidden

    Conflict -> 409 Conflict

Successful create operations can return:

    201 Created

Successful deactivation operations can return:

    204 No Content

---

## Backend Build

Before deployment, restore dependencies:

    dotnet restore

Build the solution:

    dotnet build

Run automated tests:

    dotnet test

The current backend test suite contains:

    243 passing tests
    0 failing tests

A production deployment should only continue after the required build and test checks succeed.

---

## Running the Backend

The backend can be started directly using:

    dotnet run --project InternTrack.Api

For a published deployment, create a Release build with:

    dotnet publish InternTrack.Api -c Release

The generated publish output can then be deployed to the selected hosting environment.

---

## Swagger

Swagger is enabled only in the development environment.

This means Swagger UI is not exposed automatically in production.

The current API response conventions continue to provide response metadata when Swagger is enabled.

---

## Global Exception Handling

The backend includes global exception-handling middleware.

Unexpected application errors are handled centrally instead of requiring repeated exception-handling logic in controllers.

Expected Business errors are generally represented through the application's `ServiceResult` structure.

Unexpected failures can also be recorded through `IAppLogger`.

---

## Frontend Environment Configuration

The frontend uses Vite environment variables.

The API base URL is read from:

    VITE_API_BASE_URL

Local development example:

    VITE_API_BASE_URL=http://localhost:5053/api

Production example:

    VITE_API_BASE_URL=https://api.example.com/api

The production frontend must use the deployed backend API address.

---

## Frontend Environment Validation

The frontend validates that `VITE_API_BASE_URL` is configured.

If the value is missing, the application throws an error instead of silently sending requests to an incorrect address.

This helps detect deployment configuration mistakes earlier.

---

## Frontend API Client

The Axios client is configured with:

    withCredentials: true

This is required because the backend uses HttpOnly cookies for authentication.

The client also includes:

- Centralized API base URL
- Centralized error handling
- Automatic refresh handling
- Retry protection
- Shared refresh requests

---

## Refresh Handling

When an authenticated request returns HTTP 401, the frontend can attempt to refresh the session.

The refresh mechanism excludes authentication endpoints such as:

- Login
- Refresh
- Logout

This prevents refresh loops.

The frontend also uses a shared refresh promise to avoid sending multiple refresh requests simultaneously when several requests fail at the same time.

---

## Frontend Build

Install dependencies:

    npm install

Run TypeScript checks:

    npm run typecheck

Run ESLint:

    npm run lint

Create the production build:

    npm run build

The generated Vite production output is written to:

    dist/

The generated build can then be deployed to a static hosting environment.

---

## Frontend Deployment Notes

Before deploying the frontend, verify that:

- `VITE_API_BASE_URL` points to the production backend.
- The backend CORS configuration allows the frontend origin.
- HTTPS is available when secure cookies are enabled.
- Browser requests can send credentials.
- The production build completes successfully.

---

## Environment Files

Example environment files can be included in source control.

Recommended example file:

    .env.example

Local or secret environment files should remain excluded from source control.

Examples include:

    .env

    .env.local

    .env.*.local

The `.env.example` file should contain only example values and variable names.

---

## Source Control Security

Before deployment, verify that the repository does not contain:

- Real JWT secret keys
- Production passwords
- Sensitive connection strings
- Private environment files
- Other deployment secrets

Secrets should be provided through the deployment environment or another secure secret-management mechanism.

---

## Backend Verification Checklist

Before deployment, verify:

- `dotnet restore` succeeds.
- `dotnet build` succeeds.
- `dotnet test` succeeds.
- All 238 backend tests pass.
- JWT configuration is valid.
- JWT key is provided securely.
- Database connection is valid.
- Database migrations are applied.
- Required Department records exist where needed.
- Automatic dependency registration succeeds.
- Custom application logging initializes correctly.
- CORS contains the production frontend origin.
- HTTPS is available.
- Authentication cookies work correctly.
- Rate limiting is enabled.
- Swagger remains disabled outside development.

---

## Frontend Verification Checklist

Before deployment, verify:

- `npm install` succeeds.
- `npm run typecheck` succeeds.
- `npm run lint` succeeds.
- `npm run build` succeeds.
- `VITE_API_BASE_URL` is defined.
- The API URL points to the deployed backend.
- Credential-enabled requests work.
- Login works.
- Session refresh works.
- Logout works.
- Protected routes work.

---

## Smoke Test Checklist

After deployment, perform basic application checks.

### Authentication

Verify:

- Login
- Current user retrieval
- Session refresh
- Password change
- Logout
- Protected route behavior
- Inactive Intern rejection
- Missing Intern profile rejection where applicable

### Departments

Verify:

- Department listing
- Department creation
- Department update
- Deactivation
- Reactivation

Use the appropriate user role for each operation.

### Interns

Verify:

- Intern listing
- Intern detail
- Intern creation with linked User account
- Intern update
- Deactivation
- Reactivation

### Tasks

Verify:

- Task listing
- Task creation
- Task update
- Task status transitions
- Overdue task restrictions
- Deactivation rules
- Reactivation

### Dashboard

Verify that dashboard statistics load correctly for:

- Admin
- HR
- Intern

---

## Connection Troubleshooting

If the frontend displays a connection error such as:

    ERR_CONNECTION_REFUSED

check the following:

- Backend API is running.
- Backend port is correct.
- `VITE_API_BASE_URL` contains the correct address.
- Frontend development server was restarted after environment changes.
- Backend CORS settings allow the frontend origin.

A connection-refused error can occur simply because the backend is not currently running.

---

## Dependency Injection Troubleshooting

If application startup reports a missing or ambiguous implementation, check:

- Business service interfaces inherit from `IScopedService`.
- Repository interfaces inherit from `IScopedRepository`.
- A matching concrete implementation exists.
- Multiple unintended implementations were not added.
- Infrastructure-specific dependencies are registered explicitly where required.

---

## Authentication Troubleshooting

If authentication does not work, check:

- JWT configuration values
- Cookie configuration
- CORS credentials
- HTTPS availability in production
- Refresh-token state
- Intern profile existence and active state
- Frontend credential-enabled requests

---

## Deployment Security Checklist

Before production deployment, verify:

- JWT secret is not committed.
- Authentication cookies are HttpOnly.
- Secure cookies are enabled in production.
- HTTPS is enabled.
- CORS origins are restricted.
- Refresh tokens are rotated.
- Refresh tokens are revocable.
- Refresh tokens are stored as hashes.
- Authentication endpoints are rate limited.
- Sensitive configuration is stored outside source control.
- Sensitive authentication values are not written to application logs.
- Frontend environment files do not contain private secrets.

---

## Release Verification

Before considering the deployment preparation complete, Release outputs should also be verified.

### Backend Release Publish

Create a Release publish with:

    dotnet publish InternTrack.Api -c Release

After the command completes, verify that the publish process finishes without errors and that the Release output is generated successfully.

### Frontend Production Build

Create the frontend production build with:

    npm run build

After the build completes, verify that the `dist` directory is generated successfully.

### Release Verification Checklist

Before deployment, confirm that:

- Backend Release publish completed successfully.
- Frontend production build completed successfully.
- Required environment variables are configured.
- JWT configuration is valid.
- CORS origins are configured correctly.
- Authentication cookies are configured correctly.
- All 238 backend tests pass.
- Frontend type checking passes.
- Frontend lint checks pass.
- Login, refresh, logout, and protected-route behavior were verified.
- Basic application smoke tests were completed successfully.

This final verification step helps confirm that both backend and frontend applications are ready for deployment preparation.

---

## Final Deployment Flow

A complete deployment can follow this order:

    Backend configuration
        |
        v
    Production environment variables
        |
        v
    Database migration
        |
        v
    Backend restore and build
        |
        v
    Backend tests
        |
        v
    Backend Release publish
        |
        v
    Backend deployment
        |
        v
    Frontend production API configuration
        |
        v
    Frontend typecheck and lint
        |
        v
    Frontend production build
        |
        v
    Frontend deployment
        |
        v
    Smoke tests
        |
        v
    Final verification

---

## Deployment Status

InternTrack v1 is prepared for deployment review.

Current preparation includes:

- Environment-based configuration
- JWT configuration validation
- Production CORS configuration support
- Secure cookie behavior
- HTTPS support
- Authentication rate limiting
- Automatic scoped dependency registration
- Project-owned application logging
- Centralized role interpretation
- Query-oriented DataAccess structure
- Centralized API response conventions
- Frontend environment validation
- Backend automated testing
- Frontend build verification
- Deployment documentation
- Smoke test planning

Current backend test status:

    243 tests passed
    0 tests failed

Final production values should be configured according to the selected hosting environment before deployment.