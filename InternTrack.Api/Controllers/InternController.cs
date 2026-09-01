using System.Security.Claims;
using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/interns")]
[Authorize]
public class InternController : ControllerBase
{
    private readonly IInternService _service;

    public InternController(IInternService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        var result = await _service.GetAllAsync(
            userId,
            role
        );

        return ServiceResultMapper.ToActionResult(
            this,
            result
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        var result = await _service.GetByIdAsync(
            id,
            userId,
            role
        );

        return ServiceResultMapper.ToActionResult(
            this,
            result
        );
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<IActionResult> Add(CreateInternDto dto)
    {
        var result = await _service.AddAsync(dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            StatusCodes.Status201Created
        );
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            noContentOnSuccess: true
        );
    }

    private int GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (!int.TryParse(
            userIdValue,
            out var userId))
        {
            throw new InvalidOperationException(
                "Kullanıcı kimliği token içinde bulunamadı."
            );
        }

        return userId;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirstValue(
            ClaimTypes.Role
        ) ?? string.Empty;
    }
}