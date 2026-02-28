namespace NewDevicesLab.Application.Devices;

public interface IDeviceService
{
    Task<IReadOnlyList<DeviceDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<DeviceDto> CreateAsync(string name, CancellationToken cancellationToken);
}
