using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InternTrack.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    public async Task<ServiceResult> RegisterAsync(
        RegisterDto dto)
    {
        var department =
            await _departmentRepository.GetByIdAsync(
                dto.DepartmentId
            );

        if (department == null)
        {
            return ServiceResult.ValidationError(
                "Departman bulunamadı."
            );
        }

        var emailExists =
            await _userRepository.EmailExistsAsync(
                dto.Email
            );

        if (emailExists)
        {
            return ServiceResult.Conflict(
                "Bu email adresi zaten kayıtlı."
            );
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Surname = dto.Surname.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash =
                PasswordHasher.Hash(dto.Password),
            Role = "Intern",
            MustChangePassword = false
        };

        var intern = new Intern
        {
            Name = dto.Name.Trim(),
            Surname = dto.Surname.Trim(),
            Email = dto.Email.Trim(),
            DepartmentId = dto.DepartmentId,
            User = user
        };

        user.Intern = intern;

        await _userRepository.AddAsync(user);

        return ServiceResult.Ok(
            "Stajyer hesabı başarıyla oluşturuldu."
        );
    }

    public async Task<ServiceResult<LoginResponseDto>>
        LoginAsync(LoginDto dto)
    {
        var user =
            await _userRepository.GetByEmailAsync(
                dto.Email
            );

        if (user == null)
        {
            return ServiceResult<LoginResponseDto>
                .ValidationError(
                    "Email veya şifre hatalı."
                );
        }

        var passwordIsCorrect =
            PasswordHasher.Verify(
                dto.Password,
                user.PasswordHash
            );

        if (!passwordIsCorrect)
        {
            return ServiceResult<LoginResponseDto>
                .ValidationError(
                    "Email veya şifre hatalı."
                );
        }

        var accessToken =
            _tokenService.CreateAccessToken(user);

        var refreshTokenValue =
            _tokenService.CreateRefreshToken();

        var refreshTokenDays =
            _configuration.GetValue<int>(
                "Jwt:RefreshTokenDays"
            );

        if (refreshTokenDays <= 0)
        {
            throw new InvalidOperationException(
                "Refresh token süresi geçerli değil."
            );
        }

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt =
                DateTime.UtcNow.AddDays(
                    refreshTokenDays
                ),
            UserId = user.Id
        };

        await _refreshTokenRepository.AddAsync(
            refreshToken
        );

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            Name = user.Name,
            Surname = user.Surname,
            Avatar = user.Avatar,
            Email = user.Email,
            Role = user.Role,
            MustChangePassword =
                user.MustChangePassword
        };

        return ServiceResult<LoginResponseDto>
            .Ok(response);
    }

    public async Task<ServiceResult<LoginResponseDto>>
        RefreshAsync(string refreshToken)
    {
        var storedRefreshToken =
            await _refreshTokenRepository
                .GetByTokenAsync(
                    refreshToken
                );

        if (storedRefreshToken == null)
        {
            return ServiceResult<LoginResponseDto>
                .ValidationError(
                    "Refresh token geçersiz."
                );
        }

        if (storedRefreshToken.RevokedAt != null)
        {
            return ServiceResult<LoginResponseDto>
                .ValidationError(
                    "Refresh token iptal edilmiş."
                );
        }

        if (
            storedRefreshToken.ExpiresAt
            <= DateTime.UtcNow
        )
        {
            return ServiceResult<LoginResponseDto>
                .ValidationError(
                    "Refresh token süresi dolmuş."
                );
        }

        if (storedRefreshToken.User == null)
        {
            return ServiceResult<LoginResponseDto>
                .ValidationError(
                    "Refresh token kullanıcısı bulunamadı."
                );
        }

        storedRefreshToken.RevokedAt =
            DateTime.UtcNow;

        await _refreshTokenRepository.UpdateAsync(
            storedRefreshToken
        );

        var newAccessToken =
            _tokenService.CreateAccessToken(
                storedRefreshToken.User
            );

        var newRefreshTokenValue =
            _tokenService.CreateRefreshToken();

        var refreshTokenDays =
            _configuration.GetValue<int>(
                "Jwt:RefreshTokenDays"
            );

        if (refreshTokenDays <= 0)
        {
            throw new InvalidOperationException(
                "Refresh token süresi geçerli değil."
            );
        }

        var newRefreshToken =
            new RefreshToken
            {
                Token = newRefreshTokenValue,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt =
                    DateTime.UtcNow.AddDays(
                        refreshTokenDays
                    ),
                UserId =
                    storedRefreshToken.User.Id
            };

        await _refreshTokenRepository.AddAsync(
            newRefreshToken
        );

        var response = new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            Name =
                storedRefreshToken.User.Name,
            Surname =
                storedRefreshToken.User.Surname,
            Avatar =
                storedRefreshToken.User.Avatar,
            Email =
                storedRefreshToken.User.Email,
            Role =
                storedRefreshToken.User.Role,
            MustChangePassword =
                storedRefreshToken.User
                    .MustChangePassword
        };

        return ServiceResult<LoginResponseDto>
            .Ok(response);
    }

    public async Task<ServiceResult> LogoutAsync(
        string refreshToken)
    {
        var storedRefreshToken =
            await _refreshTokenRepository
                .GetByTokenAsync(
                    refreshToken
                );

        if (storedRefreshToken == null)
        {
            return ServiceResult.ValidationError(
                "Refresh token geçersiz."
            );
        }

        if (storedRefreshToken.RevokedAt != null)
        {
            return ServiceResult.Ok(
                "Oturum zaten sonlandırılmış."
            );
        }

        storedRefreshToken.RevokedAt =
            DateTime.UtcNow;

        await _refreshTokenRepository.UpdateAsync(
            storedRefreshToken
        );

        return ServiceResult.Ok(
            "Çıkış başarılı."
        );
    }

    public async Task<ServiceResult>
        UpdateAvatarAsync(
            int userId,
            string? avatar)
    {
        var user =
            await _userRepository.GetByIdAsync(
                userId
            );

        if (user == null)
        {
            return ServiceResult.ValidationError(
                "Kullanıcı bulunamadı."
            );
        }

        user.Avatar =
            string.IsNullOrWhiteSpace(avatar)
                ? null
                : avatar.Trim();

        await _userRepository.UpdateAsync(
            user
        );

        return ServiceResult.Ok(
            "Avatar başarıyla güncellendi."
        );
    }

    public async Task<ServiceResult>
        ChangePasswordAsync(
            int userId,
            ChangePasswordDto dto)
    {
        var user =
            await _userRepository.GetByIdAsync(
                userId
            );

        if (user == null)
        {
            return ServiceResult.ValidationError(
                "Kullanıcı bulunamadı."
            );
        }

        var currentPasswordIsCorrect =
            PasswordHasher.Verify(
                dto.CurrentPassword,
                user.PasswordHash
            );

        if (!currentPasswordIsCorrect)
        {
            return ServiceResult.ValidationError(
                "Mevcut şifre hatalı."
            );
        }

        var newPasswordIsSameAsCurrent =
            PasswordHasher.Verify(
                dto.NewPassword,
                user.PasswordHash
            );

        if (newPasswordIsSameAsCurrent)
        {
            return ServiceResult.ValidationError(
                "Yeni şifre mevcut şifreden farklı olmalıdır."
            );
        }

        user.PasswordHash =
            PasswordHasher.Hash(
                dto.NewPassword
            );

        user.MustChangePassword = false;

        await _userRepository.UpdateAsync(
            user
        );

        await _refreshTokenRepository
            .RevokeAllByUserIdAsync(
                userId
            );

        return ServiceResult.Ok(
            "Şifre başarıyla değiştirildi."
        );
    }

    public async Task<ServiceResult>
        UpdateProfileAsync(
            int userId,
            UpdateProfileDto dto)
    {
        var user =
            await _userRepository.GetByIdAsync(
                userId
            );

        if (user == null)
        {
            return ServiceResult.ValidationError(
                "Kullanıcı bulunamadı."
            );
        }

        var normalizedEmail =
            dto.Email.Trim();

        var emailChanged =
            !user.Email.Equals(
                normalizedEmail,
                StringComparison.OrdinalIgnoreCase
            );

        if (emailChanged)
        {
            var emailExists =
                await _userRepository.EmailExistsAsync(
                    normalizedEmail
                );

            if (emailExists)
            {
                return ServiceResult.Conflict(
                    "Bu email adresi başka bir kullanıcı tarafından kullanılıyor."
                );
            }
        }

        user.Name =
            dto.Name.Trim();

        user.Surname =
            dto.Surname.Trim();

        user.Email =
            normalizedEmail;

        if (user.Intern != null)
        {
            user.Intern.Name =
                dto.Name.Trim();

            user.Intern.Surname =
                dto.Surname.Trim();

            user.Intern.Email =
                normalizedEmail;
        }

        await _userRepository.UpdateAsync(
            user
        );

        return ServiceResult.Ok(
            "Profil başarıyla güncellendi."
        );
    }
}