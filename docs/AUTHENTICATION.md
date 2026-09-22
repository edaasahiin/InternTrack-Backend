# InternTrack Authentication Flow

This document describes the authentication and session management flow used in the InternTrack backend.

InternTrack uses JWT-based authentication with short-lived access tokens and refresh tokens stored in HttpOnly cookies.

The authentication system includes:

- JWT access tokens
- Refresh tokens
- HttpOnly cookies
- Refresh token hashing
- Refresh token rotation
- Refresh token revocation
- Role-based authorization
- Password validation
- Inactive account validation
- Session renewal
- Logout handling

---

## Authentication Overview

The authentication process is based on two tokens:

- Access Token
- Refresh Token

The access token is short-lived and is used to authenticate API requests.

The refresh token has a longer lifetime and is used to renew the authenticated session when the access token expires.

Both tokens are stored in HttpOnly cookies.

This prevents frontend JavaScript from directly accessing authentication tokens.

---

## Access Token

The access token is a JWT token.

It contains information required to identify and authorize the authenticated user.

The token can include claims such as:

- User ID
- User role
- Token issuer
- Token audience
- Expiration time

The access token is stored in a cookie named:

    accessToken

Cookie properties include:

- HttpOnly: true
- SameSite: Lax
- Path: /api
- Secure: true outside development

The token lifetime is configured using:

    Jwt:AccessTokenMinutes

---

## Refresh Token

The refresh token is used to create a new authenticated session when the access token expires.

The refresh token is stored in a cookie named:

    refreshToken

Cookie properties include:

- HttpOnly: true
- SameSite: Lax
- Path: /api/auth
- Secure: true outside development

The refresh token lifetime is configured using:

    Jwt:RefreshTokenDays

Refresh tokens are stored securely in the backend.

The stored token value is protected using SHA-256 hashing.

---

## Login Flow

The login process begins when a user sends email and password information to the authentication endpoint.

Endpoint:

    POST /api/auth/login

General flow:

    User enters email and password
        |
        v
    Login request sent to AuthController
        |
        v
    AuthController calls AuthService
        |
        v
    User is retrieved by email
        |
        v
    Password is verified
        |
        v
    Account status is checked
        |
        v
    Access token is generated
        |
        v
    Refresh token is generated
        |
        v
    Refresh token is stored
        |
        v
    Tokens are written to HttpOnly cookies
        |
        v
    User information is returned

If the email or password is invalid, login is rejected.

If the user has the Intern role, the related Intern profile must exist and must be active.

Inactive Intern accounts are not allowed to log in.

---

## Login Response

After successful login, authentication tokens are written as HttpOnly cookies.

The API returns user information such as:

- Name
- Surname
- Avatar
- Email
- Role
- MustChangePassword

The frontend can use this information to initialize the current user session.

---

## Current User Flow

The frontend can request information about the authenticated user using:

    GET /api/auth/me

The request requires authentication.

General flow:

    Client sends request
        |
        v
    Access token is validated
        |
        v
    User ID is read from authenticated claims
        |
        v
    User is loaded from database
        |
        v
    Account status is validated
        |
        v
    User information is returned

If the authenticated Intern account is inactive, authentication cookies are cleared and the request is rejected.

---

## Access Token Expiration

Access tokens are intentionally short-lived.

When the access token expires, the user does not always need to log in again.

The frontend can request a new session using the refresh endpoint:

    POST /api/auth/refresh

The refresh token stored in the HttpOnly cookie is used for this process.

---

## Refresh Flow

The refresh process follows this general structure:

    Access token expires
        |
        v
    Client sends refresh request
        |
        v
    Refresh token is read from cookie
        |
        v
    Refresh token is validated
        |
        v
    Stored token record is retrieved
        |
        v
    Revocation status is checked
        |
        v
    Expiration is checked
        |
        v
    Related user is validated
        |
        v
    Intern account status is checked
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
    New HttpOnly cookies are written
        |
        v
    Session continues

---

## Refresh Token Validation

A refresh request can fail for several reasons.

Examples include:

- Refresh token is missing
- Refresh token does not exist
- Refresh token has already been revoked
- Refresh token has expired
- Related user no longer exists
- Intern profile does not exist
- Intern account is inactive

When the refresh process fails, authentication cookies can be cleared.

---

## Refresh Token Rotation

InternTrack uses refresh token rotation.

This means a refresh token is not repeatedly reused.

When a valid refresh operation occurs:

    Existing refresh token
        |
        v
    Revoked
        |
        v
    New refresh token created
        |
        v
    New token stored
        |
        v
    New cookie returned

This reduces the risk associated with long-term reuse of the same refresh token.

---

## Refresh Token Revocation

Refresh tokens can be revoked.

A revoked token cannot be used to create a new authenticated session.

Token revocation is used during operations such as:

- Refresh token rotation
- Logout
- Password changes
- Inactive account handling

This allows existing sessions to be invalidated when required.

---

## Inactive Intern Handling

Intern accounts have additional account-status validation.

An Intern user must have:

- A related Intern profile
- An active Intern profile

If an Intern profile is missing or inactive:

- Login is rejected.
- Refresh operations are rejected.
- Existing refresh tokens can be revoked.
- Existing authentication cookies can be cleared.

