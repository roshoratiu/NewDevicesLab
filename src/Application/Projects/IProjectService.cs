namespace NewDevicesLab.Application.Projects;

public interface IProjectService
{
    Task<ProjectDashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken);

    Task<ProjectSummaryDto> CreateProjectAsync(
        Guid userId,
        CreateProjectCommand command,
        CancellationToken cancellationToken);

    Task<ProjectSummaryDto> UpdateStatusAsync(
        Guid userId,
        Guid projectId,
        UpdateProjectStatusCommand command,
        CancellationToken cancellationToken);

    Task<OrderSheetSummaryDto> CreateOrderSheetAsync(
        Guid userId,
        Guid projectId,
        CreateOrderSheetCommand command,
        CancellationToken cancellationToken);
}
