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
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 5;

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Test Görevi",
            Description = "Test açıklaması",
            Status = "ToDo",
            Priority = "Medium",
            InternId = internId,
            CreatedByUserId = userId
        };

        var intern = new Intern
        {
            Id = internId,
            UserId = userId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com"
        };

        var updateDto = new UpdateTaskDto
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
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            userId,
            "Intern");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal(
            "Görev tamamlanmadan önce başlatılmalıdır.",
            result.Message);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InternChangesInProgressToDone_ShouldUpdateTaskSuccessfully()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 5;

        var existingTask = new TaskItem
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

        var intern = new Intern
        {
            Id = internId,
            UserId = userId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com"
        };

        var updateDto = new UpdateTaskDto
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
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            userId,
            "Intern");

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Görev güncellendi.", result.Message);
        Assert.Equal("Done", existingTask.Status);
        Assert.NotNull(existingTask.CompletedAt);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(existingTask),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InternTriesToUpdateAnotherInternTask_ShouldReturnForbidden()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var userId = 10;
        var currentInternId = 3;
        var otherInternId = 7;
        var taskId = 5;

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Başka Stajyerin Görevi",
            Description = "Test açıklaması",
            Status = "InProgress",
            Priority = "Medium",
            InternId = otherInternId,
            CreatedByUserId = 99
        };

        var currentIntern = new Intern
        {
            Id = currentInternId,
            UserId = userId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com"
        };

        var updateDto = new UpdateTaskDto
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
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(userId))
            .ReturnsAsync(currentIntern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            userId,
            "Intern");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.Forbidden, result.Type);
        Assert.Equal(
            "Bu görevi güncelleme yetkiniz yok.",
            result.Message);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InternChangesDueDateToPast_ShouldReturnValidationError()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 5;

        var existingTask = new TaskItem
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

        var intern = new Intern
        {
            Id = internId,
            UserId = userId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com"
        };

        var updateDto = new UpdateTaskDto
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
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            userId,
            "Intern");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);
        Assert.Equal(
            "Son teslim tarihi geçmiş bir tarih ve saat olamaz.",
            result.Message);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_TaskDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 999;
        var userId = 10;

        var updateDto = new UpdateTaskDto
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
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            userId,
            "Intern");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.NotFound, result.Type);
        Assert.Equal("Görev bulunamadı.", result.Message);

        internRepositoryMock.Verify(
            repository => repository.GetByUserIdAsync(It.IsAny<int>()),
            Times.Never);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AdminUpdatesTask_ShouldUpdateTaskSuccessfully()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 5;
        var adminUserId = 1;
        var newInternId = 8;

        var futureDueDate =
            DateTime.UtcNow.AddDays(5);

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Eski Başlık",
            Description = "Eski açıklama",
            Status = "ToDo",
            Priority = "Low",
            DueDate = null,
            InternId = 3,
            CreatedByUserId = adminUserId,
            CanInternDeleteWhenCompleted = false,
            IsActive = true
        };

        var selectedIntern = new Intern
        {
            Id = newInternId,
            UserId = 20,
            Name = "Yeni",
            Surname = "Stajyer",
            Email = "newintern@example.com",
            IsActive = true
        };

        var updateDto = new UpdateTaskDto
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
                repository.GetByIdIncludingInactiveAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(newInternId))
            .ReturnsAsync(selectedIntern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            adminUserId,
            "Admin");

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Görev güncellendi.", result.Message);

        Assert.Equal("Yeni Başlık", existingTask.Title);
        Assert.Equal("Yeni açıklama", existingTask.Description);
        Assert.Equal("InProgress", existingTask.Status);
        Assert.Equal("High", existingTask.Priority);
        Assert.Equal(newInternId, existingTask.InternId);
        Assert.True(existingTask.CanInternDeleteWhenCompleted);
        Assert.NotNull(existingTask.DueDate);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(existingTask),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_OverdueToDoTaskCannotBeStarted_ShouldReturnValidationError()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 20;

        var pastDueDate =
            DateTime.UtcNow.AddHours(-2);

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Geciken Görev",
            Description = "Başlatma kontrolü",
            Status = "ToDo",
            Priority = "Medium",
            DueDate = pastDueDate,
            InternId = internId,
            CreatedByUserId = userId
        };

        var intern = new Intern
        {
            Id = internId,
            UserId = userId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com"
        };

        var updateDto = new UpdateTaskDto
        {
            Title = existingTask.Title,
            Description = existingTask.Description,
            Status = "InProgress",
            Priority = existingTask.Priority,
            DueDate = existingTask.DueDate,
            InternId = internId,
            CanInternDeleteWhenCompleted = false
        };

        taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            userId,
            "Intern");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Son teslim tarihi geçmiş görevler stajyer tarafından düzenlenemez.",
            result.Message);

        Assert.Equal("ToDo", existingTask.Status);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OverdueInProgressTaskCannotBeCompleted_ShouldReturnValidationError()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 21;

        var pastDueDate =
            DateTime.UtcNow.AddHours(-2);

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Geciken Devam Eden Görev",
            Description = "Tamamlama kontrolü",
            Status = "InProgress",
            Priority = "High",
            DueDate = pastDueDate,
            InternId = internId,
            CreatedByUserId = userId,
            CompletedAt = null
        };

        var intern = new Intern
        {
            Id = internId,
            UserId = userId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com"
        };

        var updateDto = new UpdateTaskDto
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
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            userId,
            "Intern");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Son teslim tarihi geçmiş görevler stajyer tarafından düzenlenemez.",
            result.Message);

        Assert.Equal("InProgress", existingTask.Status);
        Assert.Null(existingTask.CompletedAt);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_HRChangesDueDateToPast_ShouldReturnValidationError()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 30;
        var hrUserId = 2;
        var internId = 3;

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "HR Deadline Test",
            Description = "Deadline değişikliği",
            Status = "ToDo",
            Priority = "Medium",
            DueDate = DateTime.UtcNow.AddDays(2),
            InternId = internId,
            CreatedByUserId = 1,
            IsActive = true
        };

        var selectedIntern = new Intern
        {
            Id = internId,
            UserId = 10,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            IsActive = true
        };

        var updateDto = new UpdateTaskDto
        {
            Title = existingTask.Title,
            Description = existingTask.Description,
            Status = existingTask.Status,
            Priority = existingTask.Priority,
            DueDate = DateTime.UtcNow.AddHours(-2),
            InternId = internId,
            CanInternDeleteWhenCompleted = false
        };

        taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByIdAsync(internId))
            .ReturnsAsync(selectedIntern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            hrUserId,
            "HR");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Son teslim tarihi geçmiş bir tarih ve saat olamaz.",
            result.Message);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_HRStartsOverdueTask_ShouldReturnValidationError()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 31;
        var hrUserId = 2;
        var internId = 3;

        var pastDueDate =
            DateTime.UtcNow.AddHours(-2);

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Gecikmiş HR Görevi",
            Description = "Status kontrolü",
            Status = "ToDo",
            Priority = "Medium",
            DueDate = pastDueDate,
            InternId = internId,
            CreatedByUserId = 1,
            IsActive = true
        };

        var selectedIntern = new Intern
        {
            Id = internId,
            UserId = 10,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            IsActive = true
        };

        var updateDto = new UpdateTaskDto
        {
            Title = existingTask.Title,
            Description = existingTask.Description,
            Status = "InProgress",
            Priority = existingTask.Priority,
            DueDate = pastDueDate,
            InternId = internId,
            CanInternDeleteWhenCompleted = false
        };

        taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByIdAsync(internId))
            .ReturnsAsync(selectedIntern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            hrUserId,
            "HR");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.ValidationError, result.Type);

        Assert.Equal(
            "Son teslim tarihi geçmiş görev başlatılamaz veya tamamlanamaz. Önce son teslim tarihini güncelleyin.",
            result.Message);

        Assert.Equal("ToDo", existingTask.Status);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AdminCanUpdateInactiveTask()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 32;
        var adminUserId = 1;
        var internId = 3;

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Pasif Görev",
            Description = "Eski açıklama",
            Status = "ToDo",
            Priority = "Low",
            DueDate = DateTime.UtcNow.AddDays(2),
            InternId = internId,
            CreatedByUserId = adminUserId,
            IsActive = false
        };

        var updateDto = new UpdateTaskDto
        {
            Title = "Pasif Görev Güncellendi",
            Description = "Yeni açıklama",
            Status = "ToDo",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(3),
            InternId = internId,
            CanInternDeleteWhenCompleted = true,
            IsActive = false
        };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingInactiveAsync(taskId))
            .ReturnsAsync(existingTask);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            adminUserId,
            "Admin");

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);

        Assert.Equal(
            "Pasif Görev Güncellendi",
            existingTask.Title);

        Assert.Equal("High", existingTask.Priority);
        Assert.False(existingTask.IsActive);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(existingTask),
            Times.Once);

        taskRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AdminCanChangeInactiveTaskToActive()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 33;
        var adminUserId = 1;
        var internId = 3;

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Aktifleştirilecek Görev",
            Description = "Test",
            Status = "ToDo",
            Priority = "Medium",
            DueDate = DateTime.UtcNow.AddDays(2),
            InternId = internId,
            CreatedByUserId = adminUserId,
            IsActive = false
        };

        var activeIntern = new Intern
        {
            Id = internId,
            UserId = 10,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            IsActive = true
        };

        var updateDto = new UpdateTaskDto
        {
            Title = existingTask.Title,
            Description = existingTask.Description,
            Status = existingTask.Status,
            Priority = existingTask.Priority,
            DueDate = existingTask.DueDate,
            InternId = internId,
            CanInternDeleteWhenCompleted = false,
            IsActive = true
        };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingInactiveAsync(taskId))
            .ReturnsAsync(existingTask);

        internRepositoryMock
            .Setup(repository => repository.GetByIdAsync(internId))
            .ReturnsAsync(activeIntern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.UpdateAsync(
            taskId,
            updateDto,
            adminUserId,
            "Admin");

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.True(existingTask.IsActive);

        internRepositoryMock.Verify(
            repository => repository.GetByIdAsync(internId),
            Times.Once);

        taskRepositoryMock.Verify(
            repository => repository.UpdateAsync(existingTask),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_TaskDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 999;
        var userId = 1;

        taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.DeleteAsync(
            taskId,
            userId,
            "Admin");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.NotFound, result.Type);
        Assert.Equal("Görev bulunamadı.", result.Message);

        taskRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_AdminDeletesTask_ShouldDeleteSuccessfully()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 5;
        var adminUserId = 1;

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Silinecek Görev",
            Status = "ToDo",
            Priority = "Medium",
            InternId = 3,
            CreatedByUserId = adminUserId,
            IsActive = true
        };

        taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.DeleteAsync(
            taskId,
            adminUserId,
            "Admin");

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Görev silindi.", result.Message);

        taskRepositoryMock.Verify(
            repository => repository.DeleteAsync(task),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_HRTriesToDeleteTask_ShouldReturnForbidden()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 5;
        var hrUserId = 2;

        var task = new TaskItem
        {
            Id = taskId,
            Title = "HR Silme Yetki Testi",
            Status = "ToDo",
            Priority = "Medium",
            InternId = 3,
            CreatedByUserId = 1,
            IsActive = true
        };

        taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.DeleteAsync(
            taskId,
            hrUserId,
            "HR");

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.Forbidden, result.Type);
        Assert.Equal(
            "Bu işlem için yetkiniz yok.",
            result.Message);

        taskRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_InternCreatedOwnTask_ShouldDeleteRegardlessOfCompletionPermission()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var userId = 10;
        var internId = 3;
        var taskId = 40;

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Stajyerin Kendi Görevi",
            Status = "ToDo",
            Priority = "Medium",
            InternId = internId,
            CreatedByUserId = userId,
            CanInternDeleteWhenCompleted = false,
            IsActive = true
        };

        var intern = new Intern
        {
            Id = internId,
            UserId = userId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            IsActive = true
        };

        taskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        internRepositoryMock
            .Setup(repository => repository.GetByUserIdAsync(userId))
            .ReturnsAsync(intern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result = await service.DeleteAsync(
            taskId,
            userId,
            "Intern");

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);
        Assert.Equal("Görev silindi.", result.Message);

        taskRepositoryMock.Verify(
            repository => repository.DeleteAsync(task),
            Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_TaskDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 999;

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingInactiveAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result =
            await service.RestoreAsync(taskId);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.NotFound, result.Type);
        Assert.Equal("Görev bulunamadı.", result.Message);

        taskRepositoryMock.Verify(
            repository => repository.RestoreAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_TaskAlreadyActive_ShouldReturnConflict()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 5;

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Aktif Görev",
            Status = "ToDo",
            Priority = "Medium",
            InternId = 3,
            IsActive = true
        };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingInactiveAsync(taskId))
            .ReturnsAsync(task);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result =
            await service.RestoreAsync(taskId);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.Conflict, result.Type);
        Assert.Equal("Görev zaten aktif.", result.Message);

        internRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<int>()),
            Times.Never);

        taskRepositoryMock.Verify(
            repository => repository.RestoreAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_LinkedInternIsInactive_ShouldReturnConflict()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 5;
        var internId = 3;

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Pasif Görev",
            Status = "ToDo",
            Priority = "Medium",
            InternId = internId,
            IsActive = false
        };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingInactiveAsync(taskId))
            .ReturnsAsync(task);

        internRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(internId))
            .ReturnsAsync((Intern?)null);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result =
            await service.RestoreAsync(taskId);

        // ASSERT
        Assert.False(result.Success);
        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal(
            "Görevin atandığı stajyer pasif veya bulunamadı. Önce stajyeri aktif hale getirin.",
            result.Message);

        taskRepositoryMock.Verify(
            repository => repository.RestoreAsync(It.IsAny<TaskItem>()),
            Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_InactiveTaskWithActiveIntern_ShouldRestoreSuccessfully()
    {
        // ARRANGE
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var internRepositoryMock = new Mock<IInternRepository>();

        var taskId = 5;
        var internId = 3;

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Pasif Görev",
            Status = "ToDo",
            Priority = "Medium",
            InternId = internId,
            IsActive = false
        };

        var intern = new Intern
        {
            Id = internId,
            UserId = 10,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            IsActive = true
        };

        taskRepositoryMock
            .Setup(repository =>
                repository.GetByIdIncludingInactiveAsync(taskId))
            .ReturnsAsync(task);

        internRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(internId))
            .ReturnsAsync(intern);

        var service = new TaskService(
            taskRepositoryMock.Object,
            internRepositoryMock.Object);

        // ACT
        var result =
            await service.RestoreAsync(taskId);

        // ASSERT
        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.Type);

        Assert.Equal(
            "Görev tekrar aktif hale getirildi.",
            result.Message);

        taskRepositoryMock.Verify(
            repository => repository.RestoreAsync(task),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InternUpdatingAssignedTask_ShouldPreserveManagementFields()
    {
        var taskRepository = new Mock<ITaskRepository>();
        var internRepository = new Mock<IInternRepository>();
        var dueDate = DateTime.UtcNow.AddDays(2);
        var task = new TaskItem
        {
            Id = 5,
            Title = "Original title",
            Description = "Original description",
            Status = "InProgress",
            Priority = "Low",
            DueDate = dueDate,
            InternId = 3,
            CreatedByUserId = 1,
            CanInternDeleteWhenCompleted = false,
            IsActive = true
        };
        var intern = new Intern
        {
            Id = 3,
            UserId = 10,
            Name = "Intern",
            Email = "intern@example.com"
        };
        taskRepository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(task);
        internRepository.Setup(repository => repository.GetByUserIdAsync(10)).ReturnsAsync(intern);
        var service = new TaskService(taskRepository.Object, internRepository.Object);
        var dto = new UpdateTaskDto
        {
            Title = "Changed title",
            Description = "Changed description",
            Status = "Done",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(-1),
            InternId = 99,
            CanInternDeleteWhenCompleted = true,
            IsActive = false
        };

        var result = await service.UpdateAsync(5, dto, 10, "Intern");

        Assert.True(result.Success);
        Assert.Equal("Done", task.Status);
        Assert.NotNull(task.CompletedAt);
        Assert.Equal("Original title", task.Title);
        Assert.Equal("Original description", task.Description);
        Assert.Equal("Low", task.Priority);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(3, task.InternId);
        Assert.False(task.CanInternDeleteWhenCompleted);
        Assert.True(task.IsActive);
        taskRepository.Verify(repository => repository.UpdateAsync(task), Times.Once);
        taskRepository.Verify(repository => repository.GetByIdIncludingInactiveAsync(It.IsAny<int>()), Times.Never);
        internRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }
}