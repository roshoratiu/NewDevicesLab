namespace NewDevicesLab.Application.Security;

public sealed record AuthenticatedUserDto(
    Guid Id,
    string RadboudEmail,
    string Username,
    IReadOnlyList<string> Groups,
    IReadOnlyList<string> Permissions);
