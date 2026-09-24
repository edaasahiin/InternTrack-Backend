using System.Security.Claims;
using InternTrack.Core.Helpers;

namespace InternTrack.Api.Helpers;

public static class CurrentUserHelper
{
    public static bool TryGetUserId(ClaimsPrincipal user, out int userId)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId);
    }

    public static string GetRole(ClaimsPrincipal user)
    {
        return RoleHelper.GetCanonicalRole(user.FindFirstValue(ClaimTypes.Role)) ?? string.Empty;
    }

    public static bool TryGetUserInfo(ClaimsPrincipal user, out int userId, out string role)
    {
        var hasUserId = TryGetUserId(user, out userId);

        role = GetRole(user);

        return hasUserId && !string.IsNullOrWhiteSpace(role);
    }
}
