using InternTrack.Business.Services;
using InternTrack.Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly TaskService _service;

    public TaskController(TaskService service)
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
            return NotFound("Görev bulunamadı.");
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Add(TaskItem task)
    {
        await _service.AddAsync(task);

        return Ok(task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TaskItem updatedTask)
    {
        var updated = await _service.UpdateAsync(id, updatedTask);

        if (!updated)
        {
            return NotFound("Görev bulunamadı.");
        }

        return Ok("Görev güncellendi.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound("Görev bulunamadı.");
        }

        return Ok("Görev silindi.");
    }
}