# InternTrack API Documentation

This document summarizes the main API endpoints of the InternTrack backend.

## Base URL

    /api

The API uses role-based authorization with three roles:

- Admin
- HR
- Intern

Authentication is handled using JWT access tokens and refresh tokens stored in HttpOnly cookies.

Repeated Swagger/OpenAPI response metadata is centralized through `InternTrackApiConventions`.

Runtime business result mapping is handled through `ServiceResultMapper`.

---

## Authentication Endpoints

### Register

**Endpoint:** `POST /api/auth/register`

**Access:** Anonymous

Creates a new Intern account.

Rate limiting is enabled for this endpoint.

Possible responses:

- `201 Created`
- `400 Bad Request`
- `409 Conflict`
- `429 Too Many Requests`

---

### Login

**Endpoint:** `POST /api/auth/login`

**Access:** Anonymous

Authenticates a user.

If authentication is successful, the backend writes the following tokens as HttpOnly cookies:

- `accessToken`
- `refreshToken`

Rate limiting is enabled for this endpoint.

Possible responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `429 Too Many Requests`

---

### Get Current User

**Endpoint:** `GET /api/auth/me`

**Access:** Authenticated users

Returns information about the currently authenticated user.

Returned information includes:

- Name
- Surname
- Avatar
- Email
- Role
- MustChangePassword

If the authenticated user has the Intern role, the related Intern profile must exist and must be active.

If the Intern profile is missing or inactive, authentication cookies can be cleared and the request is rejected.

Possible responses:

- `200 OK`
- `401 Unauthorized`

---

### Update Avatar

**Endpoint:** `PUT /api/auth/avatar`

**Access:** Authenticated users

Updates the avatar of the currently authenticated user.

Possible responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`

---

### Update Profile

**Endpoint:** `PUT /api/auth/profile`

**Access:** Authenticated users

Updates the profile information of the currently authenticated user.

Possible responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`
- `409 Conflict`

---

### Change Password

**Endpoint:** `PUT /api/auth/change-password`

**Access:** Authenticated users

Changes the password of the currently authenticated user.

Possible responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`

---

### Refresh Session

**Endpoint:** `POST /api/auth/refresh`

**Access:** Anonymous

Uses the `refreshToken` cookie to renew the authenticated session.

When the refresh operation succeeds:

- The existing refresh token is validated.
- The old refresh token is revoked.
- A new access token is created.
- A new refresh token is created.
- New authentication cookies are written.

The related user and Intern profile state are validated before the session is renewed.

Rate limiting is enabled for this endpoint.

Possible responses:

- `200 OK`
- `400 Bad Request` (service validation failure)
- `401 Unauthorized`
- `429 Too Many Requests`

---

### Logout

**Endpoint:** `POST /api/auth/logout`

**Access:** Anonymous

Logs the user out of the application.

If a refresh token is available, it is revoked.

Authentication cookies are then cleared.

Possible responses:

- `200 OK`

---

## Dashboard Endpoint

### Get Dashboard Statistics

**Endpoint:** `GET /api/dashboard`

**Access:** Authenticated users

Returns dashboard statistics according to the authenticated user's ID and role.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

---

## Department Endpoints

### Get Active Departments

**Endpoint:** `GET /api/departments`

**Access:** Anonymous

Returns active departments.

Possible responses:

- `200 OK`

---

### Get All Departments

**Endpoint:** `GET /api/departments/get-all`

**Access:** Admin

Returns both active and inactive departments.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`

---

### Get Department by ID

**Endpoint:** `GET /api/departments/get-by-id/{id}`

**Access:** Authenticated users

Returns a department by ID.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

---

### Create Department

**Endpoint:** `POST /api/departments`

**Access:** Admin, HR

Creates a new department.

Duplicate Department names are rejected.

The duplicate check:

- Includes inactive departments
- Ignores leading and trailing whitespace
- Normalizes case

Possible responses:

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `409 Conflict`

---

### Update Department

**Endpoint:** `PUT /api/departments/update-by-id/{id}`

**Access:** Admin, HR

Updates an existing department.

When checking for duplicate names, the current Department is excluded from its own comparison.

Possible responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`

---

### Deactivate Department

**Endpoint:** `DELETE /api/departments/{id}`

**Access:** Admin

Soft-deactivates a department.

Internal action:

    DeactivateDepartment

Service method:

    DeactivateDepartmentAsync

The record remains stored with:

    IsActive = false

Possible responses:

- `204 No Content`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`

