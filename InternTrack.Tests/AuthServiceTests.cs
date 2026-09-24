using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Business.Services;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using InternTrack.Core.Constants;
using Moq;

namespace InternTrack.Tests;

public class AuthServiceTests
{
    private static IConfiguration CreateConfiguration(int refreshTokenDays = 7)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:RefreshTokenDays"] = refreshTokenDays.ToString()
        };
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static User CreateUser(int userId = 1, string role = "Intern", bool internIsActive = true)
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
        userRepositoryMock.Setup(repository => repository.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.LoginAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Email veya şifre hatalı.", result.Message);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InactiveIntern_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var user = CreateUser(internIsActive: false);
        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "CorrectPassword"
        };
        userRepositoryMock.Setup(repository => repository.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.LoginAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Hesabınız pasif durumda. Giriş yapamazsınız.", result.Message);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
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
        userRepositoryMock.Setup(repository => repository.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        tokenServiceMock.Setup(tokenService => tokenService.CreateAccessToken(user)).Returns("access-token");
        tokenServiceMock.Setup(tokenService => tokenService.CreateRefreshToken()).Returns("refresh-token");
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

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
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(user), Times.Once);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Once);
        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.Is<RefreshToken>(token => token.Token == "refresh-token" && token.UserId == user.Id && token.RevokedAt == null && token.ExpiresAt > token.CreatedAt)),
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
        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(department);
        userRepositoryMock.Setup(repository => repository.EmailExistsAsync(dto.Email)).ReturnsAsync(true);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RegisterAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.Conflict, result.Type);
        Assert.Equal("Bu email adresi zaten kayıtlı.", result.Message);
        userRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
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
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshToken)).ReturnsAsync((RefreshToken? )null);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RefreshAsync(refreshToken);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Refresh token geçersiz.", result.Message);
        refreshTokenRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var refreshTokenValue = "revoked-refresh-token";
        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshTokenValue)).ReturnsAsync(storedRefreshToken);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RefreshAsync(refreshTokenValue);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Refresh token iptal edilmiş.", result.Message);
        userRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<int>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var refreshTokenValue = "expired-refresh-token";
        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            RevokedAt = null
        };
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshTokenValue)).ReturnsAsync(storedRefreshToken);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RefreshAsync(refreshTokenValue);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Refresh token süresi dolmuş.", result.Message);
        userRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<int>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_InactiveIntern_ShouldRevokeAllTokensAndNotIssueNewToken()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var user = CreateUser(internIsActive: false);
        var refreshTokenValue = "refresh-token";
        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshTokenValue)).ReturnsAsync(storedRefreshToken);
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RefreshAsync(refreshTokenValue);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Hesabınız pasif durumda. Oturum yenilenemez.", result.Message);
        refreshTokenRepositoryMock.Verify(repository => repository.RevokeAllByUserIdAsync(user.Id), Times.Once);
        refreshTokenRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Never);
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
        var oldRefreshTokenValue = "old-refresh-token";
        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = oldRefreshTokenValue,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(oldRefreshTokenValue)).ReturnsAsync(storedRefreshToken);
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        tokenServiceMock.Setup(tokenService => tokenService.CreateAccessToken(user)).Returns("new-access-token");
        tokenServiceMock.Setup(tokenService => tokenService.CreateRefreshToken()).Returns("new-refresh-token");
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RefreshAsync(oldRefreshTokenValue);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.NotNull(result.Data);
        Assert.Equal("new-access-token", result.Data.AccessToken);
        Assert.Equal("new-refresh-token", result.Data.RefreshToken);
        Assert.NotNull(storedRefreshToken.RevokedAt);
        refreshTokenRepositoryMock.Verify(repository => repository.UpdateAsync(storedRefreshToken), Times.Once);
        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.Is<RefreshToken>(token => token.Token == "new-refresh-token" && token.UserId == user.Id && token.RevokedAt == null && token.ExpiresAt > token.CreatedAt)),
            Times.Once);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(user), Times.Once);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Once);
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
        var user = CreateUser(userId: userId);
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123"
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.ChangePasswordAsync(userId, dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Mevcut şifre hatalı.", result.Message);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<User>()), Times.Never);
        refreshTokenRepositoryMock.Verify(
            repository => repository.RevokeAllByUserIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 7)]
    [InlineData(true, 7)]
    public async Task TokenIssuance_ShouldPreserveOperationOrderAndReturnRawToken(bool isRefresh, int lifetimeDays)
    {
        var calls = new List<string>();
        var userRepository = new Mock<IUserRepository>();
        var departmentRepository = new Mock<IDepartmentRepository>();
        var tokenService = new Mock<ITokenService>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var configuration = new Mock<IConfiguration>();
        var settings = CreateConfiguration(lifetimeDays);
        var user = CreateUser();
        user.Avatar = "avatar";
        user.MustChangePassword = true;
        var storedToken = new RefreshToken
        {
            Token = "stored-old-hash",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        userRepository.Setup(repository => repository.GetByEmailAsync(user.Email)).Callback(() => calls.Add("email lookup")).ReturnsAsync(user);
        refreshTokenRepository.Setup(repository => repository.GetByTokenAsync("old-raw-token")).Callback(() => calls.Add("token lookup")).ReturnsAsync(storedToken);
        userRepository.Setup(repository => repository.GetByIdAsync(user.Id)).Callback(() => calls.Add("user lookup")).ReturnsAsync(user);
        refreshTokenRepository.Setup(repository => repository.UpdateAsync(storedToken)).Callback(() =>
        {
            Assert.NotNull(storedToken.RevokedAt);
            Assert.Equal("stored-old-hash", storedToken.Token);
            calls.Add("revoke old token");
        }).Returns(Task.CompletedTask);
        tokenService.Setup(service => service.CreateAccessToken(user)).Callback(() => calls.Add("access token")).Returns("new-access-token");
        tokenService.Setup(service => service.CreateRefreshToken()).Callback(() => calls.Add("refresh token")).Returns("new-raw-token");
        configuration.Setup(config => config.GetSection("Jwt:RefreshTokenDays")).Callback(() => calls.Add("lifetime validation")).Returns(settings.GetSection("Jwt:RefreshTokenDays"));
        refreshTokenRepository.Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>())).Callback<RefreshToken>(token =>
        {
            Assert.Equal("new-raw-token", token.Token);
            Assert.Equal(user.Id, token.UserId);
            Assert.Null(token.RevokedAt);
            Assert.InRange((token.ExpiresAt - token.CreatedAt).TotalDays, lifetimeDays, lifetimeDays + 0.01);
            token.Token = "stored-new-hash";
            calls.Add("persist new token");
        }).Returns(Task.CompletedTask);
        var service = new AuthService(
            userRepository.Object,
            departmentRepository.Object,
            tokenService.Object,
            refreshTokenRepository.Object,
            configuration.Object,
            logger: Mock.Of<IAppLogger>());
        Task<ServiceResult<LoginResponseDto>> IssueTokens() => isRefresh ? service.RefreshAsync("old-raw-token") : service.LoginAsync(new LoginDto { Email = user.Email, Password = "CorrectPassword" });
        if (lifetimeDays == 0)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(IssueTokens);
            Assert.Equal("Refresh token süresi geçerli değil.", exception.Message);
            refreshTokenRepository.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
        }
        else
        {
            var result = await IssueTokens();
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("new-raw-token", result.Data.RefreshToken);
            Assert.Equal("new-access-token", result.Data.AccessToken);
            Assert.Equal(user.Name, result.Data.Name);
            Assert.Equal(user.Surname, result.Data.Surname);
            Assert.Equal(user.Email, result.Data.Email);
            Assert.Equal(user.Role, result.Data.Role);
            Assert.Equal(user.Avatar, result.Data.Avatar);
            Assert.True(result.Data.MustChangePassword);
        }

        var expectedCalls = isRefresh ? new List<string>
        {
            "token lookup",
            "user lookup",
            "revoke old token"
        }

        : new List<string>
        {
            "email lookup"
        };
        expectedCalls.AddRange(new[] { "access token", "refresh token", "lifetime validation" });
        if (lifetimeDays > 0)
        {
            expectedCalls.Add("persist new token");
        }

        Assert.Equal(expectedCalls, calls);
        refreshTokenRepository.Verify(repository => repository.RevokeAllByUserIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_UserDoesNotExist_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var dto = new LoginDto
        {
            Email = "notfound@example.com",
            Password = "SomePassword123"
        };
        userRepositoryMock.Setup(repository => repository.GetByEmailAsync(dto.Email)).ReturnsAsync((User? )null);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.LoginAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidCurrentPassword_ShouldUpdatePasswordAndRevokeTokens()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId: userId);
        var oldPasswordHash = user.PasswordHash;
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "CorrectPassword",
            NewPassword = "NewPassword123"
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.ChangePasswordAsync(userId, dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.NotEqual(oldPasswordHash, user.PasswordHash);
        Assert.True(PasswordHasher.Verify(dto.NewPassword, user.PasswordHash));
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
        refreshTokenRepositoryMock.Verify(repository => repository.RevokeAllByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DepartmentDoesNotExist_ShouldNotCreateUser()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var departmentId = 999;
        var dto = new RegisterDto
        {
            Name = "Test",
            Surname = "Intern",
            Email = "newintern@example.com",
            Password = "Password123",
            DepartmentId = departmentId
        };
        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync((Department? )null);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RegisterAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        userRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ValidData_ShouldCreateInternUserSuccessfully()
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
            Name = "Software Development",
            IsActive = true
        };
        var dto = new RegisterDto
        {
            Name = "New",
            Surname = "Intern",
            Email = "newintern@example.com",
            Password = "Password123",
            DepartmentId = departmentId
        };
        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(department);
        userRepositoryMock.Setup(repository => repository.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RegisterAsync(dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        userRepositoryMock.Verify(
            repository => repository.AddAsync(It.Is<User>(user => user.Name == dto.Name && user.Surname == dto.Surname && user.Email == dto.Email && user.Role == "Intern" && user.PasswordHash != dto.Password && user.Intern != null && user.Intern.Name == dto.Name && user.Intern.Surname == dto.Surname && user.Intern.Email == dto.Email && user.Intern.DepartmentId == departmentId)),
            Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserDoesNotExist_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 999;
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123",
            NewPassword = "NewPassword123"
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync((User? )null);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.ChangePasswordAsync(userId, dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<User>()), Times.Never);
        refreshTokenRepositoryMock.Verify(
            repository => repository.RevokeAllByUserIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_UserDoesNotExist_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var refreshTokenValue = "valid-refresh-token";
        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = 999,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshTokenValue)).ReturnsAsync(storedRefreshToken);
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(storedRefreshToken.UserId)).ReturnsAsync((User? )null);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RefreshAsync(refreshTokenValue);

        // ASSERT
        Assert.False(result.Success);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ValidRefreshToken_ShouldRevokeTokenSuccessfully()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var refreshTokenValue = "valid-refresh-token";
        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = 10,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshTokenValue)).ReturnsAsync(storedRefreshToken);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.LogoutAsync(refreshTokenValue);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Çıkış başarılı.", result.Message);
        Assert.NotNull(storedRefreshToken.RevokedAt);
        refreshTokenRepositoryMock.Verify(repository => repository.UpdateAsync(storedRefreshToken), Times.Once);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_RefreshTokenDoesNotExist_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var refreshTokenValue = "missing-refresh-token";
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshTokenValue)).ReturnsAsync((RefreshToken? )null);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.LogoutAsync(refreshTokenValue);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Refresh token geçersiz.", result.Message);
        refreshTokenRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_AlreadyRevokedToken_ShouldReturnSuccessWithoutUpdatingAgain()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var refreshTokenValue = "revoked-refresh-token";
        var storedRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = refreshTokenValue,
            UserId = 10,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync(refreshTokenValue)).ReturnsAsync(storedRefreshToken);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.LogoutAsync(refreshTokenValue);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Oturum zaten sonlandırılmış.", result.Message);
        refreshTokenRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(tokenService => tokenService.CreateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task UpdateAvatarAsync_UserDoesNotExist_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 999;
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync((User? )null);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateAvatarAsync(userId, "avatar-1");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Kullanıcı bulunamadı.", result.Message);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WhitespaceAvatar_ShouldSetAvatarToNull()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId: userId);
        user.Avatar = "old-avatar";
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateAvatarAsync(userId, "   ");

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Null(user.Avatar);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_ValidAvatar_ShouldTrimAndUpdateSuccessfully()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId: userId);
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateAvatarAsync(userId, "  avatar-2  ");

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("avatar-2", user.Avatar);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserDoesNotExist_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 999;
        var dto = new UpdateProfileDto
        {
            Name = "Updated",
            Surname = "User",
            Email = "updated@example.com"
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync((User? )null);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateProfileAsync(userId, dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Kullanıcı bulunamadı.", result.Message);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_EmailAlreadyUsed_ShouldReturnConflict()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId: userId);
        var dto = new UpdateProfileDto
        {
            Name = "Updated",
            Surname = "User",
            Email = "used@example.com"
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        userRepositoryMock.Setup(repository => repository.EmailExistsAsync(dto.Email)).ReturnsAsync(true);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateProfileAsync(userId, dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.Conflict, result.Type);
        Assert.Equal("Bu email adresi başka bir kullanıcı tarafından kullanılıyor.", result.Message);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_EmailUnchanged_ShouldSkipDuplicateCheckAndUpdateSuccessfully()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var email = "same@example.com";
        var user = CreateUser(userId: userId);
        user.Email = email;
        if (user.Intern != null)
        {
            user.Intern.Email = email;
        }

        var dto = new UpdateProfileDto
        {
            Name = "Updated",
            Surname = "User",
            Email = email
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateProfileAsync(userId, dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Updated", user.Name);
        Assert.Equal("User", user.Surname);
        Assert.Equal(email, user.Email);
        Assert.NotNull(user.Intern);
        Assert.Equal("Updated", user.Intern!.Name);
        Assert.Equal("User", user.Intern.Surname);
        Assert.Equal(email, user.Intern.Email);
        userRepositoryMock.Verify(repository => repository.EmailExistsAsync(It.IsAny<string>()), Times.Never);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ValuesWithWhitespace_ShouldTrimAndUpdateSuccessfully()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId: userId);
        user.Email = "old@example.com";
        if (user.Intern != null)
        {
            user.Intern.Email = "old@example.com";
        }

        var dto = new UpdateProfileDto
        {
            Name = "  Updated  ",
            Surname = "  User  ",
            Email = "  updated@example.com  "
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        userRepositoryMock.Setup(repository => repository.EmailExistsAsync("updated@example.com")).ReturnsAsync(false);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateProfileAsync(userId, dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Updated", user.Name);
        Assert.Equal("User", user.Surname);
        Assert.Equal("updated@example.com", user.Email);
        Assert.NotNull(user.Intern);
        Assert.Equal("Updated", user.Intern!.Name);
        Assert.Equal("User", user.Intern.Surname);
        Assert.Equal("updated@example.com", user.Intern.Email);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_NewPasswordSameAsCurrent_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId: userId);
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "CorrectPassword",
            NewPassword = "CorrectPassword"
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.ChangePasswordAsync(userId, dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Yeni şifre mevcut şifreden farklı olmalıdır.", result.Message);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<User>()), Times.Never);
        refreshTokenRepositoryMock.Verify(
            repository => repository.RevokeAllByUserIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserWithoutIntern_ShouldUpdateUserSuccessfully()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var configuration = CreateConfiguration();
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Name = "Old",
            Surname = "Name",
            Email = "old@example.com",
            PasswordHash = "dummy-hash",
            Role = Roles.Admin,
            Intern = null
        };
        var dto = new UpdateProfileDto
        {
            Name = "  Eda  ",
            Surname = "  Şahin  ",
            Email = "  eda@example.com  "
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        userRepositoryMock.Setup(repository => repository.EmailExistsAsync("eda@example.com")).ReturnsAsync(false);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            configuration,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateProfileAsync(userId, dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Eda", user.Name);
        Assert.Equal("Şahin", user.Surname);
        Assert.Equal("eda@example.com", user.Email);
        Assert.Null(user.Intern);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
        userRepositoryMock.Verify(repository => repository.EmailExistsAsync("eda@example.com"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_AdminWithoutIntern_ShouldLoginSuccessfully()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var configuration = CreateConfiguration();
        var password = "Test123!";
        var user = new User
        {
            Id = 1,
            Name = "Admin",
            Surname = "User",
            Email = "admin@example.com",
            PasswordHash = PasswordHasher.Hash(password),
            Role = Roles.Admin,
            MustChangePassword = false,
            Intern = null
        };
        var dto = new LoginDto
        {
            Email = "admin@example.com",
            Password = password
        };
        userRepositoryMock.Setup(repository => repository.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        tokenServiceMock.Setup(service => service.CreateAccessToken(user)).Returns("access-token");
        tokenServiceMock.Setup(service => service.CreateRefreshToken()).Returns("refresh-token");
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            configuration,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.LoginAsync(dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.NotNull(result.Data);
        Assert.Equal("access-token", result.Data.AccessToken);
        Assert.Equal("refresh-token", result.Data.RefreshToken);
        Assert.Equal(Roles.Admin, result.Data.Role);
        Assert.Equal("admin@example.com", result.Data.Email);
        refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.Is<RefreshToken>(token => token.UserId == user.Id && token.Token == "refresh-token")),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InternWithoutInternProfile_ShouldReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var configuration = CreateConfiguration();
        var password = "Test123!";
        var user = new User
        {
            Id = 1,
            Name = "Eda",
            Surname = "Şahin",
            Email = "eda@example.com",
            PasswordHash = PasswordHasher.Hash(password),
            Role = Roles.Intern,
            MustChangePassword = false,
            Intern = null
        };
        var dto = new LoginDto
        {
            Email = "eda@example.com",
            Password = password
        };
        userRepositoryMock.Setup(repository => repository.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            configuration,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.LoginAsync(dto);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Hesabınız pasif durumda. Giriş yapamazsınız.", result.Message);
        tokenServiceMock.Verify(service => service.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(service => service.CreateRefreshToken(), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_InternWithoutInternProfile_ShouldRevokeTokensAndReturnValidationError()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var configuration = CreateConfiguration();
        var user = new User
        {
            Id = 1,
            Name = "Eda",
            Surname = "Şahin",
            Email = "eda@example.com",
            PasswordHash = "dummy-hash",
            Role = Roles.Intern,
            Intern = null
        };
        var refreshToken = new RefreshToken
        {
            Id = 1,
            Token = "valid-refresh-token",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = null
        };
        refreshTokenRepositoryMock.Setup(repository => repository.GetByTokenAsync("valid-refresh-token")).ReturnsAsync(refreshToken);
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            configuration,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RefreshAsync("valid-refresh-token");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal("Hesabınız pasif durumda. Oturum yenilenemez.", result.Message);
        refreshTokenRepositoryMock.Verify(repository => repository.RevokeAllByUserIdAsync(user.Id), Times.Once);
        tokenServiceMock.Verify(service => service.CreateAccessToken(It.IsAny<User>()), Times.Never);
        tokenServiceMock.Verify(service => service.CreateRefreshToken(), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ValuesWithWhitespace_ShouldTrimAndCreateUserSuccessfully()
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
            Name = "Software Development",
            IsActive = true
        };
        var dto = new RegisterDto
        {
            Name = "  Eda  ",
            Surname = "  Şahin  ",
            Email = "  eda@example.com  ",
            Password = "Test123!",
            DepartmentId = departmentId
        };
        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(department);
        userRepositoryMock.Setup(repository => repository.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RegisterAsync(dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        userRepositoryMock.Verify(
            repository => repository.AddAsync(It.Is<User>(user => user.Name == "Eda" && user.Surname == "Şahin" && user.Email == "eda@example.com" && user.Role == Roles.Intern && user.Intern != null && user.Intern.Name == "Eda" && user.Intern.Surname == "Şahin" && user.Intern.Email == "eda@example.com")),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_EmailCaseOnlyChange_ShouldSkipDuplicateCheck()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId);
        user.Email = "eda@example.com";
        if (user.Intern != null)
        {
            user.Intern.Email = "eda@example.com";
        }

        var dto = new UpdateProfileDto
        {
            Name = "Eda",
            Surname = "Şahin",
            Email = "EDA@EXAMPLE.COM"
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateProfileAsync(userId, dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("EDA@EXAMPLE.COM", user.Email);
        Assert.NotNull(user.Intern);
        Assert.Equal("EDA@EXAMPLE.COM", user.Intern!.Email);
        userRepositoryMock.Verify(repository => repository.EmailExistsAsync(It.IsAny<string>()), Times.Never);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ValidData_ShouldSetMustChangePasswordFalse()
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
            Name = "Software Development",
            IsActive = true
        };
        var dto = new RegisterDto
        {
            Name = "Eda",
            Surname = "Şahin",
            Email = "eda@example.com",
            Password = "Test123!",
            DepartmentId = departmentId
        };
        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(department);
        userRepositoryMock.Setup(repository => repository.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        User? createdUser = null;
        userRepositoryMock.Setup(repository => repository.AddAsync(It.IsAny<User>())).Callback<User>(user => createdUser = user).Returns(Task.CompletedTask);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.RegisterAsync(dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.NotNull(createdUser);
        var user = createdUser!;
        Assert.Equal(Roles.Intern, user.Role);
        Assert.False(user.MustChangePassword);
        Assert.NotNull(user.Intern);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidPassword_ShouldSetMustChangePasswordFalse()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId);
        user.MustChangePassword = true;
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "CorrectPassword",
            NewPassword = "NewPassword123"
        };
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.ChangePasswordAsync(userId, dto);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.False(user.MustChangePassword);
        Assert.True(PasswordHasher.Verify(dto.NewPassword, user.PasswordHash));
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
        refreshTokenRepositoryMock.Verify(repository => repository.RevokeAllByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_NullAvatar_ShouldSetAvatarToNull()
    {
        // ARRANGE
        var userRepositoryMock = new Mock<IUserRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var userId = 1;
        var user = CreateUser(userId);
        user.Avatar = "old-avatar";
        userRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new AuthService(
            userRepositoryMock.Object,
            departmentRepositoryMock.Object,
            tokenServiceMock.Object,
            refreshTokenRepositoryMock.Object,
            CreateConfiguration(),
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.UpdateAvatarAsync(userId, null);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Null(user.Avatar);
        userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
    }
}
