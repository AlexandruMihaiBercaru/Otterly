using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proj.Models;

namespace Proj.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> user)
    {
        user.ToTable("application_users");


        user
            .HasMany(u => u.Memberships)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId);

        user
            .Property(u => u.FirstName)
            .HasColumnName("first_name");

        user
            .Property(u => u.LastName)
            .HasColumnName("last_name");
    }
}