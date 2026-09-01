using InternTrack.Business.Common;
using InternTrack.Core.DTOs;

namespace InternTrack.Business.Interfaces;

public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterDto dto);

    Task<ServiceResult<LoginResponseDto>> LoginAsync(
        LoginDto dto
    );

    Task<ServiceResult<LoginResponseDto>> RefreshAsync(
        string refreshToken
    );

    Task<ServiceResult> LogoutAsync(
        string refreshToken
    );
}