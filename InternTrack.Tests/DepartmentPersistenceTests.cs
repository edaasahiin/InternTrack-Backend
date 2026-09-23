using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Business.Services;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using InternTrack.DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InternTrack.Tests;

public class DepartmentPersistenceTests
{
    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ShouldNotCreateDepartmentsOnRepeatedReads()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDbContext(connection);
        await db.Database.EnsureCreatedAsync();
        var service = CreateService(db);

        Assert.Empty(await service.GetAllAsync());
        Assert.Empty(await service.GetAllAsync());
        Assert.Empty(await service.GetAllIncludingInactiveAsync());
        Assert.False(await db.Departments.IgnoreQueryFilters().AnyAsync());
        Assert.False(db.ChangeTracker.HasChanges());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddAsync_RepeatedDepartmentSetup_ShouldNotDuplicateActiveOrInactiveNames(bool isActive)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var firstDb = CreateDbContext(connection);
        await firstDb.Database.EnsureCreatedAsync();
        var firstService = CreateService(firstDb);

        var firstResult = await firstService.AddAsync(new CreateDepartmentDto { Name = "Software" });

        Assert.True(firstResult.Success);
        var department = await firstDb.Departments.SingleAsync();
        var originalId = department.Id;
        if (!isActive)
        {
            await new DepartmentRepository(firstDb).DeactivateDepartmentAsync(department);
        }

        // Separate request scope: the duplicate check must use persisted data, not tracked entities.
        await using var secondDb = CreateDbContext(connection);
        var secondService = CreateService(secondDb);
        foreach (var name in new[] { "Software", "software", " SOFTWARE " })
        {
            var result = await secondService.AddAsync(new CreateDepartmentDto { Name = name });

            Assert.Equal(ResultType.Conflict, result.Type);
            Assert.False(result.Success);
            Assert.Equal("Bu departman zaten kayıtlı.", result.Message);
        }

