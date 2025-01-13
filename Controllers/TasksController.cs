using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Proj.Data;
using Proj.Identity;
using Proj.Models;
using Proj.ViewModels;
using System.Security.Cryptography.Pkcs;
using System.Threading.Tasks;


namespace Proj.Controllers;

[Route("projects/{projectId:guid}/tasks")]
[Authorize(Policy = MemberRequirement.Policy)]
public class TasksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentUser _user;

    public TasksController(ApplicationDbContext context, CurrentUser user)
    {
        _context = context;
        _user = user;
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> Show(
        [FromRoute] Guid projectId,
        [FromRoute] Guid taskId,
        CancellationToken ct = default)
    {
        if (TempData.ContainsKey("message"))
        {
            ViewBag.Message = TempData["message"];
            ViewBag.Alert = TempData["messageType"];
        }

        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => !t.DeletedAt.HasValue && t.Id == taskId, ct);
        if (task is null)
        {
            return NotFound();
        }
        // also need to add labels!!!

        task.ProjectMembers = GetMembersUnassignedToTask(projectId, taskId);
        return View(task);
    }

    // doar organizatorul proiectului poate adauga un task la un proiect
    [HttpGet("new")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public IActionResult New([FromRoute] Guid projectId) => View();

    [HttpPost("new")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> New(
        [FromRoute] Guid projectId,
        TaskCommand.Create cmd,
        CancellationToken ct = default)
    {
        var task = Models.Task.From(cmd, projectId);

        if (!ModelState.IsValid)
        {
            return View();
        }

        var sanitizer = new HtmlSanitizer();
        task.Description = sanitizer.Sanitize(task.Description);
        await _context.Tasks.AddAsync(task, ct);
        await _context.SaveChangesAsync(ct);

        TempData["message"] = "The new task has just been successfully added.";
        TempData["messageType"] = "alert-success";
        return Redirect($"/projects/{projectId}");
    }

    [HttpGet("{taskId:guid}/edit")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> Edit(
        [FromRoute] Guid projectId,
        [FromRoute] Guid taskId,
        CancellationToken ct = default)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => !t.DeletedAt.HasValue && t.Id == taskId, ct);
        // add labels!
        return View(new TaskCommand.Edit(task.Id, task.Name, task.Description,
                                         task.Status, task.StartDate, task.EndDate,
                                         task.MediaUrl));
    }

    [HttpPost("{taskId:guid}/edit")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> Edit(
        [FromRoute] Guid projectId,
        [FromRoute] Guid taskId,
        [FromForm] TaskCommand.Edit cmd, CancellationToken ct = default)
    {
        var task = await _context.Tasks.FindAsync([taskId], ct);
        if (task is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(new TaskCommand.Edit(task.Id, task.Name, task.Description,
                                         task.Status, task.StartDate, task.EndDate,
                                         task.MediaUrl));
        }

        task.Name = cmd.Name;
        var sanitizer = new HtmlSanitizer();
        task.Description = sanitizer.Sanitize(cmd.Description);
        task.MediaUrl = cmd.MediaUrl;
        task.StartDate = cmd.StartDate;
        task.EndDate = cmd.EndDate;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        TempData["message"] = "The task has been modified";
        TempData["messageType"] = "alert-success";

        await _context.SaveChangesAsync(ct);
        return Redirect($"/projects/{projectId}");
    }


    [HttpPost("change-status")]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] Guid projectId,
        [FromForm] TaskCommand.ChangeStatus cmd,
        CancellationToken ct = default)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => !t.DeletedAt.HasValue && t.Id == cmd.TaskId, ct);
        if (task is null)
        {
            return NotFound();
        }

        task.Status = cmd.NewStatus;
        await _context.SaveChangesAsync(ct);

        return Redirect($"/projects/{projectId}");
    }

    [HttpPost("{taskId:guid}/comment")]
    public async Task<IActionResult> Comment(
        [FromRoute] Guid projectId,
        [FromRoute] Guid taskId,
        [FromForm] CommentCommand.New cmd,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return Redirect($"/projects/{projectId}/tasks/{taskId}");
        }

        var comment = new Comment(taskId, _user.Id, cmd.Content);
        await _context.Comments.AddAsync(comment, ct);
        await _context.SaveChangesAsync(ct);

        return Redirect($"/projects/{projectId}/tasks/{taskId}");
    }


    [HttpPost("{taskId:guid}/comment/{commentId:guid}")]
    public async Task<IActionResult> EditComment(
        [FromRoute] Guid projectId,
        [FromRoute] Guid taskId,
        [FromRoute] Guid commentId,
        [FromForm] CommentCommand.New cmd,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return Redirect($"/projects/{projectId}/tasks/{taskId}");
        }

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId, ct);
        if (comment is null)
        {
            return NotFound();
        }

        if (!_user.IsAdmin && comment.UserId != _user.Id)
        {
            return Unauthorized();
        }

        comment.Content = cmd.Content;
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync(ct);

        return Redirect($"/projects/{projectId}/tasks/{taskId}");
    }

    [HttpPost("{taskId:guid}/comment/{commentId:guid}/delete")]
    public async Task<IActionResult> DeleteComment([FromRoute] Guid projectId, Guid taskId,
        Guid commentId, CancellationToken ct = default)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId, ct);
        if (comment is null)
        {
            return NotFound();
        }

        if (!_user.IsAdmin && comment.UserId != _user.Id)
        {
            return Unauthorized();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync(ct);

        return Redirect($"/projects/{projectId}/tasks/{taskId}");
    }


    [HttpPost("{taskId:guid}/delete")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid taskId,
        [FromRoute] Guid projectId,
        CancellationToken ct = default)
    {
        var task = await _context.Tasks.FindAsync([taskId], ct);
        if (task is null)
        {
            return NotFound();
        }
        task.DeletedAt = DateTimeOffset.UtcNow;
        //_context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);

        TempData["message"] = "Task deleted.";
        TempData["messageType"] = "alert-success";

        return Redirect($"/projects/{projectId}");
    }



    [HttpPost("{taskId:guid}/assign")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> AssignTask([FromForm] String userId,
                                                [FromRoute] Guid taskId,
                                                [FromRoute] Guid projectId,
                                                CancellationToken ct = default)
    {
        if(ModelState.IsValid)
        {
            var assignment = new Assignment
            {
                UserId = new Guid(userId),
                TaskId = taskId
            };
            await _context.Assignments.AddAsync(assignment, ct);
            await _context.SaveChangesAsync(ct);
            TempData["message"] = "The task was successfully assigned.";
            TempData["messageType"] = "alert-success";
            return Redirect($"/projects/{projectId}");
        }
        else
        {
            return Redirect($"/projects/{projectId}/tasks/{taskId}");
        }
    }


    [NonAction]
    public IEnumerable<SelectListItem> GetMembersUnassignedToTask(Guid projectId, Guid taskId)
    {
        var selectList = new List<SelectListItem>();
        var members = _context.ApplicationUsers
                              .Where(u => u.Memberships
                                           .Any(m => !m.EndedAt.HasValue &&  
                                                     m.ProjectId == projectId && 
                                                     m.JoinedAt.HasValue)) 
                              .Where(u => !u.Assignments
                                           .Any(a => a.TaskId == taskId)); 
        foreach (var mem in members)
        {
            selectList.Add(new SelectListItem
            {
                Value = mem.Id.ToString(),
                Text = mem.FirstName + " " + mem.LastName + " (" + mem.Email + ")"
            });
        }
        return selectList;
    }
}