using InternTrack.Core.Constants;
using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;

namespace InternTrack.Business.Services;

public class DashboardService : IDashboardService
{
    private readonly IInternRepository _internRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<DashboardService>? _logger;

    public DashboardService(
        IInternRepository internRepository,
        ITaskRepository taskRepository,
        IDepartmentRepository departmentRepository,
        ILogger<DashboardService>? logger = null)
    {
        _internRepository = internRepository;

        _taskRepository = taskRepository;

        _departmentRepository = departmentRepository;

        _logger = logger;
    }

    public async Task<ServiceResult<DashboardStatsDto>> GetStatsAsync(int userId, string role)
    {
        if (role == Roles.Admin || role == Roles.HR)
        {
            var interns = await _internRepository.GetAllAsync();

            var tasks = await _taskRepository.GetAllAsync();

            var departments = await _departmentRepository.GetAllAsync();

            var now = DateTime.UtcNow;

            var stats = CreateStats(tasks, interns.Count, departments.Count, now);

            return ServiceResult<DashboardStatsDto>.Ok(stats);
        }

        var intern = await _internRepository.GetByUserIdAsync(userId);

        if (intern == null)
        {
            _logger?.LogWarning(
                "Dashboard statistics could not be retrieved because intern profile was not found. UserId: {UserId}",
                userId);

            return ServiceResult<DashboardStatsDto>.NotFound("Stajyer kaydı bulunamadı.");
        }

        var internTasks = await _taskRepository.GetByInternIdAsync(intern.Id);

        var currentTime = DateTime.UtcNow;

        var internStats = CreateStats(internTasks, internCount: 1, departmentCount: 0, now: currentTime);

        return ServiceResult<DashboardStatsDto>.Ok(internStats);
    }

    private static DashboardStatsDto CreateStats(
        List<TaskItem> tasks,
        int internCount,
        int departmentCount,
        DateTime now)
    {
        var toDoTaskCount = tasks.Count(task =>
            task.Status == TaskStatuses.ToDo &&
            (!task.DueDate.HasValue || task.DueDate.Value >= now));

        var inProgressTaskCount = tasks.Count(task =>
            task.Status == TaskStatuses.InProgress &&
            (!task.DueDate.HasValue || task.DueDate.Value >= now));

        var completedTaskCount = tasks.Count(task => task.Status == TaskStatuses.Done);

        var overdueTaskCount = tasks.Count(task =>
            task.Status != TaskStatuses.Done &&
            task.DueDate.HasValue && task.DueDate.Value < now);

        return new DashboardStatsDto
        {
            InternCount = internCount,
            TaskCount = tasks.Count,
            ToDoTaskCount = toDoTaskCount,
            InProgressTaskCount = inProgressTaskCount,
            CompletedTaskCount = completedTaskCount,
            OverdueTaskCount = overdueTaskCount,
            DepartmentCount = departmentCount
        };
    }
}
