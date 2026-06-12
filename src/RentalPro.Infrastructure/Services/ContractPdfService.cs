using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
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
        style.Font.Name = "Times New Roman";
        style.Font.Size = 10;

        var section = document.AddSection();

        section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

        AddTitle(section, contract);
        AddIntro(section, contract);

        AddSection(section, "1. Предмет договора", [
            "1.1. Исполнитель обязуется предоставить Заказчику имущество во временное владение и пользование, а Заказчик обязуется принять имущество и оплатить услуги проката.",
            "1.2. Имущество предоставляется для личного использования и не может быть передано третьим лицам без письменного согласия Исполнителя.",
            "1.3. Факт передачи имущества подтверждается подписанием Акта приема-передачи."
        ]);

        AddSection(section, "2. Срок действия договора", [
            "2.1. Договор вступает в силу с момента его подписания.",
            "2.2. Срок аренды определяется условиями оформленного заказа.",
            "2.3. Продление срока аренды осуществляется по соглашению сторон."
        ]);

        AddSection(section, "3. Стоимость и порядок расчетов", [
            $"3.1. Общая стоимость услуг проката составляет {FormatMoney(contract.TotalRentalPrice)} руб.",
            $"3.2. Размер залога составляет {FormatMoney(contract.TotalDeposit)} руб.",
            $"3.3. Общая сумма к оплате составляет {FormatMoney(contract.TotalAmount)} руб.",
            "3.4. Оплата производится наличным или безналичным способом.",
            "3.5. Залог возвращается после возврата имущества при отсутствии задолженности и повреждений."
        ]);

        AddSection(section, "4. Права и обязанности сторон", [
            "4.1. Исполнитель обязан передать имущество в исправном состоянии и предоставить необходимую информацию по эксплуатации.",
            "4.2. Заказчик обязан бережно использовать имущество, соблюдать правила эксплуатации и своевременно вернуть его.",
            "4.3. Заказчик обязан незамедлительно уведомить Исполнителя о неисправностях или утрате имущества."
        ]);

        AddSection(section, "5. Ответственность сторон", [
            "5.1. Заказчик несет ответственность за сохранность имущества с момента его получения до возврата.",
            "5.2. В случае повреждения или утраты имущества Заказчик возмещает причиненный ущерб.",
            "5.3. Исполнитель не несет ответственности за ущерб, возникший вследствие нарушения Заказчиком правил эксплуатации."
        ]);

        AddSection(section, "6. Заключительные положения", [
            "6.1. Все споры разрешаются путем переговоров.",
            "6.2. При недостижении соглашения спор подлежит рассмотрению в судебном порядке.",
            "6.3. Договор составлен в двух экземплярах, имеющих одинаковую юридическую силу."
        ]);

        AddRequisites(section, contract);

        return document;
    }

    private static void AddTitle(
        Section section,
        RentalContractDto contract)
    {
        var title = section.AddParagraph($"ДОГОВОР ПРОКАТА № {contract.ContractNumber}");
        title.Format.Font.Size = 14;
        title.Format.Font.Bold = true;
        title.Format.Alignment = ParagraphAlignment.Center;
        title.Format.SpaceAfter = Unit.FromCentimeter(0.5);

        var date = section.AddParagraph($"г. Москва                                             {FormatDate(contract.ContractDate)}");
        date.Format.SpaceAfter = Unit.FromCentimeter(0.5);
    }

    private static void AddIntro(
        Section section,
        RentalContractDto contract)
    {
        var paragraph = section.AddParagraph();

        paragraph.AddText(
            $"Общество с ограниченной ответственностью «RentalPRO», ИНН 7708123456, КПП 770801001, " +
            $"ОГРН 1237700123456, в лице менеджера {contract.UserFullName}, именуемое в дальнейшем " +
            $"«Исполнитель», с одной стороны, и {contract.Customer.FullName}, паспорт: {contract.Customer.Passport}, " +
            $"зарегистрированный(ая) по адресу: {contract.Customer.Address}, телефон: {contract.Customer.Phone}, " +
            $"e-mail: {contract.Customer.Email}, именуемый(ая) в дальнейшем «Заказчик», с другой стороны, " +
            $"заключили настоящий договор о нижеследующем.");

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
        titleParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.3);
        titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.2);

        foreach (var item in items)
        {
            var paragraph = section.AddParagraph(item);
            paragraph.Format.Alignment = ParagraphAlignment.Justify;
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.15);
        }
    }

    private static void AddRequisites(
        Section section,
        RentalContractDto contract)
    {
        var title = section.AddParagraph("7. Реквизиты и подписи сторон");
        title.Format.Font.Bold = true;
        title.Format.SpaceBefore = Unit.FromCentimeter(0.5);
        title.Format.SpaceAfter = Unit.FromCentimeter(0.3);

        var table = section.AddTable();
        table.Borders.Width = 0;

        table.AddColumn(Unit.FromCentimeter(8.5));
        table.AddColumn(Unit.FromCentimeter(8.5));

        var row = table.AddRow();
        row.TopPadding = Unit.FromCentimeter(0.1);
        row.BottomPadding = Unit.FromCentimeter(0.2);

        var contractor = row.Cells[0];
        contractor.AddParagraph("Исполнитель").Format.Font.Bold = true;
        contractor.AddParagraph("ООО «RentalPRO»");
        contractor.AddParagraph("ИНН: 7708123456");
        contractor.AddParagraph("КПП: 770801001");
        contractor.AddParagraph("ОГРН: 1237700123456");
        contractor.AddParagraph("Адрес: 125009, г. Москва, ул. Тверская, д. 15, офис 12");
        contractor.AddParagraph("Телефон: +7 (495) 123-45-67");
        contractor.AddParagraph("E-mail: info@rentalpro.ru");
        contractor.AddParagraph($"Менеджер: {contract.UserFullName}");

        var customer = row.Cells[1];
        customer.AddParagraph("Заказчик").Format.Font.Bold = true;
        customer.AddParagraph(contract.Customer.FullName);
        customer.AddParagraph($"Паспорт: {contract.Customer.Passport}");
        customer.AddParagraph($"Адрес: {contract.Customer.Address}");
        customer.AddParagraph($"Телефон: {contract.Customer.Phone}");
        customer.AddParagraph($"E-mail: {contract.Customer.Email}");

        var signatureRow = table.AddRow();
        signatureRow.TopPadding = Unit.FromCentimeter(0.7);

        signatureRow.Cells[0].AddParagraph($"Подпись: _____________ / {contract.UserFullName} /");
        signatureRow.Cells[1].AddParagraph($"Подпись: _____________ / {contract.Customer.FullName} /");
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