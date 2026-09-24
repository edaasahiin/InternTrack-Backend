using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Business.Services;
using InternTrack.Core.Models;
using InternTrack.Core.DTOs;
using InternTrack.DataAccess.Interfaces;
using Moq;

namespace InternTrack.Tests;

public class RoleAuthorizationTests
{
    [Theory]
    [InlineData("Intern", true)]
    [InlineData("intern", true)]
    [InlineData("INTERN", true)]
    [InlineData("Intern", false)]
    [InlineData("intern", false)]
    [InlineData("INTERN", false)]
    [InlineData("Unknown", true)]
    [InlineData("Unknown", false)]
    [InlineData("admin", false)]
    [InlineData("hr", false)]
    [InlineData("Admin", false)]
    [InlineData("HR", false)]
    public async Task ByIdReads_ShouldEnforceRoleAndOwnership(string role, bool ownsResource)
    {
        var currentIntern = new Intern { Id = 3, UserId = 10, Name = "Owner", Email = "owner@example.com" };
        var requestedIntern = ownsResource
            ? currentIntern
            : new Intern { Id = 7, UserId = 20, Name = "Other", Email = "other@example.com" };
        var task = new TaskItem { Id = 5, Title = "Protected task", Status = "ToDo", InternId = requestedIntern.Id };
        var interns = new Mock<IInternRepository>();
        var tasks = new Mock<ITaskRepository>();
        interns.Setup(repository => repository.GetByIdAsync(requestedIntern.Id)).ReturnsAsync(requestedIntern);
        interns.Setup(repository => repository.GetByUserIdAsync(currentIntern.UserId)).ReturnsAsync(currentIntern);
        tasks.Setup(repository => repository.GetByIdAsync(task.Id)).ReturnsAsync(task);
        var internService = new InternService(interns.Object, Mock.Of<IDepartmentRepository>(),
            Mock.Of<IUserRepository>(), Mock.Of<IAppLogger>());
        var taskService = new TaskService(tasks.Object, interns.Object, Mock.Of<IAppLogger>());

        var internResult = await internService.GetByIdAsync(requestedIntern.Id, currentIntern.UserId, role);
        var taskResult = await taskService.GetByIdAsync(task.Id, currentIntern.UserId, role);

        var allowed = role is "Admin" or "HR" || (ownsResource && role is "Intern" or "intern" or "INTERN");
        Assert.Equal(allowed ? ResultType.Success : ResultType.Forbidden, internResult.Type);
        Assert.Equal(allowed ? ResultType.Success : ResultType.Forbidden, taskResult.Type);
        if (allowed)
        {
            Assert.Equal(requestedIntern.Id, internResult.Data!.Id);
            Assert.Same(task, taskResult.Data);
        }
        else
        {
            Assert.Null(internResult.Data);
            Assert.Null(taskResult.Data);
        }
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("admin")]
    [InlineData("hr")]
    [InlineData(" Intern ")]
    [InlineData("")]
    public async Task UnsupportedRole_ShouldRejectEveryRoleDependentOperationBeforeDataAccess(string role)
    {
        var interns = new Mock<IInternRepository>(MockBehavior.Strict);
        var tasks = new Mock<ITaskRepository>(MockBehavior.Strict);
        var departments = new Mock<IDepartmentRepository>(MockBehavior.Strict);
        var internService = new InternService(interns.Object, departments.Object,
            Mock.Of<IUserRepository>(), Mock.Of<IAppLogger>());
        var taskService = new TaskService(tasks.Object, interns.Object, Mock.Of<IAppLogger>());
        var dashboardService = new DashboardService(interns.Object, tasks.Object, departments.Object, Mock.Of<IAppLogger>());

        Assert.Equal(ResultType.Forbidden, (await internService.GetAllAsync(10, role)).Type);
        Assert.Equal(ResultType.Forbidden, (await internService.GetByIdAsync(3, 10, role)).Type);
        Assert.Equal(ResultType.Forbidden, (await taskService.GetAllAsync(10, role)).Type);
        Assert.Equal(ResultType.Forbidden, (await taskService.GetByIdAsync(5, 10, role)).Type);
        Assert.Equal(ResultType.Forbidden, (await taskService.AddAsync(
            new CreateTaskDto { Title = "Task", Status = "ToDo" }, 10, role)).Type);
        Assert.Equal(ResultType.Forbidden, (await taskService.UpdateAsync(
            5, new UpdateTaskDto { Title = "Task", Status = "ToDo" }, 10, role)).Type);
        Assert.Equal(ResultType.Forbidden, (await taskService.DeactivateTaskAsync(5, 10, role)).Type);
        Assert.Equal(ResultType.Forbidden, (await dashboardService.GetStatsAsync(10, role)).Type);
    }

