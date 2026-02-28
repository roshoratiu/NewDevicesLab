namespace NewDevicesLab.Application.Administration;

public interface IAdministrationService
{
    Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);

    Task<AdminUserDto> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken);

    Task<AdminUserDto> UpdateUserGroupsAsync(UpdateUserGroupsCommand command, CancellationToken cancellationToken);

    Task<AdminGroupDto> UpdateGroupPermissionsAsync(
        UpdateGroupPermissionsCommand command,
        CancellationToken cancellationToken);
}
