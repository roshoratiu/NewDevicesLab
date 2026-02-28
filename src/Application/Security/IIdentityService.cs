namespace NewDevicesLab.Application.Security;

public interface IIdentityService
{
    Task<AuthenticatedUserDto?> ValidateCredentialsAsync(
        string identifier,
        string password,
        CancellationToken cancellationToken);

    Task<AuthenticatedUserDto?> GetAuthenticatedUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<AuthenticatedUserDto> UpdateProfileAsync(
        Guid userId,
        string username,
        CancellationToken cancellationToken);

    Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken);
}
