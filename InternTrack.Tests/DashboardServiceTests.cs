using InternTrack.Business.Common;
using InternTrack.Business.Services;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Moq;

namespace InternTrack.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetStatsAsync_Admin_ShouldReturnSystemWideStatistics()
    {
        // ARRANGE

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var now =
            DateTime.UtcNow;

        var interns =
            new List<Intern>
            {
                new()
                {
                    Id = 1,
                    Name = "Test",
                    Surname = "Intern1",
                    Email = "intern1@example.com"
                },
                new()
                {
                    Id = 2,
                    Name = "Test",
                    Surname = "Intern2",
                    Email = "intern2@example.com"
                }
            };

        var departments =
            new List<Department>
            {
                new()
                {
                    Id = 1,
                    Name = "Software Development"
                },
                new()
                {
                    Id = 2,
                    Name = "Finance"
                }
            };

        var tasks =
            new List<TaskItem>
            {
                new()
                {
                    Id = 1,
                    Title = "Task 1",
                    Status = "ToDo",
                    Priority = "Medium",
                    DueDate = now.AddDays(2),
                    InternId = 1
                },
                new()
                {
                    Id = 2,
                    Title = "Task 2",
                    Status = "InProgress",
                    Priority = "High",
                    DueDate = now.AddDays(3),
                    InternId = 1
                },
                new()
                {
                    Id = 3,
                    Title = "Task 3",
                    Status = "Done",
                    Priority = "Low",
                    DueDate = now.AddDays(-1),
                    InternId = 2
                },
                new()
                {
                    Id = 4,
                    Title = "Task 4",
                    Status = "ToDo",
                    Priority = "Medium",
                    DueDate = now.AddDays(-2),
                    InternId = 2
                }
            };

        internRepositoryMock
            .Setup(repository =>
                repository.GetAllAsync())
            .ReturnsAsync(interns);

        taskRepositoryMock
            .Setup(repository =>
                repository.GetAllAsync())
            .ReturnsAsync(tasks);

        departmentRepositoryMock
            .Setup(repository =>
                repository.GetAllAsync())
            .ReturnsAsync(departments);

        var service =
            new DashboardService(
                internRepositoryMock.Object,
                taskRepositoryMock.Object,
                departmentRepositoryMock.Object
            );

        // ACT

        var result =
            await service.GetStatsAsync(
                1,
                "Admin"
            );

        // ASSERT

        Assert.True(
            result.Success
        );

        Assert.Equal(
            ResultType.Success,
            result.Type
        );

        Assert.NotNull(
            result.Data
        );

        Assert.Equal(
            2,
            result.Data.InternCount
        );

        Assert.Equal(
            4,
            result.Data.TaskCount
        );

        Assert.Equal(
            1,
            result.Data.ToDoTaskCount
        );

        Assert.Equal(
            1,
            result.Data.InProgressTaskCount
        );

        Assert.Equal(
            1,
            result.Data.CompletedTaskCount
        );

        Assert.Equal(
            1,
            result.Data.OverdueTaskCount
        );

        Assert.Equal(
            2,
            result.Data.DepartmentCount
        );
    }

    [Fact]
    public async Task GetStatsAsync_Intern_ShouldReturnOnlyOwnTaskStatistics()
    {
        // ARRANGE

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var userId = 10;
        var internId = 3;

        var now =
            DateTime.UtcNow;

        var intern =
            new Intern
            {
                Id = internId,
                UserId = userId,
                Name = "Test",
                Surname = "Intern",
                Email = "test@example.com"
            };

        var internTasks =
            new List<TaskItem>
            {
                new()
                {
                    Id = 1,
                    Title = "Task 1",
                    Status = "ToDo",
                    Priority = "Medium",
                    DueDate = now.AddDays(2),
                    InternId = internId
                },
                new()
                {
                    Id = 2,
                    Title = "Task 2",
                    Status = "InProgress",
                    Priority = "High",
                    DueDate = now.AddDays(1),
                    InternId = internId
                },
                new()
                {
                    Id = 3,
                    Title = "Task 3",
                    Status = "Done",
                    Priority = "Low",
                    DueDate = now.AddDays(-1),
                    InternId = internId
                },
                new()
                {
                    Id = 4,
                    Title = "Task 4",
                    Status = "InProgress",
                    Priority = "Medium",
                    DueDate = now.AddDays(-2),
                    InternId = internId
                }
            };

        internRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync(
                    userId
                ))
            .ReturnsAsync(intern);

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByInternIdAsync(
                    internId
                ))
            .ReturnsAsync(internTasks);

        var service =
            new DashboardService(
                internRepositoryMock.Object,
                taskRepositoryMock.Object,
                departmentRepositoryMock.Object
            );

        // ACT

        var result =
            await service.GetStatsAsync(
                userId,
                "Intern"
            );

        // ASSERT

        Assert.True(
            result.Success
        );

        Assert.Equal(
            ResultType.Success,
            result.Type
        );

        Assert.NotNull(
            result.Data
        );

        Assert.Equal(
            1,
            result.Data.InternCount
        );

        Assert.Equal(
            4,
            result.Data.TaskCount
        );

        Assert.Equal(
            1,
            result.Data.ToDoTaskCount
        );

        Assert.Equal(
            1,
            result.Data.InProgressTaskCount
        );

        Assert.Equal(
            1,
            result.Data.CompletedTaskCount
        );

        Assert.Equal(
            1,
            result.Data.OverdueTaskCount
        );

        Assert.Equal(
            0,
            result.Data.DepartmentCount
        );

        internRepositoryMock.Verify(
            repository =>
                repository.GetAllAsync(),
            Times.Never
        );

        taskRepositoryMock.Verify(
            repository =>
                repository.GetAllAsync(),
            Times.Never
        );
    }

    [Fact]
    public async Task GetStatsAsync_InternProfileDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var departmentRepositoryMock =
            new Mock<IDepartmentRepository>();

        var userId = 99;

        internRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync(
                    userId
                ))
            .ReturnsAsync(
                (Intern?)null
            );

        var service =
            new DashboardService(
                internRepositoryMock.Object,
                taskRepositoryMock.Object,
                departmentRepositoryMock.Object
            );

        // ACT

        var result =
            await service.GetStatsAsync(
                userId,
                "Intern"
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
            "Stajyer kaydı bulunamadı.",
            result.Message
        );

        taskRepositoryMock.Verify(
            repository =>
                repository.GetByInternIdAsync(
                    It.IsAny<int>()
                ),
            Times.Never
        );
    }
}