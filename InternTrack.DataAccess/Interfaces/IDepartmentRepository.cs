using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();

    Task<Department?> GetByIdAsync(int id);

    Task<bool> NameExistsAsync(string name);

    Task AddAsync(Department department);

    Task DeleteAsync(Department department);
}
