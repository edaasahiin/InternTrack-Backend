using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface ITaskRepository : IScopedRepository
{
    Task<List<TaskItem>> GetAllAsync();

    Task<List<TaskItem>> GetAllIncludingInactiveAsync();

    Task<List<TaskItem>> GetByInternIdAsync(int internId);

    Task<TaskItem?> GetByIdAsync(int id);

    Task<TaskItem?> GetByIdIncludingInactiveAsync(int id);

    Task AddAsync(TaskItem task);

    Task UpdateAsync(TaskItem task);

    Task DeactivateTaskAsync(TaskItem task);

    Task ReactivateTaskAsync(TaskItem task);
}
