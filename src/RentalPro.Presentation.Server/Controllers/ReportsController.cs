using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Contracts.Reports;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(
    IReportsReadRepository repository,
    IExcelExportService<RevenueReportDto> revenueExportService,
    IExcelExportService<PopularToolReportDto> popularToolExportService,
    IExcelExportService<OverdueReturnReportDto> overdueReturnExportService,
    IExcelExportService<CustomerReportDto> customerExportService,
    IExcelExportService<PaymentReportDto> paymentExportService,
    IExcelExportService<ToolReportDto> toolExportService)
    : ControllerBase
{
    [HttpGet("revenue")]
    public async Task<ActionResult<IReadOnlyList<RevenueReportDto>>> GetRevenue(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetRevenueAsync(dateFrom, dateTo, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("revenue/export")]
    public async Task<IActionResult> ExportRevenue(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetRevenueAsync(dateFrom, dateTo, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Export(
            revenueExportService.Export(result.Value),
            "revenue-report");
    }

    [HttpGet("popular-tools")]
    public async Task<ActionResult<IReadOnlyList<PopularToolReportDto>>> GetPopularTools(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPopularToolsAsync(dateFrom, dateTo, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("popular-tools/export")]
    public async Task<IActionResult> ExportPopularTools(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPopularToolsAsync(dateFrom, dateTo, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Export(
            popularToolExportService.Export(result.Value),
            "popular-tools-report");
    }

    [HttpGet("overdue-returns")]
    public async Task<ActionResult<IReadOnlyList<OverdueReturnReportDto>>> GetOverdueReturns(
        CancellationToken cancellationToken)
    {
        var result = await repository.GetOverdueReturnsAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("overdue-returns/export")]
    public async Task<IActionResult> ExportOverdueReturns(
        CancellationToken cancellationToken)
    {
        var result = await repository.GetOverdueReturnsAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Export(
            overdueReturnExportService.Export(result.Value),
            "overdue-returns-report");
    }

    [HttpGet("customers")]
    public async Task<ActionResult<IReadOnlyList<CustomerReportDto>>> GetCustomers(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetCustomersAsync(dateFrom, dateTo, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("customers/export")]
    public async Task<IActionResult> ExportCustomers(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetCustomersAsync(dateFrom, dateTo, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Export(
            customerExportService.Export(result.Value),
            "customers-report");
    }

    [HttpGet("payments")]
    public async Task<ActionResult<IReadOnlyList<PaymentReportDto>>> GetPayments(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPaymentsAsync(dateFrom, dateTo, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("payments/export")]
    public async Task<IActionResult> ExportPayments(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPaymentsAsync(dateFrom, dateTo, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Export(
            paymentExportService.Export(result.Value),
            "payments-report");
    }

    [HttpGet("tools")]
    public async Task<ActionResult<IReadOnlyList<ToolReportDto>>> GetTools(
        CancellationToken cancellationToken)
    {
        var result = await repository.GetToolsAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("tools/export")]
    public async Task<IActionResult> ExportTools(
        CancellationToken cancellationToken)
    {
        var result = await repository.GetToolsAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Export(
            toolExportService.Export(result.Value),
            "tools-report");
    }

    private FileContentResult Export(
        byte[] bytes,
        string fileName)
    {
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{fileName}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
    }
}