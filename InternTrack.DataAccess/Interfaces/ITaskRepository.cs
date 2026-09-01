using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync();

    Task<List<TaskItem>> GetByInternIdAsync(int internId);

    Task<TaskItem?> GetByIdAsync(int id);

    Task AddAsync(TaskItem task);

    Task UpdateAsync(TaskItem task);

    Task DeleteAsync(TaskItem task);
}