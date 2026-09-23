using InternTrack.Business.Common;
using InternTrack.Core.DTOs;

namespace InternTrack.Business.Interfaces;

public interface IInternService : IScopedService
{
    Task<ServiceResult<List<InternResponseDto>>> GetAllAsync(int userId, string role);

    Task<ServiceResult<List<InternResponseDto>>> GetAllIncludingInactiveAsync();

    Task<ServiceResult<InternResponseDto>> GetByIdAsync(int id, int userId, string role);

    Task<ServiceResult> CreateInternWithAccountAsync(CreateInternDto dto);

    Task<ServiceResult> UpdateAsync(int id, UpdateInternDto dto);

    Task<ServiceResult> DeactivateInternAsync(int id);

    Task<ServiceResult> ReactivateInternAsync(int id);
}
