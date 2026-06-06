using ClosedXML.Excel;
using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.ExportToolsQuery;

public sealed class ExportToolsQueryHandler(
    IToolsReadRepository readRepository)
    : IQueryHandler<byte[], ExportToolsQuery>
{
    public async Task<Result<byte[], Errors>> Handle(
        ExportToolsQuery query,
        CancellationToken cancellationToken)
    {
        var categoryIdResult = CreateCategoryId(query.CategoryId);

        if (categoryIdResult.IsFailure)
            return categoryIdResult.Error.ToErrors();

        var manufacturerIdResult = CreateManufacturerId(query.ManufacturerId);

        if (manufacturerIdResult.IsFailure)
            return manufacturerIdResult.Error.ToErrors();

        var statusIdResult = CreateStatusId(query.StatusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error.ToErrors();

        var result = await readRepository.GetForExportAsync(
            query.Search,
            categoryIdResult.Value,
            manufacturerIdResult.Value,
            statusIdResult.Value,
            query.SortBy,
            query.Descending,
            cancellationToken);

        if (result.IsFailure)
            return result.Error;

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Инструменты");

        var headers = new[]
        {
            "Артикул",
            "Название",
            "Описание",
            "Категория",
            "Производитель",
            "Статус",
            "Стоимость/день",
            "Залог",
            "Серийный номер",
            "Инвентарный номер",
            "Текущее состояние",
            "Фото",
            "Дата создания",
            "Дата изменения"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var row = 2;

        foreach (var tool in result.Value)
        {
            worksheet.Cell(row, 1).Value = tool.ArticleNumber;
            worksheet.Cell(row, 2).Value = tool.Name;
            worksheet.Cell(row, 3).Value = tool.Description;
            worksheet.Cell(row, 4).Value = tool.CategoryName;
            worksheet.Cell(row, 5).Value = tool.ManufacturerName;
            worksheet.Cell(row, 6).Value = tool.StatusName;
            worksheet.Cell(row, 7).Value = tool.RentalPricePerDay;
            worksheet.Cell(row, 8).Value = tool.DepositAmount;
            worksheet.Cell(row, 9).Value = tool.SerialNumber;
            worksheet.Cell(row, 10).Value = tool.InventoryNumber;
            worksheet.Cell(row, 11).Value = tool.CurrentCondition;
            worksheet.Cell(row, 12).Value = tool.PhotoPath;
            worksheet.Cell(row, 13).Value = tool.CreatedAt;
            worksheet.Cell(row, 14).Value = tool.UpdatedAt;

            row++;
        }

        var usedRange = worksheet.RangeUsed();

        if (usedRange is not null)
        {
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        var headerRange = worksheet.Range(1, 1, 1, headers.Length);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF2FF");
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Column(7).Style.NumberFormat.Format = "#,##0.00 ₽";
        worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00 ₽";
        worksheet.Column(13).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
        worksheet.Column(14).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";

        worksheet.SheetView.FreezeRows(1);
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    private static Result<ToolCategoryId?, Error> CreateCategoryId(Guid? id)
    {
        if (id is null)
            return (ToolCategoryId?)null;

        var result = ToolCategoryId.Create(id.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }

    private static Result<ManufacturerId?, Error> CreateManufacturerId(Guid? id)
    {
        if (id is null)
            return (ManufacturerId?)null;

        var result = ManufacturerId.Create(id.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }

    private static Result<ToolStatusId?, Error> CreateStatusId(Guid? id)
    {
        if (id is null)
            return (ToolStatusId?)null;

        var result = ToolStatusId.Create(id.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }
}