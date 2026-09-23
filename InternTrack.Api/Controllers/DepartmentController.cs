using InternTrack.Api.Conventions;
using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.Constants;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
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
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.GetDepartments))]
    public async Task<IActionResult> GetAll()
    {
        var departments =
            await _service.GetAllAsync();

        return Ok(departments);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("get-all")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.GetAllDepartments))]
    public async Task<IActionResult> GetAllIncludingInactive()
    {
        var departments =
            await _service.GetAllIncludingInactiveAsync();

        return Ok(departments);
    }

    [Authorize]
    [HttpGet("get-by-id/{id:int}")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.GetDepartmentById))]
    public async Task<IActionResult> GetById(
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
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.CreateDepartment))]
    public async Task<IActionResult> Add(
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
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.UpdateDepartment))]
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
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.DeactivateDepartment))]
    public async Task<IActionResult> DeactivateDepartment(
        [FromRoute] int id)
    {
        var result =
            await _service.DeactivateDepartmentAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            noContentOnSuccess: true);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/restore")]
    [ApiConventionMethod(
        typeof(InternTrackApiConventions),
        nameof(InternTrackApiConventions.ReactivateDepartment))]
    public async Task<IActionResult> ReactivateDepartment(
        [FromRoute] int id)
    {
        var result =
            await _service.ReactivateDepartmentAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result);
    }
}