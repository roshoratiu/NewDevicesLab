using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace NewDevicesLab.Frontend.Security;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    public const string PermissionClaimType = "permission";
    public const string GroupClaimType = "group";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        var permissions = context.User.FindAll(PermissionClaimType)
            .Select(item => item.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (permissions.Contains("system.full_access")
            || permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
