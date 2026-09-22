using InternTrack.Core.Constants;
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

    private static bool AreSameMinuteUtc(
        DateTime? first,
        DateTime? second)
    {
        if (!first.HasValue && !second.HasValue)
        {
            return true;
        }

        if (!first.HasValue || !second.HasValue)
        {
            return false;
        }

        var firstUtc = first.Value.ToUniversalTime();
        var secondUtc = second.Value.ToUniversalTime();

        return firstUtc.Year == secondUtc.Year &&
            firstUtc.Month == secondUtc.Month &&
            firstUtc.Day == secondUtc.Day &&
            firstUtc.Hour == secondUtc.Hour &&
            firstUtc.Minute == secondUtc.Minute;
    }

    public async Task<ServiceResult<List<TaskItem>>> GetAllAsync(
        int userId,
        string role)
    {
        if (role == Roles.Admin || role == Roles.HR)
        {
            var allTasks =
                await _taskRepository.GetAllAsync();

            return ServiceResult<List<TaskItem>>.Ok(
                allTasks);
        }

        var intern =
            await _internRepository.GetByUserIdAsync(
                userId);

        if (intern == null)
        {
            _logger?.LogWarning(
                "Task list could not be retrieved because intern profile was not found. UserId: {UserId}",
                userId);

            return ServiceResult<List<TaskItem>>.NotFound(
                "Stajyer profili bulunamadı.");
        }

        var tasks =
            await _taskRepository.GetByInternIdAsync(
                intern.Id);

        return ServiceResult<List<TaskItem>>.Ok(
            tasks);
    }

    public async Task<ServiceResult<List<TaskItem>>>
        GetAllIncludingInactiveAsync()
    {
        var tasks =
            await _taskRepository
                .GetAllIncludingInactiveAsync();

        return ServiceResult<List<TaskItem>>.Ok(
            tasks);
    }

    public async Task<ServiceResult<TaskItem>>
        GetByIdAsync(
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
                userId);

            return ServiceResult<TaskItem>.NotFound(
                "Görev bulunamadı.");
        }

        if (role == Roles.Intern)
        {
            var intern =
                await _internRepository
                    .GetByUserIdAsync(userId);

            if (intern == null)
            {
                _logger?.LogWarning(
                    "Task access failed because intern profile was not found. TaskId: {TaskId}, UserId: {UserId}",
                    id,
                    userId);

                return ServiceResult<TaskItem>.NotFound(
                    "Stajyer profili bulunamadı.");
            }

            if (task.InternId != intern.Id)
            {
                _logger?.LogWarning(
                    "Unauthorized task access attempt. TaskId: {TaskId}, UserId: {UserId}, InternId: {InternId}",
                    id,
                    userId,
                    intern.Id);

                return ServiceResult<TaskItem>.Forbidden(
                    "Bu göreve erişim yetkiniz yok.");
            }
        }

        return ServiceResult<TaskItem>.Ok(task);
    }

    public async Task<ServiceResult> AddAsync(
        CreateTaskDto dto,
        int userId,
        string role)
    {
        int internId;

        var dueDateUtc =
            dto.DueDate?.ToUniversalTime();

        if (dueDateUtc.HasValue &&
            dueDateUtc.Value < DateTime.UtcNow)
        {
            _logger?.LogWarning(
                "Task creation rejected because due date is in the past. UserId: {UserId}, Role: {Role}",
                userId,
                role);

            return ServiceResult.ValidationError(
                "Son teslim tarihi geçmiş bir tarih ve saat olamaz.");
        }

        if (role == Roles.Intern)
        {
            var currentIntern =
                await _internRepository
                    .GetByUserIdAsync(userId);

            if (currentIntern == null)
            {
                _logger?.LogWarning(
                    "Task creation failed because intern profile was not found. UserId: {UserId}",
                    userId);

                return ServiceResult.NotFound(
                    "Stajyer profili bulunamadı.");
            }

            internId = currentIntern.Id;
        }
        else if (
            role == Roles.Admin ||
            role == Roles.HR)
        {
            var selectedIntern =
                await _internRepository
                    .GetByIdAsync(dto.InternId);

            if (selectedIntern == null)
            {
                _logger?.LogWarning(
                    "Task creation rejected because selected intern was not found. InternId: {InternId}, UserId: {UserId}",
                    dto.InternId,
                    userId);

                return ServiceResult.ValidationError(
                    "Stajyer bulunamadı.");
            }

            internId = selectedIntern.Id;
        }
        else
        {
            _logger?.LogWarning(
                "Unauthorized task creation attempt. UserId: {UserId}, Role: {Role}",
                userId,
                role);

            return ServiceResult.Forbidden(
                "Bu işlem için yetkiniz yok.");
        }

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Status = dto.Status.Trim(),
            Priority = dto.Priority.Trim(),
            DueDate = dueDateUtc,
            InternId = internId,
            CreatedByUserId = userId,

            CanInternDeleteWhenCompleted =
                role == Roles.Admin ||
                role == Roles.HR
                    ? dto.CanInternDeleteWhenCompleted
                    : false,

            CompletedAt =
                dto.Status.Trim() == TaskStatuses.Done
                    ? DateTime.UtcNow
                    : null
        };

        await _taskRepository.AddAsync(task);

        _logger?.LogInformation(
            "Task created successfully. TaskId: {TaskId}, InternId: {InternId}, CreatedByUserId: {UserId}, Role: {Role}",
            task.Id,
            internId,
            userId,
            role);

        return ServiceResult.Ok(
            "Görev oluşturuldu.");
    }

    public async Task<ServiceResult> UpdateAsync(
        int id,
        UpdateTaskDto dto,
        int userId,
        string role)
    {
        var task =
            role == Roles.Admin
                ? await _taskRepository
                    .GetByIdIncludingInactiveAsync(id)
                : await _taskRepository
                    .GetByIdAsync(id);

        if (task == null)
        {
            _logger?.LogWarning(
                "Task update failed because task was not found. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId);

            return ServiceResult.NotFound(
                "Görev bulunamadı.");
        }

        if (role == Roles.Intern)
        {
            return await UpdateForInternAsync(
                task,
                dto,
                id,
                userId);
        }

        return await UpdateForManagementAsync(
            task,
            dto,
            id,
            userId,
            role);
    }

    public async Task<ServiceResult> DeleteAsync(
        int id,
        int userId,
        string role)
    {
        var task =
            await _taskRepository.GetByIdAsync(id);

        if (task == null)
        {
            _logger?.LogWarning(
                "Task deletion failed because task was not found. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId);

            return ServiceResult.NotFound(
                "Görev bulunamadı.");
        }

        if (role == Roles.Intern)
        {
            var intern =
                await _internRepository
                    .GetByUserIdAsync(userId);

            if (intern == null)
            {
                _logger?.LogWarning(
                    "Task deletion failed because intern profile was not found. TaskId: {TaskId}, UserId: {UserId}",
                    id,
                    userId);

                return ServiceResult.NotFound(
                    "Stajyer profili bulunamadı.");
            }

            if (task.InternId != intern.Id)
            {
                _logger?.LogWarning(
                    "Unauthorized task deletion attempt. TaskId: {TaskId}, UserId: {UserId}, InternId: {InternId}",
                    id,
                    userId,
                    intern.Id);

                return ServiceResult.Forbidden(
                    "Bu görevi silme yetkiniz yok.");
            }

            var createdByCurrentIntern =
                task.CreatedByUserId == userId;

            if (createdByCurrentIntern)
            {
                await _taskRepository
                    .DeleteAsync(task);

                _logger?.LogInformation(
                    "Task soft deleted by its creator intern. TaskId: {TaskId}, UserId: {UserId}",
                    id,
                    userId);

                return ServiceResult.Ok(
                    "Görev silindi.");
            }

            var isCompleted =
                task.Status == TaskStatuses.Done;

            if (!isCompleted ||
                !task.CanInternDeleteWhenCompleted)
            {
                _logger?.LogWarning(
                    "Intern task deletion rejected by business rule. TaskId: {TaskId}, UserId: {UserId}, Status: {Status}, CanInternDeleteWhenCompleted: {CanDelete}",
                    id,
                    userId,
                    task.Status,
                    task.CanInternDeleteWhenCompleted);

                return ServiceResult.Forbidden(
                    "Bu görevi silme yetkiniz yok.");
            }
        }
        else if (role != Roles.Admin)
        {
            _logger?.LogWarning(
                "Unauthorized task deletion attempt. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}",
                id,
                userId,
                role);

            return ServiceResult.Forbidden(
                "Bu işlem için yetkiniz yok.");
        }

        await _taskRepository.DeleteAsync(task);

        _logger?.LogInformation(
            "Task soft deleted successfully. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}",
            id,
            userId,
            role);

        return ServiceResult.Ok(
            "Görev silindi.");
    }

    public async Task<ServiceResult> RestoreAsync(
        int id)
    {
        var task =
            await _taskRepository
                .GetByIdIncludingInactiveAsync(id);

        if (task == null)
        {
            _logger?.LogWarning(
                "Task restore failed because task was not found. TaskId: {TaskId}",
                id);

            return ServiceResult.NotFound(
                "Görev bulunamadı.");
        }

        if (task.IsActive)
        {
            _logger?.LogWarning(
                "Task restore rejected because task is already active. TaskId: {TaskId}",
                id);

            return ServiceResult.Conflict(
                "Görev zaten aktif.");
        }

        var intern =
            await _internRepository
                .GetByIdAsync(task.InternId);

        if (intern == null)
        {
            _logger?.LogWarning(
                "Task restore rejected because linked intern is inactive or unavailable. TaskId: {TaskId}, InternId: {InternId}",
                task.Id,
                task.InternId);

            return ServiceResult.Conflict(
                "Görevin atandığı stajyer pasif veya bulunamadı. Önce stajyeri aktif hale getirin.");
        }

        await _taskRepository.RestoreAsync(task);

        _logger?.LogInformation(
            "Task restored successfully. TaskId: {TaskId}, InternId: {InternId}",
            task.Id,
            task.InternId);

        return ServiceResult.Ok(
            "Görev tekrar aktif hale getirildi.");
    }

    private async Task<ServiceResult>
        UpdateForInternAsync(
            TaskItem task,
            UpdateTaskDto dto,
            int id,
            int userId)
    {
        var intern =
            await _internRepository
                .GetByUserIdAsync(userId);

        if (intern == null)
        {
            _logger?.LogWarning(
                "Task update failed because intern profile was not found. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId);

            return ServiceResult.NotFound(
                "Stajyer profili bulunamadı.");
        }

        if (task.InternId != intern.Id)
        {
            _logger?.LogWarning(
                "Unauthorized task update attempt. TaskId: {TaskId}, UserId: {UserId}, InternId: {InternId}",
                id,
                userId,
                intern.Id);

            return ServiceResult.Forbidden(
                "Bu görevi güncelleme yetkiniz yok.");
        }

        var currentTaskDueDateUtc =
            task.DueDate?.ToUniversalTime();

        if (currentTaskDueDateUtc.HasValue &&
            currentTaskDueDateUtc.Value <
            DateTime.UtcNow)
        {
            _logger?.LogWarning(
                "Intern task update rejected because task is overdue. TaskId: {TaskId}, UserId: {UserId}, DueDate: {DueDate}",
                id,
                userId,
                currentTaskDueDateUtc);

            return ServiceResult.ValidationError(
                "Son teslim tarihi geçmiş görevler stajyer tarafından düzenlenemez.");
        }

        var newStatus =
            dto.Status.Trim();

        var createdByCurrentIntern =
            task.CreatedByUserId == userId;

        var requestedDueDateUtc =
            dto.DueDate?.ToUniversalTime();

        var effectiveDueDateUtc =
            createdByCurrentIntern
                ? requestedDueDateUtc
                : task.DueDate?.ToUniversalTime();

        var statusChanged =
            task.Status != newStatus;

        var movingToActiveOrDone =
            newStatus == TaskStatuses.InProgress ||
            newStatus == TaskStatuses.Done;

        if (statusChanged &&
            movingToActiveOrDone &&
            effectiveDueDateUtc.HasValue &&
            effectiveDueDateUtc.Value <
            DateTime.UtcNow)
        {
            _logger?.LogWarning(
                "Task status update rejected because task is overdue. TaskId: {TaskId}, UserId: {UserId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                id,
                userId,
                task.Status,
                newStatus);

            return ServiceResult.ValidationError(
                "Son teslim tarihi geçmiş görev başlatılamaz veya tamamlanamaz. Önce son teslim tarihini güncelleyin.");
        }

        var transitionError =
            ValidateStatusTransition(
                task,
                newStatus,
                id,
                userId);

        if (transitionError != null)
        {
            return transitionError;
        }

        if (task.Status ==
                TaskStatuses.InProgress &&
            newStatus ==
                TaskStatuses.Done)
        {
            task.CompletedAt =
                DateTime.UtcNow;
        }

        task.Status = newStatus;

        if (createdByCurrentIntern)
        {
            var dueDateError =
                ApplyInternOwnedChanges(
                    task,
                    dto,
                    requestedDueDateUtc,
                    id,
                    userId);

            if (dueDateError != null)
            {
                return dueDateError;
            }
        }

        await _taskRepository.UpdateAsync(task);

        _logger?.LogInformation(
            "Task updated successfully by intern. TaskId: {TaskId}, UserId: {UserId}, Status: {Status}",
            id,
            userId,
            task.Status);

        return ServiceResult.Ok(
            "Görev güncellendi.");
    }

    private ServiceResult?
        ValidateStatusTransition(
            TaskItem task,
            string newStatus,
            int id,
            int userId)
    {
        if (task.Status == TaskStatuses.ToDo &&
            newStatus != TaskStatuses.ToDo &&
            newStatus != TaskStatuses.InProgress)
        {
            _logger?.LogWarning(
                "Invalid task status transition. TaskId: {TaskId}, UserId: {UserId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                id,
                userId,
                task.Status,
                newStatus);

            return ServiceResult.ValidationError(
                "Görev tamamlanmadan önce başlatılmalıdır.");
        }

        if (task.Status ==
                TaskStatuses.InProgress &&
            newStatus !=
                TaskStatuses.InProgress &&
            newStatus !=
                TaskStatuses.Done)
        {
            _logger?.LogWarning(
                "Invalid task status transition. TaskId: {TaskId}, UserId: {UserId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                id,
                userId,
                task.Status,
                newStatus);

            return ServiceResult.ValidationError(
                "Geçersiz görev durumu.");
        }

        if (task.Status == TaskStatuses.Done &&
            newStatus != TaskStatuses.Done)
        {
            _logger?.LogWarning(
                "Completed task reopen attempt rejected. TaskId: {TaskId}, UserId: {UserId}, NewStatus: {NewStatus}",
                id,
                userId,
                newStatus);

            return ServiceResult.ValidationError(
                "Tamamlanan görev tekrar açılamaz.");
        }

        return null;
    }

    private ServiceResult?
        ApplyInternOwnedChanges(
            TaskItem task,
            UpdateTaskDto dto,
            DateTime? requestedDueDateUtc,
            int id,
            int userId)
    {
        var newDueDateUtc =
            requestedDueDateUtc;

        var currentDueDateUtc =
            task.DueDate?.ToUniversalTime();

        var dueDateChanged =
            !AreSameMinuteUtc(
                currentDueDateUtc,
                newDueDateUtc);

        if (dueDateChanged &&
            newDueDateUtc.HasValue &&
            newDueDateUtc.Value <
            DateTime.UtcNow)
        {
            _logger?.LogWarning(
                "Task update rejected because due date is in the past. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId);

            return ServiceResult.ValidationError(
                "Son teslim tarihi geçmiş bir tarih ve saat olamaz.");
        }

        task.Priority =
            dto.Priority.Trim();

        task.DueDate =
            newDueDateUtc;

        return null;
    }

    private async Task<ServiceResult?>
        ValidateSelectedInternAsync(
            TaskItem task,
            UpdateTaskDto dto,
            int id,
            int userId,
            string role)
    {
        var desiredIsActive =
            role == Roles.Admin &&
            dto.IsActive.HasValue
                ? dto.IsActive.Value
                : task.IsActive;

        var internChanged =
            task.InternId != dto.InternId;

        var mustUseActiveIntern =
            desiredIsActive ||
            internChanged;

        if (mustUseActiveIntern)
        {
            var selectedIntern =
                await _internRepository
                    .GetByIdAsync(dto.InternId);

            if (selectedIntern == null)
            {
                _logger?.LogWarning(
                    "Task update rejected because selected intern was not found or inactive. TaskId: {TaskId}, InternId: {InternId}, UserId: {UserId}",
                    id,
                    dto.InternId,
                    userId);

                return ServiceResult.ValidationError(
                    "Seçilen stajyer pasif veya bulunamadı.");
            }
        }

        return null;
    }

    private async Task<ServiceResult>
        UpdateForManagementAsync(
            TaskItem task,
            UpdateTaskDto dto,
            int id,
            int userId,
            string role)
    {
        if (role != Roles.Admin &&
            role != Roles.HR)
        {
            _logger?.LogWarning(
                "Unauthorized task update attempt. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}",
                id,
                userId,
                role);

            return ServiceResult.Forbidden(
                "Bu işlem için yetkiniz yok.");
        }

        var selectedInternError =
            await ValidateSelectedInternAsync(
                task,
                dto,
                id,
                userId,
                role);

        if (selectedInternError != null)
        {
            return selectedInternError;
        }

        var requestedDueDateUtc =
            dto.DueDate?.ToUniversalTime();

        var currentDueDateUtc =
            task.DueDate?.ToUniversalTime();

        var dueDateChanged =
            !AreSameMinuteUtc(
                currentDueDateUtc,
                requestedDueDateUtc);

        if (role == Roles.HR &&
            dueDateChanged &&
            requestedDueDateUtc.HasValue &&
            requestedDueDateUtc.Value <
            DateTime.UtcNow)
        {
            _logger?.LogWarning(
                "Task update rejected because due date is in the past. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}",
                id,
                userId,
                role);

            return ServiceResult.ValidationError(
                "Son teslim tarihi geçmiş bir tarih ve saat olamaz.");
        }

        var newStatus =
            dto.Status.Trim();

        var statusChanged =
            task.Status != newStatus;

        var movingToActiveOrDone =
            newStatus == TaskStatuses.InProgress ||
            newStatus == TaskStatuses.Done;

        // Admin ve HR için de aynı status geçiş
        // kurallarını uygula.
        var transitionError =
            ValidateStatusTransition(
                task,
                newStatus,
                id,
                userId);

        if (transitionError != null)
        {
            return transitionError;
        }

        if (role == Roles.HR &&
            statusChanged &&
            movingToActiveOrDone &&
            requestedDueDateUtc.HasValue &&
            requestedDueDateUtc.Value <
            DateTime.UtcNow)
        {
            _logger?.LogWarning(
                "Task status update rejected because task is overdue. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                id,
                userId,
                role,
                task.Status,
                newStatus);

            return ServiceResult.ValidationError(
                "Son teslim tarihi geçmiş görev başlatılamaz veya tamamlanamaz. Önce son teslim tarihini güncelleyin.");
        }

        if (task.Status != TaskStatuses.Done &&
            newStatus == TaskStatuses.Done)
        {
            task.CompletedAt =
                DateTime.UtcNow;
        }

        if (newStatus != TaskStatuses.Done)
        {
            task.CompletedAt = null;
        }

        task.Title =
            dto.Title.Trim();

        task.Description =
            dto.Description?.Trim();

        task.Status =
            newStatus;

        task.Priority =
            dto.Priority.Trim();

        task.DueDate =
            requestedDueDateUtc;

        task.InternId =
            dto.InternId;

        task.CanInternDeleteWhenCompleted =
            dto.CanInternDeleteWhenCompleted;

        if (role == Roles.Admin &&
            dto.IsActive.HasValue)
        {
            task.IsActive =
                dto.IsActive.Value;
        }

        await _taskRepository.UpdateAsync(task);

        _logger?.LogInformation(
            "Task updated successfully. TaskId: {TaskId}, UserId: {UserId}, Role: {Role}, InternId: {InternId}, Status: {Status}",
            id,
            userId,
            role,
            task.InternId,
            task.Status);

        return ServiceResult.Ok(
            "Görev güncellendi.");
    }
}