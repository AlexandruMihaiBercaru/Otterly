using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj.Models;

public static class TaskCommand
{
    public record Create(
        [Required (ErrorMessage ="Numele task-ului este obligatoriu.")]
        [StringLength(50)]
        string Name,

        [StringLength(512)]
        [Required(ErrorMessage = "Task-ul treuie sa aiba o descriere.")]
        string Description,

        [Required(ErrorMessage = "Statusul este obligatoriu.")]
        string Status,

        [Required(ErrorMessage = "Adaugati data de inceput.")]
        DateTimeOffset StartDate,

        [Required(ErrorMessage = "Adaugati data de finalizare.")]
        DateTimeOffset EndDate,

        [Required(ErrorMessage = "Adaugati continut.")]
        string MediaUrl
    );
}
public class Task : IValidatableObject
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; private set; }
    public string MediaUrl { get; set; }
    public Guid ProjectId { get; init; } // Required FK
    public Guid? ParentTaskId { get; init; }
    public Guid? LabelId { get; init; }

    // navigation property referencing the parent task (if it exists)
    [NotMapped]
    public Task? ParentTask { get; set; }

    // collection navigation containing the subtasks for each task
    [NotMapped]
    public ICollection<Task> Subtasks { get; } = new List<Task>();

    Task() { }
    private Task(string _name, string _description, string _status, 
        DateTimeOffset _startDate, DateTimeOffset _endDate, string _media, Guid _projectId)
    {
        Name = _name;
        Description = _description;
        Status = _status;
        StartDate = _startDate;
        EndDate = _endDate;
        MediaUrl = _media;
        ProjectId = _projectId;
    }

    public static Task From(TaskCommand.Create cmd, Guid projectId)
    {
        var task = new Task(cmd.Name, cmd.Description, cmd.Status,
            cmd.StartDate, cmd.EndDate, cmd.MediaUrl, projectId);
        return task;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(DateTimeOffset.Compare(StartDate, EndDate) == -1)
        {
            yield return ValidationResult.Success;
        }
        yield return new ValidationResult("Data de inceput trebuie sa fie mai mica decat data de finalizare");
    }
}
