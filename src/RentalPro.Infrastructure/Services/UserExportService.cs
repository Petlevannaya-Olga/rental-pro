using ClosedXML.Excel;
using RentalPro.Application.Services;
using RentalPro.Contracts.Users;

namespace RentalPro.Infrastructure.Services;

public sealed class UsersExportService : IUsersExportService
{
    public byte[] ExportToExcel(IReadOnlyList<UserDto> users)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Пользователи");

        worksheet.Cell(1, 1).Value = "ФИО";
        worksheet.Cell(1, 2).Value = "Логин";
        worksheet.Cell(1, 3).Value = "Email";
        worksheet.Cell(1, 4).Value = "Телефон";
        worksheet.Cell(1, 5).Value = "Роль";
        worksheet.Cell(1, 6).Value = "Статус";
        worksheet.Cell(1, 7).Value = "Дата создания";
        worksheet.Cell(1, 8).Value = "Дата изменения";

        var header = worksheet.Range(1, 1, 1, 8);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF1FF");
        header.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        for (var i = 0; i < users.Count; i++)
        {
            var row = i + 2;
            var user = users[i];

            worksheet.Cell(row, 1).Value = user.FullName;
            worksheet.Cell(row, 2).Value = user.Login;
            worksheet.Cell(row, 3).Value = user.Email;
            worksheet.Cell(row, 4).Value = user.PhoneNumber;
            worksheet.Cell(row, 5).Value = user.RoleName;
            worksheet.Cell(row, 6).Value = user.IsActive ? "Активен" : "Заблокирован";
            worksheet.Cell(row, 7).Value = user.CreatedAt;
            worksheet.Cell(row, 8).Value = user.UpdatedAt;
        }

        worksheet.Column(7).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
        worksheet.Column(8).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return stream.ToArray();
    }
}