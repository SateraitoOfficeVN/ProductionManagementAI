using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Application.Auth;
using ProductionManagementAI.Infrastructure.Identity;

namespace ProductionManagementAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<MeResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        // Generic failure message regardless of which check fails (ADR-0002 STRIDE: Information disclosure).
        var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(await BuildMeResponseAsync(user));
    }

    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(await BuildMeResponseAsync(user));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }

    private async Task<MeResponse> BuildMeResponseAsync(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new MeResponse(user.Id, user.UserName!, user.DisplayName, roles.ToList());
    }
}
