using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proj.Models;

namespace Proj.Data.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> project)
    {
        project.ToTable("projects");

        project.HasKey((p) => p.Id);
         
        project
            .Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        project.Property(p => p.OrganizerId)
            .HasColumnName("organizer_id")
            .IsRequired();

        project
            .Property(p => p.Name)
            .IsRequired()
            .HasColumnName("name");

        project
            .Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        project
            .Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnUpdate();

        project
            .Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");


        // required one-to-many: a project has multiple memberships

        project
            .HasMany(p => p.Memberships)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .IsRequired();

        // required one-to-many: an user can create multiple projects
        // each project must have an organizer (must be createad by an user)

        project
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.OrganizerId)
            .IsRequired();
    }
}