using NewDevicesLab.Application.Security;
using NewDevicesLab.Domain.Entities;

namespace NewDevicesLab.Application.Administration;

public class AdministrationService(
    IAdministrationRepository administrationRepository,
    IPasswordHasher passwordHasher) : IAdministrationService
{
    public async Task<AdminOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var users = await administrationRepository.GetUsersAsync(cancellationToken);
        var groups = await administrationRepository.GetGroupsAsync(cancellationToken);
        var permissions = await administrationRepository.GetPermissionsAsync(cancellationToken);

        return new AdminOverviewDto(
            users.Select(MapUser).ToList(),
            groups.Select(MapGroup).ToList(),
            permissions.Select(MapPermission).ToList());
    }

    public async Task<AdminUserDto> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RadboudEmail)
            || !command.RadboudEmail.Trim().EndsWith("@ru.nl", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A valid Radboud @ru.nl email is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.Username))
        {
            throw new ArgumentException("Username is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 12)
        {
            throw new ArgumentException("Passwords must be at least 12 characters long.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.StudentNumber))
        {
            throw new ArgumentException("Student number is required.", nameof(command));
        }

        if (command.GroupIds.Length == 0)
        {
            throw new ArgumentException("At least one group must be selected.", nameof(command));
        }

        var groups = await administrationRepository.GetGroupsByIdsAsync(command.GroupIds, cancellationToken);
        if (groups.Count != command.GroupIds.Length)
        {
            throw new ArgumentException("One or more selected groups were not found.", nameof(command));
        }

        var createdAtUtc = DateTime.UtcNow;
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            RadboudEmail = command.RadboudEmail.Trim(),
            Username = command.Username.Trim(),
            PasswordHash = passwordHasher.Hash(command.Password),
            StudentNumber = command.StudentNumber.Trim(),
            EnrollmentDate = command.EnrollmentDate,
            IsActive = true,
            CreatedAtUtc = createdAtUtc,
            UserGroups = groups
                .Select(group => new UserGroup
                {
                    UserId = Guid.Empty,
                    GroupId = group.Id,
                    JoinedAtUtc = createdAtUtc
                })
                .ToList()
        };

        foreach (var userGroup in user.UserGroups)
        {
            userGroup.UserId = user.Id;
        }

        var created = await administrationRepository.AddUserAsync(user, cancellationToken);
        return MapUser(created);
    }

    public async Task<AdminUserDto> UpdateUserGroupsAsync(
        UpdateUserGroupsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.GroupIds.Length == 0)
        {
            throw new ArgumentException("At least one group must be selected.", nameof(command));
        }

        var groups = await administrationRepository.GetGroupsByIdsAsync(command.GroupIds, cancellationToken);
        if (groups.Count != command.GroupIds.Length)
        {
            throw new ArgumentException("One or more selected groups were not found.", nameof(command));
        }

        var updated = await administrationRepository.UpdateUserGroupsAsync(
            command.UserId,
            command.GroupIds,
            cancellationToken);

        return MapUser(updated);
    }

    public async Task<AdminGroupDto> UpdateGroupPermissionsAsync(
        UpdateGroupPermissionsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.PermissionIds.Length == 0)
        {
            throw new ArgumentException("At least one permission must be selected.", nameof(command));
        }

        var permissions = await administrationRepository.GetPermissionsByIdsAsync(command.PermissionIds, cancellationToken);
        if (permissions.Count != command.PermissionIds.Length)
        {
            throw new ArgumentException("One or more selected permissions were not found.", nameof(command));
        }

        var updated = await administrationRepository.UpdateGroupPermissionsAsync(
            command.GroupId,
            command.PermissionIds,
            cancellationToken);

        return MapGroup(updated);
    }

    private static AdminUserDto MapUser(AppUser user) => new(
        user.Id,
        user.RadboudEmail,
        user.Username,
        user.StudentNumber,
        user.EnrollmentDate,
        user.IsActive,
        user.UserGroups
            .Select(link => link.Group.Name)
            .OrderBy(name => name)
            .ToList());

    private static AdminGroupDto MapGroup(Group group) => new(
        group.Id,
        group.Name,
        group.Description,
        group.GroupPermissions
            .Select(link => link.Permission.Code)
            .OrderBy(code => code)
            .ToList(),
        group.UserGroups.Count);

    private static PermissionDto MapPermission(Permission permission) => new(
        permission.Id,
        permission.Code,
        permission.Name,
        permission.Description);
}
