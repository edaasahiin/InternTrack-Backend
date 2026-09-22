using InternTrack.Business.Common;
using InternTrack.Business.Services;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Moq;

namespace InternTrack.Tests;

public class DepartmentServiceTests
{
    [Fact]
    public async Task AddAsync_DepartmentNameAlreadyExists_ShouldReturnConflict()
    {
        // ARRANGE
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var internRepositoryMock = new Mock<IInternRepository>();

        var dto = new CreateDepartmentDto
        {
            Name = "Software Development"
        };

        departmentRepositoryMock.Setup(repository => repository.NameExistsAsync(dto.Name, null)).ReturnsAsync(true);

        var service = new DepartmentService(departmentRepositoryMock.Object, internRepositoryMock.Object);
        // ACT
        var result = await service.AddAsync(dto);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal("Bu departman zaten kayıtlı.", result.Message);

        departmentRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentId = 99;

        var dto = new CreateDepartmentDto
        {
            Name = "Finance"
        };

        departmentRepositoryMock.Setup(
            repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(
            (Department? )null);

        var service = new DepartmentService(departmentRepositoryMock.Object, internRepositoryMock.Object);
        // ACT
        var result = await service.UpdateAsync(departmentId, dto);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.NotFound, result.Type);

        Assert.Equal("Departman bulunamadı.", result.Message);

        departmentRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DepartmentHasInterns_ShouldReturnConflict()
    {
        // ARRANGE
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentId = 1;

        var department = new Department
        {
            Id = departmentId,
            Name = "Software Development"
        };

        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(department);

        internRepositoryMock.Setup(repository => repository.ExistsByDepartmentIdAsync(departmentId)).ReturnsAsync(true);

        var service = new DepartmentService(departmentRepositoryMock.Object, internRepositoryMock.Object);
        // ACT
        var result = await service.DeleteAsync(departmentId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal("Bu departmana bağlı stajyerler olduğu için departman silinemez.", result.Message);

        departmentRepositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DepartmentHasNoInterns_ShouldDeleteSuccessfully()
    {
        // ARRANGE
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentId = 1;

        var department = new Department
        {
            Id = departmentId,
            Name = "Software Development"
        };

        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(department);

        internRepositoryMock.Setup(
            repository => repository.ExistsByDepartmentIdAsync(departmentId)).ReturnsAsync(
            false);

        var service = new DepartmentService(departmentRepositoryMock.Object, internRepositoryMock.Object);
        // ACT
        var result = await service.DeleteAsync(departmentId);
        // ASSERT
        Assert.True(result.Success);

        Assert.Equal(ResultType.Success, result.Type);

        Assert.Equal("Departman silindi.", result.Message);

        departmentRepositoryMock.Verify(repository => repository.DeleteAsync(department), Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentId = 99;

        departmentRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(departmentId)).ReturnsAsync(
            (Department? )null);

        var service = new DepartmentService(departmentRepositoryMock.Object, internRepositoryMock.Object);
        // ACT
        var result = await service.RestoreAsync(departmentId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.NotFound, result.Type);

        Assert.Equal("Departman bulunamadı.", result.Message);

        departmentRepositoryMock.Verify(repository => repository.RestoreAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_DepartmentAlreadyActive_ShouldReturnConflict()
    {
        // ARRANGE
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentId = 1;

        var department = new Department
        {
            Id = departmentId,
            Name = "Software Development",
            IsActive = true
        };

        departmentRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(departmentId)).ReturnsAsync(
            department);

        var service = new DepartmentService(departmentRepositoryMock.Object, internRepositoryMock.Object);
        // ACT
        var result = await service.RestoreAsync(departmentId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal("Departman zaten aktif.", result.Message);

        departmentRepositoryMock.Verify(repository => repository.RestoreAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_InactiveDepartment_ShouldRestoreSuccessfully()
    {
        // ARRANGE
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentId = 1;

        var department = new Department
        {
            Id = departmentId,
            Name = "Software Development",
            IsActive = false
        };

        departmentRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(departmentId)).ReturnsAsync(
            department);

        var service = new DepartmentService(departmentRepositoryMock.Object, internRepositoryMock.Object);
        // ACT
        var result = await service.RestoreAsync(departmentId);
        // ASSERT
        Assert.True(result.Success);

        Assert.Equal(ResultType.Success, result.Type);

        Assert.Equal("Departman tekrar aktif hale getirildi.", result.Message);

        departmentRepositoryMock.Verify(repository => repository.RestoreAsync(department), Times.Once);
    }

    [Fact]
public async Task AddAsync_ValidDepartment_ShouldAddSuccessfully()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var dto = new CreateDepartmentDto
    {
        Name = "Human Resources"
    };

    departmentRepositoryMock
        .Setup(repository => repository.NameExistsAsync(dto.Name, null))
        .ReturnsAsync(false);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.AddAsync(dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);

    departmentRepositoryMock.Verify(
        repository => repository.AddAsync(
            It.Is<Department>(department =>
                department.Name == dto.Name)),
        Times.Once);
}

[Fact]
public async Task UpdateAsync_DepartmentNameAlreadyExists_ShouldReturnConflict()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 1;

    var existingDepartment = new Department
    {
        Id = departmentId,
        Name = "Software Development",
        IsActive = true
    };

    var dto = new CreateDepartmentDto
    {
        Name = "Finance"
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(existingDepartment);

    departmentRepositoryMock
        .Setup(repository => repository.NameExistsAsync(dto.Name, departmentId))
        .ReturnsAsync(true);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.UpdateAsync(
        departmentId,
        dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.Conflict, result.Type);

    departmentRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<Department>()),
        Times.Never);
}

[Fact]
public async Task UpdateAsync_ValidDepartment_ShouldUpdateSuccessfully()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 1;

    var existingDepartment = new Department
    {
        Id = departmentId,
        Name = "Software Development",
        IsActive = true
    };

    var dto = new CreateDepartmentDto
    {
        Name = "Information Technologies"
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(existingDepartment);

    departmentRepositoryMock
        .Setup(repository => repository.NameExistsAsync(dto.Name, departmentId))
        .ReturnsAsync(false);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.UpdateAsync(
        departmentId,
        dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.Equal(dto.Name, existingDepartment.Name);

    departmentRepositoryMock.Verify(
        repository => repository.UpdateAsync(existingDepartment),
        Times.Once);
}

[Fact]
public async Task DeleteAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 999;

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync((Department?)null);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.DeleteAsync(departmentId);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.NotFound, result.Type);
    Assert.Equal("Departman bulunamadı.", result.Message);

    internRepositoryMock.Verify(
        repository => repository.ExistsByDepartmentIdAsync(It.IsAny<int>()),
        Times.Never);

    departmentRepositoryMock.Verify(
        repository => repository.DeleteAsync(It.IsAny<Department>()),
        Times.Never);
}

[Fact]
public async Task AddAsync_EmptyDepartmentName_ShouldReturnValidationError()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var dto = new CreateDepartmentDto
    {
        Name = "   "
    };

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.AddAsync(dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.ValidationError, result.Type);

    Assert.Equal(
        "Departman adı boş bırakılamaz.",
        result.Message);

    departmentRepositoryMock.Verify(
        repository => repository.NameExistsAsync(
            It.IsAny<string>(),
            It.IsAny<int?>()),
        Times.Never);

    departmentRepositoryMock.Verify(
        repository => repository.AddAsync(
            It.IsAny<Department>()),
        Times.Never);
}

[Fact]
public async Task UpdateAsync_EmptyDepartmentName_ShouldReturnValidationError()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 1;

    var department = new Department
    {
        Id = departmentId,
        Name = "Software Development",
        IsActive = true
    };

    var dto = new CreateDepartmentDto
    {
        Name = "   "
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.UpdateAsync(
        departmentId,
        dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.ValidationError, result.Type);

    Assert.Equal(
        "Departman adı boş bırakılamaz.",
        result.Message);

    departmentRepositoryMock.Verify(
        repository => repository.NameExistsAsync(
            It.IsAny<string>(),
            It.IsAny<int?>()),
        Times.Never);

    departmentRepositoryMock.Verify(
        repository => repository.UpdateAsync(
            It.IsAny<Department>()),
        Times.Never);
}

[Fact]
public async Task AddAsync_DepartmentNameWithWhitespace_ShouldTrimAndAddSuccessfully()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var dto = new CreateDepartmentDto
    {
        Name = "  Finance  "
    };

    departmentRepositoryMock
        .Setup(repository => repository.NameExistsAsync("Finance", null))
        .ReturnsAsync(false);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.AddAsync(dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);

    departmentRepositoryMock.Verify(
        repository => repository.AddAsync(
            It.Is<Department>(department =>
                department.Name == "Finance")),
        Times.Once);
}

[Fact]
public async Task UpdateAsync_DepartmentNameWithWhitespace_ShouldTrimAndUpdateSuccessfully()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 1;

