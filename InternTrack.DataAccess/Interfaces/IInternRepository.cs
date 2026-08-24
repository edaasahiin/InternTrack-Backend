using InternTrack.Entities.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IInternRepository
{
    Task<List<Intern>> GetAllAsync();

    Task<Intern?> GetByIdAsync(int id);

    Task<bool> EmailExistsAsync(string email);

    Task<bool> ExistsByDepartmentIdAsync(int departmentId);

    Task AddAsync(Intern intern);

    Task UpdateAsync(Intern intern);

    Task DeleteAsync(Intern intern);
}