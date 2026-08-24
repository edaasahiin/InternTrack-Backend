using InternTrack.Entities.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task AddAsync(Department department);
    Task DeleteAsync(Department department);
}