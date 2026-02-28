using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NewDevicesLab.Application.Devices;
using NewDevicesLab.Frontend.Security;

namespace NewDevicesLab.Frontend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDeviceService deviceService) : ControllerBase
{
    /// <summary>
    /// Returns all registered devices.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PermissionPolicyProvider.PolicyPrefix + "devices.read")]
    [ProducesResponseType(typeof(IEnumerable<DeviceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var devices = await deviceService.GetAllAsync(cancellationToken);
        return Ok(devices);
    }

    /// <summary>
    /// Creates a new device.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PermissionPolicyProvider.PolicyPrefix + "devices.create")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeviceDto>> Create(
        [FromBody] CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Device name is required.");
        }

        var created = await deviceService.CreateAsync(request.Name.Trim(), cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}

public sealed record CreateDeviceRequest(string Name);
