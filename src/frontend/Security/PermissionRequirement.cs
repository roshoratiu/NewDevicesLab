using Microsoft.AspNetCore.Authorization;

namespace NewDevicesLab.Frontend.Security;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
