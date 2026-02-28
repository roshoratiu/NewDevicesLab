using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Application.Security;

public interface IIdentityRepository
{
    Task<AppUser?> FindActiveUserByIdentifierAsync(string identifier, CancellationToken cancellationToken);
}
