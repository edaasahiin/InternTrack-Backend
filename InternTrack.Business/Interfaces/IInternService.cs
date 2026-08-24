using InternTrack.Entities.DTOs;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Interfaces;

public interface IInternService
{
    Task<List<Intern>> GetAllAsync();
    Task<Intern?> GetByIdAsync(int id);
    Task<string> AddAsync(CreateInternDto dto);
    Task<bool> DeleteAsync(int id);
}