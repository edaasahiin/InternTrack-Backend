using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IInternRepository
{
    Task<List<Intern>> GetAllAsync();

    Task<List<Intern>> GetAllIncludingInactiveAsync();

    Task<Intern?> GetByIdAsync(int id);

    Task<Intern?> GetByIdIncludingInactiveAsync(int id);

    Task<Intern?> GetByUserIdAsync(int userId);

    Task<bool> EmailExistsAsync(string email);

    Task<bool> ExistsByDepartmentIdAsync(int departmentId);

    Task AddAsync(Intern intern);

    Task UpdateAsync(Intern intern);

    Task DeleteAsync(Intern intern);

    Task RestoreAsync(Intern intern);
}