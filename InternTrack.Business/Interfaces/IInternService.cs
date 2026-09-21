using InternTrack.Business.Common;
using InternTrack.Core.DTOs;

namespace InternTrack.Business.Interfaces;

public interface IInternService
{
    Task<ServiceResult<List<InternResponseDto>>> GetAllAsync(int userId, string role);

    Task<ServiceResult<List<InternResponseDto>>> GetAllIncludingInactiveAsync();

    Task<ServiceResult<InternResponseDto>> GetByIdAsync(int id, int userId, string role);

    Task<ServiceResult> AddAsync(CreateInternDto dto);

    Task<ServiceResult> UpdateAsync(int id, UpdateInternDto dto);

    Task<ServiceResult> DeleteAsync(int id);

    Task<ServiceResult> RestoreAsync(int id);
}