        var persisted = await secondDb.Departments.IgnoreQueryFilters().ToListAsync();
        var existingDepartment = Assert.Single(persisted);
        Assert.Equal(originalId, existingDepartment.Id);
        Assert.Equal("Software", existingDepartment.Name);
        Assert.Equal(isActive, existingDepartment.IsActive);
        Assert.False(secondDb.ChangeTracker.HasChanges());
    }

    [Fact]
    public async Task GetAllAsync_OnlyInactiveDepartments_ShouldNotRecreateOrReactivateThem()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDbContext(connection);
        await db.Database.EnsureCreatedAsync();
        db.Departments.Add(new Department { Name = "Software", IsActive = false });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = CreateService(db);

        Assert.Empty(await service.GetAllAsync());
        Assert.Empty(await service.GetAllAsync());

        var department = Assert.Single(await service.GetAllIncludingInactiveAsync());
        Assert.Equal("Software", department.Name);
        Assert.False(department.IsActive);
        Assert.Equal(1, await db.Departments.IgnoreQueryFilters().CountAsync());
        Assert.False(db.ChangeTracker.HasChanges());
    }

    [Theory]
    [InlineData("Finance", "Finance", true)]
    [InlineData("Finance", "FINANCE", true)]
    [InlineData("Finance", "  Finance  ", true)]
    [InlineData("Finance", "\tFINANCE\r\n", true)]
    [InlineData("  Finance  ", "finance", true)]
    [InlineData("Finance", "finance", false)]
    public async Task GetByNameIncludingInactiveAsync_ShouldReturnMatchingDepartment(
        string storedName, string queryName, bool isActive)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDbContext(connection);
        await db.Database.EnsureCreatedAsync();
        var department = new Department { Name = storedName, IsActive = isActive };
        db.Departments.Add(department);
        await db.SaveChangesAsync();
        var id = department.Id;
        db.ChangeTracker.Clear();
        var repository = new DepartmentRepository(db);

        var result = await repository.GetByNameIncludingInactiveAsync(queryName);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(storedName, result.Name);
        Assert.Equal(isActive, result.IsActive);
        Assert.Equal(EntityState.Unchanged, db.Entry(result).State);
        Assert.Equal(isActive ? 1 : 0, (await repository.GetAllAsync()).Count);
    }

    [Fact]
    public async Task GetByNameIncludingInactiveAsync_UnmatchedName_ShouldReturnNull()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDbContext(connection);
        await db.Database.EnsureCreatedAsync();
        db.Departments.Add(new Department { Name = "Finance" });
        await db.SaveChangesAsync();
        var repository = new DepartmentRepository(db);

        var result = await repository.GetByNameIncludingInactiveAsync("Marketing");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameIncludingInactiveAsync_ExcludingOnlyMatch_ShouldReturnNull()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDbContext(connection);
        await db.Database.EnsureCreatedAsync();
        var department = new Department { Name = "Finance" };
        db.Departments.Add(department);
        await db.SaveChangesAsync();
        var repository = new DepartmentRepository(db);

        var result = await repository.GetByNameIncludingInactiveAsync(" FINANCE ", department.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameIncludingInactiveAsync_ExcludingCurrentDepartment_ShouldStillFindAnotherMatch()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDbContext(connection);
        await db.Database.EnsureCreatedAsync();
        var current = new Department { Name = "Finance" };
        var other = new Department { Name = " FINANCE ", IsActive = false };
        db.Departments.AddRange(current, other);
        await db.SaveChangesAsync();
        var repository = new DepartmentRepository(db);

        var result = await repository.GetByNameIncludingInactiveAsync("Finance", current.Id);

        Assert.NotNull(result);
        Assert.Equal(other.Id, result.Id);
        Assert.False(result.IsActive);
    }

    [Theory]
    [InlineData("Finance")]
    [InlineData("FINANCE")]
    [InlineData("  Finance  ")]
    public async Task UpdateAsync_CurrentDepartmentName_ShouldNotCauseFalseConflict(string name)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDbContext(connection);
        await db.Database.EnsureCreatedAsync();
        var department = new Department { Name = "Finance" };
        db.Departments.Add(department);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.UpdateAsync(department.Id, new CreateDepartmentDto { Name = name });

        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        db.ChangeTracker.Clear();
        var stored = await db.Departments.IgnoreQueryFilters().SingleAsync();
        Assert.Equal(name.Trim(), stored.Name);
        Assert.Equal(department.Id, stored.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_AnotherDepartmentName_ShouldReturnConflictWithoutPersistingChanges(bool otherIsActive)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDbContext(connection);
        await db.Database.EnsureCreatedAsync();
        var department = new Department { Name = "Software" };
        var other = new Department { Name = " Finance ", IsActive = otherIsActive };
        db.Departments.AddRange(department, other);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.UpdateAsync(department.Id, new CreateDepartmentDto { Name = " FINANCE " });

        Assert.False(result.Success);
        Assert.Equal(ResultType.Conflict, result.Type);
        Assert.Equal("Bu departman zaten kayıtlı.", result.Message);
        Assert.False(db.ChangeTracker.HasChanges());
        db.ChangeTracker.Clear();
        Assert.Equal("Software", (await db.Departments.SingleAsync(item => item.Id == department.Id)).Name);
        Assert.Equal(2, await db.Departments.IgnoreQueryFilters().CountAsync());
        var storedOther = await db.Departments.IgnoreQueryFilters().SingleAsync(item => item.Id == other.Id);
        Assert.Equal(" Finance ", storedOther.Name);
        Assert.Equal(otherIsActive, storedOther.IsActive);
    }

    private static AppDbContext CreateDbContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        return new AppDbContext(options);
    }

    private static DepartmentService CreateService(AppDbContext db)
    {
        return new DepartmentService(new DepartmentRepository(db), Mock.Of<IInternRepository>(), Mock.Of<IAppLogger>());
    }
}
