namespace NewDevicesLab.Application.Security;

public class IdentityService(
    IIdentityRepository identityRepository,
    IPasswordHasher passwordHasher) : IIdentityService
{
    public async Task<AuthenticatedUserDto?> ValidateCredentialsAsync(
        string identifier,
        string password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await identityRepository.FindActiveUserByIdentifierAsync(identifier.Trim(), cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        var groups = user.UserGroups
            .Select(item => item.Group.Name)
            .OrderBy(item => item)
            .ToList();

        var permissions = user.UserGroups
            .SelectMany(item => item.Group.GroupPermissions)
            .Select(item => item.Permission.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();

        return new AuthenticatedUserDto(
            user.Id,
            user.RadboudEmail,
            user.Username,
            groups,
            permissions);
    }
}
