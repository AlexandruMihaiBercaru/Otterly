namespace Proj.Models;

public class Assignment
{
    public Guid UserId;

    public Guid TaskId;

    public DateTimeOffset? AssignedAt { get; private init; } = DateTimeOffset.Now;
    public virtual Task? Task { get; set; }
    public virtual ApplicationUser? User { get; set; }

    
}
