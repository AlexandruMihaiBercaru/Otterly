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

[Route("projects/{projectId:guid}")]
public class LabelsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CurrentUser _user;
    public LabelsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        CurrentUser user)
    {
        _context = context;
        _userManager = userManager;
        _user = user;
    }

    [HttpGet("addlabel")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public IActionResult New([FromRoute] Guid projectId)
    {
        return View();
    }

    [HttpPost("addlabel")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> New([FromForm] LabelCommand.Create cmd,
                                     [FromRoute] Guid projectId,
                                     CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return View(cmd);
        }
        var label = Label.From(cmd, projectId);
        await _context.Labels.AddAsync(label, ct);
        await _context.SaveChangesAsync(ct);

        TempData["message"] = "Label added successfully.";
        TempData["messageType"] = "alert-success";

        return Redirect($"/projects/{projectId}");
    }

    public IActionResult Index()
    {
        return View();
    }


}
