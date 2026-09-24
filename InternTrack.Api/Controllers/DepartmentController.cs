using InternTrack.Api.Helpers;
using InternTrack.Api.Conventions;
using InternTrack.Business.Interfaces;
using InternTrack.Core.Constants;
using InternTrack.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Produces("application/json")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentController(
        IDepartmentService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetActiveDepartments()
    {
        var departments =
            await _service.GetAllAsync();

        return Ok(departments);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllIncludingInactive()
    {
        var departments =
            await _service.GetAllIncludingInactiveAsync();

        return Ok(departments);
    }

    [Authorize]
    [HttpGet("get-by-id/{id:int}")]
    public async Task<IActionResult> GetDepartmentById(
        [FromRoute] int id)
    {
        var result =
            await _service.GetByIdAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }

    [Authorize(Roles = Roles.AdminOrHR)]
    [HttpPost]
    public async Task<IActionResult> CreateDepartment(
        [FromBody] CreateDepartmentDto dto)
    {
        var result =
            await _service.AddAsync(dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            StatusCodes.Status201Created);
    }

    [Authorize(Roles = Roles.AdminOrHR)]
    [HttpPut("update-by-id/{id:int}")]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] CreateDepartmentDto dto)
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
    [ApiConventionMethod(typeof(InternTrackApiConventions), nameof(InternTrackApiConventions.DeactivateDepartment))]
    public async Task<IActionResult> DeactivateDepartment(
        [FromRoute] int id)
    {
        var result =
            await _service.DeactivateDepartmentAsync(
                id);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            noContentOnSuccess: true);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/restore")]
    public async Task<IActionResult> ReactivateDepartment(
        [FromRoute] int id)
    {
        var result =
            await _service.ReactivateDepartmentAsync(
                id);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }
}
