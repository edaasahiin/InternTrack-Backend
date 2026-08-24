using InternTrack.Entities.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IInternRepository
{
    Task<List<Intern>> GetAllAsync();
    Task<Intern?> GetByIdAsync(int id);
    Task AddAsync(Intern intern);
    Task UpdateAsync(Intern intern);
    Task DeleteAsync(Intern intern);
}