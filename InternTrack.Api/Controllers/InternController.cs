using InternTrack.Business.Interfaces;
using InternTrack.Entities.Models;
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
            return NotFound("Stajyer bulunamadı.");
        }

        return Ok(intern);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Intern intern)
    {
        var result = await _service.AddAsync(intern);

        if (result == "Bu email adresi zaten kayıtlı.")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound("Stajyer bulunamadı.");
        }

        return Ok("Stajyer silindi.");
    }
}