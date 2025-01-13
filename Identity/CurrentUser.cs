using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Proj.Models;

namespace Proj.Identity;

public class CurrentUser
{
    public ApplicationUser? Account { get; set; }
    public ClaimsPrincipal Principal { get; set; } = default!;

    public Guid Id => Account!.Id;

    public bool IsAdmin => Principal.IsInRole("Admin");

    public bool Exists => Account is not null;
}

public static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddScoped<CurrentUser>();
        services.AddScoped<IClaimsTransformation, ClaimsTransformation>();
        return services;
    }

    private sealed class ClaimsTransformation(
        CurrentUser currentUser,
        UserManager<ApplicationUser> userManager
    ) : IClaimsTransformation
    {
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            currentUser.Principal = principal;

            if (principal.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } id)
            {
                currentUser.Account = await userManager.FindByIdAsync(id);
            }

            return principal;
        }
    }
}