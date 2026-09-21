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
        return await IncludeInternAndDepartment(_db.Tasks).ToListAsync();
    }

    public async Task<List<TaskItem>> GetAllIncludingInactiveAsync()
    {
        return await IncludeInternAndDepartment(_db.Tasks.IgnoreQueryFilters()).ToListAsync();
    }

    public async Task<List<TaskItem>> GetByInternIdAsync(int internId)
    {
        return await IncludeInternAndDepartment(_db.Tasks)
            .Where(task => task.InternId == internId)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await IncludeInternAndDepartment(_db.Tasks)
            .FirstOrDefaultAsync(task => task.Id == id);
    }

    public async Task<TaskItem?> GetByIdIncludingInactiveAsync(int id)
    {
        return await IncludeInternAndDepartment(_db.Tasks.IgnoreQueryFilters())
            .FirstOrDefaultAsync(task => task.Id == id);
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
        task.IsActive = false;

        _db.Tasks.Update(task);

        await _db.SaveChangesAsync();
    }

    public async Task RestoreAsync(TaskItem task)
    {
        task.IsActive = true;

        _db.Tasks.Update(task);

        await _db.SaveChangesAsync();
    }

    private static IQueryable<TaskItem> IncludeInternAndDepartment(IQueryable<TaskItem> query)
    {
        return query
            .Include(task => task.Intern)
            .ThenInclude(intern => intern!.Department);
    }
}
