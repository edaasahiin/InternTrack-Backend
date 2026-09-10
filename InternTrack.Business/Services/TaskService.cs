using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;

namespace InternTrack.Business.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IInternRepository _internRepository;

    public TaskService(
        ITaskRepository taskRepository,
        IInternRepository internRepository)
    {
        _taskRepository = taskRepository;
        _internRepository = internRepository;
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
                return ServiceResult<TaskItem>
                    .NotFound(
                        "Stajyer profili bulunamadı."
                    );
            }

            if (task.InternId != intern.Id)
            {
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

        if (role == "Intern")
        {
            var currentIntern =
                await _internRepository.GetByUserIdAsync(
                    userId
                );

            if (currentIntern == null)
            {
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
                return ServiceResult.ValidationError(
                    "Stajyer bulunamadı."
                );
            }

            internId =
                selectedIntern.Id;
        }
        else
        {
            return ServiceResult.Forbidden(
                "Bu işlem için yetkiniz yok."
            );
        }

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),

            Description =
                dto.Description?.Trim(),

            Status =
                dto.Status.Trim(),

            Priority =
                dto.Priority.Trim(),

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

        await _taskRepository.AddAsync(task);

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
            await _taskRepository.GetByIdAsync(id);

        if (task == null)
        {
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
                return ServiceResult.NotFound(
                    "Stajyer profili bulunamadı."
                );
            }

            if (task.InternId != intern.Id)
            {
                return ServiceResult.Forbidden(
                    "Bu görevi güncelleme yetkiniz yok."
                );
            }

            var newStatus =
                dto.Status.Trim();

            if (
                task.Status != "Done" &&
                newStatus == "Done"
            )
            {
                task.CompletedAt =
                    DateTime.UtcNow;
            }

            if (newStatus != "Done")
            {
                task.CompletedAt =
                    null;
            }

            task.Status =
                newStatus;

            var createdByCurrentIntern =
                task.CreatedByUserId == userId;

            if (createdByCurrentIntern)
            {
                task.Priority =
                    dto.Priority.Trim();
            }

            await _taskRepository.UpdateAsync(
                task
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
            return ServiceResult.ValidationError(
                "Stajyer bulunamadı."
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

        task.InternId =
            dto.InternId;

        task.CanInternDeleteWhenCompleted =
            dto.CanInternDeleteWhenCompleted;

        await _taskRepository.UpdateAsync(
            task
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
            await _taskRepository.GetByIdAsync(id);

        if (task == null)
        {
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
                return ServiceResult.NotFound(
                    "Stajyer profili bulunamadı."
                );
            }

            if (task.InternId != intern.Id)
            {
                return ServiceResult.Forbidden(
                    "Bu görevi silme yetkiniz yok."
                );
            }

            var createdByCurrentIntern =
                task.CreatedByUserId == userId;

            if (createdByCurrentIntern)
            {
                await _taskRepository.DeleteAsync(
                    task
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
            return ServiceResult.Forbidden(
                "Bu işlem için yetkiniz yok."
            );
        }

        await _taskRepository.DeleteAsync(
            task
        );

        return ServiceResult.Ok(
            "Görev silindi."
        );
    }
}