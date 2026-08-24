using InternTrack.Business.Common;
using InternTrack.Entities.DTOs;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Interfaces;

public interface IInternService
{
    Task<List<Intern>> GetAllAsync();

    Task<ServiceResult<Intern>> GetByIdAsync(int id);

    Task<ServiceResult> AddAsync(CreateInternDto dto);

    Task<ServiceResult> DeleteAsync(int id);
}