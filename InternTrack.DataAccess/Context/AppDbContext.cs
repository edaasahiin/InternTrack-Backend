using InternTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InternTrack.DataAccess.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Intern> Interns { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>().Property(department => department.IsActive).HasDefaultValue(true);

        modelBuilder.Entity<Intern>().Property(intern => intern.IsActive).HasDefaultValue(true);

        modelBuilder.Entity<TaskItem>().Property(task => task.IsActive).HasDefaultValue(true);

        modelBuilder.Entity<Department>().HasQueryFilter(department => department.IsActive);

        modelBuilder.Entity<Intern>().HasQueryFilter(intern => intern.IsActive);

        modelBuilder.Entity<TaskItem>().HasQueryFilter(task => task.IsActive);

        modelBuilder.Entity<User>().HasOne(
            user => user.Intern).WithOne(
            intern => intern.User).HasForeignKey<Intern>(
            intern => intern.UserId).OnDelete(
            DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>().HasOne(
            refreshToken => refreshToken.User).WithMany(
            user => user.RefreshTokens).HasForeignKey(
            refreshToken => refreshToken.UserId).OnDelete(
            DeleteBehavior.Cascade);
    }
}
