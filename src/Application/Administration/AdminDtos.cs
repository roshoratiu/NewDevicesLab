namespace NewDevicesLab.Application.Administration;

public sealed record AdminOverviewDto(
    IReadOnlyList<AdminUserDto> Users,
    IReadOnlyList<AdminGroupDto> Groups,
    IReadOnlyList<PermissionDto> Permissions);

public sealed record AdminUserDto(
    Guid Id,
    string RadboudEmail,
    string Username,
    string StudentNumber,
    DateOnly EnrollmentDate,
    bool IsActive,
    IReadOnlyList<string> Groups);

public sealed record AdminGroupDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<string> Permissions,
    int MemberCount);

public sealed record PermissionDto(
    Guid Id,
    string Code,
    string Name,
    string Description);

public sealed record CreateUserCommand(
    string RadboudEmail,
    string Username,
    string Password,
    string StudentNumber,
    DateOnly EnrollmentDate,
    Guid[] GroupIds);

public sealed record UpdateUserGroupsCommand(
    Guid UserId,
    Guid[] GroupIds);

public sealed record UpdateGroupPermissionsCommand(
    Guid GroupId,
    Guid[] PermissionIds);
