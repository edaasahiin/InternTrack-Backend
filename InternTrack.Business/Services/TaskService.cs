using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddAsync(TaskItem task)
    {
        await _repository.AddAsync(task);
    }

    public async Task<bool> UpdateAsync(int id, TaskItem updatedTask)
    {
        var task = await _repository.GetByIdAsync(id);

        if (task == null)
        {
            return false;
        }

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.Status = updatedTask.Status;
        task.InternId = updatedTask.InternId;

        await _repository.UpdateAsync(task);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _repository.GetByIdAsync(id);

        if (task == null)
        {
            return false;
        }

        await _repository.DeleteAsync(task);

        return true;
    }
}