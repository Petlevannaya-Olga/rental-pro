using ClosedXML.Excel;
using RentalPro.Application.Services;
using RentalPro.Contracts.Orders;

namespace RentalPro.Infrastructure.Services;

public sealed class OrdersExportService : IExcelExportService<OrderDto>
{
    public byte[] Export(
        IReadOnlyList<OrderDto> orders)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Заказы");

        worksheet.Cell(1, 1).Value = "Номер заказа";
        worksheet.Cell(1, 2).Value = "Клиент";
        worksheet.Cell(1, 3).Value = "Сотрудник";
        worksheet.Cell(1, 4).Value = "Статус";
        worksheet.Cell(1, 5).Value = "Дата заказа";
        worksheet.Cell(1, 6).Value = "Стоимость";
        worksheet.Cell(1, 7).Value = "Залог";
        worksheet.Cell(1, 8).Value = "Комментарий";
        worksheet.Cell(1, 9).Value = "Создан";
        worksheet.Cell(1, 10).Value = "Обновлен";

        for (var i = 0; i < orders.Count; i++)
        {
            var row = i + 2;
            var order = orders[i];

            worksheet.Cell(row, 1).Value = order.Id.ToString();

            worksheet.Cell(row, 2).Value = order.CustomerFullName;

            worksheet.Cell(row, 3).Value = order.UserFullName;

            worksheet.Cell(row, 4).Value = order.StatusName;

            worksheet.Cell(row, 5).Value = order.OrderDate;

            worksheet.Cell(row, 6).Value = order.TotalCost;

            worksheet.Cell(row, 7).Value = order.DepositTotal;

            worksheet.Cell(row, 8).Value = order.Comment ?? string.Empty;

            worksheet.Cell(row, 9).Value = order.CreatedAt;

            worksheet.Cell(row, 10).Value = order.UpdatedAt;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }
}