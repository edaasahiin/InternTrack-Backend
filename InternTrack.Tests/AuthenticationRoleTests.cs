using System.Security.Claims;
using System.Text.Json;
using InternTrack.Api.Controllers;
using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Business.Services;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace InternTrack.Tests;

public class AuthenticationRoleTests
{
    private const string Password = "Test-password-123!";

    // A null profile state means the profile is missing, not merely inactive.
    public static TheoryData<string, bool?, bool> AccountCases => new()
    {
        { "Admin", null, true },
        { "HR", null, true },
        { "Admin", false, true },
        { "HR", false, true },
        { "Intern", true, true },
        { "Intern", false, false },
        { "Intern", null, false },
        { "intern", true, true },
        { "intern", false, false },
        { "intern", null, false },
        { "INTERN", null, false },
        { "INTERN", true, true },
        { "INTERN", false, false },
        { "aDmIn", null, false },
        { "hr", null, false },
        { "Unknown", null, false },
        { "Unknown", true, false },
        { " Intern ", true, false }
    };

    [Theory]
    [MemberData(nameof(AccountCases))]
    public async Task LoginAsync_ShouldRequireSupportedRoleAndEligibleProfile(string role, bool? profileIsActive, bool allowed)
    {
        var user = CreateUser(role, profileIsActive);
        var users = new Mock<IUserRepository>();
        var tokens = CreateTokenService();
        var refreshTokens = new Mock<IRefreshTokenRepository>();
        users.Setup(repository => repository.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        var service = CreateService(users, tokens, refreshTokens);

        var result = await service.LoginAsync(new LoginDto { Email = user.Email, Password = Password });

        Assert.Equal(allowed, result.Success);
        if (allowed)
        {
            Assert.Equal(ResultType.Success, result.Type);
            Assert.NotNull(result.Data);
            Assert.Equal(role, result.Data.Role);
            Assert.Equal("access-token", result.Data.AccessToken);
            Assert.Equal("raw-refresh-token", result.Data.RefreshToken);
        }
        else
        {
            Assert.Equal(ResultType.ValidationError, result.Type);
            var expectedMessage = role is "Intern" or "intern" or "INTERN"
                ? "Hesabınız pasif durumda. Giriş yapamazsınız."
                : "Email veya şifre hatalı.";
            Assert.Equal(expectedMessage, result.Message);
            Assert.Null(result.Data);
        }

        tokens.Verify(service => service.CreateAccessToken(user), allowed ? Times.Once() : Times.Never());
        tokens.Verify(service => service.CreateRefreshToken(), allowed ? Times.Once() : Times.Never());
        refreshTokens.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), allowed ? Times.Once() : Times.Never());
        refreshTokens.Verify(repository => repository.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        refreshTokens.Verify(repository => repository.RevokeAllByUserIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(AccountCases))]
    public async Task RefreshAsync_ShouldPreserveProfileRejectionAndRevocation(string role, bool? profileIsActive, bool allowed)
    {
        var user = CreateUser(role, profileIsActive);
        var users = new Mock<IUserRepository>();
        var tokens = CreateTokenService();
        var refreshTokens = new Mock<IRefreshTokenRepository>();
        var storedToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "stored-token-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        users.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        refreshTokens.Setup(repository => repository.GetByTokenAsync("old-raw-token")).ReturnsAsync(storedToken);
        var service = CreateService(users, tokens, refreshTokens);

        var result = await service.RefreshAsync("old-raw-token");

        Assert.Equal(allowed, result.Success);
        if (allowed)
        {
            Assert.Equal(ResultType.Success, result.Type);
            Assert.NotNull(result.Data);
            Assert.Equal(role, result.Data.Role);
            Assert.Equal("access-token", result.Data.AccessToken);
            Assert.Equal("raw-refresh-token", result.Data.RefreshToken);
            Assert.NotNull(storedToken.RevokedAt);
        }
        else
        {
            Assert.Equal(ResultType.ValidationError, result.Type);
            var expectedMessage = role is "Intern" or "intern" or "INTERN"
                ? "Hesabınız pasif durumda. Oturum yenilenemez."
                : "Refresh token geçersiz.";
            Assert.Equal(expectedMessage, result.Message);
            Assert.Null(result.Data);
            Assert.Null(storedToken.RevokedAt);
        }

        Assert.Equal("stored-token-hash", storedToken.Token);
        refreshTokens.Verify(repository => repository.RevokeAllByUserIdAsync(user.Id), allowed ? Times.Never() : Times.Once());
        refreshTokens.Verify(repository => repository.UpdateAsync(storedToken), allowed ? Times.Once() : Times.Never());
        refreshTokens.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), allowed ? Times.Once() : Times.Never());
        tokens.Verify(service => service.CreateAccessToken(user), allowed ? Times.Once() : Times.Never());
        tokens.Verify(service => service.CreateRefreshToken(), allowed ? Times.Once() : Times.Never());
    }

    [Theory]
    [MemberData(nameof(AccountCases))]
    public async Task Me_ShouldPreserveProfileRejectionAndCookieClearing(string role, bool? profileIsActive, bool allowed)
    {
        var user = CreateUser(role, profileIsActive);
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }, "test"))
        };
        var environment = Mock.Of<IWebHostEnvironment>(host => host.EnvironmentName == "Development");
        var controller = new AuthController(Mock.Of<IAuthService>(), users.Object, CreateConfiguration(), environment)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        var result = await controller.Me();

        if (allowed)
        {
            var response = Assert.IsType<OkObjectResult>(result);
            var body = JsonSerializer.SerializeToElement(response.Value);
            Assert.Equal(role, body.GetProperty("role").GetString());
            Assert.Equal(0, context.Response.Headers.SetCookie.Count);
        }
        else
        {
            var response = Assert.IsType<UnauthorizedObjectResult>(result);
            var body = JsonSerializer.SerializeToElement(response.Value);
            var expectedMessage = role is "Intern" or "intern" or "INTERN"
                ? "Hesabınız pasif durumda."
                : "Kullanıcı bilgisi doğrulanamadı.";
            Assert.Equal(expectedMessage, body.GetProperty("message").GetString());
            Assert.Equal(4, context.Response.Headers.SetCookie.Count);
            Assert.All(context.Response.Headers.SetCookie, cookie => Assert.Contains("max-age=0", cookie));
        }
    }

    private static User CreateUser(string role, bool? profileIsActive)
    {
        return new User
        {
            Id = 42,
            Name = "Test",
            Email = "test@example.com",
            Role = role,
            PasswordHash = PasswordHasher.Hash(Password),
            Intern = profileIsActive.HasValue
                ? new Intern { Name = "Test", Email = "test@example.com", IsActive = profileIsActive.Value }
                : null
        };
    }

    private static Mock<ITokenService> CreateTokenService()
    {
        var tokens = new Mock<ITokenService>();
        tokens.Setup(service => service.CreateAccessToken(It.IsAny<User>())).Returns("access-token");
        tokens.Setup(service => service.CreateRefreshToken()).Returns("raw-refresh-token");
        return tokens;
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:RefreshTokenDays"] = "7"
        }).Build();
    }

    private static AuthService CreateService(
        Mock<IUserRepository> users, Mock<ITokenService> tokens, Mock<IRefreshTokenRepository> refreshTokens)
    {
        return new AuthService(users.Object, Mock.Of<IDepartmentRepository>(), tokens.Object,
            refreshTokens.Object, CreateConfiguration(), Mock.Of<IAppLogger>());
    }
}
