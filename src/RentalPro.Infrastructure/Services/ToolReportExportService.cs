using ClosedXML.Excel;
using RentalPro.Application.Services;
using RentalPro.Contracts.Reports;

namespace RentalPro.Infrastructure.Services;

public sealed class ToolReportExportService : IExcelExportService<ToolReportDto>
{
    public byte[] Export(IReadOnlyList<ToolReportDto> items)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Инструменты");

        worksheet.Cell(1, 1).Value = "Артикул";
        worksheet.Cell(1, 2).Value = "Инструмент";
        worksheet.Cell(1, 3).Value = "Категория";
        worksheet.Cell(1, 4).Value = "Производитель";
        worksheet.Cell(1, 5).Value = "Статус";
        worksheet.Cell(1, 6).Value = "Цена за день";
        worksheet.Cell(1, 7).Value = "Залог";

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = item.ArticleNumber;
            worksheet.Cell(row, 2).Value = item.ToolName;
            worksheet.Cell(row, 3).Value = item.CategoryName;
            worksheet.Cell(row, 4).Value = item.ManufacturerName;
            worksheet.Cell(row, 5).Value = item.StatusName;
            worksheet.Cell(row, 6).Value = item.RentalPricePerDay;
            worksheet.Cell(row, 7).Value = item.DepositAmount;
        }

        ApplyStyle(worksheet);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    private static void ApplyStyle(IXLWorksheet worksheet)
    {
        var range = worksheet.RangeUsed();

        if (range is not null)
        {
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Style.Font.Bold = true;
        }

        worksheet.Columns().AdjustToContents();
    }
}