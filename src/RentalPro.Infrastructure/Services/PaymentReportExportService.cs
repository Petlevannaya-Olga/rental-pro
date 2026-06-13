using ClosedXML.Excel;
using RentalPro.Application.Services;
using RentalPro.Contracts.Reports;

namespace RentalPro.Infrastructure.Services;

public sealed class PaymentReportExportService : IExcelExportService<PaymentReportDto>
{
    public byte[] Export(IReadOnlyList<PaymentReportDto> items)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Платежи");

        worksheet.Cell(1, 1).Value = "Тип платежа";
        worksheet.Cell(1, 2).Value = "Количество";
        worksheet.Cell(1, 3).Value = "Сумма";

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = item.PaymentType;
            worksheet.Cell(row, 2).Value = item.PaymentsCount;
            worksheet.Cell(row, 3).Value = item.Amount;
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