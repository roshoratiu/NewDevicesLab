namespace NewDevicesLab.Domain.Entities;

public class UserGroup
{
    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid GroupId { get; set; }

    public Group Group { get; set; } = null!;

    public DateTime JoinedAtUtc { get; set; }
}
