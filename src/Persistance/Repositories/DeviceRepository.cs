using Microsoft.EntityFrameworkCore;
using NewDevicesLab.Application.Devices;
using NewDevicesLab.Domain.Entities;
using NewDevicesLab.Persistance.Data;

namespace NewDevicesLab.Persistance.Repositories;

public class DeviceRepository(AppDbContext dbContext) : IDeviceRepository
{
    public async Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Devices
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Device> AddAsync(Device device, CancellationToken cancellationToken)
    {
        dbContext.Devices.Add(device);
        await dbContext.SaveChangesAsync(cancellationToken);
        return device;
    }
}
