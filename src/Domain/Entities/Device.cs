namespace NewDevicesLab.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
