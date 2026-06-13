using ClosedXML.Excel;
using RentalPro.Application.Services;
using RentalPro.Contracts.Reports;

namespace RentalPro.Infrastructure.Services;

public sealed class OverdueReturnReportExportService : IExcelExportService<OverdueReturnReportDto>
{
    public byte[] Export(IReadOnlyList<OverdueReturnReportDto> items)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Просроченные возвраты");

        worksheet.Cell(1, 1).Value = "№ заказа";
        worksheet.Cell(1, 2).Value = "Клиент";
        worksheet.Cell(1, 3).Value = "Инструменты";
        worksheet.Cell(1, 4).Value = "Плановый возврат";
        worksheet.Cell(1, 5).Value = "Дней просрочки";

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = item.OrderNumber;
            worksheet.Cell(row, 2).Value = item.CustomerFullName;
            worksheet.Cell(row, 3).Value = item.ToolsNames;
            worksheet.Cell(row, 4).Value = item.PlannedReturnDate.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 5).Value = item.OverdueDays;
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