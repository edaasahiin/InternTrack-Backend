using InternTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Intern> Interns { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
}
