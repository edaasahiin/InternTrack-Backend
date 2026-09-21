using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Business.Services;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace InternTrack.Tests;

public class AuthServiceTests
{
    private static IConfiguration CreateConfiguration(
        int refreshTokenDays = 7)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:RefreshTokenDays"] = refreshTokenDays.ToString()
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static User CreateUser(
        int userId = 1,
        string role = "Intern",
        bool internIsActive = true)
    {
        var user = new User
        {
            Id = userId,
            Name = "Test",
            Surname = "User",
            Email = "test@example.com",
            PasswordHash = PasswordHasher.Hash("CorrectPassword"),
            Role = role,
            MustChangePassword = false
        };

        if (role == "Intern")
        {
            var intern = new Intern
            {
                Id = 10,
                UserId = userId,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                DepartmentId = 1,
                IsActive = internIsActive,
                User = user
            };

            user.Intern = intern;
        }

        return user;
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var user = CreateUser();

        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result = await service.LoginAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Email veya şifre hatalı.", result.Message);

        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateAccessToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InactiveIntern_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var user = CreateUser(
            internIsActive: false);

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "CorrectPassword"
        };

        userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result = await service.LoginAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Hesabınız pasif durumda. Giriş yapamazsınız.",
            result.Message);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateAccessToken(It.IsAny<User>()),
            Times.Never);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateRefreshToken(),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldCreateAndPersistRefreshToken()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var user = CreateUser();

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "CorrectPassword"
        };

        userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        tokenServiceMock
            .Setup(tokenService => tokenService.CreateAccessToken(user))
            .Returns("access-token");

        tokenServiceMock
            .Setup(tokenService => tokenService.CreateRefreshToken())
            .Returns("refresh-token");

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result = await service.LoginAsync(dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);

        Assert.NotNull(result.Data);
        Assert.Equal("access-token", result.Data.AccessToken);
        Assert.Equal("refresh-token", result.Data.RefreshToken);

        Assert.Equal(user.Name, result.Data.Name);
        Assert.Equal(user.Surname, result.Data.Surname);
        Assert.Equal(user.Email, result.Data.Email);
        Assert.Equal(user.Role, result.Data.Role);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateAccessToken(user),
            Times.Once);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateRefreshToken(),
            Times.Once);

        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<RefreshToken>(
                    token =>
                        token.Token == "refresh-token" &&
                        token.UserId == user.Id &&
                        token.RevokedAt == null &&
                        token.ExpiresAt > token.CreatedAt)),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ShouldReturnConflict()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var departmentId = 1;

        var department = new Department
        {
            Id = departmentId,
            Name = "Software Development"
        };

        var dto = new RegisterDto
        {
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            Password = "123456",
            DepartmentId = departmentId
        };

        departmentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(departmentId))
            .ReturnsAsync(department);

        userRepositoryMock
            .Setup(repository => repository.EmailExistsAsync(dto.Email))
            .ReturnsAsync(true);

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result = await service.RegisterAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal(
            "Bu email adresi zaten kayıtlı.",
            result.Message);

        userRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_InvalidRefreshToken_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var refreshToken = "invalid-refresh-token";

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(refreshToken))
            .ReturnsAsync((RefreshToken?)null);

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result = await service.RefreshAsync(refreshToken);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Refresh token geçersiz.",
            result.Message);

        refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<RefreshToken>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateAccessToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var refreshTokenValue =
            "revoked-refresh-token";

        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(refreshTokenValue))
            .ReturnsAsync(storedRefreshToken);

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result =
            await service.RefreshAsync(
                refreshTokenValue);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Refresh token iptal edilmiş.",
            result.Message);

        userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<int>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<RefreshToken>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var refreshTokenValue =
            "expired-refresh-token";

        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            RevokedAt = null
        };

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(refreshTokenValue))
            .ReturnsAsync(storedRefreshToken);

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result =
            await service.RefreshAsync(
                refreshTokenValue);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Refresh token süresi dolmuş.",
            result.Message);

        userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<int>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<RefreshToken>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_InactiveIntern_ShouldRevokeAllTokensAndNotIssueNewToken()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var user = CreateUser(
            internIsActive: false);

        var refreshTokenValue =
            "refresh-token";

        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(refreshTokenValue))
            .ReturnsAsync(storedRefreshToken);

        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result =
            await service.RefreshAsync(
                refreshTokenValue);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Hesabınız pasif durumda. Oturum yenilenemez.",
            result.Message);

        refreshTokenRepositoryMock.Verify(
            repository =>
                repository.RevokeAllByUserIdAsync(user.Id),
            Times.Once);

        refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<RefreshToken>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateAccessToken(It.IsAny<User>()),
            Times.Never);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateRefreshToken(),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ShouldRevokeOldTokenAndCreateNewToken()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var user = CreateUser();

        var oldRefreshTokenValue =
            "old-refresh-token";

        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = oldRefreshTokenValue,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(oldRefreshTokenValue))
            .ReturnsAsync(storedRefreshToken);

        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        tokenServiceMock
            .Setup(tokenService => tokenService.CreateAccessToken(user))
            .Returns("new-access-token");

        tokenServiceMock
            .Setup(tokenService => tokenService.CreateRefreshToken())
            .Returns("new-refresh-token");

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result =
            await service.RefreshAsync(
                oldRefreshTokenValue);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);

        Assert.NotNull(result.Data);
        Assert.Equal(
            "new-access-token",
            result.Data.AccessToken);

        Assert.Equal(
            "new-refresh-token",
            result.Data.RefreshToken);

        Assert.NotNull(
            storedRefreshToken.RevokedAt);

        refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(storedRefreshToken),
            Times.Once);

        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<RefreshToken>(
                    token =>
                        token.Token == "new-refresh-token" &&
                        token.UserId == user.Id &&
                        token.RevokedAt == null &&
                        token.ExpiresAt > token.CreatedAt)),
            Times.Once);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateAccessToken(user),
            Times.Once);

        tokenServiceMock.Verify(
            tokenService => tokenService.CreateRefreshToken(),
            Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var userId = 1;

        var user = CreateUser(
            userId: userId);

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123"
        };

        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration());

        // ACT
        var result =
            await service.ChangePasswordAsync(
                userId,
                dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Mevcut şifre hatalı.",
            result.Message);

        userRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<User>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            repository =>
                repository.RevokeAllByUserIdAsync(It.IsAny<int>()),
            Times.Never);
    }
}