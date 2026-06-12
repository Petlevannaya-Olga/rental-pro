using CSharpFunctionalExtensions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Services;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Services;

public sealed class ContractDocumentService(
    IHostEnvironment environment,
    ILogger<ContractDocumentService> logger)
    : IContractDocumentService
{
    private const string TemplateRelativePath = "Templates/RentalContract.docx";

    public async Task<Result<byte[], Errors>> GenerateRentalContractAsync(
        RentalContractDto contract,
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
                    "RentalContract.docx"));

            if (!File.Exists(templatePath))
            {
                return CommonErrors.NotFound(
                        "contract.template.not.found",
                        "Шаблон договора не найден")
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
                ReplacePlaceholders(document, contract);
            }

            return stream.ToArray();
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("generate.contract.document.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to generate rental contract document for order {OrderId}",
                contract.OrderId);

            return CommonErrors.Db(
                    "generate.contract.document.exception",
                    "Failed to generate rental contract document")
                .ToErrors();
        }
    }

    private static void ReplacePlaceholders(
        WordprocessingDocument document,
        RentalContractDto contract)
    {
        var replacements = new Dictionary<string, string>
        {
            ["{{ContractNumber}}"] = contract.ContractNumber,
            ["{{ContractDate}}"] = FormatDate(contract.ContractDate),

            ["{{UserFullName}}"] = contract.UserFullName,

            ["{{CustomerFullName}}"] = contract.Customer.FullName,
            ["{{CustomerPassport}}"] = contract.Customer.Passport,
            ["{{CustomerAddress}}"] = contract.Customer.Address,
            ["{{CustomerPhone}}"] = contract.Customer.Phone,
            ["{{CustomerEmail}}"] = contract.Customer.Email,

            ["{{TotalRentalPrice}}"] = FormatMoney(contract.TotalRentalPrice),
            ["{{TotalDeposit}}"] = FormatMoney(contract.TotalDeposit),
            ["{{TotalAmount}}"] = FormatMoney(contract.TotalAmount)
        };

        var body = document.MainDocumentPart?.Document.Body;

        if (body is null)
            return;

        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            ReplaceInParagraph(paragraph, replacements);
        }

        document.MainDocumentPart!.Document.Save();
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
                replacement.Value);

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