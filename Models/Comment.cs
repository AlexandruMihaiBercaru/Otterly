using System.ComponentModel.DataAnnotations;

namespace Proj.Models;

public static class CommentCommand
{
    public record New([Required] string Content);
}

public class Comment
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public Task? Task { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string Content { get; set; }

    public DateTimeOffset CreatedAt { get; private init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset DeletedAt { get; private set; }

    public Comment(Guid taskId, Guid userId, string content)
    {
        TaskId = taskId;
        UserId = userId;
        Content = content;
    }
}