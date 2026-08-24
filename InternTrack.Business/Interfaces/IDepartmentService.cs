using InternTrack.Entities.Models;

namespace InternTrack.Business.Interfaces;

public interface IDepartmentService
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task AddAsync(Department department);
    Task<bool> DeleteAsync(int id);
}