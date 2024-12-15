using Microsoft.AspNetCore.Mvc;
using Proj.Models;

namespace Proj.Views.Shared.Components;

public class InviteMemberForm : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(Guid projectId)
    {
        return View(new ProjectCommand.InviteMember(projectId, ""));
    }
}