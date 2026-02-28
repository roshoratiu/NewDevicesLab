using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Application.Administration;

public interface IAdministrationRepository
{
    Task<IReadOnlyList<AppUser>> GetUsersAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Group>> GetGroupsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Permission>> GetPermissionsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Group>> GetGroupsByIdsAsync(
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Permission>> GetPermissionsByIdsAsync(
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken);

    Task<AppUser> AddUserAsync(AppUser user, CancellationToken cancellationToken);

    Task<AppUser> UpdateUserGroupsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken);

    Task<Group> UpdateGroupPermissionsAsync(
        Guid groupId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken);
}
