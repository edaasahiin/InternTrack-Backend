using InternTrack.Business.Interfaces;
using InternTrack.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentController(IDepartmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _service.GetAllAsync();
        return Ok(departments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _service.GetByIdAsync(id);

        if (department == null)
        {
            return NotFound("Departman bulunamadı.");
        }

        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateDepartmentDto dto)
    {
        await _service.AddAsync(dto);

        return Ok("Departman eklendi.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound("Departman bulunamadı.");
        }

        return Ok("Departman silindi.");
    }
}