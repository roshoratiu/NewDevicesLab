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
}
