using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proj.Models;

namespace Proj.Data.Configurations;

public sealed class CommentsConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> comment)
    {
        comment.ToTable("comments");

        comment.HasKey(c => c.Id);

        comment.Property(c => c.Id).HasColumnName("id").IsRequired();

        comment.Property(c => c.TaskId).HasColumnName("task_id").IsRequired();

        comment
            .HasOne(c => c.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .IsRequired();

        comment.Property(c => c.UserId).HasColumnName("user_id").IsRequired();

        comment
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .IsRequired();

        comment.Property(c => c.Content).HasColumnName("content").IsRequired();

        comment
            .Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        comment
            .Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnUpdate();

        comment
            .Property(c => c.DeletedAt)
            .HasColumnName("deleted_at");
    }
}