using Proj.Models;

namespace Proj.ViewModels;

public static class Projects
{
    public record Settings(
        IEnumerable<Membership> Memberships,
        Guid ProjectId);
}