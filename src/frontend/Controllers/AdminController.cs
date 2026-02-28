using Microsoft.AspNetCore.Mvc;
using NewDevicesLab.Application.Administration;
using Microsoft.AspNetCore.Authorization;
using NewDevicesLab.Frontend.Security;

namespace NewDevicesLab.Frontend.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = PermissionPolicyProvider.PolicyPrefix + "admin.access")]
public class AdminController(IAdministrationService administrationService) : ControllerBase
{
    [HttpGet("overview")]
    [ProducesResponseType(typeof(AdminOverviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var overview = await administrationService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpPost("users")]
    [Authorize(Policy = PermissionPolicyProvider.PolicyPrefix + "admin.users.manage")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminUserDto>> CreateUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await administrationService.CreateUserAsync(
                new CreateUserCommand(
                    request.RadboudEmail,
                    request.Username,
                    request.Password,
                    request.StudentNumber,
                    request.EnrollmentDate,
                    request.GroupIds),
                cancellationToken);

            return Created("/api/admin/overview", created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("users/{userId:guid}/groups")]
    [Authorize(Policy = PermissionPolicyProvider.PolicyPrefix + "admin.users.manage")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminUserDto>> UpdateUserGroups(
        Guid userId,
        [FromBody] UpdateUserGroupsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await administrationService.UpdateUserGroupsAsync(
                new UpdateUserGroupsCommand(userId, request.GroupIds),
                cancellationToken);

            return Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("groups/{groupId:guid}/permissions")]
    [Authorize(Policy = PermissionPolicyProvider.PolicyPrefix + "admin.groups.manage")]
    [ProducesResponseType(typeof(AdminGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminGroupDto>> UpdateGroupPermissions(
        Guid groupId,
        [FromBody] UpdateGroupPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await administrationService.UpdateGroupPermissionsAsync(
                new UpdateGroupPermissionsCommand(groupId, request.PermissionIds),
                cancellationToken);

            return Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}

public sealed record CreateAdminUserRequest(
    string RadboudEmail,
    string Username,
    string Password,
    string StudentNumber,
    DateOnly EnrollmentDate,
    Guid[] GroupIds);

public sealed record UpdateUserGroupsRequest(Guid[] GroupIds);

public sealed record UpdateGroupPermissionsRequest(Guid[] PermissionIds);
