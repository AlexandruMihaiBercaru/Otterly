namespace Proj.Models;

public record Membership(Guid UserId, Guid ProjectId, DateTimeOffset? EndedAt = null)
{
    public DateTimeOffset JoinedAt { get; private init; } = DateTimeOffset.Now;
    public Project Project { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public bool IsEnded => EndedAt.HasValue;

    public static Membership Active(Guid userId, Guid projectId) => new(userId, projectId, null);

    public Membership End()
    {
        if (IsEnded) throw new Exceptions.EndedMembership();

        return this with { EndedAt = DateTimeOffset.Now };
    }
}