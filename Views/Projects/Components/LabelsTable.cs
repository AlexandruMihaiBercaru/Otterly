using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj.Data;

namespace Proj.Views.Projects.Components;

public class LabelsTable : ViewComponent
{
    public record ViewModel(Guid ProjectId, List<Models.Label> Labels);

    private readonly ApplicationDbContext _context;

    public LabelsTable(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid projectId)
    {
        var labels = await _context.Labels.Where(l => l.ProjectId == projectId).ToListAsync();

        return View(new ViewModel(projectId, labels));
    }
}