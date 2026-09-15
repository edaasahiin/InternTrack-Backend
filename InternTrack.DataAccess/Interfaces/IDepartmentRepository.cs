using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();

    Task<Department?> GetByIdAsync(int id);

    Task<bool> NameExistsAsync(
        string name,
        int? excludeId = null
    );

    Task AddAsync(
        Department department
    );

    Task UpdateAsync(
        Department department
    );

    Task DeleteAsync(
        Department department
    );
}