using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proj.Models;

namespace Proj.Data.Configurations;

public sealed class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> label)
    {
        label.ToTable("labels");

        label.HasKey(l => l.Id);

        label
            .Property(l => l.Id)
            .HasColumnName("id")
            .IsRequired();

        // FK
        label
            .Property(l => l.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        label
            .Property(l => l.Name)
            .HasColumnName("name")
            .IsRequired();

        label
            .Property(l => l.Color)
            .HasColumnName("color")
            .IsRequired();

        // required one-to-many: a project has multiple labels
        // each label must be associated with a project

        label
            .HasOne<Project>()
            .WithMany()
            .HasForeignKey(l => l.ProjectId)
            .IsRequired();

    }
}

