namespace InternTrack.Core.DTOs;

public class DashboardStatsDto
{
    public int InternCount { get; set; }

    public int TaskCount { get; set; }

    public int ToDoTaskCount { get; set; }

    public int InProgressTaskCount { get; set; }

    public int CompletedTaskCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public int DepartmentCount { get; set; }
}