using InternTrack.Business.Common;
using InternTrack.Entities.DTOs;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Interfaces;

public interface IDepartmentService
{
    Task<List<Department>> GetAllAsync();

    Task<ServiceResult<Department>> GetByIdAsync(int id);

    Task<ServiceResult> AddAsync(CreateDepartmentDto dto);

    Task<ServiceResult> DeleteAsync(int id);
}