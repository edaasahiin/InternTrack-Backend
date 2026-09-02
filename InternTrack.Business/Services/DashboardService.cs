using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.DataAccess.Interfaces;

namespace InternTrack.Business.Services;

public class DashboardService : IDashboardService
{
    private readonly IInternRepository _internRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public DashboardService(
        IInternRepository internRepository,
        ITaskRepository taskRepository,
        IDepartmentRepository departmentRepository)
    {
        _internRepository = internRepository;
        _taskRepository = taskRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<ServiceResult<DashboardStatsDto>> GetStatsAsync(
        int userId,
        string role)
    {
        var departments =
            await _departmentRepository.GetAllAsync();

        if (
            role == "Admin" ||
            role == "HR"
        )
        {
            var interns =
                await _internRepository.GetAllAsync();

            var tasks =
                await _taskRepository.GetAllAsync();

            var completedTaskCount =
                tasks.Count(
                    task =>
                        task.Status == "Done"
                );

            var pendingTaskCount =
                tasks.Count(
                    task =>
                        task.Status != "Done"
                );

            var stats =
                new DashboardStatsDto
                {
                    InternCount = interns.Count,
                    TaskCount = tasks.Count,
                    CompletedTaskCount =
                        completedTaskCount,
                    PendingTaskCount =
                        pendingTaskCount,
                    DepartmentCount =
                        departments.Count
                };

            return ServiceResult<DashboardStatsDto>
                .Ok(stats);
        }

        var intern =
            await _internRepository.GetByUserIdAsync(
                userId
            );

        if (intern == null)
        {
            return ServiceResult<DashboardStatsDto>
                .NotFound(
                    "Stajyer kaydı bulunamadı."
                );
        }

        var internTasks =
            await _taskRepository.GetByInternIdAsync(
                intern.Id
            );

        var internCompletedTaskCount =
            internTasks.Count(
                task =>
                    task.Status == "Done"
            );

        var internPendingTaskCount =
            internTasks.Count(
                task =>
                    task.Status != "Done"
            );

        var internStats =
            new DashboardStatsDto
            {
                InternCount = 1,
                TaskCount = internTasks.Count,
                CompletedTaskCount =
                    internCompletedTaskCount,
                PendingTaskCount =
                    internPendingTaskCount,
                DepartmentCount =
                    departments.Count
            };

        return ServiceResult<DashboardStatsDto>
            .Ok(internStats);
    }
}