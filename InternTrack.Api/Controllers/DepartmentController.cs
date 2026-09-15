using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Produces("application/json")]
public class DepartmentController :
    ControllerBase
{
    private readonly IDepartmentService
        _service;

    public DepartmentController(
        IDepartmentService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(
        typeof(List<Department>),
        StatusCodes.Status200OK
    )]
    public async Task<IActionResult> GetAll()
    {
        var departments =
            await _service.GetAllAsync();

        return Ok(
            departments
        );
    }

    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(Department),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> GetById(
        [FromRoute] int id)
    {
        var result =
            await _service.GetByIdAsync(
                id
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result
            );
    }

    [Authorize(Roles = "Admin,HR")]
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
        StatusCodes.Status409Conflict
    )]
    public async Task<IActionResult> Add(
        [FromBody] CreateDepartmentDto dto)
    {
        var result =
            await _service.AddAsync(
                dto
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result,
                StatusCodes
                    .Status201Created
            );
    }

    [Authorize(Roles = "Admin,HR")]
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
    [ProducesResponseType(
        StatusCodes.Status409Conflict
    )]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] CreateDepartmentDto dto)
    {
        var result =
            await _service.UpdateAsync(
                id,
                dto
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result
            );
    }

    [Authorize(Roles = "Admin,HR")]
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
    [ProducesResponseType(
        StatusCodes.Status409Conflict
    )]
    public async Task<IActionResult> Delete(
        [FromRoute] int id)
    {
        var result =
            await _service.DeleteAsync(
                id
            );

        return ServiceResultMapper
            .ToActionResult(
                this,
                result,
                noContentOnSuccess: true
            );
    }
}