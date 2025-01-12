using Microsoft.AspNetCore.Mvc;
using Proj.Models;
using Task = System.Threading.Tasks.Task;

namespace Proj.Views.Shared.Components;

public class ChangeStatusForm : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(Guid projectId, Guid taskId)
    {
        return View(new TaskCommand.ChangeStatus(taskId, projectId, String.Empty));
    }
}