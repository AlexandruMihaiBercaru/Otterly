using Microsoft.AspNetCore.Identity;

namespace Proj.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    public Membership Accept(Membership membership)
    {
        if (!Memberships.Contains(membership))
            throw new Exceptions.UnknownMembership(membership.UserId, membership.ProjectId);

        if (membership.JoinedAt is not null)
            throw new Exceptions.ActiveMembership();

        return membership.Join();
    }

    public Membership EndMembership(Membership membership)
    {
        if (!Memberships.Contains(membership))
            throw new Exceptions.UnknownMembership(membership.UserId, membership.ProjectId);

        if (membership.EndedAt is not null)
            throw new Exceptions.EndedMembership();

        return membership.End();
    }
}