using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using Microsoft.Extensions.Logging;

namespace InternTrack.Business.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IInternRepository _internRepository;
    private readonly ILogger<TaskService>? _logger;

    public TaskService(
        ITaskRepository taskRepository,
        IInternRepository internRepository,
        ILogger<TaskService>? logger = null)
    {
        _taskRepository = taskRepository;
        _internRepository = internRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<List<TaskItem>>> GetAllAsync(
        int userId,
        string role)
    {
        if (role == "Admin" || role == "HR")
        {
            var allTasks =
                await _taskRepository.GetAllAsync();

            return ServiceResult<List<TaskItem>>
                .Ok(allTasks);
        }

        var intern =
            await _internRepository.GetByUserIdAsync(
                userId
            );

        if (intern == null)
        {
            _logger?.LogWarning(
                "Task list could not be retrieved because intern profile was not found. UserId: {UserId}",
                userId
            );

            return ServiceResult<List<TaskItem>>
                .NotFound(
                    "Stajyer profili bulunamadı."
                );
        }

        var tasks =
            await _taskRepository.GetByInternIdAsync(
                intern.Id
            );

        return ServiceResult<List<TaskItem>>
            .Ok(tasks);
    }

    public async Task<ServiceResult<TaskItem>> GetByIdAsync(
        int id,
        int userId,
        string role)
    {
        var task =
            await _taskRepository.GetByIdAsync(id);

        if (task == null)
        {
            _logger?.LogWarning(
                "Task was not found. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId
            );

            return ServiceResult<TaskItem>
                .NotFound(
                    "Görev bulunamadı."
                );
        }

        if (role == "Intern")
        {
            var intern =
                await _internRepository.GetByUserIdAsync(
                    userId
                );

            if (intern == null)
            {
                _logger?.LogWarning(
                    "Task access failed because intern profile was not found. TaskId: {TaskId}, UserId: {UserId}",
                    id,
                    userId
                );

                return ServiceResult<TaskItem>
                    .NotFound(
                        "Stajyer profili bulunamadı."
                    );
            }

            if (task.InternId != intern.Id)
            {
                _logger?.LogWarning(
                    "Unauthorized task access attempt. TaskId: {TaskId}, UserId: {UserId}, InternId: {InternId}",
                    id,
                    userId,
                    intern.Id
                );

                return ServiceResult<TaskItem>
                    .Forbidden(
                        "Bu göreve erişim yetkiniz yok."
                    );
            }
        }

        return ServiceResult<TaskItem>
            .Ok(task);
    }

    public async Task<ServiceResult> AddAsync(
        CreateTaskDto dto,
        int userId,
        string role)
    {
        int internId;

        var dueDateUtc =
            dto.DueDate?.ToUniversalTime();

        if (
            dueDateUtc.HasValue &&
            dueDateUtc.Value < DateTime.UtcNow
        )
        {
            _logger?.LogWarning(
                "Task creation rejected because due date is in the past. UserId: {UserId}, Role: {Role}",
                userId,
                role
            );

            return ServiceResult.ValidationError(
                "Son teslim tarihi geçmiş bir tarih ve saat olamaz."
            );
        }

        if (role == "Intern")
        {
            var currentIntern =
                await _internRepository.GetByUserIdAsync(
                    userId
                );

            if (currentIntern == null)
            {
                _logger?.LogWarning(
                    "Task creation failed because intern profile was not found. UserId: {UserId}",
                    userId
                );

                return ServiceResult.NotFound(
                    "Stajyer profili bulunamadı."
                );
            }

            internId =
                currentIntern.Id;
        }
        else if (
            role == "Admin" ||
            role == "HR"
        )
        {
            var selectedIntern =
                await _internRepository.GetByIdAsync(
                    dto.InternId
                );

            if (selectedIntern == null)
            {
                _logger?.LogWarning(
                    "Task creation rejected because selected intern was not found. InternId: {InternId}, UserId: {UserId}",
                    dto.InternId,
                    userId
                );

                return ServiceResult.ValidationError(
                    "Stajyer bulunamadı."
                );
            }

            internId =
                selectedIntern.Id;
        }
        else
        {
            _logger?.LogWarning(
                "Unauthorized task creation attempt. UserId: {UserId}, Role: {Role}",
                userId,
                role
            );

            return ServiceResult.Forbidden(
                "Bu işlem için yetkiniz yok."
            );
        }

        var task = new TaskItem
        {
            Title =
                dto.Title.Trim(),

            Description =
                dto.Description?.Trim(),

            Status =
                dto.Status.Trim(),

            Priority =
                dto.Priority.Trim(),

            DueDate =
                dueDateUtc,

            InternId =
                internId,

            CreatedByUserId =
                userId,

            CanInternDeleteWhenCompleted =
                role == "Admin" ||
                role == "HR"
                    ? dto.CanInternDeleteWhenCompleted
                    : false,

            CompletedAt =
                dto.Status.Trim() == "Done"
                    ? DateTime.UtcNow
                    : null
        };

        await _taskRepository.AddAsync(
            task
        );

        _logger?.LogInformation(
            "Task created successfully. TaskId: {TaskId}, InternId: {InternId}, CreatedByUserId: {UserId}, Role: {Role}",
            task.Id,
            internId,
            userId,
            role
        );

        return ServiceResult.Ok(
            "Görev oluşturuldu."
        );
    }

    public async Task<ServiceResult> UpdateAsync(
        int id,
        UpdateTaskDto dto,
        int userId,
        string role)
    {
        var task =
            await _taskRepository.GetByIdAsync(
                id
            );

        if (task == null)
        {
            _logger?.LogWarning(
                "Task update failed because task was not found. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId
            );

            return ServiceResult.NotFound(
                "Görev bulunamadı."
            );
        }

        if (role == "Intern")
        {
            var intern =
                await _internRepository.GetByUserIdAsync(
                    userId
                );

            if (intern == null)
            {
                _logger?.LogWarning(
                    "Task update failed because intern profile was not found. TaskId: {TaskId}, UserId: {UserId}",
                    id,
                    userId
                );

                return ServiceResult.NotFound(
                    "Stajyer profili bulunamadı."
                );
            }

            if (task.InternId != intern.Id)
            {
                _logger?.LogWarning(
                    "Unauthorized task update attempt. TaskId: {TaskId}, UserId: {UserId}, InternId: {InternId}",
                    id,
                    userId,
                    intern.Id
                );

                return ServiceResult.Forbidden(
                    "Bu görevi güncelleme yetkiniz yok."
                );
            }

            var newStatus =
                dto.Status.Trim();

            if (
                task.Status == "ToDo" &&
                newStatus != "ToDo" &&
                newStatus != "InProgress"
            )
            {
                _logger?.LogWarning(
                    "Invalid task status transition. TaskId: {TaskId}, UserId: {UserId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                    id,
                    userId,
                    task.Status,
                    newStatus
                );

                return ServiceResult.ValidationError(
                    "Görev tamamlanmadan önce başlatılmalıdır."
                );
            }

            if (
                task.Status == "InProgress" &&
                newStatus != "InProgress" &&
                newStatus != "Done"
            )
            {
                _logger?.LogWarning(
                    "Invalid task status transition. TaskId: {TaskId}, UserId: {UserId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                    id,
                    userId,
                    task.Status,
                    newStatus
                );

                return ServiceResult.ValidationError(
                    "Geçersiz görev durumu."
                );
            }

            if (
                task.Status == "Done" &&
                newStatus != "Done"
            )
            {
                _logger?.LogWarning(
                    "Completed task reopen attempt rejected. TaskId: {TaskId}, UserId: {UserId}, NewStatus: {NewStatus}",
                    id,
                    userId,
                    newStatus
                );

                return ServiceResult.ValidationError(
                    "Tamamlanan görev tekrar açılamaz."
                );
            }

            if (
                task.Status == "InProgress" &&
                newStatus == "Done"
            )
            {
                task.CompletedAt =
                    DateTime.UtcNow;
            }

            task.Status =
                newStatus;

            var createdByCurrentIntern =
                task.CreatedByUserId ==
                userId;

            if (createdByCurrentIntern)
            {
                var newDueDateUtc =
                    dto.DueDate?.ToUniversalTime();

                var currentDueDateUtc =
                    task.DueDate?.ToUniversalTime();

                var dueDateChanged =
                    currentDueDateUtc !=
                    newDueDateUtc;

                if (
                    dueDateChanged &&
                    newDueDateUtc.HasValue &&
                    newDueDateUtc.Value <
                    DateTime.UtcNow
                )
                {
                    _logger?.LogWarning(
                        "Task update rejected because due date is in the past. TaskId: {TaskId}, UserId: {UserId}",
                        id,
                        userId
                    );

                    return ServiceResult.ValidationError(
                        "Son teslim tarihi geçmiş bir tarih ve saat olamaz."
                    );
                }

                task.Priority =
                    dto.Priority.Trim();

                task.DueDate =
                    newDueDateUtc;
            }

            await _taskRepository.UpdateAsync(
                task
            );

            _logger?.LogInformation(
                "Task updated successfully by intern. TaskId: {TaskId}, UserId: {UserId}, Status: {Status}",
                id,
                userId,
                task.Status
            );

            return ServiceResult.Ok(
                "Görev güncellendi."
            );
        }

        if (
            role != "Admin" &&
            role != "HR"
        )
        {
            _logger?.LogWarning(
                "Unauthorized task update attempt. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}",
                id,
                userId,
                role
            );

            return ServiceResult.Forbidden(
                "Bu işlem için yetkiniz yok."
            );
        }

        var selectedIntern =
            await _internRepository.GetByIdAsync(
                dto.InternId
            );

        if (selectedIntern == null)
        {
            _logger?.LogWarning(
                "Task update rejected because selected intern was not found. TaskId: {TaskId}, InternId: {InternId}, UserId: {UserId}",
                id,
                dto.InternId,
                userId
            );

            return ServiceResult.ValidationError(
                "Stajyer bulunamadı."
            );
        }

        var newAdminDueDateUtc =
            dto.DueDate?.ToUniversalTime();

        var currentAdminDueDateUtc =
            task.DueDate?.ToUniversalTime();

        var adminDueDateChanged =
            currentAdminDueDateUtc !=
            newAdminDueDateUtc;

        if (
            adminDueDateChanged &&
            newAdminDueDateUtc.HasValue &&
            newAdminDueDateUtc.Value <
            DateTime.UtcNow
        )
        {
            _logger?.LogWarning(
                "Task update rejected because due date is in the past. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}",
                id,
                userId,
                role
            );

            return ServiceResult.ValidationError(
                "Son teslim tarihi geçmiş bir tarih ve saat olamaz."
            );
        }

        var newAdminStatus =
            dto.Status.Trim();

        if (
            task.Status != "Done" &&
            newAdminStatus == "Done"
        )
        {
            task.CompletedAt =
                DateTime.UtcNow;
        }

        if (newAdminStatus != "Done")
        {
            task.CompletedAt =
                null;
        }

        task.Title =
            dto.Title.Trim();

        task.Description =
            dto.Description?.Trim();

        task.Status =
            newAdminStatus;

        task.Priority =
            dto.Priority.Trim();

        task.DueDate =
            newAdminDueDateUtc;

        task.InternId =
            dto.InternId;

        task.CanInternDeleteWhenCompleted =
            dto.CanInternDeleteWhenCompleted;

        await _taskRepository.UpdateAsync(
            task
        );

        _logger?.LogInformation(
            "Task updated successfully. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}, InternId: {InternId}, Status: {Status}",
            id,
            userId,
            role,
            task.InternId,
            task.Status
        );

        return ServiceResult.Ok(
            "Görev güncellendi."
        );
    }

    public async Task<ServiceResult> DeleteAsync(
        int id,
        int userId,
        string role)
    {
        var task =
            await _taskRepository.GetByIdAsync(
                id
            );

        if (task == null)
        {
            _logger?.LogWarning(
                "Task deletion failed because task was not found. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId
            );

            return ServiceResult.NotFound(
                "Görev bulunamadı."
            );
        }

        if (role == "Intern")
        {
            var intern =
                await _internRepository.GetByUserIdAsync(
                    userId
                );

            if (intern == null)
            {
                _logger?.LogWarning(
                    "Task deletion failed because intern profile was not found. TaskId: {TaskId}, UserId: {UserId}",
                    id,
                    userId
                );

                return ServiceResult.NotFound(
                    "Stajyer profili bulunamadı."
                );
            }

            if (task.InternId != intern.Id)
            {
                _logger?.LogWarning(
                    "Unauthorized task deletion attempt. TaskId: {TaskId}, UserId: {UserId}, InternId: {InternId}",
                    id,
                    userId,
                    intern.Id
                );

                return ServiceResult.Forbidden(
                    "Bu görevi silme yetkiniz yok."
                );
            }

            var createdByCurrentIntern =
                task.CreatedByUserId ==
                userId;

            if (createdByCurrentIntern)
            {
                await _taskRepository.DeleteAsync(
                    task
                );

                _logger?.LogInformation(
                    "Task deleted by its creator intern. TaskId: {TaskId}, UserId: {UserId}",
                    id,
                    userId
                );

                return ServiceResult.Ok(
                    "Görev silindi."
                );
            }

            var isCompleted =
                task.Status == "Done";

            if (
                !isCompleted ||
                !task.CanInternDeleteWhenCompleted
            )
            {
                _logger?.LogWarning(
                    "Intern task deletion rejected by business rule. TaskId: {TaskId}, UserId: {UserId}, Status: {Status}, CanInternDeleteWhenCompleted: {CanDelete}",
                    id,
                    userId,
                    task.Status,
                    task.CanInternDeleteWhenCompleted
                );

                return ServiceResult.Forbidden(
                    "Bu görevi silme yetkiniz yok."
                );
            }
        }
        else if (
            role != "Admin" &&
            role != "HR"
        )
        {
            _logger?.LogWarning(
                "Unauthorized task deletion attempt. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}",
                id,
                userId,
                role
            );

            return ServiceResult.Forbidden(
                "Bu işlem için yetkiniz yok."
            );
        }

        await _taskRepository.DeleteAsync(
            task
        );

        _logger?.LogInformation(
            "Task deleted successfully. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}",
            id,
            userId,
            role
        );

        return ServiceResult.Ok(
            "Görev silindi."
        );
    }
}