namespace InternTrack.Core.Models;

public class TaskItem
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public required string Status { get; set; }

    public string Priority { get; set; } = "Medium";

    public int InternId { get; set; }

    public Intern? Intern { get; set; }

    public int? CreatedByUserId { get; set; }

    public bool CanInternDeleteWhenCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}