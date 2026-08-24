using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Entities.DTOs;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IInternRepository _internRepository;

    public TaskService(
        ITaskRepository taskRepository,
        IInternRepository internRepository)
    {
        _taskRepository = taskRepository;
        _internRepository = internRepository;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _taskRepository.GetAllAsync();
    }

    public async Task<ServiceResult<TaskItem>> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null)
        {
            return ServiceResult<TaskItem>.NotFound(
                "Görev bulunamadı."
            );
        }

        return ServiceResult<TaskItem>.Ok(task);
    }

    public async Task<ServiceResult> AddAsync(CreateTaskDto dto)
    {
        var intern = await _internRepository.GetByIdAsync(dto.InternId);

        if (intern == null)
        {
            return ServiceResult.ValidationError(
                "Stajyer bulunamadı."
            );
        }

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Status = dto.Status.Trim(),
            InternId = dto.InternId
        };

        await _taskRepository.AddAsync(task);

        return ServiceResult.Ok(
            "Görev oluşturuldu."
        );
    }

    public async Task<ServiceResult> UpdateAsync(
        int id,
        UpdateTaskDto dto)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null)
        {
            return ServiceResult.NotFound(
                "Görev bulunamadı."
            );
        }

        var intern = await _internRepository.GetByIdAsync(dto.InternId);

        if (intern == null)
        {
            return ServiceResult.ValidationError(
                "Stajyer bulunamadı."
            );
        }

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.Status = dto.Status.Trim();
        task.InternId = dto.InternId;

        await _taskRepository.UpdateAsync(task);

        return ServiceResult.Ok(
            "Görev güncellendi."
        );
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        if (task == null)
        {
            return ServiceResult.NotFound(
                "Görev bulunamadı."
            );
        }

        await _taskRepository.DeleteAsync(task);

        return ServiceResult.Ok(
            "Görev silindi."
        );
    }
}