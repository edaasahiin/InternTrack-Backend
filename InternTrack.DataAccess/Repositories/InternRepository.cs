using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Repositories;

public class InternRepository : IInternRepository
{
    private readonly AppDbContext _db;

    public InternRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Intern>> GetAllAsync()
    {
        return await _db.Interns
            .Include(x => x.Department)
            .ToListAsync();
    }

    public async Task<Intern?> GetByIdAsync(int id)
    {
        return await _db.Interns
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Intern intern)
    {
        await _db.Interns.AddAsync(intern);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Intern intern)
    {
        _db.Interns.Update(intern);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Intern intern)
    {
        _db.Interns.Remove(intern);
        await _db.SaveChangesAsync();
    }
}