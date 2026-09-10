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
        _internRepository =
            internRepository;

        _taskRepository =
            taskRepository;

        _departmentRepository =
            departmentRepository;
    }

    public async Task<ServiceResult<DashboardStatsDto>> GetStatsAsync(
        int userId,
        string role)
    {
        if (
            role == "Admin" ||
            role == "HR"
        )
        {
            var interns =
                await _internRepository.GetAllAsync();

            var tasks =
                await _taskRepository.GetAllAsync();

            var departments =
                await _departmentRepository.GetAllAsync();

            var now =
                DateTime.UtcNow;

            var toDoTaskCount =
                tasks.Count(
                    task =>
                        task.Status == "ToDo" &&
                        (
                            !task.DueDate.HasValue ||
                            task.DueDate.Value >= now
                        )
                );

            var inProgressTaskCount =
                tasks.Count(
                    task =>
                        task.Status == "InProgress" &&
                        (
                            !task.DueDate.HasValue ||
                            task.DueDate.Value >= now
                        )
                );

            var completedTaskCount =
                tasks.Count(
                    task =>
                        task.Status == "Done"
                );

            var overdueTaskCount =
                tasks.Count(
                    task =>
                        task.Status != "Done" &&
                        task.DueDate.HasValue &&
                        task.DueDate.Value < now
                );

            var stats =
                new DashboardStatsDto
                {
                    InternCount =
                        interns.Count,

                    TaskCount =
                        tasks.Count,

                    ToDoTaskCount =
                        toDoTaskCount,

                    InProgressTaskCount =
                        inProgressTaskCount,

                    CompletedTaskCount =
                        completedTaskCount,

                    OverdueTaskCount =
                        overdueTaskCount,

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

        var currentTime =
            DateTime.UtcNow;

        var internToDoTaskCount =
            internTasks.Count(
                task =>
                    task.Status == "ToDo" &&
                    (
                        !task.DueDate.HasValue ||
                        task.DueDate.Value >= currentTime
                    )
            );

        var internInProgressTaskCount =
            internTasks.Count(
                task =>
                    task.Status == "InProgress" &&
                    (
                        !task.DueDate.HasValue ||
                        task.DueDate.Value >= currentTime
                    )
            );

        var internCompletedTaskCount =
            internTasks.Count(
                task =>
                    task.Status == "Done"
            );

        var internOverdueTaskCount =
            internTasks.Count(
                task =>
                    task.Status != "Done" &&
                    task.DueDate.HasValue &&
                    task.DueDate.Value < currentTime
            );

        var internStats =
            new DashboardStatsDto
            {
                InternCount =
                    1,

                TaskCount =
                    internTasks.Count,

                ToDoTaskCount =
                    internToDoTaskCount,

                InProgressTaskCount =
                    internInProgressTaskCount,

                CompletedTaskCount =
                    internCompletedTaskCount,

                OverdueTaskCount =
                    internOverdueTaskCount,

                DepartmentCount =
                    0
            };

        return ServiceResult<DashboardStatsDto>
            .Ok(internStats);
    }
}