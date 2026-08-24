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
            return NotFound(new
            {
                message = "Departman bulunamadı."
            });
        }

        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateDepartmentDto dto)
    {
        await _service.AddAsync(dto);

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Departman oluşturuldu."
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Departman bulunamadı."
            });
        }

        return NoContent();
    }
}