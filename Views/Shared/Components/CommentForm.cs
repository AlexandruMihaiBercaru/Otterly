using Microsoft.AspNetCore.Mvc;
using Proj.Models;
using Task = System.Threading.Tasks.Task;

namespace Proj.Views.Shared.Components;

public class CommentForm : ViewComponent
{
    public record Model(Guid ProjectId, Guid TaskId, CommentCommand.New Cmd);

    public async Task<IViewComponentResult> InvokeAsync(Guid projectId, Guid taskId)
    {
        return View(new Model(projectId, taskId, new CommentCommand.New(string.Empty)));
    }
}