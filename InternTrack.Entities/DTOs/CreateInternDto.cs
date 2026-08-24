namespace InternTrack.Entities.DTOs;

public class CreateInternDto
{
    public required string Name { get; set; }

    public required string Email { get; set; }

    public int DepartmentId { get; set; }
}