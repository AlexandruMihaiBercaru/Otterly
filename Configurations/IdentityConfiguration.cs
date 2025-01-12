using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Proj.Data;
using Proj.Identity;
using Proj.Models;

namespace Proj.Configurations;

public static partial class Extensions
{
    public static WebApplicationBuilder ConfigureIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddPolicy<OrganizerAuthorizationHandler>(OrganizerRequirement.Policy,
            policy => policy.Requirements.Add(new OrganizerRequirement()));
        builder.Services.AddPolicy<MemberAuthorizationHandler>(MemberRequirement.Policy,
            policy => policy.Requirements.Add(new MemberRequirement()));

        builder.Services
            .AddDefaultIdentity<ApplicationUser>(opt =>
            {
                opt.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddCurrentUser();

        return builder;
    }

    private static IServiceCollection AddPolicy<THandler>(this IServiceCollection services,
        string policyName,
        Action<AuthorizationPolicyBuilder> policy) where THandler : class, IAuthorizationHandler
    {
        services.AddAuthorization(opt => opt.AddPolicy(policyName, policy));
        services.AddScoped<IAuthorizationHandler, THandler>();

        return services;
    }
}