using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Application.Devices;

public class DeviceService(IDeviceRepository deviceRepository) : IDeviceService
{
    public async Task<IReadOnlyList<DeviceDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var devices = await deviceRepository.GetAllAsync(cancellationToken);
        return devices
            .Select(MapToDto)
            .ToList();
    }

    public async Task<DeviceDto> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAtUtc = DateTime.UtcNow
        };

        var created = await deviceRepository.AddAsync(device, cancellationToken);
        return MapToDto(created);
    }

    private static DeviceDto MapToDto(Device device) => new(
        device.Id,
        device.Name,
        device.CreatedAtUtc);
}
