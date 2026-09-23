using InternTrack.Api.Conventions;
using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Helpers;
using InternTrack.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _userRepository = userRepository;
        _configuration = configuration;
        _environment = environment;
    }

    [AllowAnonymous]
    [EnableRateLimiting("RegisterPolicy")]
    [HttpPost("register")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.Register))]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto)
    {
        var result =
            await _authService.RegisterAsync(dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            StatusCodes.Status201Created);
    }

    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.Login))]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        var result =
            await _authService.LoginAsync(dto);

        if (!result.Success ||
            result.Data == null)
        {
            return ServiceResultMapper.ToActionResult(
                this,
                result);
        }

        WriteTokenCookies(result.Data);

        return Ok(new
        {
            name = result.Data.Name,
            surname = result.Data.Surname,
            avatar = result.Data.Avatar,
            email = result.Data.Email,
            role = result.Data.Role,
            mustChangePassword =
                result.Data.MustChangePassword
        });
    }

    [Authorize]
    [HttpGet("me")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.CurrentUser))]
    public async Task<IActionResult> Me()
    {
        if (!CurrentUserHelper.TryGetUserId(
                User,
                out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "Kullanıcı bilgisi doğrulanamadı."
            });
        }

        var user =
            await _userRepository.GetByIdAsync(
                userId);

        if (user == null)
        {
            return Unauthorized(new
            {
                message =
                    "Kullanıcı bulunamadı."
            });
        }

        var requiresInternProfile =
            RoleHelper.IsInternAccountRole(
                user.Role);

        var internProfileIsMissing =
            user.Intern is null;

        var internProfileIsInactive =
            user.Intern is
            {
                IsActive: false
            };

        if (requiresInternProfile &&
            (internProfileIsMissing ||
             internProfileIsInactive))
        {
            DeleteTokenCookies();

            return Unauthorized(new
            {
                message =
                    "Hesabınız pasif durumda."
            });
        }

        return Ok(new
        {
            name = user.Name,
            surname = user.Surname,
            avatar = user.Avatar,
            email = user.Email,
            role = user.Role,
            mustChangePassword =
                user.MustChangePassword
        });
    }

    [Authorize]
    [HttpPut("avatar")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.UpdateAvatar))]
    public async Task<IActionResult> UpdateAvatar(
        [FromBody] UpdateAvatarDto dto)
    {
        if (!CurrentUserHelper.TryGetUserId(
                User,
                out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "Kullanıcı bilgisi doğrulanamadı."
            });
        }

        var result =
            await _authService.UpdateAvatarAsync(
                userId,
                dto.Avatar);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [Authorize]
    [HttpPut("profile")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.UpdateProfile))]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileDto dto)
    {
        if (!CurrentUserHelper.TryGetUserId(
                User,
                out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "Kullanıcı bilgisi doğrulanamadı."
            });
        }

        var result =
            await _authService.UpdateProfileAsync(
                userId,
                dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [Authorize]
    [HttpPut("change-password")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.ChangePassword))]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto)
    {
        if (!CurrentUserHelper.TryGetUserId(
                User,
                out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "Kullanıcı bilgisi doğrulanamadı."
            });
        }

        var result =
            await _authService.ChangePasswordAsync(
                userId,
                dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [AllowAnonymous]
    [EnableRateLimiting("RefreshPolicy")]
    [HttpPost("refresh")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.Refresh))]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken =
            Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(
                refreshToken))
        {
            return Unauthorized(new
            {
                message =
                    "Refresh token bulunamadı."
            });
        }

        var result =
            await _authService.RefreshAsync(
                refreshToken);

        if (!result.Success ||
            result.Data == null)
        {
            DeleteTokenCookies();

            return ServiceResultMapper.ToActionResult(
                this,
                result);
        }

        WriteTokenCookies(result.Data);

        return Ok(new
        {
            name = result.Data.Name,
            surname = result.Data.Surname,
            avatar = result.Data.Avatar,
            email = result.Data.Email,
            role = result.Data.Role,
            mustChangePassword =
                result.Data.MustChangePassword
        });
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.Logout))]
    public async Task<IActionResult> Logout()
    {
        var refreshToken =
            Request.Cookies["refreshToken"];

        if (!string.IsNullOrWhiteSpace(
                refreshToken))
        {
            await _authService.LogoutAsync(
                refreshToken);
        }

        DeleteTokenCookies();

        return Ok(new
        {
            message =
                "Çıkış başarılı."
        });
    }

    private void WriteTokenCookies(
        LoginResponseDto response)
    {
        DeleteLegacyTokenCookies();

        var accessTokenMinutes =
            _configuration.GetValue<int>(
                "Jwt:AccessTokenMinutes");

        var refreshTokenDays =
            _configuration.GetValue<int>(
                "Jwt:RefreshTokenDays");

        var secureCookie =
            !_environment.IsDevelopment();

        Response.Cookies.Append(
            "accessToken",
            response.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = secureCookie,
                SameSite = SameSiteMode.Lax,
                Path = "/api",
                Expires =
                    DateTimeOffset.UtcNow
                        .AddMinutes(
                            accessTokenMinutes)
            });

        Response.Cookies.Append(
            "refreshToken",
            response.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = secureCookie,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth",
                Expires =
                    DateTimeOffset.UtcNow
                        .AddDays(
                            refreshTokenDays)
            });
    }

    private void DeleteTokenCookies()
    {
        var secureCookie =
            !_environment.IsDevelopment();

        var expiredAt =
            DateTimeOffset.UtcNow
                .AddDays(-1);

        Response.Cookies.Append(
            "accessToken",
            string.Empty,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = secureCookie,
                SameSite = SameSiteMode.Lax,
                Path = "/api",
                Expires = expiredAt,
                MaxAge = TimeSpan.Zero
            });

        Response.Cookies.Append(
            "refreshToken",
            string.Empty,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = secureCookie,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth",
                Expires = expiredAt,
                MaxAge = TimeSpan.Zero
            });

        DeleteLegacyTokenCookies();
    }

    private void DeleteLegacyTokenCookies()
    {
        var secureCookie =
            !_environment.IsDevelopment();

        var expiredAt =
            DateTimeOffset.UtcNow
                .AddDays(-1);

        Response.Cookies.Append(
            "accessToken",
            string.Empty,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = secureCookie,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = expiredAt,
                MaxAge = TimeSpan.Zero
            });

        Response.Cookies.Append(
            "refreshToken",
            string.Empty,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = secureCookie,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = expiredAt,
                MaxAge = TimeSpan.Zero
            });
    }
}