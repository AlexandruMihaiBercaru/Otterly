using System.ComponentModel.DataAnnotations;

namespace Proj.Models;

public static class ProjectCommand
{
    public record Create(
        [StringLength(128, MinimumLength = 1, ErrorMessage = "Numele Proiectului este necesar")]
        string Name
    );
}

public class Project
{
    public Guid Id { get; private init; } = Guid.NewGuid();

    public Guid OrganizerId { get; private init; }
    public string Name { get; private set; }

    public IEnumerable<Membership> Memberships { get; private set; } = new List<Membership>();

    public DateTimeOffset CreatedAt { get; private init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; private set; }

    private Project(string name, Guid organizerId)
    {
        OrganizerId = organizerId;
        Name = name;
    }

    public static (Project, Membership) From(ProjectCommand.Create cmd, Guid organizerId)
    {
        var project = new Project(cmd.Name, organizerId);
        var membership = project.AddMember(organizerId);

        return (project, membership);
    }

    public Membership AddMember(Guid userId)
    {
        if (Memberships.Any(m => m.UserId == userId))
            throw new Exceptions.ExistingMember(userId);

        return Membership.Active(userId, Id);
    }
}