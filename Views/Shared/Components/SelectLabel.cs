using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj.Data;
using Proj.Models;

namespace Proj.Views.Shared.Components;

public class SelectLabel : ViewComponent
{
    public record ViewModel(
        IEnumerable<Proj.Models.Label> Options,
        Proj.Models.Label? Default,
        string Name);

    private readonly ApplicationDbContext _context;

    public SelectLabel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        Guid projectId,
        string name,
        Guid? defaultLabelId = null
    )
    {
        var labels = await _context.Labels.Where(l => l.ProjectId == projectId).ToListAsync();
        var @default = defaultLabelId is null ? null : labels.Find(l => l.Id == defaultLabelId);
        return View(new ViewModel(labels, @default, name));
    }
}