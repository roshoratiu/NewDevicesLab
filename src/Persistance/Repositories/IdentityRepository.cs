using Microsoft.EntityFrameworkCore;
using NewDevicesLab.Application.Security;
using NewDevicesLab.Domain.Entities;
using NewDevicesLab.Persistance.Data;

namespace NewDevicesLab.Persistance.Repositories;

public class IdentityRepository(AppDbContext dbContext) : IIdentityRepository
{
    public async Task<AppUser?> FindActiveUserByIdentifierAsync(string identifier, CancellationToken cancellationToken)
    {
        var normalized = identifier.Trim();

        return await dbContext.Users
            .AsNoTracking()
            .Include(item => item.UserGroups)
                .ThenInclude(item => item.Group)
                    .ThenInclude(item => item.GroupPermissions)
                        .ThenInclude(item => item.Permission)
            .SingleOrDefaultAsync(
                item => item.IsActive
                    && (item.Username == normalized || item.RadboudEmail == normalized),
                cancellationToken);
    }

    public async Task<AppUser?> FindActiveUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Include(item => item.UserGroups)
                .ThenInclude(item => item.Group)
                    .ThenInclude(item => item.GroupPermissions)
                        .ThenInclude(item => item.Permission)
            .SingleOrDefaultAsync(
                item => item.IsActive && item.Id == userId,
                cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(
        string username,
        Guid? excludedUserId,
        CancellationToken cancellationToken)
    {
        var normalized = username.Trim();

        return await dbContext.Users.AnyAsync(
            item => item.IsActive
                && item.Username == normalized
                && (!excludedUserId.HasValue || item.Id != excludedUserId.Value),
            cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
