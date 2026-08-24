using InternTrack.DataAccess.Context;
using InternTrack.Entities.Models;

namespace InternTrack.DataAccess;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!db.Departments.Any())
        {
            await db.Departments.AddRangeAsync(
                new Department { Name = "Software" },
                new Department { Name = "Human Resources" },
                new Department { Name = "Finance" },
                new Department { Name = "Marketing" },
                new Department { Name = "Operations" }
            );

            await db.SaveChangesAsync();
        }
    }
}