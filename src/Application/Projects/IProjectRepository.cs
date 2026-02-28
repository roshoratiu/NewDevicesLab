using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Application.Projects;

public interface IProjectRepository
{
    Task<AppUser?> GetUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AppUser>> GetUsersByEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken);

    Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken);

    Task<Project> AddProjectAsync(Project project, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
