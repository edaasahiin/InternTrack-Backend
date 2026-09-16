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
    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldReturnValidationError()
    {
        // ARRANGE

        var userRepositoryMock =
            new Mock<IUserRepository>();

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var tokenServiceMock =
            new Mock<ITokenService>();

        var refreshTokenRepositoryMock =
            new Mock<IRefreshTokenRepository>();

        var configurationMock =
            new Mock<IConfiguration>();

        var user =
            new User
            {
                Id = 1,
                Name = "Test",
                Surname = "User",
                Email = "test@example.com",
                PasswordHash =
                    PasswordHasher.Hash(
                        "CorrectPassword"
                    ),
                Role = "Intern",
                MustChangePassword = false
            };

        var dto =
            new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

        userRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    dto.Email
                ))
            .ReturnsAsync(user);

        var service =
            new AuthService(
                userRepositoryMock.Object,
                departmentRepositoryMock.Object,
                tokenServiceMock.Object,
                refreshTokenRepositoryMock.Object,
                configurationMock.Object
            );

        // ACT

        var result =
            await service.LoginAsync(
                dto
            );

        // ASSERT

        Assert.False(
            result.Success
        );

        Assert.Equal(
            ResultType.ValidationError,
            result.Type
        );

        Assert.Equal(
            "Email veya şifre hatalı.",
            result.Message
        );

        refreshTokenRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<RefreshToken>()
                ),
            Times.Never
        );

        tokenServiceMock.Verify(
            service =>
                service.CreateAccessToken(
                    It.IsAny<User>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ShouldReturnConflict()
    {
        // ARRANGE

        var userRepositoryMock =
            new Mock<IUserRepository>();

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var tokenServiceMock =
            new Mock<ITokenService>();

        var refreshTokenRepositoryMock =
            new Mock<IRefreshTokenRepository>();

        var configurationMock =
            new Mock<IConfiguration>();

        var departmentId = 1;

        var department =
            new Department
            {
                Id = departmentId,
                Name = "Software Development"
            };

        var dto =
            new RegisterDto
            {
                Name = "Test",
                Surname = "Intern",
                Email = "test@example.com",
                Password = "123456",
                DepartmentId = departmentId
            };

        departmentRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    departmentId
                ))
            .ReturnsAsync(department);

        userRepositoryMock
            .Setup(repository =>
                repository.EmailExistsAsync(
                    dto.Email
                ))
            .ReturnsAsync(true);

        var service =
            new AuthService(
                userRepositoryMock.Object,
                departmentRepositoryMock.Object,
                tokenServiceMock.Object,
                refreshTokenRepositoryMock.Object,
                configurationMock.Object
            );

        // ACT

        var result =
            await service.RegisterAsync(
                dto
            );

        // ASSERT

        Assert.False(
            result.Success
        );

        Assert.Equal(
            ResultType.Conflict,
            result.Type
        );

        Assert.Equal(
            "Bu email adresi zaten kayıtlı.",
            result.Message
        );

        userRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<User>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshAsync_InvalidRefreshToken_ShouldReturnValidationError()
    {
        // ARRANGE

        var userRepositoryMock =
            new Mock<IUserRepository>();

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var tokenServiceMock =
            new Mock<ITokenService>();

        var refreshTokenRepositoryMock =
            new Mock<IRefreshTokenRepository>();

        var configurationMock =
            new Mock<IConfiguration>();

        var refreshToken =
            "invalid-refresh-token";

        refreshTokenRepositoryMock
            .Setup(repository =>
                repository.GetByTokenAsync(
                    refreshToken
                ))
            .ReturnsAsync(
                (RefreshToken?)null
            );

        var service =
            new AuthService(
                userRepositoryMock.Object,
                departmentRepositoryMock.Object,
                tokenServiceMock.Object,
                refreshTokenRepositoryMock.Object,
                configurationMock.Object
            );

        // ACT

        var result =
            await service.RefreshAsync(
                refreshToken
            );

        // ASSERT

        Assert.False(
            result.Success
        );

        Assert.Equal(
            ResultType.ValidationError,
            result.Type
        );

        Assert.Equal(
            "Refresh token geçersiz.",
            result.Message
        );

        refreshTokenRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    It.IsAny<RefreshToken>()
                ),
            Times.Never
        );

        refreshTokenRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<RefreshToken>()
                ),
            Times.Never
        );

        tokenServiceMock.Verify(
            service =>
                service.CreateAccessToken(
                    It.IsAny<User>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ShouldReturnValidationError()
    {
        // ARRANGE

        var userRepositoryMock =
            new Mock<IUserRepository>();

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var tokenServiceMock =
            new Mock<ITokenService>();

        var refreshTokenRepositoryMock =
            new Mock<IRefreshTokenRepository>();

        var configurationMock =
            new Mock<IConfiguration>();

        var userId = 1;

        var user =
            new User
            {
                Id = userId,
                Name = "Test",
                Surname = "User",
                Email = "test@example.com",
                PasswordHash =
                    PasswordHasher.Hash(
                        "CorrectPassword"
                    ),
                Role = "Intern",
                MustChangePassword = false
            };

        var dto =
            new ChangePasswordDto
            {
                CurrentPassword =
                    "WrongPassword",

                NewPassword =
                    "NewPassword123"
            };

        userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    userId
                ))
            .ReturnsAsync(user);

        var service =
            new AuthService(
                userRepositoryMock.Object,
                departmentRepositoryMock.Object,
                tokenServiceMock.Object,
                refreshTokenRepositoryMock.Object,
                configurationMock.Object
            );

        // ACT

        var result =
            await service.ChangePasswordAsync(
                userId,
                dto
            );

        // ASSERT

        Assert.False(
            result.Success
        );

        Assert.Equal(
            ResultType.ValidationError,
            result.Type
        );

        Assert.Equal(
            "Mevcut şifre hatalı.",
            result.Message
        );

        userRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    It.IsAny<User>()
                ),
            Times.Never
        );

        refreshTokenRepositoryMock.Verify(
            repository =>
                repository.RevokeAllByUserIdAsync(
                    It.IsAny<int>()
                ),
            Times.Never
        );
    }
}