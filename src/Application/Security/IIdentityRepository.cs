using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Application.Security;

public interface IIdentityRepository
{
    Task<AppUser?> FindActiveUserByIdentifierAsync(string identifier, CancellationToken cancellationToken);

    Task<AppUser?> FindActiveUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> UsernameExistsAsync(
        string username,
        Guid? excludedUserId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
