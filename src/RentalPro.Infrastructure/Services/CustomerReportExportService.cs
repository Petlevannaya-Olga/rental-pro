using ClosedXML.Excel;
using RentalPro.Application.Services;
using RentalPro.Contracts.Reports;

namespace RentalPro.Infrastructure.Services;

public sealed class CustomerReportExportService : IExcelExportService<CustomerReportDto>
{
    public byte[] Export(IReadOnlyList<CustomerReportDto> items)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Клиенты");

        worksheet.Cell(1, 1).Value = "Клиент";
        worksheet.Cell(1, 2).Value = "Количество заказов";
        worksheet.Cell(1, 3).Value = "Сумма аренд";
        worksheet.Cell(1, 4).Value = "Последняя аренда";

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = item.CustomerFullName;
            worksheet.Cell(row, 2).Value = item.OrdersCount;
            worksheet.Cell(row, 3).Value = item.RentAmount;
            worksheet.Cell(row, 4).Value = item.LastOrderDate?.ToString("dd.MM.yyyy") ?? "-";
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