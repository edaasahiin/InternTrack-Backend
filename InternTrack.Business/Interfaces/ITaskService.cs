using InternTrack.Business.Common;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;

namespace InternTrack.Business.Interfaces;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync();

    Task<ServiceResult<TaskItem>> GetByIdAsync(int id);

    Task<ServiceResult> AddAsync(CreateTaskDto dto);

    Task<ServiceResult> UpdateAsync(int id, UpdateTaskDto dto);

    Task<ServiceResult> DeleteAsync(int id);
}
