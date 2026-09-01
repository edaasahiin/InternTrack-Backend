using InternTrack.Core.Models;
using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _db.Tasks
            .Include(x => x.Intern)
            .ThenInclude(i => i!.Department)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetByInternIdAsync(
        int internId)
    {
        return await _db.Tasks
            .Include(x => x.Intern)
            .ThenInclude(i => i!.Department)
            .Where(x => x.InternId == internId)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _db.Tasks
            .Include(x => x.Intern)
            .ThenInclude(i => i!.Department)
            .FirstOrDefaultAsync(
                x => x.Id == id
            );
    }

    public async Task AddAsync(TaskItem task)
    {
        await _db.Tasks.AddAsync(task);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }
}