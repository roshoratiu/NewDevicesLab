namespace NewDevicesLab.Application.Projects;

public sealed record ProjectDashboardDto(
    IReadOnlyList<ProjectSummaryDto> Projects,
    bool CanCreateProject,
    bool CanReviewGroupProjects,
    bool CanReviewGroupOrderSheets);

public sealed record ProjectSummaryDto(
    Guid Id,
    string Name,
    string Idea,
    string Description,
    string Status,
    string CreatedByUsername,
    DateTime CreatedAtUtc,
    IReadOnlyList<string> MemberEmails,
    bool CanManageStatus,
    bool CanCreateOrderSheet,
    IReadOnlyList<OrderSheetSummaryDto> OrderSheets);

public sealed record OrderSheetSummaryDto(
    Guid Id,
    string Status,
    string CreatedByUsername,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc,
    decimal TotalEuro,
    IReadOnlyList<OrderSheetItemDto> Items);

public sealed record OrderSheetItemDto(
    Guid Id,
    string SiteName,
    string ComponentName,
    string Brand,
    string Link,
    decimal PriceEuro);

public sealed record CreateProjectCommand(
    string Name,
    string Idea,
    string Description,
    string[] MemberEmails);

public sealed record UpdateProjectStatusCommand(string Status);

public sealed record CreateOrderSheetCommand(
    IReadOnlyList<CreateOrderSheetItemCommand> Items,
    bool Submit);

public sealed record CreateOrderSheetItemCommand(
    string SiteName,
    string ComponentName,
    string Brand,
    string Link,
    decimal PriceEuro);
