using Microsoft.AspNetCore.Mvc;
using Proj.Identity;
using Proj.Models;

namespace Proj.Views.Shared;

public class InvitationActionsForm : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(Guid projectId)
    {
        return View(new ProjectCommand.HandleInvitationRespose(projectId, string.Empty));
    }
}