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

- SecretKey
- Issuer
- Audience
- AccessTokenMinutes
- RefreshTokenDays

Example configuration structure:

    Jwt:
      SecretKey: <your-secret-key>
      Issuer: InternTrack
      Audience: InternTrackClient
      AccessTokenMinutes: 15
      RefreshTokenDays: 7

The real secret key should not be committed to the repository.

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

## 7. Build the Backend

Build the solution:

    dotnet build

The build should complete without errors before the API is started.

---

## 8. Run the Backend

Start the API with:

    dotnet run --project InternTrack.Api

The application will use the URLs configured by ASP.NET Core.

Check the terminal output to see the active local API address.

---

## 9. Swagger

Swagger is available in the development environment.

After starting the API, open the Swagger URL displayed by the application.

Swagger can be used to:

- View available endpoints
- Inspect request and response models
- Test API requests
- Review authentication requirements

---

## 10. Run Backend Tests

Run the automated tests with:

    dotnet test

The current backend test suite contains:

    144 tests passed
    0 tests failed

The tests cover areas such as:

- Authentication
- Authorization
- Departments
- Interns
- Tasks
- Dashboard statistics
- Soft delete
- Restore operations
- Refresh tokens
- Task status transitions
- Validation scenarios

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

The frontend must communicate with the running backend API.

The API base URL should point to the local backend address.

For example:

    http://localhost:<backend-port>/api

Use the URL configured by the backend development environment.

If the frontend uses environment variables, define the API URL in the appropriate environment file.

For example:

    VITE_API_BASE_URL=http://localhost:<backend-port>/api

The exact variable name should match the frontend implementation.

---

## 4. Cookie-Based Requests

InternTrack uses HttpOnly cookies for authentication.

Frontend API requests must support credentials when communicating with the backend.

Axios requests should be configured to send cookies when required.

This is important for:

- Login
- Authenticated API requests
- Refresh token operations
- Logout

The backend CORS configuration must also allow the frontend development origin.

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

---

# Recommended Local Startup Order

When running the full application locally, use the following order:

    1. Configure backend settings
    2. Apply database migrations
    3. Start the backend API
    4. Confirm Swagger is available
    5. Configure the frontend API URL
    6. Start the frontend
    7. Open the frontend application
    8. Test authentication and application operations

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

For example:

    http://localhost:5173

The exact frontend port may vary.

Production origins should be configured separately from local development origins.

---

# Authentication Configuration

InternTrack uses:

- JWT access tokens
- Refresh tokens
- HttpOnly cookies
- Refresh token rotation
- Refresh token revocation

Access token configuration includes:

    Jwt:AccessTokenMinutes

Refresh token configuration includes:

    Jwt:RefreshTokenDays

The refresh token is sent only to authentication-related backend routes because of its configured cookie path.

---

# Development Environment

In development mode:

- Swagger is available.
- Secure cookies can be disabled for local HTTP development.
- Local frontend origins can be allowed through CORS.

Outside development:

- Secure cookies should be enabled.
- HTTPS should be used.
- Production CORS origins should be configured.
- Sensitive secrets should be provided through secure configuration sources.

---

# Verification Checklist

Before considering the local setup complete, verify the following:

- Backend dependencies restored successfully
- Database configuration is valid
- Database migrations applied successfully
- Backend build succeeds
- Backend API starts successfully
- Swagger opens successfully
- Backend tests pass
- Frontend dependencies installed successfully
- Frontend API URL points to the correct backend
- Frontend development server starts successfully
- TypeScript checks pass
- ESLint checks pass
- Frontend production build succeeds
- Login works
- Authenticated requests work
- Refresh flow works
- Logout works

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

---

## Database Errors

Check:

- SQLite database configuration
- Existing migrations
- Entity Framework CLI installation

Try:

    dotnet ef database update --project InternTrack.DataAccess --startup-project InternTrack.Api

---

## Frontend Cannot Reach Backend

Check:

- Backend is running
- Frontend API base URL is correct
- Backend CORS configuration allows the frontend origin
- Browser requests include credentials where required
- Backend port matches frontend configuration

---

## Authentication Does Not Work

Check:

- Backend API is running
- Cookie credentials are enabled
- CORS settings are correct
- JWT settings are configured
- Access and refresh token lifetimes are valid
- Browser cookies are not blocked

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

The backend provides the API, authentication, business rules, persistence, and automated tests.

The frontend provides the user interface, routing, authentication state, and API communication.

Both applications must be configured correctly for the complete InternTrack system to work.