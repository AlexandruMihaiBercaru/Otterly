using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Proj.Models;

namespace Proj.Data.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<Models.Task>
{
    public void Configure(EntityTypeBuilder<Models.Task> task)
    {
        task.ToTable("tasks");

        task.HasKey(t => t.Id);

        task.Property(t => t.Id).HasColumnName("id").IsRequired();

        task.Property(t => t.Name).HasColumnName("name").IsRequired();

        task.Property(t => t.Description).HasColumnName("description").IsRequired();

        task.Property(t => t.Status).HasColumnName("status").IsRequired();

        task.Property(t => t.StartDate).HasColumnName("start_date").IsRequired();

        task.Property(t => t.EndDate).HasColumnName("end_date").IsRequired();

        task.Property(t => t.UpdatedAt).HasColumnName("updated_at").ValueGeneratedOnUpdate();
        
        task.Property(t => t.DeletedAt).HasColumnName("deleted_at");

        task.Property(t => t.MediaUrl).HasColumnName("media_url").IsRequired();

        task.Property(t => t.ProjectId).HasColumnName("project_id").IsRequired();

        task.Property(t => t.ParentTaskId).HasColumnName("parent_task_id");

        task.Property(t => t.LabelId).HasColumnName("label_id");

        

        // one-to-many relation: a projects has multiple tasks

        task
            .HasOne<Project>(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .IsRequired();

        // one-to-many relation: a label can be attached to multiple tasks
        // each task can only have one label
        task
            .HasOne<Label>()
            .WithMany()
            .HasForeignKey(t => t.LabelId);

        // self-referencing one-to-many
        // a task can have mutiple subtasks

        task
            .HasOne(t => t.ParentTask)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(t => t.ParentTaskId)
            .IsRequired(false);
    }
}
