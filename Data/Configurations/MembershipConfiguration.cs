using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proj.Models;

namespace Proj.Data.Configurations;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> membership)
    {
        membership.ToTable("membership");

        membership.HasKey(m => new { m.UserId, m.ProjectId });

        membership
            .Property(m => m.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        membership
            .Property(m => m.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        membership
            .Property(m => m.EndedAt)
            .HasColumnName("ended_at");

        membership
            .Property(m => m.JoinedAt)
            .HasColumnName("joined_at")
            .IsRequired();
    }
}