using InternTrack.Core.Constants;

namespace InternTrack.Core.Helpers;

public static class RoleHelper
{
    // Business authorization preserves the exact casing of the existing role checks.
    public static bool IsAdminClaim(string? role) => role == Roles.Admin;

    public static bool IsHrClaim(string? role) => role == Roles.HR;

    public static bool IsInternClaim(string? role) => role == Roles.Intern;

    // Authentication historically accepts different casing for stored Intern roles.
    // Do not use this more permissive interpretation for business authorization.
    public static bool IsInternAccountRole(string role)
    {
        return role.Equals(Roles.Intern, StringComparison.OrdinalIgnoreCase);
    }
}
