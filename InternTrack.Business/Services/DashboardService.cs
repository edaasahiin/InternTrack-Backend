using InternTrack.Core.Constants;
using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
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

            var toDoTaskCount = tasks.Count(
                task => task.Status == TaskStatuses.ToDo && (!task.DueDate.HasValue || task.DueDate.Value >= now));

            var inProgressTaskCount = tasks.Count(
                task => task.Status == TaskStatuses.InProgress && (!task.DueDate.HasValue || task.DueDate.Value >= now));

            var completedTaskCount = tasks.Count(task => task.Status == TaskStatuses.Done);

            var overdueTaskCount = tasks.Count(
                task => task.Status != TaskStatuses.Done && task.DueDate.HasValue && task.DueDate.Value < now);

            var stats = new DashboardStatsDto
            {
                InternCount = interns.Count,

                TaskCount = tasks.Count,

                ToDoTaskCount = toDoTaskCount,

                InProgressTaskCount = inProgressTaskCount,

                CompletedTaskCount = completedTaskCount,

                OverdueTaskCount = overdueTaskCount,

                DepartmentCount = departments.Count
            };

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

        var internToDoTaskCount = internTasks.Count(
            task => task.Status == TaskStatuses.ToDo && (!task.DueDate.HasValue || task.DueDate.Value >= currentTime));

        var internInProgressTaskCount = internTasks.Count(
            task => task.Status == TaskStatuses.InProgress && (!task.DueDate.HasValue || task.DueDate.Value >= currentTime));

        var internCompletedTaskCount = internTasks.Count(task => task.Status == TaskStatuses.Done);

        var internOverdueTaskCount = internTasks.Count(
            task => task.Status != TaskStatuses.Done && task.DueDate.HasValue && task.DueDate.Value < currentTime);

        var internStats = new DashboardStatsDto
        {
            InternCount = 1,

            TaskCount = internTasks.Count,

            ToDoTaskCount = internToDoTaskCount,

            InProgressTaskCount = internInProgressTaskCount,

            CompletedTaskCount = internCompletedTaskCount,

            OverdueTaskCount = internOverdueTaskCount,

            DepartmentCount = 0
        };

        return ServiceResult<DashboardStatsDto>.Ok(internStats);
    }
}
