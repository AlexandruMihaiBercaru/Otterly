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
    private readonly CurrentUser _user;

    public ProjectsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        CurrentUser user)
    {
        _context = context;
        _userManager = userManager;
        _user = user;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        if (_user.IsAdmin)
        {
            var adminProjects = await _context.Projects.ToListAsync(ct);
            return View(adminProjects);
        }

        var projects = await _context.Memberships
            .Where(m => !m.EndedAt.HasValue && m.UserId == _user.Id && m.JoinedAt.HasValue)
            .Select(m => m.Project)
            .ToListAsync(ct);

        return View(projects);
    }

    [HttpGet("{projectId:guid}")]
    [Authorize(Policy = MemberRequirement.Policy)]
    public async Task<IActionResult> Show(
        [FromRoute] Guid projectId,
        CancellationToken ct = default)
    {
        if (TempData.ContainsKey("message"))
        {
            ViewBag.Message = TempData["message"];
            ViewBag.Alert = TempData["messageType"];
        }

        var project = await _context.Projects
            .FirstOrDefaultAsync(p => !p.DeletedAt.HasValue && p.Id == projectId, ct);
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
        [FromForm] ProjectCommand.Create cmd,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var (project, membership) = Project.From(cmd, _user.Id);
        await _context.Projects.AddAsync(project, ct);
        await _context.Memberships.AddAsync(membership, ct);
        await _context.SaveChangesAsync(ct);

        TempData["message"] = "The project has been successfully added.";
        TempData["messageType"] = "alert-success";

        return Redirect($"/projects/{project.Id}");
    }

    [HttpGet("edit/{projectId:guid}")]
    public async Task<IActionResult> Edit(
        [FromRoute] Guid projectId,
        CancellationToken ct = default)
    {
        var project = await _context.Projects.FindAsync([projectId], ct);
        if (project is null)
        {
            return NotFound();
        }

        return View(new ProjectCommand.Edit(project.Id, project.Name, project.Summary));
    }

    [HttpPost("edit/{projectId:guid}")]
    public async Task<IActionResult> Edit(
        [FromRoute] Guid projectId,
        [FromForm] ProjectCommand.Edit cmd,
        CancellationToken ct = default)
    {
        var project = await _context.Projects.FindAsync([projectId], ct);
        if (project is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(new ProjectCommand.Edit(project.Id, project.Name, project.Summary));
        }

        project.Edit(cmd);
        await _context.SaveChangesAsync(ct);

        TempData["message"] = "The project has been modified";
        TempData["messageType"] = "alert-success";
        return Redirect($"/projects/{projectId}");
    }

    [HttpGet("{projectId:guid}/settings")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> Settings(
        [FromRoute] Guid projectId,
        CancellationToken ct = default
    )
    {
        var project = await _context.Projects.FindAsync([projectId], ct);
        if (project is null)
        {
            return NotFound();
        }

        var members = await _context.Memberships
            .Where(m => m.ProjectId == projectId && !m.EndedAt.HasValue)
            .Include(m => m.User)
            .ToListAsync(ct);

        return View(new Projects.Settings(project, members));
    }

    [HttpPost("{projectId:guid}/settings/invite-member")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> InviteMember(
        [FromRoute] Guid projectId,
        [FromForm] ProjectCommand.InviteMember cmd,
        CancellationToken ct = default)
    {
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
            return RedirectToAction("Settings", new { projectId });
        }

        var invitee = await _userManager.FindByEmailAsync(cmd.Email);
        if (invitee is null)
        {
            TempData["toastError"] = $"User with email address {cmd.Email} does not exist.";
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

    [HttpPost("{projectId:guid}/invitations")]
    public async Task<IActionResult> HandleInvitationRespose(
        [FromRoute] Guid projectId,
        [FromForm] ProjectCommand.HandleInvitationRespose cmd,
        CancellationToken ct = default
    )
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", "Inbox");
        }

        var user = await _context.ApplicationUsers
            .AsNoTracking()
            .Include(u =>
                u.Memberships.Where(m => !m.JoinedAt.HasValue))
            .FirstOrDefaultAsync(u => u.Id == _user.Id, ct);
        if (user is null)
        {
            return NotFound();
        }

        var membership = user.Memberships
            .FirstOrDefault(m => m.ProjectId == cmd.ProjectId);

        if (cmd.Intent == "accept")
        {
            var accepted = user.Accept(membership);
            _context.Entry(accepted).State = EntityState.Modified;
        }
        else
        {
            _context.Entry(membership).State = EntityState.Deleted;
        }

        await _context.SaveChangesAsync(ct);

        return RedirectToAction("Index", "Inbox");
    }

    [HttpPost("{projectId:guid}/delete")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid projectId,
        CancellationToken ct = default)
    {
        var project = await _context.Projects.FindAsync([projectId], ct);
        if (project is null)
        {
            return NotFound();
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);

        return Redirect("/projects");
    }
}