    [Theory]
    [InlineData("Intern")]
    [InlineData("intern")]
    [InlineData("INTERN")]
    public async Task InternListsAndDashboard_ShouldRemainScopedToOwnProfile(string role)
    {
        var intern = new Intern { Id = 3, UserId = 10, Name = "Owner", Email = "owner@example.com" };
        var ownedTasks = new List<TaskItem>
        {
            new() { Id = 5, Title = "Own task", Status = "ToDo", InternId = intern.Id }
        };
        var interns = new Mock<IInternRepository>(MockBehavior.Strict);
        var tasks = new Mock<ITaskRepository>(MockBehavior.Strict);
        var departments = new Mock<IDepartmentRepository>(MockBehavior.Strict);
        interns.Setup(repository => repository.GetByUserIdAsync(intern.UserId)).ReturnsAsync(intern);
        tasks.Setup(repository => repository.GetByInternIdAsync(intern.Id)).ReturnsAsync(ownedTasks);
        var internService = new InternService(interns.Object, departments.Object,
            Mock.Of<IUserRepository>(), Mock.Of<IAppLogger>());
        var taskService = new TaskService(tasks.Object, interns.Object, Mock.Of<IAppLogger>());
        var dashboardService = new DashboardService(interns.Object, tasks.Object, departments.Object, Mock.Of<IAppLogger>());

        Assert.Equal(intern.Id, Assert.Single((await internService.GetAllAsync(intern.UserId, role)).Data!).Id);
        Assert.Same(ownedTasks, (await taskService.GetAllAsync(intern.UserId, role)).Data);
        var stats = (await dashboardService.GetStatsAsync(intern.UserId, role)).Data;
        Assert.NotNull(stats);
        Assert.Equal(1, stats.InternCount);
        Assert.Equal(1, stats.TaskCount);
        Assert.Equal(0, stats.DepartmentCount);
    }

    [Theory]
    [InlineData("Intern")]
    [InlineData("intern")]
    [InlineData("INTERN")]
    public async Task InternCreation_ShouldIgnoreRequestedAssigneeAndManagementPermission(string role)
    {
        var intern = new Intern { Id = 3, UserId = 10, Name = "Owner", Email = "owner@example.com" };
        var interns = new Mock<IInternRepository>(MockBehavior.Strict);
        var tasks = new Mock<ITaskRepository>();
        interns.Setup(repository => repository.GetByUserIdAsync(intern.UserId)).ReturnsAsync(intern);
        var service = new TaskService(tasks.Object, interns.Object, Mock.Of<IAppLogger>());

        var result = await service.AddAsync(new CreateTaskDto
        {
            Title = "Own task", Status = "ToDo", Priority = "Medium",
            InternId = 99, CanInternDeleteWhenCompleted = true
        }, intern.UserId, role);

        Assert.True(result.Success);
        tasks.Verify(repository => repository.AddAsync(It.Is<TaskItem>(task =>
            task.InternId == intern.Id && task.CreatedByUserId == intern.UserId && !task.CanInternDeleteWhenCompleted)), Times.Once);
    }

