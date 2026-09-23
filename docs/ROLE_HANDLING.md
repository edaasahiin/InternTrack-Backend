# Role Handling

Persisted roles, JWT role claims, and response role values remain strings with their original spelling.

The `Roles` constants remain the source of canonical role values used by the application and authorization attributes.

No database schema change or role normalization migration is required.

---

## Centralized Role Interpretation

Role interpretation is centralized in:

    InternTrack.Core/Helpers/RoleHelper.cs

`RoleHelper` provides:

- `IsAdminClaim`
- `IsHrClaim`
- `IsInternClaim`
- `IsInternAccountRole`

`IsAdminClaim`, `IsHrClaim`, and `IsInternClaim` preserve the existing exact, case-sensitive Business-layer comparisons.

Unknown roles, different casing, and surrounding whitespace do not match these checks.

`IsInternAccountRole` preserves the existing ordinal, case-insensitive Intern recognition used by:

- Login
- Refresh
- `/api/auth/me`

This method does not trim role values.

It only determines whether authentication requires an Intern profile.

It does not grant Business permissions.

---

## Why the Checks Differ

The claim-oriented checks and authentication-specific Intern check intentionally use different comparison behavior.

This preserves the application's existing authorization behavior.

Changing all role comparisons to case-insensitive matching could change which Business-service branches execute.

Authorization attributes, stored role values, JWT role claims, and JWT claim generation remain unchanged.

---

## Intern Profile Validation

`AuthService` and `AuthController.Me` identify authentication conditions separately.

| Condition | Meaning |
| --- | --- |
| `requiresInternProfile` | The stored role identifies an Intern account. |
| `internProfileIsMissing` | There is no linked Intern profile. |
| `internProfileIsInactive` | A linked Intern profile exists and has `IsActive = false`. |

Non-Intern users do not require an Intern profile.

Intern accounts with a missing or inactive profile are rejected using the existing authentication behavior.

The authentication flow now distinguishes clearly between:

1. A user who does not require an Intern profile.
2. An Intern user whose Intern profile is missing.
3. An Intern user whose Intern profile is inactive.
4. An Intern user whose Intern profile is active.

---

## Refresh Behavior

When refresh is rejected because an Intern account is no longer valid:

- The request is rejected.
- Existing refresh tokens for the user can be revoked according to the current AuthService flow.
- No new tokens are issued.
- Authentication cookies can be cleared by the API flow.

The existing refresh behavior is preserved.

---

## Current User Behavior

`GET /api/auth/me` performs the same Intern profile validation.

If an authenticated Intern has a missing or inactive profile:

- The request is rejected.
- Authentication cookies are cleared.

Non-Intern users are not required to have an Intern profile.

---

## Removed Helper

The previous:

    IsInactiveIntern

helper has been removed.

Its responsibilities are now expressed through clearly named conditions.

Role-related `StringComparison.OrdinalIgnoreCase` usage now appears only inside `RoleHelper`.

Case-insensitive email comparisons remain unchanged because they serve a separate purpose.

---

## Compatibility

This refactor does not change:

- Database schema
- Stored role values
- JWT claims
- Authorization attributes
- API contracts
- Existing authentication messages
- Existing authentication rejection behavior

The purpose of the refactor is to centralize role interpretation and make authentication intent easier to understand.