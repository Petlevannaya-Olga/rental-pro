namespace RentalPro.Application.Services;

public interface IExcelExportService<in T>
{
    byte[] Export(IReadOnlyList<T> items);
}