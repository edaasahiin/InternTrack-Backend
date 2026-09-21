using System.Text.Json.Serialization;

namespace InternTrack.Core.Models;

public class Department
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public List<Intern> Interns { get; set; } = new();
}