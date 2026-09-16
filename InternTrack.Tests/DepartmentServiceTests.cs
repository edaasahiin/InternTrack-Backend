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

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var dto =
            new CreateDepartmentDto
            {
                Name = "Software Development"
            };

        departmentRepositoryMock
            .Setup(repository =>
                repository.NameExistsAsync(
                    dto.Name,
                    null
                ))
            .ReturnsAsync(true);

        var service =
            new DepartmentService(
                departmentRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.AddAsync(
                dto
            );

        // ASSERT

        Assert.False(
            result.Success
        );

        Assert.Equal(
            ResultType.Conflict,
            result.Type
        );

        Assert.Equal(
            "Bu departman zaten kayıtlı.",
            result.Message
        );

        departmentRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<Department>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var departmentId = 99;

        var dto =
            new CreateDepartmentDto
            {
                Name = "Finance"
            };

        departmentRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    departmentId
                ))
            .ReturnsAsync(
                (Department?)null
            );

        var service =
            new DepartmentService(
                departmentRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.UpdateAsync(
                departmentId,
                dto
            );

        // ASSERT

        Assert.False(
            result.Success
        );

        Assert.Equal(
            ResultType.NotFound,
            result.Type
        );

        Assert.Equal(
            "Departman bulunamadı.",
            result.Message
        );

        departmentRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    It.IsAny<Department>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task DeleteAsync_DepartmentHasInterns_ShouldReturnConflict()
    {
        // ARRANGE

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var departmentId = 1;

        var department =
            new Department
            {
                Id = departmentId,
                Name = "Software Development"
            };

        departmentRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    departmentId
                ))
            .ReturnsAsync(
                department
            );

        internRepositoryMock
            .Setup(repository =>
                repository.ExistsByDepartmentIdAsync(
                    departmentId
                ))
            .ReturnsAsync(true);

        var service =
            new DepartmentService(
                departmentRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.DeleteAsync(
                departmentId
            );

        // ASSERT

        Assert.False(
            result.Success
        );

        Assert.Equal(
            ResultType.Conflict,
            result.Type
        );

        Assert.Equal(
            "Bu departmana bağlı stajyerler olduğu için departman silinemez.",
            result.Message
        );

        departmentRepositoryMock.Verify(
            repository =>
                repository.DeleteAsync(
                    It.IsAny<Department>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task DeleteAsync_DepartmentHasNoInterns_ShouldDeleteSuccessfully()
    {
        // ARRANGE

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var departmentId = 1;

        var department =
            new Department
            {
                Id = departmentId,
                Name = "Software Development"
            };

        departmentRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    departmentId
                ))
            .ReturnsAsync(
                department
            );

        internRepositoryMock
            .Setup(repository =>
                repository.ExistsByDepartmentIdAsync(
                    departmentId
                ))
            .ReturnsAsync(false);

        var service =
            new DepartmentService(
                departmentRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.DeleteAsync(
                departmentId
            );

        // ASSERT

        Assert.True(
            result.Success
        );

        Assert.Equal(
            ResultType.Success,
            result.Type
        );

        Assert.Equal(
            "Departman silindi.",
            result.Message
        );

        departmentRepositoryMock.Verify(
            repository =>
                repository.DeleteAsync(
                    department
                ),
            Times.Once
        );
    }
}