    var department = new Department
    {
        Id = departmentId,
        Name = "Old Name",
        IsActive = true
    };

    var dto = new CreateDepartmentDto
    {
        Name = "  Finance  "
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    departmentRepositoryMock
        .Setup(repository => repository.NameExistsAsync("Finance", departmentId))
        .ReturnsAsync(false);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.UpdateAsync(
        departmentId,
        dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);

    Assert.Equal("Finance", department.Name);

    departmentRepositoryMock.Verify(
        repository => repository.UpdateAsync(department),
        Times.Once);
}

[Fact]
public async Task GetByIdAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 999;

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync((Department?)null);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.GetByIdAsync(departmentId);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.NotFound, result.Type);
    Assert.Equal(
        "Departman bulunamadı.",
        result.Message);
}

[Fact]
public async Task GetByIdAsync_DepartmentExists_ShouldReturnDepartmentSuccessfully()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 1;

    var department = new Department
    {
        Id = departmentId,
        Name = "Software Development",
        IsActive = true
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.GetByIdAsync(departmentId);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);

    Assert.Equal(departmentId, result.Data.Id);
    Assert.Equal("Software Development", result.Data.Name);
    Assert.True(result.Data.IsActive);

    departmentRepositoryMock.Verify(
        repository => repository.GetByIdAsync(departmentId),
        Times.Once);
}

