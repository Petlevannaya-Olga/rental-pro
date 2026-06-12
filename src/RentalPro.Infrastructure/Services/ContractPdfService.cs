using CSharpFunctionalExtensions;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Services;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Services;

public sealed class ContractPdfService(
    ILogger<ContractPdfService> logger)
    : IContractPdfService
{
    public Task<Result<byte[], Errors>> GenerateRentalContractPdfAsync(
        RentalContractDto contract,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = CreateDocument(contract);

            var renderer = new PdfDocumentRenderer
            {
                Document = document
            };

            renderer.RenderDocument();

            using var stream = new MemoryStream();

            renderer.PdfDocument.Save(stream, false);

            return Task.FromResult<Result<byte[], Errors>>(
                stream.ToArray());
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult<Result<byte[], Errors>>(
                CommonErrors
                    .OperationCancelled("generate.contract.pdf.was.cancelled")
                    .ToErrors());
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to generate contract pdf for order {OrderId}",
                contract.OrderId);

            return Task.FromResult<Result<byte[], Errors>>(
                CommonErrors.Db(
                        "generate.contract.pdf.exception",
                        "Failed to generate contract pdf")
                    .ToErrors());
        }
    }

    private static Document CreateDocument(RentalContractDto contract)
    {
        var document = new Document();

        document.Info.Title = $"Договор проката № {contract.ContractNumber}";

        var style = document.Styles["Normal"];
        style.Font.Name = "Arial";
        style.Font.Size = 10;

        var section = document.AddSection();

        section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

        AddTitle(section, contract);
        AddIntro(section, contract);

        AddSection(section, "1. Предмет договора",
        [
            "1.1. Исполнитель обязуется предоставить Заказчику имущество во временное владение и пользование, а Заказчик обязуется принять имущество и оплатить услуги проката.",
            "1.2. Имущество предоставляется для личного использования и не может быть передано третьим лицам без письменного согласия Исполнителя.",
            "1.3. Факт передачи имущества подтверждается подписанием Акта приема-передачи, являющегося неотъемлемой частью настоящего договора."
        ]);

        AddSection(section, "2. Срок действия договора",
        [
            "2.1. Договор вступает в силу с момента его подписания.",
            "2.2. Срок аренды определяется условиями оформленного заказа.",
            "2.3. Продление срока аренды осуществляется по соглашению сторон."
        ]);

        AddSection(section, "3. Стоимость и порядок расчетов",
        [
            $"3.1. Общая стоимость услуг проката составляет {FormatMoney(contract.TotalRentalPrice)} руб.",
            $"3.2. Размер залога составляет {FormatMoney(contract.TotalDeposit)} руб.",
            $"3.3. Общая сумма к оплате составляет {FormatMoney(contract.TotalAmount)} руб.",
            "3.4. Оплата производится наличным или безналичным способом.",
            "3.5. Залог возвращается после возврата имущества при отсутствии задолженности и повреждений."
        ]);

        AddSection(section, "4. Права и обязанности сторон",
        [
            "4.1. Исполнитель обязан передать имущество в исправном состоянии и предоставить необходимую информацию по эксплуатации.",
            "4.2. Заказчик обязан бережно использовать имущество, соблюдать правила эксплуатации и своевременно вернуть его.",
            "4.3. Заказчик обязан незамедлительно уведомить Исполнителя о неисправностях или утрате имущества."
        ]);

        AddSection(section, "5. Ответственность сторон",
        [
            "5.1. Заказчик несет ответственность за сохранность имущества с момента его получения до возврата.",
            "5.2. В случае повреждения или утраты имущества Заказчик возмещает причиненный ущерб.",
            "5.3. Исполнитель не несет ответственности за ущерб, возникший вследствие нарушения Заказчиком правил эксплуатации."
        ]);

        AddSection(section, "6. Форс-мажор",
        [
            "6.1. Стороны освобождаются от ответственности за неисполнение обязательств вследствие обстоятельств непреодолимой силы.",
            "6.2. Сторона, для которой возникли такие обстоятельства, обязана уведомить другую сторону в разумный срок."
        ]);

        AddSection(section, "7. Порядок разрешения споров",
        [
            "7.1. Все споры и разногласия разрешаются путем переговоров.",
            "7.2. При недостижении соглашения спор подлежит рассмотрению в судебном порядке в соответствии с законодательством Российской Федерации."
        ]);

        AddSection(section, "8. Заключительные положения",
        [
            "8.1. Договор составлен в двух экземплярах, имеющих одинаковую юридическую силу.",
            "8.2. Подписание договора подтверждает согласие Заказчика с его условиями.",
            "8.3. Любые изменения и дополнения оформляются в письменной форме."
        ]);

        AddRequisites(section, contract);

        return document;
    }

    private static void AddTitle(
        Section section,
        RentalContractDto contract)
    {
        var title = section.AddParagraph(
            $"ДОГОВОР ПРОКАТА № {contract.ContractNumber}");

        title.Format.Font.Size = 14;
        title.Format.Font.Bold = true;
        title.Format.Alignment = ParagraphAlignment.Center;
        title.Format.SpaceAfter = Unit.FromCentimeter(0.4);

        var date = section.AddParagraph();

        date.AddText("г. Москва");
        date.AddTab();
        date.AddText(FormatDate(contract.ContractDate));

        date.Format.TabStops.AddTabStop(
            Unit.FromCentimeter(15),
            TabAlignment.Right);

        date.Format.SpaceAfter = Unit.FromCentimeter(0.5);
    }

    private static void AddIntro(
        Section section,
        RentalContractDto contract)
    {
        var paragraph = section.AddParagraph();

        paragraph.AddText(
            "Общество с ограниченной ответственностью «RentalPRO», " +
            "ИНН 7708123456, КПП 770801001, ОГРН 1237700123456, " +
            $"в лице менеджера {contract.UserFullName}, действующего от имени организации, " +
            "именуемое в дальнейшем ");

        paragraph.AddFormattedText("«Исполнитель»", TextFormat.Bold);

        paragraph.AddText(
            ", с одной стороны, и " +
            $"{contract.Customer.FullName}, паспорт: {contract.Customer.Passport}, " +
            $"зарегистрированный(ая) по адресу: {contract.Customer.Address}, " +
            $"телефон: {contract.Customer.Phone}, адрес электронной почты: {contract.Customer.Email}, " +
            "именуемый(ая) в дальнейшем ");

        paragraph.AddFormattedText("«Заказчик»", TextFormat.Bold);

        paragraph.AddText(
            ", с другой стороны, совместно именуемые ");

        paragraph.AddFormattedText("«Стороны»", TextFormat.Bold);

        paragraph.AddText(
            ", заключили настоящий договор о нижеследующем.");

        paragraph.Format.Alignment = ParagraphAlignment.Justify;
        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);
    }

    private static void AddSection(
        Section section,
        string title,
        IReadOnlyList<string> items)
    {
        var titleParagraph = section.AddParagraph(title);

        titleParagraph.Format.Font.Bold = true;
        titleParagraph.Format.Font.Size = 11;
        titleParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.25);
        titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.15);

        foreach (var item in items)
        {
            var paragraph = section.AddParagraph(item);

            paragraph.Format.Alignment = ParagraphAlignment.Justify;
            paragraph.Format.FirstLineIndent = Unit.FromCentimeter(0.5);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);
        }
    }

    private static void AddRequisites(
        Section section,
        RentalContractDto contract)
    {
        var title = section.AddParagraph("9. Реквизиты и подписи сторон");

        title.Format.Font.Bold = true;
        title.Format.Font.Size = 11;
        title.Format.SpaceBefore = Unit.FromCentimeter(0.35);
        title.Format.SpaceAfter = Unit.FromCentimeter(0.25);

        var table = section.AddTable();

        table.Borders.Width = 0;
        table.Rows.LeftIndent = 0;

        table.AddColumn(Unit.FromCentimeter(8.4));
        table.AddColumn(Unit.FromCentimeter(8.4));

        var row = table.AddRow();

        row.TopPadding = Unit.FromCentimeter(0.05);
        row.BottomPadding = Unit.FromCentimeter(0.2);

        FillContractorCell(row.Cells[0], contract);
        FillCustomerCell(row.Cells[1], contract);

        var signatureRow = table.AddRow();

        signatureRow.TopPadding = Unit.FromCentimeter(0.7);

        signatureRow.Cells[0].AddParagraph("Подпись: ___________________");
        signatureRow.Cells[1].AddParagraph("Подпись: ___________________");
    }

    private static void FillContractorCell(
        Cell cell,
        RentalContractDto contract)
    {
        AddBoldParagraph(cell, "Исполнитель");

        cell.AddParagraph("ООО «RentalPRO»");
        cell.AddParagraph("ИНН: 7708123456");
        cell.AddParagraph("КПП: 770801001");
        cell.AddParagraph("ОГРН: 1237700123456");
        cell.AddParagraph("Адрес: 125009, г. Москва, ул. Тверская, д. 15, офис 12");
        cell.AddParagraph("Телефон: +7 (495) 123-45-67");
        cell.AddParagraph("E-mail: info@rentalpro.ru");
        cell.AddParagraph($"Менеджер: {contract.UserFullName}");
    }

    private static void FillCustomerCell(
        Cell cell,
        RentalContractDto contract)
    {
        AddBoldParagraph(cell, "Заказчик");

        cell.AddParagraph(contract.Customer.FullName);
        cell.AddParagraph($"Паспорт: {contract.Customer.Passport}");
        cell.AddParagraph($"Адрес: {contract.Customer.Address}");
        cell.AddParagraph($"Телефон: {contract.Customer.Phone}");
        cell.AddParagraph($"E-mail: {contract.Customer.Email}");
    }

    private static void AddBoldParagraph(
        Cell cell,
        string text)
    {
        var paragraph = cell.AddParagraph();

        paragraph.AddFormattedText(text, TextFormat.Bold);
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