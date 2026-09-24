# Final backend review — 2026-09-24

The original cleanup review preserved runtime business behavior, authorization attributes, routes, response mapping, persistence boundaries, and schema. Existing working-tree edits to API conventions and controllers were retained. The subsequently authorized role-casing security fix is recorded below. No commit or push was performed.

## Findings

| Severity | Finding | Outcome |
| --- | --- | --- |
| Critical, conditional on stored role values | `TaskService.GetByIdAsync` and `InternService.GetByIdAsync` enforce ownership only when `IsInternClaim(role)` matches the exact string `Intern`. Authentication accepts differently cased Intern account roles, and JWT creation preserves the stored string. A valid session with `intern` or `INTERN` can therefore skip these ownership branches. Unknown role values take the same by-ID path. | Requires a separate authorization fix and negative access tests. Not changed in this cleanup-only review. Inspect persisted role values without exposing account data; do not assume helper unit tests establish endpoint authorization safety. |
| Medium | Broad Create/Update/Deactivate API conventions overlap with exact exceptions. An actual MVC ApiExplorer regression test initially failed: `Auth.UpdateAvatar` advertised `[200,400,401,403,404,409]` instead of its intended `[200,400,401,404]`. | Fixed with five explicit exception overrides. The other 25 actions use assembly-level convention matching. |
| Medium | Refresh's convention and API documentation omitted HTTP 400, which ServiceResultMapper returns for service validation failures. | Added metadata/documentation only; runtime behavior is unchanged. |
| Medium | Generated API descriptions and runtime result mapping lacked direct regression coverage after recent controller/convention changes. | Added coverage for all 30 actions' routes, verbs, authorization attributes and documented status codes, plus generic/non-generic ServiceResult mapping. Full HTTP middleware/security integration is still a separate testing concern. |
| Medium | Refresh-token read, revocation, and replacement persistence are separate operations. Concurrent refreshes are not covered by the current mock-based rotation tests. | Retained existing sequencing. Review atomic consumption and concurrency separately. No schema/transaction changes made. |
| Medium | AuthController.Me directly reads a repository and repeats profile eligibility checks already present in login/refresh. Task update helpers remain long and order-sensitive. | Left intact: moving these checks or regrouping task mutations could change validation precedence, cookies, and side effects. Existing role/profile and task tests remain. |
| Medium | Status-only conventions and IActionResult leave many success-body schemas unspecified. | Status-code metadata is now verified; review generated OpenAPI body schemas before relying on generated clients. No new response DTOs or contract changes introduced. |
| Cleanup only | Inconsistent indentation, verbose/duplicated Arrange sections, and stale test names. | Formatted the five service test files with syntax-token preservation. Added a small Department service-construction helper and reused Auth's existing configuration helper in four tests. Renamed deactivation test descriptions and the password-change missing-user test to match its existing ValidationError assertion. |
| Cleanup only | Two Department deactivation tests duplicated existing scenarios and assertions. | Removed only the two redundant methods listed below. |
| Cleanup only | Documentation described general per-action ApiConventionMethod usage instead of assembly-level matching. Test-count snapshots needed updating after this review. | Corrected API/architecture/deployment explanations and counts in README, architecture, authentication, setup and deployment documentation. |

## Test review and changes

All 13 existing test classes were reviewed for scenario coverage, setup repetition, naming, and fragility. Existing role, logging, persistence and dependency-registration test classes were retained without assertion changes.

Removed:

- `DeactivateDepartmentAsync_DepartmentDoesNotExist_ShouldNotCheckInterns`: the retained `...ShouldReturnNotFound` test already checks the same result, message, skipped relationship query, and skipped write.
- `DeactivateDepartmentAsync_DepartmentHasInterns_ShouldNotDeleteDepartment`: the retained `...ShouldReturnConflict` test already checks the same result, message, and skipped write. Explicit `IsActive = true` in the removed fixture was equivalent to the model default.

The Auth and role-matrix tests were not merged: token issuance/order, profile eligibility, raw-token handling, role-string preservation, and cookie clearing are different observables. Repository-call assertions preventing writes or token issuance on rejected requests are meaningful security regression coverage, not disposable implementation details. The overlapping logging tests separately verify redaction and event output.

New coverage: one ApiExplorer integration test covering 30 actions, five failure-mapping cases, and one success-mapping case. No new packages, test base classes, or production architecture layers were introduced.

Test-count calculation: 238 existing cases - 2 duplicates + 7 new cases = 243.

## Other reviewed areas

- All five repositories and their interfaces keep query execution inside DataAccess. DepartmentQueries composes an internal IQueryable; it does not expose IQueryable to Business. Active/inactive filters, tracking, includes and SaveChanges boundaries remain intact. Existing email-existence and relationship queries have distinct scopes and were retained.
- Department duplicate validation is sequential application validation, not a concurrent uniqueness guarantee. SQLite non-ASCII case-folding limitations remain documented. No raw SQL was added.
- Automatic DI scanning is limited to the two intended assemblies, excludes markers/abstract/open-generic implementations, and rejects zero/multiple implementations. The application calls it once. Token generation and logging appropriately remain explicit scoped registrations. Calling the scanning extension twice would duplicate descriptors; current startup does not do so.
- Role comparisons remain centralized; persisted/JWT strings and the intentional account-role versus claim-role casing distinction are unchanged. The by-ID security concern above is about callers' fallback paths, not a newly changed role helper.
- Logging calls do not pass passwords, hashes, tokens, profile objects, or request bodies. ConsoleAppLogger serializes only scalar properties and exception type; it is not a general secret detector for arbitrary strings supplied by future callers. Its output is synchronous and best-effort; hosting owns collection/retention.
- Exception handling remains centralized, preserving the existing 500 body and trace ID. Exceptions after the response starts are rethrown and are outside its response-rewrite path.
- No DbSeeder invocation remains. Department setup is explicit. `/restore` routes and ordinary repository CRUD names in documentation are intentional, not stale internal method names.

## Validation

Final verification, with generated artifacts directed to a temporary directory:

- `dotnet build InternTrack.slnx --artifacts-path <temporary-artifacts-directory> --nologo`: succeeded, 0 warnings, 0 errors; elapsed 6.65 seconds.
- `dotnet test InternTrack.slnx --artifacts-path <temporary-artifacts-directory> --no-build --no-restore --nologo`: succeeded, 243 passed, 0 failed, 0 skipped; reported test duration 4 seconds.
- `git diff --check`: passed.

These results describe the original cleanup review. Refresh concurrency safety remains a separate follow-up.

## Follow-up: role-casing authorization fix

The conditional ownership bypass identified above has now been fixed. `RoleHelper.GetCanonicalRole` is the shared interpretation for account eligibility, JWT creation/validation, CurrentUserHelper and Business authorization. Intern casing variants resolve to `Intern`; only exact `Admin` and `HR` retain management privileges. Unsupported and whitespace-padded roles are rejected, rather than treated as unrestricted users.

Previously issued JWTs are normalized after ordinary signature/issuer/audience/lifetime validation and before ASP.NET authorization. Unsupported or ambiguous role claims fail authentication. Login, refresh and `/me` reject unsupported stored roles; refresh revokes the account's refresh tokens. Role-dependent Business operations also reject unknown roles before data access. Stored values, routes, authorization attributes and schema remain unchanged.

Regression tests reproduced the original cross-owner/unknown-role bypass before the fix. Coverage now includes canonical and mixed-case Interns, own/other resources, missing profiles, task permissions, unsupported roles, actual JWT bearer authentication and ASP.NET role policies. See [ROLE_HANDLING.md](ROLE_HANDLING.md) for the effective-role rules.
