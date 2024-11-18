using Microsoft.AspNetCore.Identity;
using Proj.Data;

namespace Proj.Configurations;

public static partial class Extensions
{
    public static WebApplicationBuilder ConfigureIdentity(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDefaultIdentity<IdentityUser>(opt => { opt.SignIn.RequireConfirmedAccount = true; })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return builder;
    }
}