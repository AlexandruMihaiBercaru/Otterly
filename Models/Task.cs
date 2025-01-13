using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Proj.Models.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;

namespace Proj.Models;

public static class TaskCommand
{
    public record Create(
        [Required(ErrorMessage = "Numele task-ului este obligatoriu.")]
        [StringLength(50)]
        string Name,
        [StringLength(512)]
        [Required(ErrorMessage = "Task-ul treuie sa aiba o descriere.")]
        string Description,
        [Required(ErrorMessage = "Statusul este obligatoriu.")]
        string Status,
        [Required(ErrorMessage = "Adaugati data de inceput.")]
        DateTimeOffset StartDate,
        [Required(ErrorMessage = "Adaugati data de finalizare."),
         SameOrAfter(nameof(StartDate),
             errorMessage: "Data de sfarsit trebuie sa fie dupa cea de inceput")]
        DateTimeOffset EndDate,
        [Required(ErrorMessage = "Adaugati continut.")]
        string MediaUrl,
        Guid? LabelId
    );

    public record Edit(
        Guid Id,
        [Required(ErrorMessage = "Numele task-ului este obligatoriu.")]
        [StringLength(50)]
        string Name,
        [StringLength(512)]
        [Required(ErrorMessage = "Task-ul treuie sa aiba o descriere.")]
        string Description,
        [Required(ErrorMessage = "Statusul este obligatoriu.")]
        string Status,
        [Required(ErrorMessage = "Adaugati data de inceput.")]
        DateTimeOffset StartDate,
        [Required(ErrorMessage = "Adaugati data de finalizare."),
         SameOrAfter(nameof(StartDate),
             errorMessage: "Data de sfarsit trebuie sa fie dupa cea de inceput")]
        DateTimeOffset EndDate,
        [Required(ErrorMessage = "Adaugati continut.")]
        string MediaUrl,
        Guid? LabelId
    );

    public record ChangeStatus(
        [Required] Guid TaskId,
        [Required] Guid ProjectId,
        [Required, OneOf("Not Started", "In Progress", "Done")]
        string NewStatus
    );
}

public class Task
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
    public string MediaUrl { get; set; }
    public Guid ProjectId { get; init; } // Required FK
    public Guid? ParentTaskId { get; init; }
    public Guid? LabelId { get; set; }
    public Label? Label { get; set; } = default;


    public virtual Project? Project { get; set; }

    // navigation property referencing the parent task (if it exists)
    [NotMapped] public virtual Task? ParentTask { get; set; }

    // collection navigation containing the subtasks for each task
    [NotMapped] public virtual ICollection<Task> Subtasks { get; } = new List<Task>();

    public virtual ICollection<Assignment>? Assignments { get; set; }

    public IEnumerable<Comment> Comments { get; set; } = new List<Comment>();

    [NotMapped] public IEnumerable<SelectListItem>? ProjectMembers { get; set; }


    Task()
    {
    }

    private Task(string name, string description, string status,
        DateTimeOffset startDate, DateTimeOffset endDate, string media, Guid projectId,
        Guid? labelId)
    {
        Name = name;
        Description = description;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
        MediaUrl = media;
        ProjectId = projectId;
        LabelId = labelId;
    }

    public static Task From(TaskCommand.Create cmd, Guid projectId)
    {
        var task = new Task(cmd.Name, cmd.Description, cmd.Status,
            cmd.StartDate, cmd.EndDate, cmd.MediaUrl, projectId, cmd.LabelId);
        return task;
    }

    /*
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateTimeOffset.Compare(StartDate, EndDate) == -1)
        {
            yield return ValidationResult.Success;
        }

        yield return new ValidationResult(
            "Data de inceput trebuie sa fie mai mica decat data de finalizare");
    }
    */
}