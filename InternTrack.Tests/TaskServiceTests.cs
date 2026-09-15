using InternTrack.Business.Common;
using InternTrack.Business.Services;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Moq;

namespace InternTrack.Tests;

public class TaskServiceTests
{
    [Fact]
    public async Task UpdateAsync_InternTriesToChangeToDoDirectlyToDone_ShouldReturnValidationError()
    {
        // ARRANGE

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 5;

        var existingTask =
            new TaskItem
            {
                Id = taskId,
                Title = "Test Görevi",
                Description = "Test açıklaması",
                Status = "ToDo",
                Priority = "Medium",
                InternId = internId,
                CreatedByUserId = userId
            };

        var intern =
            new Intern
            {
                Id = internId,
                UserId = userId,
                Name = "Test",
                Surname = "Intern",
                Email = "test@example.com"
            };

        var updateDto =
            new UpdateTaskDto
            {
                Title = existingTask.Title,
                Description = existingTask.Description,
                Status = "Done",
                Priority = existingTask.Priority,
                DueDate = existingTask.DueDate,
                InternId = internId,
                CanInternDeleteWhenCompleted = false
            };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service =
            new TaskService(
                taskRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.UpdateAsync(
                taskId,
                updateDto,
                userId,
                "Intern"
            );

        // ASSERT

        Assert.False(result.Success);

        Assert.Equal(
            ResultType.ValidationError,
            result.Type
        );

        Assert.Equal(
            "Görev tamamlanmadan önce başlatılmalıdır.",
            result.Message
        );

        taskRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    It.IsAny<TaskItem>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAsync_InternChangesInProgressToDone_ShouldUpdateTaskSuccessfully()
    {
        // ARRANGE

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 5;

        var existingTask =
            new TaskItem
            {
                Id = taskId,
                Title = "Test Görevi",
                Description = "Test açıklaması",
                Status = "InProgress",
                Priority = "Medium",
                InternId = internId,
                CreatedByUserId = userId,
                CompletedAt = null
            };

        var intern =
            new Intern
            {
                Id = internId,
                UserId = userId,
                Name = "Test",
                Surname = "Intern",
                Email = "test@example.com"
            };

        var updateDto =
            new UpdateTaskDto
            {
                Title = existingTask.Title,
                Description = existingTask.Description,
                Status = "Done",
                Priority = existingTask.Priority,
                DueDate = existingTask.DueDate,
                InternId = internId,
                CanInternDeleteWhenCompleted = false
            };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service =
            new TaskService(
                taskRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.UpdateAsync(
                taskId,
                updateDto,
                userId,
                "Intern"
            );

        // ASSERT

        Assert.True(result.Success);

        Assert.Equal(
            ResultType.Success,
            result.Type
        );

        Assert.Equal(
            "Görev güncellendi.",
            result.Message
        );

        Assert.Equal(
            "Done",
            existingTask.Status
        );

        Assert.NotNull(
            existingTask.CompletedAt
        );

        taskRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    existingTask
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAsync_InternTriesToUpdateAnotherInternTask_ShouldReturnForbidden()
    {
        // ARRANGE

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var userId = 10;
        var currentInternId = 3;
        var otherInternId = 7;
        var taskId = 5;

        var existingTask =
            new TaskItem
            {
                Id = taskId,
                Title = "Başka Stajyerin Görevi",
                Description = "Test açıklaması",
                Status = "InProgress",
                Priority = "Medium",
                InternId = otherInternId,
                CreatedByUserId = 99
            };

        var currentIntern =
            new Intern
            {
                Id = currentInternId,
                UserId = userId,
                Name = "Test",
                Surname = "Intern",
                Email = "test@example.com"
            };

        var updateDto =
            new UpdateTaskDto
            {
                Title = existingTask.Title,
                Description = existingTask.Description,
                Status = "Done",
                Priority = existingTask.Priority,
                DueDate = existingTask.DueDate,
                InternId = otherInternId,
                CanInternDeleteWhenCompleted = false
            };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync(userId))
            .ReturnsAsync(currentIntern);

        var service =
            new TaskService(
                taskRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.UpdateAsync(
                taskId,
                updateDto,
                userId,
                "Intern"
            );

        // ASSERT

        Assert.False(result.Success);

        Assert.Equal(
            ResultType.Forbidden,
            result.Type
        );

        Assert.Equal(
            "Bu görevi güncelleme yetkiniz yok.",
            result.Message
        );

        taskRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    It.IsAny<TaskItem>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAsync_InternChangesDueDateToPast_ShouldReturnValidationError()
    {
        // ARRANGE

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 5;

        var existingTask =
            new TaskItem
            {
                Id = taskId,
                Title = "Deadline Test Görevi",
                Description = "Deadline kontrolü",
                Status = "ToDo",
                Priority = "Medium",
                DueDate = DateTime.UtcNow.AddDays(3),
                InternId = internId,
                CreatedByUserId = userId
            };

        var intern =
            new Intern
            {
                Id = internId,
                UserId = userId,
                Name = "Test",
                Surname = "Intern",
                Email = "test@example.com"
            };

        var updateDto =
            new UpdateTaskDto
            {
                Title = existingTask.Title,
                Description = existingTask.Description,
                Status = "ToDo",
                Priority = existingTask.Priority,
                DueDate = DateTime.UtcNow.AddDays(-1),
                InternId = internId,
                CanInternDeleteWhenCompleted = false
            };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service =
            new TaskService(
                taskRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.UpdateAsync(
                taskId,
                updateDto,
                userId,
                "Intern"
            );

        // ASSERT

        Assert.False(
            result.Success
        );

        Assert.Equal(
            ResultType.ValidationError,
            result.Type
        );

        Assert.Equal(
            "Son teslim tarihi geçmiş bir tarih ve saat olamaz.",
            result.Message
        );

        taskRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    It.IsAny<TaskItem>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAsync_TaskDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var taskId = 999;
        var userId = 10;

        var updateDto =
            new UpdateTaskDto
            {
                Title = "Olmayan Görev",
                Description = "Test",
                Status = "ToDo",
                Priority = "Medium",
                DueDate = null,
                InternId = 3,
                CanInternDeleteWhenCompleted = false
            };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var service =
            new TaskService(
                taskRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.UpdateAsync(
                taskId,
                updateDto,
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
            "Görev bulunamadı.",
            result.Message
        );

        internRepositoryMock.Verify(
            repository =>
                repository.GetByUserIdAsync(
                    It.IsAny<int>()
                ),
            Times.Never
        );

        taskRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    It.IsAny<TaskItem>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAsync_AdminUpdatesTask_ShouldUpdateTaskSuccessfully()
    {
        // ARRANGE

        var taskRepositoryMock =
            new Mock<ITaskRepository>();

        var internRepositoryMock =
            new Mock<IInternRepository>();

        var taskId = 5;
        var adminUserId = 1;
        var newInternId = 8;

        var futureDueDate =
            DateTime.UtcNow.AddDays(5);

        var existingTask =
            new TaskItem
            {
                Id = taskId,
                Title = "Eski Başlık",
                Description = "Eski açıklama",
                Status = "ToDo",
                Priority = "Low",
                DueDate = null,
                InternId = 3,
                CreatedByUserId = adminUserId,
                CanInternDeleteWhenCompleted = false
            };

        var selectedIntern =
            new Intern
            {
                Id = newInternId,
                UserId = 20,
                Name = "Yeni",
                Surname = "Stajyer",
                Email = "newintern@example.com"
            };

        var updateDto =
            new UpdateTaskDto
            {
                Title = "Yeni Başlık",
                Description = "Yeni açıklama",
                Status = "InProgress",
                Priority = "High",
                DueDate = futureDueDate,
                InternId = newInternId,
                CanInternDeleteWhenCompleted = true
            };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(newInternId))
            .ReturnsAsync(selectedIntern);

        var service =
            new TaskService(
                taskRepositoryMock.Object,
                internRepositoryMock.Object
            );

        // ACT

        var result =
            await service.UpdateAsync(
                taskId,
                updateDto,
                adminUserId,
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

        Assert.Equal(
            "Görev güncellendi.",
            result.Message
        );

        Assert.Equal(
            "Yeni Başlık",
            existingTask.Title
        );

        Assert.Equal(
            "Yeni açıklama",
            existingTask.Description
        );

        Assert.Equal(
            "InProgress",
            existingTask.Status
        );

        Assert.Equal(
            "High",
            existingTask.Priority
        );

        Assert.Equal(
            newInternId,
            existingTask.InternId
        );

        Assert.True(
            existingTask.CanInternDeleteWhenCompleted
        );

        Assert.NotNull(
            existingTask.DueDate
        );

        taskRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    existingTask
                ),
            Times.Once
        );
    }
}