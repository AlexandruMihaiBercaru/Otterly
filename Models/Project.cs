using System.ComponentModel.DataAnnotations;
using Proj.Models.Validation;

namespace Proj.Models;

public static class ProjectCommand
{
    public record Create(
        [Required(ErrorMessage = "Numele proiectului este necesar")]
        [StringLength(128, MinimumLength = 1,
            ErrorMessage = "Numele nu poate avea mai mult de 128 de caractere")]
        string Name,
        [Required(ErrorMessage = "Descrierea este obligatorie")]
        [StringLength(256, MinimumLength = 1,
            ErrorMessage = "Descrierea nu poate avea mai mult de 256 de caractere")]
        string Summary
    );

    public record Edit(
        Guid Id,
        [Required(ErrorMessage = "Numele proiectului este necesar")]
        [StringLength(128, MinimumLength = 1,
            ErrorMessage = "Numele nu poate avea mai mult de 128 de caractere")]
        string Name,
        [Required(ErrorMessage = "Descrierea este obligatorie")]
        [StringLength(256, MinimumLength = 1,
            ErrorMessage = "Descrierea nu poate avea mai mult de 256 de caractere")]
        string Summary
    );

    public record InviteMember(
        Guid ProjectId,
        [Required, EmailAddress] string Email
    );

    public record HandleInvitationRespose(
        [Required] Guid ProjectId,
        [Required, OneOf("accept", "reject")] string Intent
    );
}

public class Project
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public Guid OrganizerId { get; private init; }

    public string Name { get; private set; }

    public string Summary { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.Now;
    public DateTimeOffset? DeletedAt { get; private set; }


    public ICollection<Task>? Tasks { get; set; } = new List<Task>();
    public IEnumerable<Membership> Memberships { get; private set; } = new List<Membership>();

    private Project(string name, string summary, Guid organizerId)
    {
        OrganizerId = organizerId;
        Name = name;
        Summary = summary;
    }

    // creates a new project as well as the membership of the organizer of the project
    public static (Project, Membership) From(ProjectCommand.Create cmd, Guid organizerId)
    {
        var project = new Project(cmd.Name, cmd.Summary, organizerId);
        var membership = project.AddMember(organizerId);

        return (project, membership);
    }

    public Membership AddMember(Guid userId)
    {
        if (Memberships.Any(m => m.UserId == userId))
            throw new Exceptions.ExistingMember(userId);

        return Membership.Active(userId, Id);
    }

    public Membership InviteMember(Guid userId)
    {
        if (Memberships.Any(m => m.UserId == userId))
            throw new Exceptions.ExistingMember(userId);

        return Membership.Pending(userId, Id);
    }

    public void Edit(ProjectCommand.Edit cmd)
    {
        Name = cmd.Name;
        Summary = cmd.Summary;
    }
}