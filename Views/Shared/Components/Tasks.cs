using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj.Data;

namespace Proj.Views.Shared.Components;

public class Tasks : ViewComponent
{
    public record ViewModel(IEnumerable<Proj.Models.Task> Tasks, bool CanEdit);

    private readonly ApplicationDbContext _context;

    public Tasks(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid projectId, bool canEdit)
    {
        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId && !t.DeletedAt.HasValue)
            .Include(t => t.Assignments)
            .ThenInclude(a => a.User)
            .Include(t => t.Label)
            .ToListAsync();

        return View(new ViewModel(tasks, canEdit));
    }
}