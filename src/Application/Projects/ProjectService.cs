using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Application.Projects;

public class ProjectService(IProjectRepository projectRepository) : IProjectService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft",
        "Active",
        "Review",
        "Completed",
        "Paused"
    };

    public async Task<ProjectDashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(userId, cancellationToken);
        var access = GetAccess(currentUser);

        var projects = await projectRepository.GetProjectsAsync(cancellationToken);
        var visibleProjects = projects
            .Where(project => CanSeeProject(project, currentUser.Id, access))
            .OrderByDescending(project => project.CreatedAtUtc)
            .Select(project => MapProject(project, currentUser.Id, access))
            .ToList();

        return new ProjectDashboardDto(
            visibleProjects,
            access.CanCreateProject,
            access.CanViewGroupProjects,
            access.CanViewGroupOrderSheets);
    }

    public async Task<ProjectSummaryDto> CreateProjectAsync(
        Guid userId,
        CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(userId, cancellationToken);
        var access = GetAccess(currentUser);

        if (!access.CanCreateProject)
        {
            throw new UnauthorizedAccessException("You do not have permission to create projects.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Project name is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.Idea))
        {
            throw new ArgumentException("Project idea is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            throw new ArgumentException("Project description is required.", nameof(command));
        }

        var normalizedEmails = command.MemberEmails
            .Select(email => email.Trim())
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var invitedUsers = normalizedEmails.Length == 0
            ? []
            : await projectRepository.GetUsersByEmailsAsync(normalizedEmails, cancellationToken);

        if (invitedUsers.Count != normalizedEmails.Length)
        {
            throw new ArgumentException(
                "Every invited teammate must already exist in the app and use a Radboud email.",
                nameof(command));
        }

        var now = DateTime.UtcNow;
        var members = invitedUsers
            .Append(currentUser)
            .DistinctBy(item => item.Id)
            .Select(user => new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.Empty,
                UserId = user.Id,
                IsOwner = user.Id == currentUser.Id,
                AddedAtUtc = now
            })
            .ToList();

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Idea = command.Idea.Trim(),
            Description = command.Description.Trim(),
            Status = "Draft",
            CreatedByUserId = currentUser.Id,
            CreatedAtUtc = now,
            Members = members
        };

        foreach (var member in project.Members)
        {
            member.ProjectId = project.Id;
        }

        var created = await projectRepository.AddProjectAsync(project, cancellationToken);
        return MapProject(created, currentUser.Id, access);
    }

    public async Task<ProjectSummaryDto> UpdateStatusAsync(
        Guid userId,
        Guid projectId,
        UpdateProjectStatusCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(userId, cancellationToken);
        var access = GetAccess(currentUser);

        if (!access.CanManageProjectStatus)
        {
            throw new UnauthorizedAccessException("You do not have permission to update project statuses.");
        }

        if (string.IsNullOrWhiteSpace(command.Status) || !AllowedStatuses.Contains(command.Status.Trim()))
        {
            throw new ArgumentException("Status is invalid.", nameof(command));
        }

        var project = await projectRepository.GetProjectByIdAsync(projectId, cancellationToken)
            ?? throw new ArgumentException("Project was not found.", nameof(projectId));

        if (!CanSeeProject(project, currentUser.Id, access))
        {
            throw new UnauthorizedAccessException("You do not have access to this project.");
        }

        project.Status = command.Status.Trim();
        await projectRepository.SaveChangesAsync(cancellationToken);

        return MapProject(project, currentUser.Id, access);
    }

    public async Task<OrderSheetSummaryDto> CreateOrderSheetAsync(
        Guid userId,
        Guid projectId,
        CreateOrderSheetCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(userId, cancellationToken);
        var access = GetAccess(currentUser);

        if (!access.CanCreateOrderSheets)
        {
            throw new UnauthorizedAccessException("You do not have permission to create order sheets.");
        }

        if (command.Items.Count == 0)
        {
            throw new ArgumentException("Add at least one order item.", nameof(command));
        }

        var project = await projectRepository.GetProjectByIdAsync(projectId, cancellationToken)
            ?? throw new ArgumentException("Project was not found.", nameof(projectId));

        if (!IsProjectParticipant(project, currentUser.Id) && !access.CanViewGroupProjects)
        {
            throw new UnauthorizedAccessException("You can only order for projects you participate in.");
        }

        if (command.Submit && !access.CanSubmitOrderSheets)
        {
            throw new UnauthorizedAccessException("You do not have permission to submit order sheets.");
        }

        var now = DateTime.UtcNow;
        var orderSheet = new OrderSheet
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            CreatedByUserId = currentUser.Id,
            CreatedByUser = currentUser,
            Status = command.Submit ? "Submitted" : "Draft",
            CreatedAtUtc = now,
            SubmittedAtUtc = command.Submit ? now : null,
            Items = command.Items.Select(item =>
            {
                if (string.IsNullOrWhiteSpace(item.SiteName)
                    || string.IsNullOrWhiteSpace(item.ComponentName)
                    || string.IsNullOrWhiteSpace(item.Brand)
                    || string.IsNullOrWhiteSpace(item.Link))
                {
                    throw new ArgumentException("All order item fields are required.", nameof(command));
                }

                if (item.PriceEuro <= 0)
                {
                    throw new ArgumentException("Each item price must be greater than zero.", nameof(command));
                }

                return new OrderSheetItem
                {
                    Id = Guid.NewGuid(),
                    SiteName = item.SiteName.Trim(),
                    ComponentName = item.ComponentName.Trim(),
                    Brand = item.Brand.Trim(),
                    Link = item.Link.Trim(),
                    PriceEuro = decimal.Round(item.PriceEuro, 2, MidpointRounding.AwayFromZero)
                };
            }).ToList()
        };

        project.OrderSheets.Add(orderSheet);
        await projectRepository.SaveChangesAsync(cancellationToken);

        return MapOrderSheet(orderSheet);
    }

    private async Task<AppUser> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await projectRepository.GetUserWithPermissionsAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("The current user could not be resolved.");
    }

    private static AccessContext GetAccess(AppUser user)
    {
        var permissions = user.UserGroups
            .SelectMany(item => item.Group.GroupPermissions)
            .Select(item => item.Permission.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var fullAccess = permissions.Contains("system.full_access");

        return new AccessContext(
            CanCreateProject: fullAccess || permissions.Contains("projects.create"),
            CanViewOwnProjects: fullAccess || permissions.Contains("projects.view.own"),
            CanViewGroupProjects: fullAccess || permissions.Contains("projects.view.group"),
            CanManageProjectStatus: fullAccess || permissions.Contains("projects.status.manage"),
            CanCreateOrderSheets: fullAccess || permissions.Contains("ordersheets.create"),
            CanViewOwnOrderSheets: fullAccess || permissions.Contains("ordersheets.view.own"),
            CanViewGroupOrderSheets: fullAccess || permissions.Contains("ordersheets.view.group"),
            CanSubmitOrderSheets: fullAccess || permissions.Contains("ordersheets.submit"));
    }

    private static bool CanSeeProject(Project project, Guid currentUserId, AccessContext access)
    {
        if (access.CanViewGroupProjects)
        {
            return true;
        }

        if (!access.CanViewOwnProjects)
        {
            return false;
        }

        return IsProjectParticipant(project, currentUserId);
    }

    private static bool IsProjectParticipant(Project project, Guid currentUserId)
    {
        return project.CreatedByUserId == currentUserId
            || project.Members.Any(member => member.UserId == currentUserId);
    }

    private static ProjectSummaryDto MapProject(Project project, Guid currentUserId, AccessContext access)
    {
        var visibleOrderSheets = project.OrderSheets
            .Where(orderSheet => CanSeeOrderSheet(orderSheet, currentUserId, access))
            .OrderByDescending(orderSheet => orderSheet.CreatedAtUtc)
            .Select(MapOrderSheet)
            .ToList();

        return new ProjectSummaryDto(
            project.Id,
            project.Name,
            project.Idea,
            project.Description,
            project.Status,
            project.CreatedByUser.Username,
            project.CreatedAtUtc,
            project.Members
                .Select(member => member.User.RadboudEmail)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(email => email)
                .ToList(),
            access.CanManageProjectStatus,
            access.CanCreateOrderSheets && (IsProjectParticipant(project, currentUserId) || access.CanViewGroupProjects),
            visibleOrderSheets);
    }

    private static bool CanSeeOrderSheet(OrderSheet orderSheet, Guid currentUserId, AccessContext access)
    {
        if (access.CanViewGroupOrderSheets)
        {
            return true;
        }

        if (!access.CanViewOwnOrderSheets)
        {
            return false;
        }

        return orderSheet.CreatedByUserId == currentUserId;
    }

    private static OrderSheetSummaryDto MapOrderSheet(OrderSheet orderSheet)
    {
        return new OrderSheetSummaryDto(
            orderSheet.Id,
            orderSheet.Status,
            orderSheet.CreatedByUser.Username,
            orderSheet.CreatedAtUtc,
            orderSheet.SubmittedAtUtc,
            orderSheet.Items.Sum(item => item.PriceEuro),
            orderSheet.Items
                .OrderBy(item => item.ComponentName)
                .Select(item => new OrderSheetItemDto(
                    item.Id,
                    item.SiteName,
                    item.ComponentName,
                    item.Brand,
                    item.Link,
                    item.PriceEuro))
                .ToList());
    }

    private sealed record AccessContext(
        bool CanCreateProject,
        bool CanViewOwnProjects,
        bool CanViewGroupProjects,
        bool CanManageProjectStatus,
        bool CanCreateOrderSheets,
        bool CanViewOwnOrderSheets,
        bool CanViewGroupOrderSheets,
        bool CanSubmitOrderSheets);
}
