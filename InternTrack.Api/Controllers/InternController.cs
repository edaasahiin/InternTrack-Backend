using InternTrack.Api.Conventions;
using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.Constants;
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

    public InternController(
        IInternService service)
    {
        _service = service;
    }

    [HttpGet]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.GetInterns))]
    public async Task<IActionResult> GetAll()
    {
        if (!CurrentUserHelper.TryGetUserInfo(
                User,
                out var userId,
                out var role))
        {
            return Unauthorized(new
            {
                message = "Kullanıcı bilgileri doğrulanamadı."
            });
        }

        var result =
            await _service.GetAllAsync(
                userId,
                role);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("all")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.GetAllInterns))]
    public async Task<IActionResult> GetAllIncludingInactive()
    {
        var result =
            await _service.GetAllIncludingInactiveAsync();

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [HttpGet("{id:int}")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.GetInternById))]
    public async Task<IActionResult> GetById(
        [FromRoute] int id)
    {
        if (!CurrentUserHelper.TryGetUserInfo(
                User,
                out var userId,
                out var role))
        {
            return Unauthorized(new
            {
                message = "Kullanıcı bilgileri doğrulanamadı."
            });
        }

        var result =
            await _service.GetByIdAsync(
                id,
                userId,
                role);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [Authorize(Roles = Roles.AdminOrHR)]
    [HttpPost]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.CreateIntern))]
    public async Task<IActionResult> CreateInternWithAccount(
        [FromBody] CreateInternDto dto)
    {
        var result =
            await _service.CreateInternWithAccountAsync(dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            StatusCodes.Status201Created);
    }

    [Authorize(Roles = Roles.AdminOrHR)]
    [HttpPut("{id:int}")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.UpdateIntern))]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateInternDto dto)
    {
        var result =
            await _service.UpdateAsync(
                id,
                dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.DeactivateIntern))]
    public async Task<IActionResult> DeactivateIntern(
        [FromRoute] int id)
    {
        var result =
            await _service.DeactivateInternAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            noContentOnSuccess: true);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/restore")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.ReactivateIntern))]
    public async Task<IActionResult> ReactivateIntern(
        [FromRoute] int id)
    {
        var result =
            await _service.ReactivateInternAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }
}