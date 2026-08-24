using InternTrack.Business.Interfaces;
using InternTrack.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/interns")]
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
        var interns = await _service.GetAllAsync();

        return Ok(interns);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var intern = await _service.GetByIdAsync(id);

        if (intern == null)
        {
            return NotFound(new
            {
                message = "Stajyer bulunamadı."
            });
        }

        return Ok(intern);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateInternDto dto)
    {
        var result = await _service.AddAsync(dto);

        if (result == "Departman bulunamadı.")
        {
            return BadRequest(new
            {
                message = result
            });
        }

        if (result == "Bu email adresi zaten kayıtlı.")
        {
            return BadRequest(new
            {
                message = result
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = result
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
                message = "Stajyer bulunamadı."
            });
        }

        return NoContent();
    }
}