using Proj.Models;

namespace Proj.ViewModels;

public static class Projects
{
    public record Settings(
        Project Project,
        IEnumerable<Membership> Memberships);
}