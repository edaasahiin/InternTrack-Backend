using InternTrack.Core.Helpers;

namespace InternTrack.Tests;

public class RoleHelperTests
{
    [Theory]
    [InlineData("Admin", true, false, false)]
    [InlineData("HR", false, true, false)]
    [InlineData("Intern", false, false, true)]
    [InlineData("admin", false, false, false)]
    [InlineData("hr", false, false, false)]
    [InlineData("intern", false, false, false)]
    [InlineData("INTERN", false, false, false)]
    [InlineData(" Admin ", false, false, false)]
    [InlineData("Unknown", false, false, false)]
    [InlineData("", false, false, false)]
    [InlineData(null, false, false, false)]
    public void ClaimChecks_ShouldPreserveExactRoleMatching(string? role, bool isAdmin, bool isHr, bool isIntern)
    {
        Assert.Equal(isAdmin, RoleHelper.IsAdminClaim(role));
        Assert.Equal(isHr, RoleHelper.IsHrClaim(role));
        Assert.Equal(isIntern, RoleHelper.IsInternClaim(role));
    }

    [Theory]
    [InlineData("Intern", true)]
    [InlineData("intern", true)]
    [InlineData("INTERN", true)]
    [InlineData("iNtErN", true)]
    [InlineData("Admin", false)]
    [InlineData("HR", false)]
    [InlineData(" Intern ", false)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void AccountCheck_ShouldPreserveCaseInsensitiveInternMatchingWithoutTrimming(string role, bool expected)
    {
        Assert.Equal(expected, RoleHelper.IsInternAccountRole(role));
    }
}
