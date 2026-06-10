using ClosedXML.Excel;
using RentalPro.Application.Services;
using RentalPro.Contracts.Customers;

namespace RentalPro.Infrastructure.Services;

public sealed class CustomersExportService : IExcelExportService<CustomerDto>
{
    public byte[] Export(
        IReadOnlyList<CustomerDto> customers)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Клиенты");

        worksheet.Cell(1, 1).Value = "ФИО";
        worksheet.Cell(1, 2).Value = "Телефон";
        worksheet.Cell(1, 3).Value = "Email";
        worksheet.Cell(1, 4).Value = "Паспорт";
        worksheet.Cell(1, 5).Value = "Адрес";
        worksheet.Cell(1, 6).Value = "Заказов";
        worksheet.Cell(1, 7).Value = "Задолженность";
        worksheet.Cell(1, 8).Value = "Создан";
        worksheet.Cell(1, 9).Value = "Изменён";

        for (var i = 0; i < customers.Count; i++)
        {
            var customer = customers[i];

            var row = i + 2;

            worksheet.Cell(row, 1).Value = customer.FullName;
            worksheet.Cell(row, 2).Value = customer.PhoneNumber;
            worksheet.Cell(row, 3).Value = customer.Email;
            worksheet.Cell(row, 4).Value = customer.Passport;
            worksheet.Cell(row, 5).Value = customer.Address;
            worksheet.Cell(row, 6).Value = customer.OrdersCount;
            worksheet.Cell(row, 7).Value = customer.HasDebt ? "Да" : "Нет";
            worksheet.Cell(row, 8).Value = customer.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            worksheet.Cell(row, 9).Value = customer.UpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "-";
        }

        var range = worksheet.RangeUsed();

        if (range is not null)
        {
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Row(1).Style.Font.Bold = true;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }
}