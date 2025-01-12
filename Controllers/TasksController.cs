using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
public class TasksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public TasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("{taskId:guid}")]
    public IActionResult Show([FromRoute] Guid projectId, [FromRoute] Guid taskId)
    {
        if (TempData.ContainsKey("message"))
        {
            ViewBag.Message = TempData["message"];
            ViewBag.Alert = TempData["messageType"];
        }
        var task = _context.Tasks.Where(t => !t.DeletedAt.HasValue && t.Id == taskId).First();
        // also need to add comments (activities) and labels!!!

        return View(task);
    }

    // doar organizatorul proiectului poate adauga un task la un proiect
    [HttpGet("new")]
    public IActionResult New([FromRoute] Guid projectId)
    {
        if(checkOrganizer(projectId) == true)
        {
            //ShowOrganizerButtons(projectId);
            return View();
        }
        else
        {
            TempData["message"] = "You are not allowed to add a new task to the project!";
            TempData["messageType"] = "alert-danger";
            return Redirect($"/projects/{projectId}");
        }
    }


    [HttpPost("new")]
    public async Task<IActionResult> New([FromRoute] Guid projectId, 
                                         TaskCommand.Create cmd,
                                         CancellationToken ct = default)
    {
        var sanitizer = new HtmlSanitizer();

        if(ModelState.IsValid)
        {
            var task = Models.Task.From(cmd, projectId);
            task.Description = sanitizer.Sanitize(task.Description);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(ct);
            TempData["message"] = "The new task has just been successfully added.";
            TempData["messageType"] = "alert-success";
            return Redirect($"/projects/{projectId}");
        }
        else
        {
            var task = Models.Task.From(cmd, projectId);
            return View(task);
        }
    }

    [HttpGet("{taskId:guid}/edit")]
    public IActionResult Edit([FromRoute] Guid projectId, [FromRoute] Guid taskId)
    {
        if (checkOrganizer(projectId) == true)
        {
            var task = _context.Tasks.Where(t => !t.DeletedAt.HasValue && t.Id == taskId).First();
            // add comments and labels!
            return View(task);
        }
        else
        {
            TempData["message"] = "You are not allowed to edit the tasks assigned to this project!";
            TempData["messageType"] = "alert-danger";
            return Redirect($"/projects/{projectId}");
        }
    }

    [HttpPost("{taskId:guid}/edit")]
    public IActionResult Edit([FromRoute] Guid projectId, [FromRoute] Guid taskId, TaskCommand.Create cmd)
    {
        var sanitizer = new HtmlSanitizer();
        var requestTask = Models.Task.From(cmd, projectId);
        Models.Task task = _context.Tasks.Find(taskId);
        if (ModelState.IsValid)
        {
            if (checkOrganizer(projectId) == true)
            {
                task.Name = requestTask.Name;
                task.Description = sanitizer.Sanitize(requestTask.Description); ;
                task.MediaUrl = requestTask.MediaUrl;
                task.StartDate = requestTask.StartDate;
                task.EndDate = requestTask.EndDate;
                task.UpdatedAt = DateTimeOffset.UtcNow;

                TempData["message"] = "The task has been modified";
                TempData["messageType"] = "alert-success";

                _context.SaveChanges();
                return Redirect($"/projects/{projectId}");
            }
            else
            {
                TempData["message"] = "You are not allowed to edit the tasks assigned to this project!";
                TempData["messageType"] = "alert-danger";
                return Redirect($"/projects/{projectId}");
            }
        }
        else
        {
            return View(requestTask);
        }
    }

    private bool checkOrganizer(Guid pId)
    {
        var userId = new Guid(_userManager.GetUserId(User));
        var project = _context.Projects
                            .Where(p => p.Id == pId && !p.DeletedAt.HasValue)
                            .First();
        return userId == project.OrganizerId; 
    }

    private void ShowOrganizerButtons(Guid Id)
    {
        ViewBag.ShowButtons = false;
        if(checkOrganizer(Id))
        {
            ViewBag.ShowButtons = true;
        }
    }
    
    //[Route("/tasks/changestatus", Name ="changestaskstatus")]
    [HttpPost("/changestatus")]
    public IActionResult ChangeStatus(Guid projectId, 
                                      Guid taskId, 
                                      int newStatus)
    {
        var task = _context.Tasks.Where(t => !t.DeletedAt.HasValue && t.Id == taskId).First();
        if (task != null)
        {
            if(newStatus == 1)
                task.Status = "Not Started";
            if (newStatus == 2)
                task.Status = "In Progress";
            if (newStatus == 3)
                task.Status = "Done";
            _context.SaveChanges();
        }
        return Redirect($"/projects/{projectId}");
    }

}
