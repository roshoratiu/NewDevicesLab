using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewDevicesLab.Application.Projects;

namespace NewDevicesLab.Frontend.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ProjectDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProjectDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var dashboard = await projectService.GetDashboardAsync(userId.Value, cancellationToken);
        return Ok(dashboard);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProjectSummaryDto>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var created = await projectService.CreateProjectAsync(
                userId.Value,
                new CreateProjectCommand(
                    request.Name,
                    request.Idea,
                    request.Description,
                    request.MemberEmails ?? []),
                cancellationToken);

            return Created($"/api/projects/{created.Id}", created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{projectId:guid}/status")]
    [ProducesResponseType(typeof(ProjectSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProjectSummaryDto>> UpdateStatus(
        Guid projectId,
        [FromBody] UpdateProjectStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await projectService.UpdateStatusAsync(
                userId.Value,
                projectId,
                new UpdateProjectStatusCommand(request.Status),
                cancellationToken);

            return Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{projectId:guid}/order-sheets")]
    [ProducesResponseType(typeof(OrderSheetSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderSheetSummaryDto>> CreateOrderSheet(
        Guid projectId,
        [FromBody] CreateOrderSheetRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var created = await projectService.CreateOrderSheetAsync(
                userId.Value,
                projectId,
                new CreateOrderSheetCommand(
                    (request.Items ?? []).Select(item => new CreateOrderSheetItemCommand(
                        item.SiteName,
                        item.ComponentName,
                        item.Brand,
                        item.Link,
                        item.PriceEuro)).ToList(),
                    request.Submit),
                cancellationToken);

            return Created($"/api/projects/{projectId}/order-sheets/{created.Id}", created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}

public sealed record CreateProjectRequest(
    string Name,
    string Idea,
    string Description,
    string[] MemberEmails);

public sealed record UpdateProjectStatusRequest(string Status);

public sealed record CreateOrderSheetRequest(
    CreateOrderSheetItemRequest[] Items,
    bool Submit);

public sealed record CreateOrderSheetItemRequest(
    string SiteName,
    string ComponentName,
    string Brand,
    string Link,
    decimal PriceEuro);
