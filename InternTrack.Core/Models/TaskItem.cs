namespace InternTrack.Core.Models;

public class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string Status { get; set; }

    public int InternId { get; set; }
    public Intern? Intern { get; set; }
}