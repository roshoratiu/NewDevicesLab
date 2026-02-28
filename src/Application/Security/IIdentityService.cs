namespace NewDevicesLab.Application.Security;

public interface IIdentityService
{
    Task<AuthenticatedUserDto?> ValidateCredentialsAsync(
        string identifier,
        string password,
        CancellationToken cancellationToken);
}
