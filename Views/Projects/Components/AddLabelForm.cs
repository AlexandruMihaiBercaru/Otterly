using Microsoft.AspNetCore.Mvc;
using Proj.Models;

namespace Proj.Views.Projects.Components;

public class AddLabelForm : ViewComponent
{
    public record ViewModel(Guid ProjectId, LabelCommand.Create Cmd);

    public IViewComponentResult Invoke(Guid projectId)
    {
        return View(new ViewModel(projectId, new LabelCommand.Create(string.Empty, string.Empty)));
    }
}