---

### Reactivate Department

**Endpoint:** `PATCH /api/departments/{id}/restore`

**Access:** Admin

Reactivates an inactive Department.

Internal action:

    ReactivateDepartment

Service method:

    ReactivateDepartmentAsync

The external `/restore` route remains unchanged.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`

---

## Intern Endpoints

### Get Interns

**Endpoint:** `GET /api/interns`

**Access:** Authenticated users

Returns Intern records according to the authenticated user's role and authorization rules.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

---

### Get All Interns

**Endpoint:** `GET /api/interns/all`

**Access:** Admin

Returns both active and inactive Intern records.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`

---

### Get Intern by ID

**Endpoint:** `GET /api/interns/{id}`

**Access:** Authenticated users

Returns an Intern record according to role-based authorization rules.

Intern users can only access records permitted by the Business rules.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

---

### Create Intern

**Endpoint:** `POST /api/interns`

**Access:** Admin, HR

Creates a new Intern record and its linked User account.

Internal action:

    CreateInternWithAccount

Service method:

    CreateInternWithAccountAsync

Possible responses:

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `409 Conflict`

---

### Update Intern

**Endpoint:** `PUT /api/interns/{id}`

**Access:** Admin, HR

Updates an existing Intern record.

Possible responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`

---

### Deactivate Intern

**Endpoint:** `DELETE /api/interns/{id}`

**Access:** Admin

Soft-deactivates an Intern record.

Internal action:

    DeactivateIntern

Service method:

    DeactivateInternAsync

The record remains stored with:

    IsActive = false

Possible responses:

- `204 No Content`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

---

### Reactivate Intern

**Endpoint:** `PATCH /api/interns/{id}/restore`

**Access:** Admin

Reactivates an inactive Intern.

Internal action:

    ReactivateIntern

Service method:

    ReactivateInternAsync

The external `/restore` route remains unchanged.

Reactivation operations are subject to related Business rules.

For example:

- The related Department must be active before the Intern can be reactivated.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`

---

## Task Endpoints

### Get Tasks

**Endpoint:** `GET /api/tasks`

**Access:** Authenticated users

Returns tasks according to the authenticated user's ID, role, and authorization rules.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

---

### Get All Tasks

**Endpoint:** `GET /api/tasks/all`

**Access:** Admin

Returns both active and inactive tasks.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`

---

### Get Task by ID

**Endpoint:** `GET /api/tasks/{id}`

**Access:** Authenticated users

Returns a task according to authorization rules.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

---

### Create Task

**Endpoint:** `POST /api/tasks`

**Access:** Authenticated users

Creates a task according to role-based Business rules.

The authenticated user's ID and role are used when validating the operation.

Possible responses:

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

---

### Update Task

**Endpoint:** `PUT /api/tasks/{id}`

**Access:** Authenticated users

Updates a task according to Business and authorization rules.

Validation may include:

- User role
- Task ownership
- Task status transition rules
- Overdue task restrictions
- Related entity validation

Possible responses:

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

---

### Deactivate Task

**Endpoint:** `DELETE /api/tasks/{id}`

**Access:** Authenticated users

Deactivates a task according to role-based authorization rules.

Internal action:

    DeactivateTask

Service method:

    DeactivateTaskAsync

The record remains stored with:

    IsActive = false

Possible responses:

- `204 No Content`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

---

### Reactivate Task

**Endpoint:** `PATCH /api/tasks/{id}/restore`

**Access:** Admin

Reactivates an inactive task.

Internal action:

    ReactivateTask

Service method:

    ReactivateTaskAsync

The external `/restore` route remains unchanged.

Reactivation operations are subject to related Business rules.

For example:

- The assigned Intern must be active before the task can be reactivated.

Possible responses:

- `200 OK`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`

---

## API Response Conventions

Repeated response metadata is centralized through:

    InternTrackApiConventions

Assembly-level `ApiConventionType` registration in `ApiConventionRegistration.cs` applies conventions by action name. Five actions explicitly select `ApiConventionMethod` for Task creation/update, Department deactivation, and Auth avatar/profile updates, preventing broad prefix conventions from supplying the wrong response set.

