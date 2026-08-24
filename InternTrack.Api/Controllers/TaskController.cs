using InternTrack.Api.Helpers;
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
        var result = await _service.GetByIdAsync(id);

        return ServiceResultMapper.ToActionResult(
            this,
            result
        );
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateTaskDto dto)
    {
        var result = await _service.AddAsync(dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result,
            StatusCodes.Status201Created
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateTaskDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        return ServiceResultMapper.ToActionResult(
            this,
            result
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