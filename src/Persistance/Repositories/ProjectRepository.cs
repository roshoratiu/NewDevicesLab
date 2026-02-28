using Microsoft.EntityFrameworkCore;
using NewDevicesLab.Application.Projects;
using NewDevicesLab.Domain.Entities;
using NewDevicesLab.Persistance.Data;

namespace NewDevicesLab.Persistance.Repositories;

public class ProjectRepository(AppDbContext dbContext) : IProjectRepository
{
    public async Task<AppUser?> GetUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken)
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

    public async Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Projects
            .AsNoTracking()
            .Include(item => item.CreatedByUser)
            .Include(item => item.Members)
                .ThenInclude(item => item.User)
            .Include(item => item.OrderSheets)
                .ThenInclude(item => item.CreatedByUser)
            .Include(item => item.OrderSheets)
                .ThenInclude(item => item.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppUser>> GetUsersByEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken)
    {
        if (emails.Count == 0)
        {
            return [];
        }

        return await dbContext.Users
            .Where(item => item.IsActive && emails.Contains(item.RadboudEmail))
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await dbContext.Projects
            .Include(item => item.CreatedByUser)
            .Include(item => item.Members)
                .ThenInclude(item => item.User)
            .Include(item => item.OrderSheets)
                .ThenInclude(item => item.CreatedByUser)
            .Include(item => item.OrderSheets)
                .ThenInclude(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == projectId, cancellationToken);
    }

    public async Task<Project> AddProjectAsync(Project project, CancellationToken cancellationToken)
    {
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetProjectByIdAsync(project.Id, cancellationToken)
            ?? throw new InvalidOperationException("Created project could not be loaded.");
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
