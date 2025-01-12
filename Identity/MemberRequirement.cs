using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Proj.Data;

namespace Proj.Identity;

public class MemberRequirement : IAuthorizationRequirement
{
    public const string Policy = "MemberPolicy";
}

public class MemberAuthorizationHandler : AuthorizationHandler<MemberRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentUser _user;

    public MemberAuthorizationHandler(ApplicationDbContext context, CurrentUser user)
    {
        _context = context;
        _user = user;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MemberRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;
        if (httpContext == null) return;

        if (_user.IsAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (!httpContext.Request.RouteValues.TryGetValue("projectId", out var rawProjectId)) return;
        if (!Guid.TryParse(rawProjectId?.ToString(), out var projectId)) return;

        var membership = await _context.Memberships.FirstOrDefaultAsync(m =>
            m.ProjectId == projectId &&
            m.UserId == _user.Id &&
            !m.EndedAt.HasValue &&
            m.JoinedAt.HasValue);
        if (membership is not null)
        {
            context.Succeed(requirement);
        }
    }
}