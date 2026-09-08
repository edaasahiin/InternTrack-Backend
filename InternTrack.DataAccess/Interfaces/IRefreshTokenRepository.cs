using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);

    Task<RefreshToken?> GetByTokenAsync(string token);

    Task UpdateAsync(RefreshToken refreshToken);

    Task RevokeAllByUserIdAsync(int userId);
}