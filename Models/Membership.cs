namespace Proj.Models;

public record Membership(
    Guid UserId,
    Guid ProjectId,
    DateTimeOffset? EndedAt = null)
{
    public DateTimeOffset? JoinedAt { get; private init; }
    public Project? Project { get; set; } = null!;
    public ApplicationUser? User { get; set; } = null!;

    public bool IsEnded => EndedAt.HasValue;

    public static Membership Pending(Guid userId, Guid projectId) =>
        new(userId, projectId);

    public static Membership Active(Guid userId, Guid projectId) =>
        new(userId, projectId) { JoinedAt = DateTimeOffset.Now };

    public Membership Join()
    {
        if (JoinedAt is not null) throw new Exception();
        if (IsEnded) throw new Exceptions.EndedMembership();

        return this with { JoinedAt = DateTimeOffset.Now };
    }

    public Membership End()
    {
        if (IsEnded) throw new Exceptions.EndedMembership();

        return this with { EndedAt = DateTimeOffset.Now };
    }
}