using InternTrack.Api.Helpers;
using InternTrack.Api.Conventions;
using InternTrack.Business.Interfaces;
using InternTrack.Core.Constants;
using InternTrack.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
[Produces("application/json")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _service;

    public TaskController(
        ITaskService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
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
            await _service.GetAllAsync(
                userId,
                role);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllIncludingInactive()
    {
        var result =
            await _service.GetAllIncludingInactiveAsync();

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [HttpGet("{id:int}")]
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
                message =
                    "Kullanıcı bilgileri doğrulanamadı."
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

    [HttpPost]
    [ApiConventionMethod(typeof(InternTrackApiConventions), nameof(InternTrackApiConventions.CreateTask))]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskDto dto)
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
            await _service.AddAsync(
                dto,
                userId,
                role);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ApiConventionMethod(typeof(InternTrackApiConventions), nameof(InternTrackApiConventions.UpdateTask))]
    public async Task<IActionResult> UpdateTask(
        [FromRoute] int id,
        [FromBody] UpdateTaskDto dto)
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
            await _service.UpdateAsync(
                id,
                dto,
                userId,
                role);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateTask(
        [FromRoute] int id)
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
            await _service.DeactivateTaskAsync(
                id,
                userId,
                role);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            noContentOnSuccess: true);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/restore")]
    public async Task<IActionResult> ReactivateTask(
        [FromRoute] int id)
    {
        var result =
            await _service.ReactivateTaskAsync(
                id);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }
}
