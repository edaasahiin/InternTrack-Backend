using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.Models;
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
        return await _db.TaskItems
            .Include(x => x.Intern)
            .ThenInclude(i => i!.Department)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _db.TaskItems
            .Include(x => x.Intern)
            .ThenInclude(i => i!.Department)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(TaskItem task)
    {
        await _db.TaskItems.AddAsync(task);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _db.TaskItems.Update(task);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _db.TaskItems.Remove(task);
        await _db.SaveChangesAsync();
    }
}
