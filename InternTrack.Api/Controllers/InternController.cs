using InternTrack.Api.Helpers;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
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
        var result = await _service.GetByIdAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result
        );
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateInternDto dto)
    {
        var result = await _service.AddAsync(dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            StatusCodes.Status201Created
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            noContentOnSuccess: true
        );
    }
}
