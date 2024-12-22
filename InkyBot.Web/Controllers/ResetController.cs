using System.Net.NetworkInformation;
using InkyBot.EInk;
using InkyBot.Users;
using Microsoft.AspNetCore.Mvc;

namespace InkyBot.Web.Controllers;

public class ResetController(
    IUserRepository userRepository,
    IOpenEPaperService service
) : ControllerBase
{
    [HttpGet("api/reset")]
    public async Task<IActionResult> GetAsync(string address, CancellationToken cancellationToken = default)
    {
        if (!PhysicalAddress.TryParse(address, out var parsed))
        {
            ModelState.AddModelError(nameof(address), "Invalid mac address");
            return BadRequest(ModelState);
        }

        if (await userRepository.GetUserByPhysicalAddress(parsed) is not { } user)
        {
            ModelState.AddModelError(nameof(address), "Display not registered");
            return BadRequest(ModelState);
        }

        await userRepository.DeleteUser(user.Id);
        await service.ResetDisplay(parsed, cancellationToken);
        return NoContent();
    }
}