using RentalPro.Contracts.Users;

namespace RentalPro.Application.Services;

public interface IUsersExportService
{
    byte[] ExportToExcel(IReadOnlyList<UserDto> users);
}