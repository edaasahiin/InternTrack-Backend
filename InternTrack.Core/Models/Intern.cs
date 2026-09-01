using System.Text.Json.Serialization;

namespace InternTrack.Core.Models;

public class Intern
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    [JsonIgnore]
    public List<TaskItem> Tasks { get; set; } = new();
}