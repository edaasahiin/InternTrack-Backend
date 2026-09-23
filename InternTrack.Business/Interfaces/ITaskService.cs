using InternTrack.Business.Common;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;

namespace InternTrack.Business.Interfaces;

public interface ITaskService : IScopedService
{
    Task<ServiceResult<List<TaskItem>>> GetAllAsync(int userId, string role);

    Task<ServiceResult<List<TaskItem>>> GetAllIncludingInactiveAsync();

    Task<ServiceResult<TaskItem>> GetByIdAsync(int id, int userId, string role);

    Task<ServiceResult> AddAsync(CreateTaskDto dto, int userId, string role);

    Task<ServiceResult> UpdateAsync(int id, UpdateTaskDto dto, int userId, string role);

    Task<ServiceResult> DeactivateTaskAsync(int id, int userId, string role);

    Task<ServiceResult> ReactivateTaskAsync(int id);
}
