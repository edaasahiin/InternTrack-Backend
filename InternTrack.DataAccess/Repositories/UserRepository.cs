using InternTrack.Core.Models;
using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _db.Users
            .FirstOrDefaultAsync(
                x => x.Email.ToLower() == normalizedEmail
            );
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _db.Users
            .AnyAsync(
                x => x.Email.ToLower() == normalizedEmail
            );
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);

        await _db.SaveChangesAsync();
    }
}