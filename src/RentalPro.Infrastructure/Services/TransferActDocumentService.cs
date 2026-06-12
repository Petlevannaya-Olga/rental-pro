using CSharpFunctionalExtensions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Services;

public sealed class TransferActDocumentService(
    IHostEnvironment environment,
    ILogger<TransferActDocumentService> logger)
    : ITransferActDocumentService
{
    public async Task<Result<byte[], Errors>> GenerateTransferActAsync(
        TransferActDto act,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var templatePath = Path.GetFullPath(
                Path.Combine(
                    environment.ContentRootPath,
                    "..",
                    "RentalPro.Infrastructure",
                    "Templates",
                    "TransferAct.docx"));

            if (!File.Exists(templatePath))
            {
                return CommonErrors.NotFound(
                        "transfer.act.template.not.found",
                        "Шаблон акта выдачи не найден")
                    .ToErrors();
            }

            var bytes = await File.ReadAllBytesAsync(
                templatePath,
                cancellationToken);

            using var stream = new MemoryStream();

            await stream.WriteAsync(
                bytes,
                cancellationToken);

            stream.Position = 0;

            using (var document = WordprocessingDocument.Open(
                       stream,
                       true))
            {
                ReplacePlaceholders(document, act);
            }

            return stream.ToArray();
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("generate.transfer.act.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to generate transfer act for order {OrderId} and date {ActDate}",
                act.OrderId,
                act.ActDate);

            return CommonErrors.Db(
                    "generate.transfer.act.exception",
                    "Failed to generate transfer act")
                .ToErrors();
        }
    }

    private static void ReplacePlaceholders(
        WordprocessingDocument document,
        TransferActDto act)
    {
        var replacements = new Dictionary<string, string>
        {
            ["{{ActNumber}}"] = act.ActNumber,
            ["{{ActDate}}"] = FormatDate(act.ActDate),

            ["{{ContractNumber}}"] = act.ContractNumber,
            ["{{ContractDate}}"] = FormatDate(act.ContractDate),

            ["{{UserFullName}}"] = act.UserFullName,

            ["{{CustomerFullName}}"] = act.Customer.FullName,
            ["{{CustomerPassport}}"] = act.Customer.Passport,
            ["{{CustomerAddress}}"] = act.Customer.Address,
            ["{{CustomerPhone}}"] = act.Customer.Phone,
            ["{{CustomerEmail}}"] = act.Customer.Email,

            ["{{TotalRentalPrice}}"] = FormatMoney(act.TotalRentalPrice),
            ["{{TotalDeposit}}"] = FormatMoney(act.TotalDeposit)
        };

        var body = document.MainDocumentPart?.Document.Body;

        if (body is null)
            return;

        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            ReplaceInParagraph(paragraph, replacements);
        }

        ReplaceItemsTable(body, act.Items);

        document.MainDocumentPart!.Document.Save();
    }

    private static void ReplaceItemsTable(
        Body body,
        IReadOnlyList<TransferActItemDto> items)
    {
        var table = body
            .Descendants<Table>()
            .FirstOrDefault(x => x.InnerText.Contains("{{RowNumber}}"));

        if (table is null)
            return;

        var templateRow = table
            .Descendants<TableRow>()
            .FirstOrDefault(x => x.InnerText.Contains("{{RowNumber}}"));

        if (templateRow is null)
            return;

        foreach (var item in items.Select((value, index) => new { value, index }))
        {
            var row = (TableRow)templateRow.CloneNode(true);

            var replacements = new Dictionary<string, string>
            {
                ["{{RowNumber}}"] = (item.index + 1).ToString(),
                ["{{ToolName}}"] = item.value.ToolName,
                ["{{InventoryNumber}}"] = item.value.InventoryNumber,
                ["{{SerialNumber}}"] = item.value.SerialNumber,
                ["{{PlannedReturnDate}}"] = FormatDate(item.value.PlannedReturnDate),
                ["{{RentalDays}}"] = item.value.RentalDays.ToString(),
                ["{{RentalPrice}}"] = FormatMoney(item.value.RentalPrice),
                ["{{DepositAmount}}"] = FormatMoney(item.value.DepositAmount),
                ["{{Condition}}"] = item.value.Condition,
            };

            foreach (var paragraph in row.Descendants<Paragraph>())
            {
                ReplaceInParagraph(paragraph, replacements);
            }

            table.InsertBefore(row, templateRow);
        }

        templateRow.Remove();
    }

    private static void ReplaceInParagraph(
        Paragraph paragraph,
        Dictionary<string, string> replacements)
    {
        var texts = paragraph
            .Descendants<Text>()
            .ToList();

        if (texts.Count == 0)
            return;

        var paragraphText = string.Concat(texts.Select(x => x.Text));

        var changed = false;

        foreach (var replacement in replacements)
        {
            if (!paragraphText.Contains(replacement.Key))
                continue;

            paragraphText = paragraphText.Replace(
                replacement.Key,
                replacement.Value ?? string.Empty);

            changed = true;
        }

        if (!changed)
            return;

        texts[0].Text = paragraphText;

        for (var i = 1; i < texts.Count; i++)
        {
            texts[i].Text = string.Empty;
        }
    }

    private static string FormatDate(DateOnly date)
    {
        return date.ToString("dd.MM.yyyy");
    }

    private static string FormatMoney(decimal value)
    {
        return value.ToString("N2");
    }
}