This prevents inactive Intern users from continuing to use an existing session.

---

## Logout Flow

Logout is performed using:

    POST /api/auth/logout

General flow:

    Client sends logout request
        |
        v
    Refresh token is read from cookie
        |
        v
    Refresh token is revoked when available
        |
        v
    Access token cookie is cleared
        |
        v
    Refresh token cookie is cleared
        |
        v
    Logout response is returned

After logout, the previous session can no longer continue normally.

---

## Password Change Flow

Authenticated users can change their passwords using:

    PUT /api/auth/change-password

General flow:

    User submits current and new password
        |
        v
    Authenticated user is identified
        |
        v
    Current password is verified
        |
        v
    New password is validated
        |
        v
    New password hash is stored
        |
        v
    MustChangePassword is set to false
        |
        v
    Existing refresh tokens are revoked

Revoking refresh tokens after a password change prevents older sessions from continuing to refresh their authentication state.

---

## MustChangePassword

The user model includes the `MustChangePassword` value.

This value can be used by the frontend to determine whether a user should be required to change their password.

After a successful password change:

    MustChangePassword = false

This value is included in authentication-related user responses.

---

## Profile Update Flow

Authenticated users can update their profile using:

    PUT /api/auth/profile

The operation can update user information such as:

- Name
- Surname
- Email

When the authenticated user has a related Intern profile, relevant information can also be synchronized with the Intern record.

Email-related validation is applied when necessary.

Duplicate email addresses are rejected.

---

## Avatar Update Flow

Authenticated users can update their avatar using:

    PUT /api/auth/avatar

The avatar value is stored with the user profile.

Null or empty avatar values can be used to remove an existing avatar where supported by the service logic.

---

## Cookie Security

InternTrack uses HttpOnly cookies for authentication tokens.

HttpOnly cookies help prevent frontend JavaScript from directly reading token values.

Cookie configuration includes:

- `HttpOnly = true`
- `SameSite = Lax`
- Secure cookies outside development
- Restricted cookie paths

Access token path:

    /api

Refresh token path:

    /api/auth

Using different paths limits where each cookie is sent.

---

## Development and Production Cookies

Cookie security behavior changes depending on the application environment.

In development:

    Secure = false

Outside development:

    Secure = true

This allows local development without HTTPS while enforcing secure cookie transmission in production environments.

---

## Role-Based Authorization

After authentication, access to API endpoints is controlled using roles.

Supported roles:

- Admin
- HR
- Intern

Examples of API authorization attributes include:

    [Authorize]

    [Authorize(Roles = Roles.Admin)]

    [Authorize(Roles = Roles.AdminOrHR)]

    [AllowAnonymous]

Authentication determines who the user is.

Authorization determines what the user is allowed to do.

---

## Authentication and Business Rules

Authentication and authorization do not rely only on frontend behavior.

The backend also applies service-level validation.

Examples include:

- Checking whether an Intern profile belongs to the authenticated user
- Preventing inactive Intern accounts from continuing sessions
- Restricting task operations based on roles
- Restricting access to records
- Validating task status transitions

This prevents direct API requests from bypassing important application rules.

---

## Authentication Error Scenarios

Common authentication-related error scenarios include:

- Invalid email
- Invalid password
- Missing user
- Missing access token
- Missing refresh token
- Expired refresh token
- Revoked refresh token
- Inactive Intern
- Missing Intern profile
- Unauthorized endpoint access
- Forbidden role-based operation

The API returns appropriate HTTP responses depending on the operation and error type.

---

## Rate Limiting

Authentication endpoints use rate limiting.

Rate-limited operations include:

- Registration
- Login
- Refresh

Policies include:

- `RegisterPolicy`
- `LoginPolicy`
- `RefreshPolicy`

When the configured request limit is exceeded, the API may return:

    HTTP 429 Too Many Requests

Rate limiting helps reduce repeated authentication requests and provides additional API protection.

---

## Authentication Security Summary

The authentication system currently includes:

- JWT access tokens
- Short-lived access token lifetime
- Refresh tokens
- Refresh token hashing
- Refresh token rotation
- Refresh token revocation
- HttpOnly cookies
- Secure cookies outside development
- Role-based authorization
- Business-level permission checks
- Inactive account validation
- Password hashing
- Session invalidation after password changes
- Rate limiting
- Configurable token lifetimes

---

## Authentication Flow Summary

The complete authentication lifecycle can be summarized as:

    Register
        |
        v
    Login
        |
        v
    Access Token + Refresh Token
        |
        v
    Authenticated API Requests
        |
        v
    Access Token Expires
        |
        v
    Refresh
        |
        v
    Token Rotation
        |
        v
    Session Continues
        |
        v
    Logout or Password Change
        |
        v
    Refresh Token Revocation
        |
        v
    Session Ends

---

## Documentation Status

The main authentication and session-management behavior of InternTrack is documented in this file.

The documented areas include:

- Login
- Current user retrieval
- Access tokens
- Refresh tokens
- Token rotation
- Token revocation
- HttpOnly cookies
- Inactive Intern handling
- Password changes
- Profile updates
- Avatar updates
- Logout
- Role-based authorization
- Rate limiting
- Authentication security