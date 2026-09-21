using System.Security.Cryptography;
using System.Text;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        // The caller supplies a raw token; the tracked entity stores only its hash after this boundary.
        refreshToken.Token = HashRawToken(refreshToken.Token);

        await _db.RefreshTokens.AddAsync(refreshToken);

        await _db.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        var tokenHash = HashRawToken(token);

        return await _db.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == tokenHash);
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        // Loaded tokens are already hashed. Updates must preserve the stored hash.
        _db.RefreshTokens.Update(refreshToken);

        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllByUserIdAsync(int userId)
    {
        var unrevokedRefreshTokens = await _db.RefreshTokens
            .Where(refreshToken => refreshToken.UserId == userId && refreshToken.RevokedAt == null)
            .ToListAsync();

        if (unrevokedRefreshTokens.Count == 0)
        {
            return;
        }

        var revokedAt = DateTime.UtcNow;

        foreach (var refreshToken in unrevokedRefreshTokens)
        {
            refreshToken.RevokedAt = revokedAt;
        }

        await _db.SaveChangesAsync();
    }

    private static string HashRawToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Refresh token boş olamaz.", nameof(token));
        }

        var tokenBytes = Encoding.UTF8.GetBytes(token);

        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}
