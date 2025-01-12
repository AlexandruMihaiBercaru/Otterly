using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj.Data;
using Proj.Identity;

namespace Proj.Views.Shared.Components;

public class Inbox : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentUser _user;

    public Inbox(ApplicationDbContext context, CurrentUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _context.ApplicationUsers
            .Where(u => u.Id == _user.Id)
            .Include(u => u.Memberships.Where(m => !m.JoinedAt.HasValue))
            .ThenInclude(m => m.Project)
            .FirstOrDefaultAsync();

        return View(user.Memberships);
    }
}