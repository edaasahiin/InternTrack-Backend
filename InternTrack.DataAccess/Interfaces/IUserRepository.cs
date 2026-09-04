using InternTrack.Core.Models;

namespace InternTrack.DataAccess.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(int id);

    Task<bool> EmailExistsAsync(string email);

    Task AddAsync(User user);

    Task UpdateAsync(User user);
}