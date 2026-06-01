using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Tools;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.MaintenanceRecords;

public sealed class MaintenanceRecord : AuditableEntity<MaintenanceRecordId>
{
    private MaintenanceRecord(
        ToolId toolId,
        UserId userId,
        MaintenanceStatusId statusId,
        DateOnly startDate,
        DateOnly? endDate,
        Description description,
        Money? cost,
        Comment? result)
        : base(MaintenanceRecordId.NewId())
    {
        ToolId = toolId;
        UserId = userId;
        StatusId = statusId;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
        Cost = cost;
        Result = result;
    }

    public ToolId ToolId { get; private set; }

    public UserId UserId { get; private set; }

    public MaintenanceStatusId StatusId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public Description Description { get; private set; }

    public Money? Cost { get; private set; }

    public Comment? Result { get; private set; }

    public static Result<MaintenanceRecord, Error> Create(
        Guid toolId,
        Guid userId,
        Guid statusId,
        DateOnly startDate,
        DateOnly? endDate,
        string description,
        decimal? cost,
        string? result)
    {
        var toolIdResult = ToolId.Create(toolId);
        if (toolIdResult.IsFailure)
            return toolIdResult.Error;

        var userIdResult = UserId.Create(userId);
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var statusIdResult = MaintenanceStatusId.Create(statusId);
        if (statusIdResult.IsFailure)
            return statusIdResult.Error;

        var datesValidationResult = ValidateDates(startDate, endDate);
        if (datesValidationResult.IsFailure)
            return datesValidationResult.Error;

        var descriptionResult = Description.Create(description);
        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        var costResult = CreateCost(cost);
        if (costResult.IsFailure)
            return costResult.Error;

        var resultResult = CreateResult(result);
        if (resultResult.IsFailure)
            return resultResult.Error;

        return new MaintenanceRecord(
            toolIdResult.Value,
            userIdResult.Value,
            statusIdResult.Value,
            startDate,
            endDate,
            descriptionResult.Value,
            costResult.Value,
            resultResult.Value);
    }

    public UnitResult<Error> Update(
        Guid statusId,
        DateOnly startDate,
        DateOnly? endDate,
        string description,
        decimal? cost,
        string? result)
    {
        var statusIdResult = MaintenanceStatusId.Create(statusId);
        if (statusIdResult.IsFailure)
            return statusIdResult.Error;

        var datesValidationResult = ValidateDates(startDate, endDate);
        if (datesValidationResult.IsFailure)
            return datesValidationResult.Error;

        var descriptionResult = Description.Create(description);
        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        var costResult = CreateCost(cost);
        if (costResult.IsFailure)
            return costResult.Error;

        var resultResult = CreateResult(result);
        if (resultResult.IsFailure)
            return resultResult.Error;

        StatusId = statusIdResult.Value;
        StartDate = startDate;
        EndDate = endDate;
        Description = descriptionResult.Value;
        Cost = costResult.Value;
        Result = resultResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangeStatus(Guid statusId)
    {
        var statusIdResult = MaintenanceStatusId.Create(statusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error;

        StatusId = statusIdResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Complete(
        DateOnly endDate,
        decimal? cost,
        string? result)
    {
        if (endDate < StartDate)
        {
            return CommonErrors.Validation(
                nameof(endDate),
                "End date cannot be earlier than start date");
        }

        var costResult = CreateCost(cost);

        if (costResult.IsFailure)
            return costResult.Error;

        var resultResult = CreateResult(result);

        if (resultResult.IsFailure)
            return resultResult.Error;

        EndDate = endDate;
        Cost = costResult.Value;
        Result = resultResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(MaintenanceRecord));
    }

    private static UnitResult<Error> ValidateDates(
        DateOnly startDate,
        DateOnly? endDate)
    {
        if (endDate.HasValue && endDate.Value < startDate)
        {
            return CommonErrors.Validation(
                nameof(endDate),
                "End date cannot be earlier than start date");
        }

        return UnitResult.Success<Error>();
    }

    private static Result<Money?, Error> CreateCost(decimal? cost)
    {
        if (cost is null)
            return (Money?)null;

        var costResult = Money.Create(cost.Value);

        if (costResult.IsFailure)
            return costResult.Error;

        return costResult.Value;
    }

    private static Result<Comment?, Error> CreateResult(string? result)
    {
        if (string.IsNullOrWhiteSpace(result))
            return (Comment?)null;

        var resultResult = Comment.Create(result);

        if (resultResult.IsFailure)
            return resultResult.Error;

        return resultResult.Value;
    }
}