using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj.Data;
using Proj.Identity;
using Proj.Models;

namespace Proj.Controllers;

[Authorize, Route("projects")]
public class ProjectsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public ProjectsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var adminProjects =
                await _context.Memberships.Include(m => m.Project)
                    .Where(m => !m.Project.DeletedAt.HasValue)
                    .ToListAsync();
            return View(adminProjects);
        }

        var projects = await _context.Memberships
            .Where(x => !x.EndedAt.HasValue && x.UserId == user.Id)
            .Include(m => m.Project).ToListAsync();

        return View(projects);
    }

    [HttpGet("{id:guid}")]
    public IActionResult Show([FromRoute] Guid id)
    {
        var project = _context.Projects
            .Where(p => !p.DeletedAt.HasValue)
            .FirstOrDefault(p => p.Id == id);
        if (project is null)
        {
            return NotFound();
        }

        return View(project);
    }

    [HttpPost("new")]
    public async Task<IActionResult> New(
        [FromBody] ProjectCommand.Create cmd,
        CancellationToken ct = default)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var (project, membership) = Project.From(cmd, user!.Id);

        _context.Projects.Add(project);
        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync(ct);

        return RedirectToAction("Show");
    }

    [HttpPost("{projectId:guid}/delete")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> Delete([FromRoute] Guid projectId,
        CancellationToken ct = default)
    {
        return View();
    }
}