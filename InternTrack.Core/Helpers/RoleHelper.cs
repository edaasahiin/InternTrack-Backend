using InternTrack.Core.Constants;

namespace InternTrack.Core.Helpers;

public static class RoleHelper
{
    // Preserve existing Intern account casing support without granting management
    // permissions to noncanonical Admin/HR values. Whitespace is not ignored.
    public static string? GetCanonicalRole(string? role)
    {
        if (role is Roles.Admin or Roles.HR)
        {
            return role;
        }

        return string.Equals(role, Roles.Intern, StringComparison.OrdinalIgnoreCase)
            ? Roles.Intern
            : null;
    }

    public static bool IsKnownRole(string? role) => GetCanonicalRole(role) is not null;

    public static bool IsAdminClaim(string? role) => GetCanonicalRole(role) == Roles.Admin;

    public static bool IsHrClaim(string? role) => GetCanonicalRole(role) == Roles.HR;

    public static bool IsInternClaim(string? role) => GetCanonicalRole(role) == Roles.Intern;

    public static bool IsInternAccountRole(string? role) => IsInternClaim(role);
}
