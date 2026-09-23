using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.Models;
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
        var interns = await IncludeDepartmentAndUser(_db.Interns).ToListAsync();

        foreach (var intern in interns)
        {
            PopulateAvatar(intern);
        }

        return interns;
    }

    public async Task<List<Intern>> GetAllIncludingInactiveAsync()
    {
        var interns = await IncludeDepartmentAndUser(_db.Interns.IgnoreQueryFilters()).ToListAsync();

        foreach (var intern in interns)
        {
            PopulateAvatar(intern);
        }

        return interns;
    }

    public async Task<Intern?> GetByIdAsync(int id)
    {
        var intern = await IncludeDepartmentAndUser(_db.Interns)
            .FirstOrDefaultAsync(candidate => candidate.Id == id);

        PopulateAvatar(intern);

        return intern;
    }

    public async Task<Intern?> GetByIdIncludingInactiveAsync(int id)
    {
        var intern = await IncludeDepartmentAndUser(_db.Interns.IgnoreQueryFilters())
            .FirstOrDefaultAsync(candidate => candidate.Id == id);

        PopulateAvatar(intern);

        return intern;
    }

    public async Task<Intern?> GetByUserIdAsync(int userId)
    {
        var intern = await IncludeDepartmentAndUser(_db.Interns)
            .FirstOrDefaultAsync(candidate => candidate.UserId == userId);

        PopulateAvatar(intern);

        return intern;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLower();

        return await _db.Interns
            .IgnoreQueryFilters()
            .AnyAsync(intern => intern.Email.ToLower() == normalizedEmail);
    }

    public async Task<bool> ExistsByDepartmentIdAsync(int departmentId)
    {
        return await _db.Interns.AnyAsync(intern => intern.DepartmentId == departmentId);
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

    public async Task DeactivateInternAsync(Intern intern)
    {
        intern.IsActive = false;

        _db.Interns.Update(intern);

        await _db.SaveChangesAsync();
    }

    public async Task ReactivateInternAsync(Intern intern)
    {
        intern.IsActive = true;

        _db.Interns.Update(intern);

        await _db.SaveChangesAsync();
    }

    private static IQueryable<Intern> IncludeDepartmentAndUser(IQueryable<Intern> query)
    {
        return query
            .Include(intern => intern.Department)
            .Include(intern => intern.User);
    }

    private static void PopulateAvatar(Intern? intern)
    {
        if (intern == null)
        {
            return;
        }

        intern.Avatar = intern.User?.Avatar;
    }
}
