using Microsoft.AspNetCore.Mvc;

namespace Proj.Views.Shared.Components;

public class ThemeSwitcher : ViewComponent
{
    public record Option(string Key, string Label, string Icon);

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var options = new Option[]
        {
            new("light", "Light", "sun-fill"),
            new("dark", "Dark", "moon-stars-fill"),
        };

        return View(options);
    }
}