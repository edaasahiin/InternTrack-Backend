using InternTrack.DataAccess.Context;
using InternTrack.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Repositories;

public class DepartmentRepository
{
    private readonly AppDbContext _db;

    public DepartmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _db.Departments.ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _db.Departments
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Department department)
    {
        await _db.Departments.AddAsync(department);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Department department)
    {
        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();
    }
}