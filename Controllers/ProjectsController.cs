using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj.Data;
using Proj.Identity;
using Proj.Models;

namespace Proj.Controllers;

[Route("projects")]
public class ProjectsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public ProjectsController(ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("index")]
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

    [HttpGet("show/{id:guid}")]
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
    public IActionResult New()
    {
        return View();
    }

    [HttpPost("new")]
    public async Task<IActionResult> New(ProjectCommand.Create cmd,
                                         CancellationToken ct = default)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        if (ModelState.IsValid)
        {
            var (project, membership) = Project.From(cmd, user!.Id);
            _context.Projects.Add(project);
            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync(ct);

            TempData["message"] = "The project has been successfully added.";
            TempData["messageType"] = "alert-success";

            return Redirect("/projects/show/" + project.Id);
        }
        else
        {
            return View();
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