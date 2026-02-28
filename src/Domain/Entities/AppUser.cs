namespace NewDevicesLab.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; }

    public string RadboudEmail { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string StudentNumber { get; set; } = string.Empty;

    public DateOnly EnrollmentDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();

    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();

    public ICollection<OrderSheet> CreatedOrderSheets { get; set; } = new List<OrderSheet>();
}
