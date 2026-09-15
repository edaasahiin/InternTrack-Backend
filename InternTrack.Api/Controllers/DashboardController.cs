using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(
        IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> GetStats()
    {
        if (!CurrentUserHelper.TryGetUserInfo(
            User,
            out var userId,
            out var role))
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