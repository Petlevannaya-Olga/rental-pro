using ClosedXML.Excel;
using RentalPro.Application.Services;
using RentalPro.Contracts.Payments;

namespace RentalPro.Infrastructure.Services;

public sealed class PaymentsExportService : IExcelExportService<PaymentDto>
{
    public byte[] Export(
        IReadOnlyList<PaymentDto> payments)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Оплаты");

        worksheet.Cell(1, 1).Value = "Номер заказа";
        worksheet.Cell(1, 2).Value = "Дата заказа";
        worksheet.Cell(1, 3).Value = "Клиент";
        worksheet.Cell(1, 4).Value = "Тип оплаты";
        worksheet.Cell(1, 5).Value = "Способ оплаты";
        worksheet.Cell(1, 6).Value = "Дата оплаты";
        worksheet.Cell(1, 7).Value = "Сумма";
        worksheet.Cell(1, 8).Value = "Комментарий";

        for (var i = 0; i < payments.Count; i++)
        {
            var row = i + 2;
            var payment = payments[i];

            worksheet.Cell(row, 1).Value = payment.OrderNumber;
            worksheet.Cell(row, 2).Value = payment.OrderDate;
            worksheet.Cell(row, 3).Value = payment.CustomerFullName;
            worksheet.Cell(row, 4).Value = payment.PaymentTypeName;
            worksheet.Cell(row, 5).Value = payment.PaymentMethodName;
            worksheet.Cell(row, 6).Value = payment.PaymentDate;
            worksheet.Cell(row, 7).Value = payment.Amount;
            worksheet.Cell(row, 8).Value = payment.Comment ?? string.Empty;
        }

        var totalRow = payments.Count + 3;

        worksheet.Cell(totalRow, 6).Value = "Общая сумма платежей:";
        worksheet.Cell(totalRow, 7).Value = payments.Sum(x => x.Amount);

        worksheet.Cell(totalRow + 1, 6).Value = "Чистые поступления:";
        worksheet.Cell(totalRow + 1, 7).Value = payments.Sum(x =>
            x.PaymentTypeName == "Возврат залога"
                ? -x.Amount
                : x.Amount);

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }
}