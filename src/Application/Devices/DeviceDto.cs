namespace NewDevicesLab.Application.Devices;

public sealed record DeviceDto(Guid Id, string Name, DateTime CreatedAtUtc);
