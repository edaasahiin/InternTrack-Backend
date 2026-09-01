using System.Security.Claims;
using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result =
            await _authService.RegisterAsync(dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            StatusCodes.Status201Created
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result =
            await _authService.LoginAsync(dto);

        if (!result.Success || result.Data == null)
        {
            return ServiceResultMapper.ToActionResult(
                this,
                result
            );
        }

        WriteTokenCookies(result.Data);

        return Ok(new
        {
            name = result.Data.Name,
            email = result.Data.Email,
            role = result.Data.Role
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var name =
            User.FindFirstValue(
                ClaimTypes.Name
            );

        var email =
            User.FindFirstValue(
                ClaimTypes.Email
            );

        var role =
            User.FindFirstValue(
                ClaimTypes.Role
            );

        return Ok(new
        {
            name,
            email,
            role
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken =
            Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new
            {
                message = "Refresh token bulunamadı."
            });
        }

        var result =
            await _authService.RefreshAsync(
                refreshToken
            );

        if (!result.Success || result.Data == null)
        {
            return ServiceResultMapper.ToActionResult(
                this,
                result
            );
        }

        WriteTokenCookies(result.Data);

        return Ok(new
        {
            name = result.Data.Name,
            email = result.Data.Email,
            role = result.Data.Role
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken =
            Request.Cookies["refreshToken"];

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.LogoutAsync(
                refreshToken
            );
        }

        DeleteTokenCookies();

        return Ok(new
        {
            message = "Çıkış başarılı."
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
            response.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires =
                    DateTimeOffset.UtcNow.AddMinutes(
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
                SameSite = SameSiteMode.Lax,
                Expires =
                    DateTimeOffset.UtcNow.AddDays(
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
                SameSite = SameSiteMode.Lax
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