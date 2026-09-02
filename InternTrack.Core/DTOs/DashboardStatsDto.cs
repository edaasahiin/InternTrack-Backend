namespace InternTrack.Core.DTOs;

public class DashboardStatsDto
{
    public int InternCount { get; set; }

    public int TaskCount { get; set; }

    public int CompletedTaskCount { get; set; }

    public int PendingTaskCount { get; set; }

    public int DepartmentCount { get; set; }
}