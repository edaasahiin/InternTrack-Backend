using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IDepartmentRepository : IScopedRepository
{
    Task<List<Department>> GetAllAsync();

    Task<List<Department>> GetAllIncludingInactiveAsync();

    Task<Department?> GetByIdAsync(int id);

    Task<Department?> GetByIdIncludingInactiveAsync(int id);

    /// <summary>
    /// Retrieves a department by trimmed, case-insensitive name, including inactive records.
    /// Optionally excludes a department ID from the search.
    /// </summary>
    Task<Department?> GetByNameIncludingInactiveAsync(string name, int? excludeId = null);

    Task AddAsync(Department department);

    Task UpdateAsync(Department department);

    Task DeactivateDepartmentAsync(Department department);

    Task ReactivateDepartmentAsync(Department department);
}
