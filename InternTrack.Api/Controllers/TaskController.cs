using InternTrack.Business.Interfaces;
using InternTrack.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _service;

    public TaskController(ITaskService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _service.GetAllAsync();

        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);

        if (task == null)
        {
            return NotFound(new
            {
                message = "Görev bulunamadı."
            });
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateTaskDto dto)
    {
        var result = await _service.AddAsync(dto);

        if (result == "Stajyer bulunamadı.")
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTaskDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        if (result == "Görev bulunamadı.")
        {
            return NotFound(new
            {
                message = result
            });
        }

        if (result == "Stajyer bulunamadı.")
        {
            return BadRequest(new
            {
                message = result
            });
        }

        return Ok(new
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
                message = "Görev bulunamadı."
            });
        }

        return NoContent();
    }
}