using InternTrack.Business.Common;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;

namespace InternTrack.Business.Interfaces;

public interface IDepartmentService
{
    Task<List<Department>> GetAllAsync();

    Task<List<Department>> GetAllIncludingInactiveAsync();

    Task<ServiceResult<Department>> GetByIdAsync(int id);

    Task<ServiceResult> AddAsync(CreateDepartmentDto dto);

    Task<ServiceResult> UpdateAsync(int id, CreateDepartmentDto dto);

    Task<ServiceResult> DeleteAsync(int id);

    Task<ServiceResult> RestoreAsync(int id);
}
