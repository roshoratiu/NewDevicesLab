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

        return MapAuthenticatedUser(user);
    }

    public async Task<AuthenticatedUserDto?> GetAuthenticatedUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await identityRepository.FindActiveUserByIdAsync(userId, cancellationToken);
        return user is null ? null : MapAuthenticatedUser(user);
    }

    public async Task<AuthenticatedUserDto> UpdateProfileAsync(
        Guid userId,
        string username,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }

        var normalizedUsername = username.Trim();

        var user = await identityRepository.FindActiveUserByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("User account was not found.");

        var usernameExists = await identityRepository.UsernameExistsAsync(
            normalizedUsername,
            userId,
            cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException("That username is already in use.");
        }

        user.Username = normalizedUsername;
        await identityRepository.SaveChangesAsync(cancellationToken);

        return MapAuthenticatedUser(user);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            throw new ArgumentException("Current password is required.", nameof(currentPassword));
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 12)
        {
            throw new ArgumentException("New passwords must be at least 12 characters long.", nameof(newPassword));
        }

        var user = await identityRepository.FindActiveUserByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("User account was not found.");

        if (!passwordHasher.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("The current password is incorrect.");
        }

        user.PasswordHash = passwordHasher.Hash(newPassword);
        await identityRepository.SaveChangesAsync(cancellationToken);
    }

    private static AuthenticatedUserDto MapAuthenticatedUser(Domain.Entities.AppUser user)
    {
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
