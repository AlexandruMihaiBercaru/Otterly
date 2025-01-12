using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj.Identity;

namespace Proj.Controllers;

[Authorize, Route("inbox")]
public class InboxController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}