This reduces repeated `ProducesResponseType` declarations while preserving Swagger/OpenAPI response documentation.

Examples of centralized response metadata include:

- Authentication responses
- Dashboard responses
- Department responses
- Intern responses
- Task responses
- Create responses
- Update responses
- Deactivation responses
- Reactivation responses

Runtime HTTP response behavior is not handled by the convention class.

Business results are mapped through:

    ServiceResultMapper

Examples:

    ValidationError -> 400 Bad Request

    NotFound -> 404 Not Found

    Forbidden -> 403 Forbidden

    Conflict -> 409 Conflict

Successful create operations can return:

    201 Created

Successful deactivation operations can return:

    204 No Content

---

## Authentication Cookies

InternTrack uses two authentication cookies.

### Access Token Cookie

- Name: `accessToken`
- Path: `/api`
- HttpOnly: `true`
- SameSite: `Lax`
- Secure: `true` outside development

The expiration time is configured using:

    Jwt:AccessTokenMinutes

### Refresh Token Cookie

- Name: `refreshToken`
- Path: `/api/auth`
- HttpOnly: `true`
- SameSite: `Lax`
- Secure: `true` outside development

The expiration time is configured using:

    Jwt:RefreshTokenDays

---

## Role Handling

Role interpretation is centralized through `RoleHelper`.

Role-related string comparisons are not scattered through Business services.

The helper includes clearly named operations for:

- Admin claim handling
- HR claim handling
- Intern claim handling
- Authentication-specific Intern account recognition

Authentication additionally distinguishes whether an Intern profile:

- Is required
- Is missing
- Is inactive
- Is active

Non-Intern users do not require an Intern profile.

---

## Rate Limiting

Rate limiting is enabled for authentication-related endpoints.

The configured policies include:

- `RegisterPolicy`
- `LoginPolicy`
- `RefreshPolicy`

When a configured request limit is exceeded, the API may return:

    HTTP 429 Too Many Requests

---

## Authorization Summary

| Operation | Anonymous | Intern | HR | Admin |
| --- | --- | --- | --- | --- |
| Register | Yes | Yes | Yes | Yes |
| Login | Yes | Yes | Yes | Yes |
| Refresh Session | Yes | Yes | Yes | Yes |
| Logout | Yes | Yes | Yes | Yes |
| Get Current User | No | Yes | Yes | Yes |
| Get Active Departments | Yes | Yes | Yes | Yes |
| Get All Departments | No | No | No | Yes |
| Create Department | No | No | Yes | Yes |
| Update Department | No | No | Yes | Yes |
| Deactivate Department | No | No | No | Yes |
| Reactivate Department | No | No | No | Yes |
| View Interns | No | Restricted | Yes | Yes |
| View All Interns | No | No | No | Yes |
| Create Intern | No | No | Yes | Yes |
| Update Intern | No | No | Yes | Yes |
| Deactivate Intern | No | No | No | Yes |
| Reactivate Intern | No | No | No | Yes |
| View Tasks | No | Restricted | Yes | Yes |
| View All Tasks | No | No | No | Yes |
| Create Task | No | Restricted | Restricted | Yes |
| Update Task | No | Restricted | Restricted | Yes |
| Deactivate Task | No | Restricted | Restricted | Yes |
| Reactivate Task | No | No | No | Yes |

`Restricted` means that access depends on additional authorization and Business rules implemented in the Business layer.

---

## API Security Notes

Frontend authorization is not considered the main security boundary of the application.

Critical authorization checks and Business rules are also implemented in the backend Business layer.

This prevents important application rules from being bypassed through direct API requests.

The backend also includes:

- JWT validation
- Role-based authorization
- Centralized role interpretation
- HttpOnly authentication cookies
- Refresh token rotation
- Refresh token revocation
- Refresh token hashing
- Rate limiting
- CORS configuration
- HTTPS redirection outside development
- Global exception handling
- Project-owned application logging
- Centralized API response metadata

Sensitive values such as passwords, hashes, access tokens, refresh tokens, and secret keys should not be written to application logs.

---

## Documentation Status

The main InternTrack API endpoints are documented in this file.

The documented areas include:

- Authentication
- Dashboard
- Departments
- Interns
- Tasks
- Role-based authorization
- Role handling
- Authentication cookies
- Rate limiting
- API response conventions
- Main API security behavior