[Fact]
public async Task GetAllIncludingInactiveAsync_ShouldReturnActiveAndInactiveDepartments()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departments = new List<Department>
    {
        new()
        {
            Id = 1,
            Name = "Software Development",
            IsActive = true
        },
        new()
        {
            Id = 2,
            Name = "Human Resources",
            IsActive = false
        }
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetAllIncludingInactiveAsync())
        .ReturnsAsync(departments);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result =
        await service.GetAllIncludingInactiveAsync();

    // ASSERT
    Assert.Equal(2, result.Count);

    Assert.Contains(
        result,
        department =>
            department.Id == 1 &&
            department.IsActive);

    Assert.Contains(
        result,
        department =>
            department.Id == 2 &&
            !department.IsActive);

    departmentRepositoryMock.Verify(
        repository => repository.GetAllIncludingInactiveAsync(),
        Times.Once);
}

[Fact]
public async Task GetAllAsync_ShouldReturnDepartments()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departments = new List<Department>
    {
        new()
        {
            Id = 1,
            Name = "Software Development",
            IsActive = true
        },
        new()
        {
            Id = 2,
            Name = "Human Resources",
            IsActive = true
        }
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetAllAsync())
        .ReturnsAsync(departments);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.GetAllAsync();

    // ASSERT
    Assert.Equal(2, result.Count);

    Assert.Equal(
        "Software Development",
        result[0].Name);

    Assert.Equal(
        "Human Resources",
        result[1].Name);

    departmentRepositoryMock.Verify(
        repository => repository.GetAllAsync(),
        Times.Once);

    departmentRepositoryMock.Verify(
        repository => repository.GetAllIncludingInactiveAsync(),
        Times.Never);
}

[Fact]
public async Task DeleteAsync_DepartmentDoesNotExist_ShouldNotCheckInterns()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 999;

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync((Department?)null);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.DeleteAsync(departmentId);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.NotFound, result.Type);
    Assert.Equal(
        "Departman bulunamadı.",
        result.Message);

    internRepositoryMock.Verify(
        repository => repository.ExistsByDepartmentIdAsync(
            It.IsAny<int>()),
        Times.Never);

    departmentRepositoryMock.Verify(
        repository => repository.DeleteAsync(
            It.IsAny<Department>()),
        Times.Never);
}

[Fact]
public async Task DeleteAsync_DepartmentHasInterns_ShouldNotDeleteDepartment()
{
    // ARRANGE
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var internRepositoryMock = new Mock<IInternRepository>();

    var departmentId = 1;

    var department = new Department
    {
        Id = departmentId,
        Name = "Software Development",
        IsActive = true
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    internRepositoryMock
        .Setup(repository => repository.ExistsByDepartmentIdAsync(departmentId))
        .ReturnsAsync(true);

    var service = new DepartmentService(
        departmentRepositoryMock.Object,
        internRepositoryMock.Object);

    // ACT
    var result = await service.DeleteAsync(departmentId);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.Conflict, result.Type);

    Assert.Equal(
        "Bu departmana bağlı stajyerler olduğu için departman silinemez.",
        result.Message);

    departmentRepositoryMock.Verify(
        repository => repository.DeleteAsync(
            It.IsAny<Department>()),
        Times.Never);
}
}