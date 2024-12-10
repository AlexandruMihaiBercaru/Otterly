using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proj.Models;

namespace Proj.Data.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> proj)
    {
        proj.ToTable("projects");

        proj.HasKey((p) => p.Id);
        proj
            .Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        proj
            .HasMany(p => p.Memberships)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .IsRequired();

        proj
            .Property(p => p.Name)
            .IsRequired()
            .HasColumnName("name");

        proj
            .Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        proj
            .Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnUpdate();

        proj
            .Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");
    }
}