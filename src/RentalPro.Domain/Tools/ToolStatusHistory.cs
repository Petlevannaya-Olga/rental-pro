using CSharpFunctionalExtensions;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class ToolStatusHistory
{
    private ToolStatusHistory(
        ToolId toolId,
        ToolStatusId? oldStatusId,
        ToolStatusId newStatusId,
        UserId userId,
        DateTime changeDate,
        Comment? comment)
    {
        Id = ToolStatusHistoryId.NewId();

        ToolId = toolId;
        OldStatusId = oldStatusId;
        NewStatusId = newStatusId;
        UserId = userId;
        ChangeDate = changeDate;
        Comment = comment;

        CreatedAt = DateTime.UtcNow;
    }

    public ToolStatusHistoryId Id { get; private set; }

    public ToolId ToolId { get; private set; }

    public ToolStatusId? OldStatusId { get; private set; }

    public ToolStatusId NewStatusId { get; private set; }

    public UserId UserId { get; private set; }

    public DateTime ChangeDate { get; private set; }

    public Comment? Comment { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Result<ToolStatusHistory, Error> Create(
        Guid toolId,
        Guid? oldStatusId,
        Guid newStatusId,
        Guid userId,
        DateTime changeDate,
        string? comment)
    {
        var toolIdResult = ToolId.Create(toolId);

        if (toolIdResult.IsFailure)
            return toolIdResult.Error;

        var oldStatusIdResult = CreateOldStatusId(oldStatusId);

        if (oldStatusIdResult.IsFailure)
            return oldStatusIdResult.Error;

        var newStatusIdResult = ToolStatusId.Create(newStatusId);

        if (newStatusIdResult.IsFailure)
            return newStatusIdResult.Error;

        var userIdResult = UserId.Create(userId);

        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var commentResult = CreateComment(comment);

        if (commentResult.IsFailure)
            return commentResult.Error;

        return new ToolStatusHistory(
            toolIdResult.Value,
            oldStatusIdResult.Value,
            newStatusIdResult.Value,
            userIdResult.Value,
            changeDate,
            commentResult.Value);
    }

    private static Result<ToolStatusId?, Error> CreateOldStatusId(Guid? oldStatusIdValue)
    {
        if (oldStatusIdValue is null)
            return (ToolStatusId?)null;

        var result = ToolStatusId.Create(oldStatusIdValue.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }

    private static Result<Comment?, Error> CreateComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return (Comment?)null;

        var result = Comment.Create(comment);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }
}