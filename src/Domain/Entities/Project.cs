namespace NewDevicesLab.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Idea { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public AppUser CreatedByUser { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

    public ICollection<OrderSheet> OrderSheets { get; set; } = new List<OrderSheet>();
}
