using Microsoft.AspNetCore.Mvc;

namespace Proj.Controllers
{
    public class LabelsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
