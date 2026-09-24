using System.Security.Claims;
using InternTrack.Core.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace InternTrack.Api.Authentication;

public sealed class RoleValidationEvents : JwtBearerEvents
{
    public override Task TokenValidated(TokenValidatedContext context)
    {
        var identities = context.Principal?.Identities.ToArray();
        if (identities is not { Length: 1 } || !identities[0].IsAuthenticated)
        {
            context.Fail("A single authenticated identity is required.");
            return Task.CompletedTask;
        }

        var identity = identities[0];
        var roleClaims = identity.FindAll(identity.RoleClaimType).ToArray();
        var role = roleClaims.Length == 1 ? RoleHelper.GetCanonicalRole(roleClaims[0].Value) : null;
        if (role is null)
        {
            context.Fail("A single supported role is required.");
            return Task.CompletedTask;
        }

        // Normalize already-issued tokens too, before ASP.NET evaluates role attributes.
        var originalClaim = roleClaims[0];
        identity.RemoveClaim(originalClaim);
        identity.AddClaim(new Claim(identity.RoleClaimType, role, originalClaim.ValueType,
            originalClaim.Issuer, originalClaim.OriginalIssuer));
        return Task.CompletedTask;
    }
}
