namespace NewDevicesLab.Domain.Entities;

public class Group
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSystem { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    public ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
}
