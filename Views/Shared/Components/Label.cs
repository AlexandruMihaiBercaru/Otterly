using Microsoft.AspNetCore.Mvc;

namespace Proj.Views.Shared.Components;

public class Label : ViewComponent
{
    public record ViewModel(string Color, string Name);
    
    public IViewComponentResult Invoke(string color, string name)
    {
        return View(new ViewModel(color, name));
    }
}