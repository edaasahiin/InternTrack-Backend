using System.Security.Claims;
using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(
        IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var role =
            User.FindFirstValue(
                ClaimTypes.Role
            );

        if (
            !int.TryParse(
                userIdClaim,
                out var userId
            ) ||
            string.IsNullOrWhiteSpace(role)
        )
        {
            return Unauthorized(new
            {
                message =
                    "Kullanıcı bilgileri doğrulanamadı."
            });
        }

        var result =
            await _dashboardService.GetStatsAsync(
                userId,
                role
            );

        return ServiceResultMapper.ToActionResult(
            this,
            result
        );
    }
}