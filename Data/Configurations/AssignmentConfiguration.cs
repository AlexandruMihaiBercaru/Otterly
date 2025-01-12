using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proj.Models;


namespace Proj.Data.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> assignment)
    {
        assignment.ToTable("assignments");

        assignment.HasKey(a => new { a.UserId, a.TaskId });

        assignment
            .Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        assignment
            .Property(a => a.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        assignment
            .Property(a => a.AssignedAt)
            .HasColumnName("assigned_at");


        //a task can be assigned to multiple members
        assignment
            .HasOne(a => a.Task)
            .WithMany(a => a.Assignments)
            .HasForeignKey(a => a.TaskId);

        //a member can be assigned to multiple tasks
        assignment
            .HasOne(a => a.User)
            .WithMany(a => a.Assignments)
            .HasForeignKey(a => a.UserId);
    }
}
