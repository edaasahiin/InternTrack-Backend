using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
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
    [ProducesResponseType(
        typeof(List<TaskItem>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
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
                role
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result
            );
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(TaskItem),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
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
                role
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result
            );
    }

    [HttpPost]
    [ProducesResponseType(
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> Add(
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
                role
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result,
                StatusCodes.Status201Created
            );
    }

    [HttpPut("{id:int}")]
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
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> Update(
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
                role
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result
            );
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        typeof(void),
        StatusCodes.Status204NoContent
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> Delete(
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
            await _service.DeleteAsync(
                id,
                userId,
                role
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result,
                noContentOnSuccess: true
            );
    }
}