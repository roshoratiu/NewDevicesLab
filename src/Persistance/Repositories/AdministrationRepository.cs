using Microsoft.EntityFrameworkCore;
using NewDevicesLab.Application.Administration;
using NewDevicesLab.Domain.Entities;
using NewDevicesLab.Persistance.Data;

namespace NewDevicesLab.Persistance.Repositories;

public class AdministrationRepository(AppDbContext dbContext) : IAdministrationRepository
{
    public async Task<IReadOnlyList<AppUser>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(item => item.UserGroups)
                .ThenInclude(item => item.Group)
            .OrderBy(item => item.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Group>> GetGroupsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Groups
            .AsNoTracking()
            .Include(item => item.UserGroups)
            .Include(item => item.GroupPermissions)
                .ThenInclude(item => item.Permission)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Permissions
            .AsNoTracking()
            .OrderBy(item => item.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Group>> GetGroupsByIdsAsync(
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken)
    {
        if (groupIds.Count == 0)
        {
            return [];
        }

        return await dbContext.Groups
            .Where(item => groupIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsByIdsAsync(
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        if (permissionIds.Count == 0)
        {
            return [];
        }

        return await dbContext.Permissions
            .Where(item => permissionIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<AppUser> AddUserAsync(AppUser user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Users
            .AsNoTracking()
            .Include(item => item.UserGroups)
                .ThenInclude(item => item.Group)
            .SingleAsync(item => item.Id == user.Id, cancellationToken);
    }

    public async Task<AppUser> UpdateUserGroupsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(item => item.UserGroups)
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new ArgumentException("User was not found.", nameof(userId));
        }

        dbContext.UserGroups.RemoveRange(user.UserGroups);

        var now = DateTime.UtcNow;
        var updatedUserGroups = groupIds
            .Distinct()
            .Select(groupId => new UserGroup
            {
                UserId = user.Id,
                GroupId = groupId,
                JoinedAtUtc = now
            })
            .ToList();

        dbContext.UserGroups.AddRange(updatedUserGroups);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Users
            .AsNoTracking()
            .Include(item => item.UserGroups)
                .ThenInclude(item => item.Group)
            .SingleAsync(item => item.Id == user.Id, cancellationToken);
    }

    public async Task<Group> UpdateGroupPermissionsAsync(
        Guid groupId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        var group = await dbContext.Groups
            .Include(item => item.GroupPermissions)
            .Include(item => item.UserGroups)
            .SingleOrDefaultAsync(item => item.Id == groupId, cancellationToken);

        if (group is null)
        {
            throw new ArgumentException("Group was not found.", nameof(groupId));
        }

        dbContext.GroupPermissions.RemoveRange(group.GroupPermissions);

        var now = DateTime.UtcNow;
        var updatedGroupPermissions = permissionIds
            .Distinct()
            .Select(permissionId => new GroupPermission
            {
                GroupId = group.Id,
                PermissionId = permissionId,
                GrantedAtUtc = now
            })
            .ToList();

        dbContext.GroupPermissions.AddRange(updatedGroupPermissions);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Groups
            .AsNoTracking()
            .Include(item => item.UserGroups)
            .Include(item => item.GroupPermissions)
                .ThenInclude(item => item.Permission)
            .SingleAsync(item => item.Id == group.Id, cancellationToken);
    }
}
