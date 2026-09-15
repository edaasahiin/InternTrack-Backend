using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _authService = authService;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status409Conflict
    )]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto)
    {
        var result =
            await _authService.RegisterAsync(
                dto
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result,
                StatusCodes.Status201Created
            );
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        var result =
            await _authService.LoginAsync(
                dto
            );

        if (
            !result.Success ||
            result.Data == null
        )
        {
            return ServiceResultMapper
                .ToActionResult(
                    this,
                    result
                );
        }

        WriteTokenCookies(
            result.Data
        );

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
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
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
            await _userRepository
                .GetByIdAsync(
                    userId
                );

        if (user == null)
        {
            return Unauthorized(new
            {
                message =
                    "Kullanıcı bulunamadı."
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
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
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
            await _authService
                .UpdateAvatarAsync(
                    userId,
                    dto.Avatar
                );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result
            );
    }

    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        StatusCodes.Status409Conflict
    )]
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
            await _authService
                .UpdateProfileAsync(
                    userId,
                    dto
                );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result
            );
    }

    [Authorize]
    [HttpPut("change-password")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
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
            await _authService
                .ChangePasswordAsync(
                    userId,
                    dto
                );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result
            );
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken =
            Request.Cookies[
                "refreshToken"
            ];

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
            await _authService
                .RefreshAsync(
                    refreshToken
                );

        if (
            !result.Success ||
            result.Data == null
        )
        {
            return ServiceResultMapper
                .ToActionResult(
                    this,
                    result
                );
        }

        WriteTokenCookies(
            result.Data
        );

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
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    public async Task<IActionResult> Logout()
    {
        var refreshToken =
            Request.Cookies[
                "refreshToken"
            ];

        if (!string.IsNullOrWhiteSpace(
            refreshToken))
        {
            await _authService
                .LogoutAsync(
                    refreshToken
                );
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
        var accessTokenMinutes =
            _configuration.GetValue<int>(
                "Jwt:AccessTokenMinutes"
            );

        var refreshTokenDays =
            _configuration.GetValue<int>(
                "Jwt:RefreshTokenDays"
            );

        Response.Cookies.Append(
            "accessToken",
            response.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite =
                    SameSiteMode.Lax,
                Expires =
                    DateTimeOffset.UtcNow
                        .AddMinutes(
                            accessTokenMinutes
                        )
            }
        );

        Response.Cookies.Append(
            "refreshToken",
            response.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite =
                    SameSiteMode.Lax,
                Expires =
                    DateTimeOffset.UtcNow
                        .AddDays(
                            refreshTokenDays
                        )
            }
        );
    }

    private void DeleteTokenCookies()
    {
        var cookieOptions =
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite =
                    SameSiteMode.Lax
            };

        Response.Cookies.Delete(
            "accessToken",
            cookieOptions
        );

        Response.Cookies.Delete(
            "refreshToken",
            cookieOptions
        );
    }
}