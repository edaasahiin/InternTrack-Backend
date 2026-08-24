using InternTrack.Entities.DTOs;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Interfaces;

public interface IDepartmentService
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task AddAsync(CreateDepartmentDto dto);
    Task<bool> DeleteAsync(int id);
}