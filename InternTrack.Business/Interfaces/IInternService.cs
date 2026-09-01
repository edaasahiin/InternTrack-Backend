using InternTrack.Business.Common;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;

namespace InternTrack.Business.Interfaces;

public interface IInternService
{
    Task<ServiceResult<List<Intern>>> GetAllAsync(
        int userId,
        string role
    );

    Task<ServiceResult<Intern>> GetByIdAsync(
        int id,
        int userId,
        string role
    );

    Task<ServiceResult> AddAsync(
        CreateInternDto dto
    );

    Task<ServiceResult> DeleteAsync(
        int id
    );
}