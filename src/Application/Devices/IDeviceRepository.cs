using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Application.Devices;

public interface IDeviceRepository
{
    Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken);

    Task<Device> AddAsync(Device device, CancellationToken cancellationToken);
}
