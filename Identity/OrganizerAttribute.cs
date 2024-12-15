using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Proj.Data;
using Proj.Models;
using Task = System.Threading.Tasks.Task;


namespace Proj.Identity;

public class OrganizerRequirement : IAuthorizationRequirement
{
    public const string Policy = "OrganizerPolicy";
}

public class OrganizerAuthorizationHandler : AuthorizationHandler<OrganizerRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrganizerAuthorizationHandler(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizerRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;
        if (httpContext == null) return;

        if (httpContext.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var user = await _userManager.GetUserAsync(httpContext.User);
        if (user is null) return;

        if (!httpContext.Request.RouteValues.TryGetValue("projectId", out var rawProjectId)) return;
        if (!Guid.TryParse(rawProjectId?.ToString(), out var projectId)) return;

        var project = await _context.Projects.FindAsync(projectId);
        if (project?.OrganizerId == user.Id)
        {
            context.Succeed(requirement);
        }
    }
}