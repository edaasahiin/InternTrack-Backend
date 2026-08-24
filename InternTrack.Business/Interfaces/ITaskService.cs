using InternTrack.Entities.Models;

namespace InternTrack.Business.Interfaces;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task AddAsync(TaskItem task);
    Task<bool> UpdateAsync(int id, TaskItem updatedTask);
    Task<bool> DeleteAsync(int id);
}