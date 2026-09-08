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
        await _db.RefreshTokens.AddAsync(refreshToken);

        await _db.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(
        string token)
    {
        return await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Token == token
            );
    }

    public async Task UpdateAsync(
        RefreshToken refreshToken)
    {
        _db.RefreshTokens.Update(
            refreshToken
        );

        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllByUserIdAsync(
        int userId)
    {
        var activeRefreshTokens =
            await _db.RefreshTokens
                .Where(
                    x =>
                        x.UserId == userId &&
                        x.RevokedAt == null
                )
                .ToListAsync();

        if (activeRefreshTokens.Count == 0)
        {
            return;
        }

        var revokedAt =
            DateTime.UtcNow;

        foreach (
            var refreshToken
            in activeRefreshTokens
        )
        {
            refreshToken.RevokedAt =
                revokedAt;
        }

        await _db.SaveChangesAsync();
    }
}