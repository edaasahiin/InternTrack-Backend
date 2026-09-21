using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IRefreshTokenRepository
{
    /// <summary>
    /// Hashes the raw Token value in place before storing the refresh token.
    /// </summary>
    Task AddAsync(RefreshToken refreshToken);

    /// <summary>
    /// Looks up a raw token by its hash. The returned entity contains the stored hash.
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Updates an entity whose Token value is already hashed, without hashing it again.
    /// </summary>
    Task UpdateAsync(RefreshToken refreshToken);

    Task RevokeAllByUserIdAsync(int userId);
}
