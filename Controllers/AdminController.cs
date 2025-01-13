using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj.Models;

namespace Proj.Controllers;

[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken ct = default)
    {
        var users = await _userManager.Users.ToListAsync(ct);

        return View(users);
    }

    [HttpPost("users/{userId:guid}/impersonate")]
    public async Task<IActionResult> Impersonate([FromRoute] Guid userId)
    {
        var user = await _signInManager.UserManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound();
        }

        // Create a new claims principal for the impersonated user
        var userPrincipal = await _signInManager.CreateUserPrincipalAsync(user);

        // Add an impersonation claim to track the original admin
        var impersonationClaim = new Claim("Impersonator", User.Identity.Name);
        var claimsIdentity = (ClaimsIdentity)userPrincipal.Identity;
        claimsIdentity.AddClaim(impersonationClaim);

        // Sign in the impersonated user
        await _signInManager.SignOutAsync();
        await _signInManager.Context.SignInAsync(
            IdentityConstants.ApplicationScheme,
            userPrincipal);
        return RedirectToAction("Index", "Home");
    }
}