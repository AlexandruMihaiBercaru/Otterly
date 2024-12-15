namespace Proj.Models;

public static class Exceptions
{
    public sealed class UnknownMembership(Guid userId, Guid projectId)
        : Exception(
            $"User with with id {userId} doesn't have an active membership for the project with id {projectId}");

    public sealed class ActiveMembership() : Exception("Cannot join an already active membership");
    
    public sealed class EndedMembership() : Exception("Cannot end an already ended membership");

    public sealed class ExistingMember(Guid userId)
        : Exception($"User with id {userId} has an active membership");
}