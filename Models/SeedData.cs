using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proj.Data;

namespace Proj.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());
        if (context.Roles.Any())
        {
            return;
        }

        context.Roles.AddRange(
            new IdentityRole<Guid>
            {
                Id = new Guid("c7dd3690-a024-49e0-ae70-96541eccfe60"),
                Name = "Admin",
                NormalizedName = "Admin".ToUpper(),
            },
            new IdentityRole<Guid>
            {
                Id = new Guid("c7dd3690-a024-49e0-ae70-96541eccfe61"),
                Name = "User",
                NormalizedName = "User".ToUpper()
            }
        );

        var hasher = new PasswordHasher<ApplicationUser>();

        context.Users.AddRange(
            new ApplicationUser
            {
                Id = new Guid("8e445865-a24d-4543-a6c6-9443d048cdb0"),
                UserName = "admin@test.com",
                EmailConfirmed = true,
                NormalizedEmail = "ADMIN@TEST.COM",
                Email = "admin@test.com",
                NormalizedUserName = "ADMIN@TEST.COM",
                PasswordHash = hasher.HashPassword(null, "Admin1!"),
                SecurityStamp = Guid.NewGuid().ToString()
            },
            new ApplicationUser
            {
                Id = new Guid("8e445865-a24d-4543-a6c6-9443d048cdb1"),
                UserName = "user@test.com",
                EmailConfirmed = true,
                NormalizedEmail = "USER@TEST.COM",
                Email = "user@test.com",
                NormalizedUserName = "USER@TEST.COM",
                PasswordHash = hasher.HashPassword(null, "User1!"),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        );


        context.UserRoles.AddRange(
            new IdentityUserRole<Guid>
            {
                RoleId = new Guid("c7dd3690-a024-49e0-ae70-96541eccfe60"),
                UserId = new Guid("8e445865-a24d-4543-a6c6-9443d048cdb0")
            },
            new IdentityUserRole<Guid>
            {
                RoleId = new Guid("c7dd3690-a024-49e0-ae70-96541eccfe61"),
                UserId = new Guid("8e445865-a24d-4543-a6c6-9443d048cdb1")
            }
        );
        context.SaveChanges();
    }
}