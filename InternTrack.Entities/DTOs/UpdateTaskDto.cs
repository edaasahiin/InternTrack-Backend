namespace InternTrack.Entities.DTOs;

public class UpdateTaskDto
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public required string Status { get; set; }

    public int InternId { get; set; }
}