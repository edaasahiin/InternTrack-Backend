using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternTrack.Api.Authentication;
using InternTrack.Api.Helpers;
using InternTrack.Core.Models;
using InternTrack.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace InternTrack.Tests;

public class JwtRoleTests
{
    private const string SigningKey = "role-regression-test-signing-key-at-least-32-bytes";
    private const string Issuer = "role-tests";
    private const string Audience = "role-test-api";

    [Theory]
    [InlineData("Intern", "Intern")]
    [InlineData("intern", "Intern")]
    [InlineData("INTERN", "Intern")]
    [InlineData("Admin", "Admin")]
    [InlineData("HR", "HR")]
    public async Task IssuedToken_ShouldUseCanonicalRoleWithoutChangingStoredRole(string storedRole, string expectedRole)
    {
        var user = new User { Id = 10, Name = "Test", Email = "test@example.com", PasswordHash = "unused", Role = storedRole };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = SigningKey, ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience, ["Jwt:AccessTokenMinutes"] = "5"
        }).Build();
        var token = new JwtTokenService(configuration).CreateAccessToken(user);
        using var services = CreateServices();

        var result = await AuthenticateAsync(services, token);

        Assert.True(result.Succeeded);
        Assert.Equal(expectedRole, result.Principal!.FindFirstValue(ClaimTypes.Role));
        Assert.Equal(storedRole, user.Role);
    }

    [Theory]
    [InlineData("Intern", "Intern")]
    [InlineData("intern", "Intern")]
    [InlineData("INTERN", "Intern")]
    [InlineData("Admin", "Admin")]
    [InlineData("HR", "HR")]
    [InlineData("Unknown", null)]
    [InlineData("admin", null)]
    [InlineData("hr", null)]
    [InlineData(" Intern ", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public async Task ExistingToken_ShouldUseSameRoleInBearerAuthenticationCurrentUserAndPolicies(string? rawRole, string? expectedRole)
    {
        using var services = CreateServices();
        var token = CreatePreviouslyIssuedToken(rawRole is null ? [] : [rawRole]);

        var result = await AuthenticateAsync(services, token);

        Assert.Equal(expectedRole is not null, result.Succeeded);
        if (expectedRole is null)
        {
            Assert.NotNull(result.Failure);
            Assert.Null(result.Principal);
            return;
        }

        var principal = result.Principal!;
        Assert.True(CurrentUserHelper.TryGetUserInfo(principal, out var userId, out var role));
        Assert.Equal(10, userId);
        Assert.Equal(expectedRole, role);
        Assert.Equal(expectedRole, Assert.Single(principal.FindAll(ClaimTypes.Role)).Value);
        var authorization = services.GetRequiredService<IAuthorizationService>();
        var policyProvider = services.GetRequiredService<IAuthorizationPolicyProvider>();
        foreach (var allowedRole in new[] { "Admin", "HR", "Intern" })
        {
            var policy = await AuthorizationPolicy.CombineAsync(policyProvider,
                new[] { new AuthorizeAttribute { Roles = allowedRole } });
            var decision = await authorization.AuthorizeAsync(principal, resource: null, policy!);
            Assert.Equal(expectedRole == allowedRole, decision.Succeeded);
        }
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("admin")]
    [InlineData("hr")]
    [InlineData(" Intern ")]
    [InlineData("")]
    public void UnsupportedRole_ShouldNotIssueTokensOrProduceCurrentUserInfo(string role)
    {
        var user = new User { Name = "Test", Email = "test@example.com", PasswordHash = "unused", Role = role };
        var tokenService = new JwtTokenService(new ConfigurationBuilder().Build());
        Assert.Throws<InvalidOperationException>(() => tokenService.CreateAccessToken(user));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "10"), new Claim(ClaimTypes.Role, role)
        }, "test"));
        Assert.False(CurrentUserHelper.TryGetUserInfo(principal, out _, out var currentRole));
        Assert.Equal(string.Empty, currentRole);
    }

    [Theory]
    [InlineData("Intern", "Admin")]
    [InlineData("Intern", "intern")]
    [InlineData("Unknown", "Admin")]
    public async Task MultipleRoleClaims_ShouldRejectAmbiguousIdentity(string firstRole, string secondRole)
    {
        using var services = CreateServices();
        var result = await AuthenticateAsync(services, CreatePreviouslyIssuedToken([firstRole, secondRole]));
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
    }

    [Fact]
    public async Task InvalidSignature_ShouldStillRejectAnOtherwiseSupportedRole()
    {
        using var services = CreateServices();
        var token = CreatePreviouslyIssuedToken(["Intern"], "different-test-signing-key-at-least-32-bytes");
        var result = await AuthenticateAsync(services, token);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Events = new RoleValidationEvents();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = Issuer,
                ValidateAudience = true, ValidAudience = Audience,
                ValidateLifetime = true, ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey))
            };
        });
        services.AddAuthorization();
        return services.BuildServiceProvider();
    }

    private static async Task<AuthenticateResult> AuthenticateAsync(ServiceProvider services, string token)
    {
        using var scope = services.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Headers.Authorization = $"Bearer {token}";
        return await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
    }

    private static string CreatePreviouslyIssuedToken(string[] roles, string signingKey = SigningKey)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "10") };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var token = new JwtSecurityToken(Issuer, Audience, claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
