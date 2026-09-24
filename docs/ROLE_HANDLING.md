# Role Handling

Persisted roles and response role values remain strings with their original spelling. JWT role claims use the canonical effective role.

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
- `GetCanonicalRole`
- `IsKnownRole`

All checks use `GetCanonicalRole`, including the Intern account check used by:

- Login
- Refresh
- `/api/auth/me`

| Input | Effective role |
| --- | --- |
| `Admin` | `Admin` |
| `HR` | `HR` |
| `Intern`, `intern`, `INTERN`, other Intern casing variants | `Intern` |
| Other values, including `admin`, `hr`, whitespace-padded roles, empty or null values | Unsupported; rejected |

Intern matching uses ordinal, case-insensitive comparison without trimming. Privileged role values must remain exactly `Admin` or `HR`: the fix does not grant management permissions to previously noncanonical values.

---

## One Effective Role Across Authentication and Authorization

Login and refresh reject unsupported account roles before token issuance. Refresh also revokes that account's refresh tokens. `JwtTokenService` emits a canonical role claim without modifying the User entity and refuses unsupported roles.

After normal JWT signature, issuer, audience and lifetime validation, `RoleValidationEvents` requires one supported role claim and normalizes it before ASP.NET authorization runs. This also handles previously issued JWTs containing differently cased Intern roles. Missing, unsupported or multiple role claims fail authentication.

`CurrentUserHelper` uses the same canonical interpretation. Role-dependent Business entry points reject unsupported roles before accessing repositories. All Intern casing variants enter the existing ownership and task-permission branches; they never fall through to unrestricted reads.

ASP.NET authorization attributes continue to use canonical `Roles` constants unchanged. No route, schema or persisted role value changes are required.

---

## Intern Profile Validation

`AuthService` and `AuthController.Me` identify authentication conditions separately.

| Condition | Meaning |
| --- | --- |
| `requiresInternProfile` | The stored role identifies an Intern account. |
| `internProfileIsMissing` | There is no linked Intern profile. |
| `internProfileIsInactive` | A linked Intern profile exists and has `IsActive = false`. |

Supported Admin and HR accounts do not require an Intern profile.

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

Supported Admin and HR accounts are not required to have an Intern profile. An unsupported stored role is rejected and authentication cookies are cleared.

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
- Authorization attributes
- Route and response-body contracts
- Existing authentication messages
- Missing/inactive Intern profile rejection and revocation behavior

Intentional security changes: mixed-case Intern roles receive canonical JWT claims and the same ownership restrictions as `Intern`; unsupported roles no longer authenticate or fall through Business authorization checks. Existing login/refresh validation-error and `/me` unauthorized response shapes are reused.

Regression coverage is in `RoleHelperTests`, `AuthenticationRoleTests`, `RoleAuthorizationTests` and `JwtRoleTests`. It covers own/other resources, unknown roles, profile requirements, task permission restrictions, JWT validation and ASP.NET role policies.
