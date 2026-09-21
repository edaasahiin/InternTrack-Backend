using InternTrack.Business.Common;
using InternTrack.Business.Services;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Moq;

namespace InternTrack.Tests;

public class InternServiceTests
{
    [Fact]
    public async Task DeleteAsync_InternDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 99;

        internRepositoryMock.Setup(repository => repository.GetByIdAsync(internId)).ReturnsAsync((Intern? )null);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object);
        // ACT
        var result = await service.DeleteAsync(internId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.NotFound, result.Type);

        Assert.Equal("Stajyer bulunamadı.", result.Message);

        internRepositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<Intern>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_InternExists_ShouldDeleteSuccessfully()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 1;

        var intern = new Intern
        {
            Id = internId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            DepartmentId = 1,
            UserId = 10,
            IsActive = true
        };

        internRepositoryMock.Setup(repository => repository.GetByIdAsync(internId)).ReturnsAsync(intern);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object);
        // ACT
        var result = await service.DeleteAsync(internId);
        // ASSERT
        Assert.True(result.Success);

        Assert.Equal(ResultType.Success, result.Type);

        Assert.Equal("Stajyer pasif hale getirildi.", result.Message);

        internRepositoryMock.Verify(repository => repository.DeleteAsync(intern), Times.Once);

        userRepositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_InternDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 99;

        internRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(internId)).ReturnsAsync(
            (Intern? )null);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object);
        // ACT
        var result = await service.RestoreAsync(internId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.NotFound, result.Type);

        Assert.Equal("Stajyer bulunamadı.", result.Message);

        internRepositoryMock.Verify(repository => repository.RestoreAsync(It.IsAny<Intern>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_InternAlreadyActive_ShouldReturnConflict()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 1;

        var intern = new Intern
        {
            Id = internId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            DepartmentId = 1,
            UserId = 10,
            IsActive = true
        };

        internRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(internId)).ReturnsAsync(
            intern);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object);
        // ACT
        var result = await service.RestoreAsync(internId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal("Stajyer zaten aktif.", result.Message);

        internRepositoryMock.Verify(repository => repository.RestoreAsync(It.IsAny<Intern>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_DepartmentIsInactive_ShouldReturnConflict()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 1;
        var departmentId = 5;

        var intern = new Intern
        {
            Id = internId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            DepartmentId = departmentId,
            UserId = 10,
            IsActive = false
        };

        internRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(internId)).ReturnsAsync(
            intern);

        departmentRepositoryMock.Setup(
            repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(
            (Department? )null);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object);
        // ACT
        var result = await service.RestoreAsync(internId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal(
            "Stajyerin bağlı olduğu departman pasif veya bulunamadı. Önce departmanı aktif hale getirin.",
            result.Message);

        internRepositoryMock.Verify(repository => repository.RestoreAsync(It.IsAny<Intern>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_InactiveInternWithActiveDepartment_ShouldRestoreSuccessfully()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 1;
        var departmentId = 5;

        var intern = new Intern
        {
            Id = internId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            DepartmentId = departmentId,
            UserId = 10,
            IsActive = false
        };

        var department = new Department
        {
            Id = departmentId,
            Name = "Software Development",
            IsActive = true
        };

        internRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(internId)).ReturnsAsync(
            intern);

        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(department);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object);
        // ACT
        var result = await service.RestoreAsync(internId);
        // ASSERT
        Assert.True(result.Success);

        Assert.Equal(ResultType.Success, result.Type);

        Assert.Equal("Stajyer tekrar aktif hale getirildi.", result.Message);

        internRepositoryMock.Verify(repository => repository.RestoreAsync(intern), Times.Once);
    }
}
