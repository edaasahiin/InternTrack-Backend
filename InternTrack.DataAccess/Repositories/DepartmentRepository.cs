using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using InternTrack.DataAccess.Queries;
using InternTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Repositories;

public class DepartmentRepository : IDepartmentRepository
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

    public async Task<List<Department>> GetAllIncludingInactiveAsync()
    {
        return await _db.Departments.IgnoreQueryFilters().ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _db.Departments.FirstOrDefaultAsync(department => department.Id == id);
    }

    public async Task<Department?> GetByIdIncludingInactiveAsync(int id)
    {
        return await _db.Departments.IgnoreQueryFilters().FirstOrDefaultAsync(department => department.Id == id);
    }

    public async Task<Department?> GetByNameIncludingInactiveAsync(string name, int? excludeId = null)
    {
        return await DepartmentQueries.ByNameIncludingInactive(_db.Departments, name, excludeId)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Department department)
    {
        await _db.Departments.AddAsync(department);

        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        _db.Departments.Update(department);

        await _db.SaveChangesAsync();
    }

    public async Task DeactivateDepartmentAsync(Department department)
    {
        department.IsActive = false;

        _db.Departments.Update(department);

        await _db.SaveChangesAsync();
    }

    public async Task ReactivateDepartmentAsync(Department department)
    {
        department.IsActive = true;

        _db.Departments.Update(department);

        await _db.SaveChangesAsync();
    }
}
