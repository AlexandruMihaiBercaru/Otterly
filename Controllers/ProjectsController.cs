using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj.Data;
using Proj.Identity;
using Proj.Models;
using Proj.ViewModels;

namespace Proj.Controllers;

[Route("projects")]
public class ProjectsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, ILogger<ProjectsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var adminProjects =
                await _context.Memberships
                    .Where(m => !m.Project.DeletedAt.HasValue)
                    .Include(m => m.Project)
                    .ToListAsync();
            return View(adminProjects);
        }

        var projects = await _context.Memberships
            .Where(m => !m.EndedAt.HasValue && m.UserId == user.Id && m.JoinedAt.HasValue)
            .Include(m => m.Project)
            .ToListAsync();

        return View(projects);
    }

    [HttpGet("{id:guid}")]
    public IActionResult Show([FromRoute] Guid id)
    {
        if (TempData.ContainsKey("message"))
        {
            ViewBag.Message = TempData["message"];
            ViewBag.Alert = TempData["messageType"];
        }

        var project = _context.Projects
            .Where(p => !p.DeletedAt.HasValue)
            .FirstOrDefault(p => p.Id == id);
        if (project is null)
        {
            return NotFound();
        }

        return View(project);
    }

    [HttpGet("new")]
    public IActionResult New() => View();

    [HttpPost("new")]
    public async Task<IActionResult> New(
        ProjectCommand.Create cmd,
        CancellationToken ct = default)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        var (project, membership) = Project.From(cmd, user!.Id);
        _context.Projects.Add(project);
        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync(ct);

        TempData["message"] = "The project has been successfully added.";
        TempData["messageType"] = "alert-success";

        return Redirect($"/projects/{project.Id}");
    }

    [HttpGet("{projectId:guid}/settings")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> Settings([FromRoute] Guid projectId)
    {
        var members = await _context.Memberships
            .Where(m => m.ProjectId == projectId && !m.EndedAt.HasValue)
            .Include(m => m.User)
            .ToListAsync();

        return View(new Projects.Settings(members, projectId));
    }

    [HttpPost("{projectId:guid}/settings/invite-member")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> InviteMember(
        [FromRoute] Guid projectId,
        [FromForm] ProjectCommand.InviteMember cmd,
        CancellationToken ct = default)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var project = await _context.Projects
            .Where(p => p.Id == projectId && !p.DeletedAt.HasValue)
            .Include(p => p.Memberships)
            .FirstOrDefaultAsync(ct);
        if (project is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogInformation("invalid model");
            return RedirectToAction("Settings", new { projectId });
        }

        var invitee = await _userManager.FindByEmailAsync(cmd.Email);
        if (invitee is null)
        {
            ModelState.AddModelError("Email", $"User with email address {cmd.Email} does not exist.");
            TempData["toastError"] = $"User with email address {cmd.Email} does not exist.";
            _logger.LogInformation("invitee not found");

            return RedirectToAction("Settings", new { projectId });
        }

        try
        {
            var invitation = project.InviteMember(invitee.Id);
            _context.Memberships.Add(invitation);
            await _context.SaveChangesAsync(ct);

            TempData["message"] = $"Invitation was sent to {cmd.Email}.";
            return RedirectToAction("Settings", new { projectId });
        }
        catch (Exceptions.ExistingMember err)
        {
            TempData["toastError"] = err.Message;
            return RedirectToAction("Settings", new { projectId });
        }
    }

    [HttpPost("{projectId:guid}/delete")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> Delete([FromRoute] Guid projectId,
        CancellationToken ct = default)
    {
        return View();
    }
}