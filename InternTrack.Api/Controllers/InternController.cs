using InternTrack.Core.Constants;
using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/interns")]
[Authorize]
[Produces("application/json")]
public class InternController : ControllerBase
{
    private readonly IInternService _service;

    public InternController(IInternService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<InternResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll()
    {
        if (!CurrentUserHelper.TryGetUserInfo(User, out var userId, out var role))
        {
            return Unauthorized(new { message = "Kullanıcı bilgileri doğrulanamadı." });
        }

        var result = await _service.GetAllAsync(userId, role);

        return ServiceResultMapper.ToActionResult(this, result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("all")]
    [ProducesResponseType(typeof(List<InternResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllIncludingInactive()
    {
        var result = await _service.GetAllIncludingInactiveAsync();

        return ServiceResultMapper.ToActionResult(this, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InternResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        if (!CurrentUserHelper.TryGetUserInfo(User, out var userId, out var role))
        {
            return Unauthorized(new { message = "Kullanıcı bilgileri doğrulanamadı." });
        }

        var result = await _service.GetByIdAsync(id, userId, role);

        return ServiceResultMapper.ToActionResult(this, result);
    }

    [Authorize(Roles = Roles.AdminOrHR)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] CreateInternDto dto)
    {
        var result = await _service.AddAsync(dto);

        return ServiceResultMapper.ToActionResult(this, result, StatusCodes.Status201Created);
    }

    [Authorize(Roles = Roles.AdminOrHR)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateInternDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        return ServiceResultMapper.ToActionResult(this, result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await _service.DeleteAsync(id);

        return ServiceResultMapper.ToActionResult(this, result, noContentOnSuccess: true);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Restore([FromRoute] int id)
    {
        var result = await _service.RestoreAsync(id);

        return ServiceResultMapper.ToActionResult(this, result);
    }
}