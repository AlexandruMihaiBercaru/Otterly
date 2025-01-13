using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj.Data;
using Proj.Identity;
using Proj.Models;

namespace Proj.Controllers;

[Route("projects/{projectId:guid}")]
public class LabelsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LabelsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("addlabel")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public IActionResult New([FromRoute] Guid projectId) => View();


    [HttpPost("addlabel")]
    [Authorize(Policy = OrganizerRequirement.Policy)]
    public async Task<IActionResult> New(
        [FromRoute] Guid projectId,
        [FromForm] LabelCommand.Create cmd,
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

        return Redirect($"/projects/{projectId}/settings");
    }
}