    [Theory]
    [InlineData("Intern")]
    [InlineData("intern")]
    [InlineData("INTERN")]
    public async Task InternWrites_ShouldRejectAnotherInternsTaskWithoutChangingIt(string role)
    {
        var intern = new Intern { Id = 3, UserId = 10, Name = "Owner", Email = "owner@example.com" };
        var task = new TaskItem { Id = 5, Title = "Other task", Status = "InProgress", InternId = 7 };
        var interns = new Mock<IInternRepository>();
        var tasks = new Mock<ITaskRepository>();
        interns.Setup(repository => repository.GetByUserIdAsync(intern.UserId)).ReturnsAsync(intern);
        tasks.Setup(repository => repository.GetByIdAsync(task.Id)).ReturnsAsync(task);
        var service = new TaskService(tasks.Object, interns.Object, Mock.Of<IAppLogger>());

        var update = await service.UpdateAsync(task.Id, new UpdateTaskDto
        {
            Title = "Changed", Status = "Done", Priority = "Medium", InternId = intern.Id, IsActive = false
        }, intern.UserId, role);
        var deactivate = await service.DeactivateTaskAsync(task.Id, intern.UserId, role);

        Assert.Equal(ResultType.Forbidden, update.Type);
        Assert.Equal(ResultType.Forbidden, deactivate.Type);
        Assert.Equal("Other task", task.Title);
        Assert.Equal("InProgress", task.Status);
        Assert.True(task.IsActive);
        tasks.Verify(repository => repository.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
        tasks.Verify(repository => repository.DeactivateTaskAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Theory]
    [InlineData("Intern")]
    [InlineData("intern")]
    [InlineData("INTERN")]
    public async Task AssignedIntern_ShouldCompleteOwnTaskWithoutGainingManagementPermissions(string role)
    {
        var intern = new Intern { Id = 3, UserId = 10, Name = "Owner", Email = "owner@example.com" };
        var task = new TaskItem
        {
            Id = 5, Title = "Assigned task", Status = "InProgress", InternId = intern.Id,
            CreatedByUserId = 99, CanInternDeleteWhenCompleted = false
        };
        var interns = new Mock<IInternRepository>();
        var tasks = new Mock<ITaskRepository>();
        interns.Setup(repository => repository.GetByUserIdAsync(intern.UserId)).ReturnsAsync(intern);
        tasks.Setup(repository => repository.GetByIdAsync(task.Id)).ReturnsAsync(task);
        var service = new TaskService(tasks.Object, interns.Object, Mock.Of<IAppLogger>());

        var update = await service.UpdateAsync(task.Id, new UpdateTaskDto
        {
            Title = "Changed", Status = "Done", Priority = "High", InternId = 7,
            IsActive = false, CanInternDeleteWhenCompleted = true
        }, intern.UserId, role);
        var deactivate = await service.DeactivateTaskAsync(task.Id, intern.UserId, role);

        Assert.True(update.Success);
        Assert.Equal("Done", task.Status);
        Assert.NotNull(task.CompletedAt);
        Assert.Equal("Assigned task", task.Title);
        Assert.Equal(intern.Id, task.InternId);
        Assert.True(task.IsActive);
        Assert.False(task.CanInternDeleteWhenCompleted);
        Assert.Equal(ResultType.Forbidden, deactivate.Type);
        tasks.Verify(repository => repository.UpdateAsync(task), Times.Once);
        tasks.Verify(repository => repository.DeactivateTaskAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Theory]
    [InlineData("Intern")]
    [InlineData("intern")]
    [InlineData("INTERN")]
    public async Task MissingInternProfile_ShouldNeverBypassOwnershipChecks(string role)
    {
        var interns = new Mock<IInternRepository>();
        var tasks = new Mock<ITaskRepository>();
        interns.Setup(repository => repository.GetByIdAsync(3))
            .ReturnsAsync(new Intern { Id = 3, Name = "Other", Email = "other@example.com" });
        tasks.Setup(repository => repository.GetByIdAsync(5))
            .ReturnsAsync(new TaskItem { Id = 5, Title = "Other task", Status = "ToDo", InternId = 3 });
        var internService = new InternService(interns.Object, Mock.Of<IDepartmentRepository>(),
            Mock.Of<IUserRepository>(), Mock.Of<IAppLogger>());
        var taskService = new TaskService(tasks.Object, interns.Object, Mock.Of<IAppLogger>());

        var internResult = await internService.GetByIdAsync(3, 10, role);
        var taskResult = await taskService.GetByIdAsync(5, 10, role);

        Assert.Equal(ResultType.NotFound, internResult.Type);
        Assert.Null(internResult.Data);
        Assert.Equal(ResultType.NotFound, taskResult.Type);
        Assert.Null(taskResult.Data);
    }
}
