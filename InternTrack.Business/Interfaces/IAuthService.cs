using InternTrack.Business.Common;
using InternTrack.Core.DTOs;

namespace InternTrack.Business.Interfaces;

public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterDto dto);

    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto dto);
}