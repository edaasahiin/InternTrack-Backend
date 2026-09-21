using InternTrack.Core.Models;

namespace InternTrack.Core.DTOs;

public class InternResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Surname { get; set; } = "";

    public string Email { get; set; } = "";

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public int UserId { get; set; }

    public string? Avatar { get; set; }

    public bool IsActive { get; set; }
}