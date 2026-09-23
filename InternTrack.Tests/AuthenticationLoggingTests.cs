using System.Text.Json;
using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Business.Services;
using InternTrack.Core.Constants;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Moq;

namespace InternTrack.Tests;

public class AuthenticationLoggingTests
{
    [Theory]
    [InlineData("missing", "Warning", "credentials are invalid")]
    [InlineData("wrong-password", "Warning", "credentials are invalid")]
    [InlineData("inactive", "Warning", "intern account is inactive")]
    [InlineData("success", "Information", "logged in successfully")]
    public async Task LoginAsync_ShouldLogOutcomeWithoutCredentials(string scenario, string level, string message)
    {
        using var output = new StringWriter();
        var logger = new ConsoleAppLogger(output);
        var users = new Mock<IUserRepository>();
        var tokens = new Mock<ITokenService>();
        var refreshTokens = new Mock<IRefreshTokenRepository>();
        var user = new User
        {
            Id = 42,
            Name = "Private Name",
            Surname = "Private Surname",
            Email = "private@example.com",
            PasswordHash = PasswordHasher.Hash("private-password"),
            Role = Roles.Intern,
            Intern = new Intern
            {
                Name = "Private Name",
                Email = "private@example.com",
                IsActive = scenario != "inactive"
            }
        };
        users.Setup(repository => repository.GetByEmailAsync(user.Email))
            .ReturnsAsync(scenario == "missing" ? null : user);
        tokens.Setup(service => service.CreateAccessToken(user)).Returns("private-access-token");
        tokens.Setup(service => service.CreateRefreshToken()).Returns("private-refresh-token");
        refreshTokens.Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:RefreshTokenDays"] = "7"
        }).Build();
        var service = new AuthService(users.Object, Mock.Of<IDepartmentRepository>(), tokens.Object,
            refreshTokens.Object, configuration, logger);

        var result = await service.LoginAsync(new LoginDto
        {
            Email = user.Email,
            Password = scenario == "wrong-password" ? "private-wrong-password" : "private-password"
        });

        Assert.Equal(scenario == "success", result.Success);
        var json = output.ToString();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(level, document.RootElement.GetProperty("Level").GetString());
        Assert.Contains(message, document.RootElement.GetProperty("MessageTemplate").GetString());
        Assert.DoesNotContain(user.Email, json);
        Assert.DoesNotContain(user.Name, json);
        Assert.DoesNotContain(user.Surname, json);
        Assert.DoesNotContain(user.PasswordHash, json);
        Assert.DoesNotContain("private-password", json);
        Assert.DoesNotContain("private-wrong-password", json);
        Assert.DoesNotContain("private-access-token", json);
        Assert.DoesNotContain("private-refresh-token", json);
    }
}
