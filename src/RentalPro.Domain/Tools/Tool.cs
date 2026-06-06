using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class Tool : AuditableEntity<ToolId>
{
    private Tool()
        : base(ToolId.NewId())
    {
    }

    private Tool(
        ArticleNumber articleNumber,
        ToolName name,
        Description? description,
        ToolCategoryId categoryId,
        ManufacturerId manufacturerId,
        ToolStatusId statusId,
        Money rentalPricePerDay,
        Money depositAmount,
        SerialNumber serialNumber,
        InventoryNumber inventoryNumber,
        ReturnCondition? currentCondition,
        PhotoPath? photoPath)
        : base(ToolId.NewId())
    {
        ArticleNumber = articleNumber;
        Name = name;
        Description = description;
        CategoryId = categoryId;
        ManufacturerId = manufacturerId;
        StatusId = statusId;
        RentalPricePerDay = rentalPricePerDay;
        DepositAmount = depositAmount;
        SerialNumber = serialNumber;
        InventoryNumber = inventoryNumber;
        CurrentCondition = currentCondition;
        PhotoPath = photoPath;
    }

    public ArticleNumber ArticleNumber { get; private set; }

    public ToolName Name { get; private set; }

    public Description? Description { get; private set; }

    public ToolCategoryId CategoryId { get; private set; }

    public ManufacturerId ManufacturerId { get; private set; }

    public ToolStatusId StatusId { get; private set; }

    public Money RentalPricePerDay { get; private set; }

    public Money DepositAmount { get; private set; }

    public SerialNumber SerialNumber { get; private set; }

    public InventoryNumber InventoryNumber { get; private set; }

    public ReturnCondition? CurrentCondition { get; private set; }

    public PhotoPath? PhotoPath { get; private set; }

    public ToolCategory? Category { get; private set; }

    public Manufacturer? Manufacturer { get; private set; }

    public ToolStatus? Status { get; private set; }

    public static Result<Tool, Error> Create(
        string articleNumber,
        string name,
        string? description,
        Guid categoryId,
        Guid manufacturerId,
        Guid statusId,
        decimal rentalPricePerDay,
        decimal depositAmount,
        string serialNumber,
        string inventoryNumber,
        string? currentCondition,
        string? photoPath)
    {
        var articleNumberResult = ArticleNumber.Create(articleNumber);

        if (articleNumberResult.IsFailure)
            return articleNumberResult.Error;

        var nameResult = ToolName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var descriptionResult = CreateDescription(description);

        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        var categoryIdResult = ToolCategoryId.Create(categoryId);

        if (categoryIdResult.IsFailure)
            return categoryIdResult.Error;

        var manufacturerIdResult = ManufacturerId.Create(manufacturerId);

        if (manufacturerIdResult.IsFailure)
            return manufacturerIdResult.Error;

        var statusIdResult = ToolStatusId.Create(statusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error;

        var rentalPriceResult = Money.Create(rentalPricePerDay);

        if (rentalPriceResult.IsFailure)
            return rentalPriceResult.Error;

        var depositAmountResult = Money.Create(depositAmount);

        if (depositAmountResult.IsFailure)
            return depositAmountResult.Error;

        var serialNumberResult = SerialNumber.Create(serialNumber);

        if (serialNumberResult.IsFailure)
            return serialNumberResult.Error;

        var inventoryNumberResult = InventoryNumber.Create(inventoryNumber);

        if (inventoryNumberResult.IsFailure)
            return inventoryNumberResult.Error;

        var currentConditionResult = CreateCurrentCondition(currentCondition);

        if (currentConditionResult.IsFailure)
            return currentConditionResult.Error;

        var photoPathResult = CreatePhotoPath(photoPath);

        if (photoPathResult.IsFailure)
            return photoPathResult.Error;

        return new Tool(
            articleNumberResult.Value,
            nameResult.Value,
            descriptionResult.Value,
            categoryIdResult.Value,
            manufacturerIdResult.Value,
            statusIdResult.Value,
            rentalPriceResult.Value,
            depositAmountResult.Value,
            serialNumberResult.Value,
            inventoryNumberResult.Value,
            currentConditionResult.Value,
            photoPathResult.Value);
    }

    public UnitResult<Error> UpdateMainInfo(
        string articleNumber,
        string name,
        string? description,
        Guid categoryId,
        Guid manufacturerId,
        Guid statusId,
        decimal rentalPricePerDay,
        decimal depositAmount,
        string serialNumber,
        string inventoryNumber,
        string? currentCondition,
        string? photoPath)
    {
        var articleNumberResult = ArticleNumber.Create(articleNumber);

        if (articleNumberResult.IsFailure)
            return articleNumberResult.Error;

        var nameResult = ToolName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var descriptionResult = CreateDescription(description);

        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        var categoryIdResult = ToolCategoryId.Create(categoryId);

        if (categoryIdResult.IsFailure)
            return categoryIdResult.Error;

        var manufacturerIdResult = ManufacturerId.Create(manufacturerId);

        if (manufacturerIdResult.IsFailure)
            return manufacturerIdResult.Error;

        var statusIdResult = ToolStatusId.Create(statusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error;

        var rentalPriceResult = Money.Create(rentalPricePerDay);

        if (rentalPriceResult.IsFailure)
            return rentalPriceResult.Error;

        var depositAmountResult = Money.Create(depositAmount);

        if (depositAmountResult.IsFailure)
            return depositAmountResult.Error;

        var serialNumberResult = SerialNumber.Create(serialNumber);

        if (serialNumberResult.IsFailure)
            return serialNumberResult.Error;

        var inventoryNumberResult = InventoryNumber.Create(inventoryNumber);

        if (inventoryNumberResult.IsFailure)
            return inventoryNumberResult.Error;

        var currentConditionResult = CreateCurrentCondition(currentCondition);

        if (currentConditionResult.IsFailure)
            return currentConditionResult.Error;

        var photoPathResult = CreatePhotoPath(photoPath);

        if (photoPathResult.IsFailure)
            return photoPathResult.Error;

        ArticleNumber = articleNumberResult.Value;
        Name = nameResult.Value;
        Description = descriptionResult.Value;
        CategoryId = categoryIdResult.Value;
        ManufacturerId = manufacturerIdResult.Value;
        StatusId = statusIdResult.Value;
        RentalPricePerDay = rentalPriceResult.Value;
        DepositAmount = depositAmountResult.Value;
        SerialNumber = serialNumberResult.Value;
        InventoryNumber = inventoryNumberResult.Value;
        CurrentCondition = currentConditionResult.Value;
        PhotoPath = photoPathResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangeStatus(Guid statusId)
    {
        var statusIdResult = ToolStatusId.Create(statusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error;

        StatusId = statusIdResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SetPhotoPath(string? photoPath)
    {
        var photoPathResult = CreatePhotoPath(photoPath);

        if (photoPathResult.IsFailure)
            return photoPathResult.Error;

        PhotoPath = photoPathResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(Tool));
    }

    private static Result<Description?, Error> CreateDescription(
        string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return (Description?)null;

        var descriptionResult = Description.Create(description);

        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        return descriptionResult.Value;
    }

    private static Result<ReturnCondition?, Error> CreateCurrentCondition(
        string? currentCondition)
    {
        if (string.IsNullOrWhiteSpace(currentCondition))
            return (ReturnCondition?)null;

        var currentConditionResult = ReturnCondition.Create(currentCondition);

        if (currentConditionResult.IsFailure)
            return currentConditionResult.Error;

        return currentConditionResult.Value;
    }

    private static Result<PhotoPath?, Error> CreatePhotoPath(
        string? photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
            return (PhotoPath?)null;

        var photoPathResult = PhotoPath.Create(photoPath);

        if (photoPathResult.IsFailure)
            return photoPathResult.Error;

        return photoPathResult.Value;
    }
}