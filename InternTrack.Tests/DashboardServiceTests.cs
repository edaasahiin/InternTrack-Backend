using InternTrack.Business.Interfaces;
using InternTrack.Business.Common;
using InternTrack.Business.Services;
using InternTrack.Core.Constants;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Moq;

namespace InternTrack.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetStatsAsync_Admin_ShouldReturnGlobalStatistics()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var now = DateTime.UtcNow;
        var interns = new List<Intern>
        {
            new()
            {
                Id = 1,
                Name = "Intern",
                Surname = "One",
                Email = "intern1@example.com"
            },
            new()
            {
                Id = 2,
                Name = "Intern",
                Surname = "Two",
                Email = "intern2@example.com"
            }
        };
        var departments = new List<Department>
        {
            new()
            {
                Id = 1,
                Name = "Software"
            },
            new()
            {
                Id = 2,
                Name = "Finance"
            }
        };
        var tasks = new List<TaskItem>
        {
            new()
            {
                Id = 1,
                Title = "Aktif ToDo",
                Status = TaskStatuses.ToDo,
                Priority = "Medium",
                InternId = 1,
                DueDate = now.AddDays(1)
            },
            new()
            {
                Id = 2,
                Title = "Aktif InProgress",
                Status = TaskStatuses.InProgress,
                Priority = "High",
                InternId = 1,
                DueDate = now.AddDays(2)
            },
            new()
            {
                Id = 3,
                Title = "Tamamlanan Görev",
                Status = TaskStatuses.Done,
                Priority = "Low",
                InternId = 2,
                DueDate = now.AddDays(-1)
            },
            new()
            {
                Id = 4,
                Title = "Geciken ToDo",
                Status = TaskStatuses.ToDo,
                Priority = "Medium",
                InternId = 2,
                DueDate = now.AddDays(-2)
            },
            new()
            {
                Id = 5,
                Title = "Deadline Olmayan Görev",
                Status = TaskStatuses.ToDo,
                Priority = "Medium",
                InternId = 1,
                DueDate = null
            }
        };
        internRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(interns);
        taskRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(tasks);
        departmentRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(departments);
        var service = new DashboardService(
            internRepositoryMock.Object,
            taskRepositoryMock.Object,
            departmentRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.GetStatsAsync(1, Roles.Admin);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.InternCount);
        Assert.Equal(5, result.Data.TaskCount);
        Assert.Equal(2, result.Data.ToDoTaskCount);
        Assert.Equal(1, result.Data.InProgressTaskCount);
        Assert.Equal(1, result.Data.CompletedTaskCount);
        Assert.Equal(1, result.Data.OverdueTaskCount);
        Assert.Equal(2, result.Data.DepartmentCount);
        internRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
        taskRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
        departmentRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_HR_ShouldReturnGlobalStatistics()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        internRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Intern>());
        taskRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<TaskItem>());
        departmentRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Department>());
        var service = new DashboardService(
            internRepositoryMock.Object,
            taskRepositoryMock.Object,
            departmentRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.GetStatsAsync(2, Roles.HR);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.InternCount);
        Assert.Equal(0, result.Data.TaskCount);
        Assert.Equal(0, result.Data.ToDoTaskCount);
        Assert.Equal(0, result.Data.InProgressTaskCount);
        Assert.Equal(0, result.Data.CompletedTaskCount);
        Assert.Equal(0, result.Data.OverdueTaskCount);
        Assert.Equal(0, result.Data.DepartmentCount);
        internRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
        taskRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
        departmentRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_Intern_ShouldReturnOnlyOwnTaskStatistics()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var userId = 10;
        var internId = 3;
        var now = DateTime.UtcNow;
        var intern = new Intern
        {
            Id = internId,
            UserId = userId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com"
        };
        var tasks = new List<TaskItem>
        {
            new()
            {
                Id = 1,
                Title = "ToDo",
                Status = TaskStatuses.ToDo,
                Priority = "Medium",
                InternId = internId,
                DueDate = now.AddDays(1)
            },
            new()
            {
                Id = 2,
                Title = "InProgress",
                Status = TaskStatuses.InProgress,
                Priority = "High",
                InternId = internId,
                DueDate = null
            },
            new()
            {
                Id = 3,
                Title = "Done",
                Status = TaskStatuses.Done,
                Priority = "Low",
                InternId = internId,
                DueDate = now.AddDays(-1)
            },
            new()
            {
                Id = 4,
                Title = "Overdue",
                Status = TaskStatuses.ToDo,
                Priority = "Medium",
                InternId = internId,
                DueDate = now.AddHours(-2)
            }
        };
        internRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId)).ReturnsAsync(intern);
        taskRepositoryMock.Setup(repository => repository.GetByInternIdAsync(internId)).ReturnsAsync(tasks);
        var service = new DashboardService(
            internRepositoryMock.Object,
            taskRepositoryMock.Object,
            departmentRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.GetStatsAsync(userId, Roles.Intern);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.InternCount);
        Assert.Equal(4, result.Data.TaskCount);
        Assert.Equal(1, result.Data.ToDoTaskCount);
        Assert.Equal(1, result.Data.InProgressTaskCount);
        Assert.Equal(1, result.Data.CompletedTaskCount);
        Assert.Equal(1, result.Data.OverdueTaskCount);
        Assert.Equal(0, result.Data.DepartmentCount);
        internRepositoryMock.Verify(repository => repository.GetByUserIdAsync(userId), Times.Once);
        taskRepositoryMock.Verify(repository => repository.GetByInternIdAsync(internId), Times.Once);
        internRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Never);
        departmentRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetStatsAsync_InternProfileDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var userId = 999;
        internRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId)).ReturnsAsync((Intern? )null);
        var service = new DashboardService(
            internRepositoryMock.Object,
            taskRepositoryMock.Object,
            departmentRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.GetStatsAsync(userId, Roles.Intern);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.NotFound, result.Type);
        Assert.Equal("Stajyer kaydı bulunamadı.", result.Message);
        taskRepositoryMock.Verify(repository => repository.GetByInternIdAsync(It.IsAny<int>()), Times.Never);
        internRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Never);
        departmentRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetStatsAsync_CompletedPastDueTask_ShouldNotCountAsOverdue()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var task = new TaskItem
        {
            Id = 1,
            Title = "Tamamlanmış Eski Görev",
            Status = TaskStatuses.Done,
            Priority = "Medium",
            InternId = 1,
            DueDate = DateTime.UtcNow.AddDays(-5)
        };
        internRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Intern>());
        taskRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<TaskItem> { task });
        departmentRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Department>());
        var service = new DashboardService(
            internRepositoryMock.Object,
            taskRepositoryMock.Object,
            departmentRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.GetStatsAsync(1, Roles.Admin);

        // ASSERT
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.CompletedTaskCount);
        Assert.Equal(0, result.Data.OverdueTaskCount);
    }

    [Fact]
    public async Task GetStatsAsync_TaskWithoutDueDate_ShouldNotCountAsOverdue()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var task = new TaskItem
        {
            Id = 1,
            Title = "Deadline Olmayan Görev",
            Status = TaskStatuses.ToDo,
            Priority = "Medium",
            InternId = 1,
            DueDate = null
        };
        internRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Intern>());
        taskRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<TaskItem> { task });
        departmentRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Department>());
        var service = new DashboardService(
            internRepositoryMock.Object,
            taskRepositoryMock.Object,
            departmentRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.GetStatsAsync(1, Roles.Admin);

        // ASSERT
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TaskCount);
        Assert.Equal(1, result.Data.ToDoTaskCount);
        Assert.Equal(0, result.Data.OverdueTaskCount);
    }

    [Fact]
    public async Task GetStatsAsync_OverdueInProgressTask_ShouldCountAsOverdue()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var departmentRepositoryMock = new Mock<IDepartmentRepository>();
        var task = new TaskItem
        {
            Id = 1,
            Title = "Geciken Devam Eden Görev",
            Status = TaskStatuses.InProgress,
            Priority = "High",
            InternId = 1,
            DueDate = DateTime.UtcNow.AddHours(-3)
        };
        internRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Intern>());
        taskRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<TaskItem> { task });
        departmentRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Department>());
        var service = new DashboardService(
            internRepositoryMock.Object,
            taskRepositoryMock.Object,
            departmentRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());

        // ACT
        var result = await service.GetStatsAsync(1, Roles.Admin);

        // ASSERT
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TaskCount);
        Assert.Equal(0, result.Data.InProgressTaskCount);
        Assert.Equal(1, result.Data.OverdueTaskCount);
        Assert.Equal(0, result.Data.CompletedTaskCount);
    }
}
