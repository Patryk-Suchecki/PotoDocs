using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PotoDocs.API.Invoices;

public class InvoiceDocument(InvoiceViewModel model) : IDocument
{
    private readonly InvoiceViewModel _model = model;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer doc)
    {
        doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => InvoiceStyles.Base);

            page.Header().Component(new InvoiceHeaderComponent(_model));

            page.Content().Column(col =>
            {
                col.Item().Component(new InvoiceAddressComponent(_model));

                col.Item().PaddingVertical(10).Element(x => x.Height(1).Background(InvoiceStyles.PrimaryColorDark));

                col.Item().Component(new InvoiceDetailsComponent(_model));

                col.Item().Component(new InvoiceTableComponent(_model));

                col.Item().PaddingTop(15).Component(new InvoiceCurrencySummaryComponent(_model));
            });

            page.Footer().Component(new InvoiceFooterComponent());
        });
    }
}

public class InvoiceHeaderComponent(InvoiceViewModel model) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(model.DocumentTitle).Style(InvoiceStyles.Header);
                column.Item().Text(model.DocumentNumber).FontColor(InvoiceStyles.PrimaryColorDark);
            });

            if (!string.IsNullOrEmpty(model.LogoPath) && File.Exists(model.LogoPath))
            {
                row.ConstantItem(100).Height(50).AlignRight().AlignTop().Image(model.LogoPath);
            }
        });
    }
}

public class InvoiceAddressComponent(InvoiceViewModel model) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c => AddressBlock(c, "Sprzedawca:", model.Seller));
            row.RelativeItem().Element(c => AddressBlock(c, "Nabywca:", model.Buyer));
        });
    }

    private void AddressBlock(IContainer container, string title, PartyInfoViewModel party)
    {
        container.Column(col =>
        {
            col.Item().LabelValueRow(title, party.Name);
            col.Item().LabelValueRow("Adres:", party.Address);
            col.Item().LabelValueRow("NIP:", party.NIP);
        });
    }
}

public class InvoiceDetailsComponent(InvoiceViewModel model) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().LabelValueRow("Miejsce wystawienia:", model.PlaceOfIssue);

                col.Item().PaddingTop(10).LabelValueRow("Data sprzedaży:", model.SaleDate.ToString("dd-MM-yyyy"));
                col.Item().LabelValueRow("Data wystawienia:", model.IssueDate.ToString("dd-MM-yyyy"));

                col.Item().PaddingTop(10).LabelValueRow("Sposób zapłaty:", model.PaymentMethod);
                col.Item().LabelValueRow("Termin zapłaty:", model.PaymentDeadline);

                if (!string.IsNullOrEmpty(model.CorrectionReason))
                {
                    col.Item().LabelValueRow("Przyczyna korekty:", model.CorrectionReason);
                }
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().LabelValueRow("Bank:", model.Bank.BankName);

                col.Item().PaddingTop(10).LabelValueRow("Nr. Konta (PLN):", model.Bank.AccountPLN);

                if (!string.IsNullOrEmpty(model.Bank.AccountEUR))
                {
                    col.Item().LabelValueRow("Nr. Konta (EUR):", model.Bank.AccountEUR);
                }

                col.Item().PaddingTop(10).LabelValueRow("Nr IBAN:", model.Bank.IBAN);
                col.Item().LabelValueRow("SWIFT/BIC:", model.Bank.SWIFT);
            });
        });
    }
}

public class InvoiceTableComponent(InvoiceViewModel model) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingVertical(10).Element(c =>
            {
                c.Border(1).BorderColor(InvoiceStyles.PrimaryColorDark).Padding(5)
                 .Row(row => {
                     row.RelativeItem().Text(text => {
                         text.Span("Uwagi:").Style(InvoiceStyles.Label);
                         text.Span(" " + model.Comments);
                     });
                 });
            });

            if (model.IsCorrection)
            {
                col.Item().PaddingTop(10).Text("Było:").Bold().FontSize(10).FontColor(InvoiceStyles.PrimaryColorDark);
                DrawTable(col.Item(), model.OriginalItems, InvoiceStyles.PrimaryColorLight, InvoiceStyles.PrimaryColorDark, false);

                col.Item().PaddingTop(10).Text("Powinno być:").Bold().FontSize(10).FontColor(InvoiceStyles.PrimaryColorDark);
            }

            var bg = model.IsCorrection ? InvoiceStyles.SecondaryColorLight : InvoiceStyles.PrimaryColorLight;
            var border = model.IsCorrection ? InvoiceStyles.SecondaryColorDark : InvoiceStyles.PrimaryColorDark;

            DrawTable(col.Item(), model.Items, bg, border, !model.IsCorrection);

            if (model.IsCorrection)
            {
                col.Item().PaddingTop(10).Text("Podsumowanie:").Bold().FontSize(10).FontColor(InvoiceStyles.PrimaryColorDark);
                DrawTable(col.Item(), model.DifferenceItems, InvoiceStyles.PrimaryColorLight, InvoiceStyles.PrimaryColorDark, true);
            }
        });
    }

    private void DrawTable(IContainer container, List<InvoiceItemViewModel> items, string colorLight, string colorDark, bool showSummary)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1); // Lp
                columns.RelativeColumn(3); // Nazwa
                columns.RelativeColumn(1); // Ilość
                columns.RelativeColumn(1); // JM
                columns.RelativeColumn(2); // Netto
                columns.RelativeColumn(2); // Wartość
                columns.RelativeColumn(1); // VAT
                columns.RelativeColumn(2); // Kwota VAT
                columns.RelativeColumn(2); // Brutto
            });

            string[] headers = ["Lp.", "Nazwa", "Ilość", "JM", "Cena Netto", "Wartość netto", "Stawka VAT", "Kwota VAT", "Wartość brutto"];
            table.Header(header =>
            {
                foreach (var text in headers)
                {
                    header.Cell().Background(colorLight)
                          .Border(1).BorderColor(colorDark)
                          .Padding(5).MinHeight(25).AlignMiddle().AlignCenter()
                          .Text(text).Style(InvoiceStyles.TableHeader);
                }
            });

            foreach (var item in items)
            {
                IContainer CellStyle(IContainer c) => c.Border(1).BorderColor(colorLight).Padding(5).AlignMiddle();
                IContainer Center(IContainer c) => CellStyle(c).AlignCenter();
                IContainer Left(IContainer c) => CellStyle(c).AlignLeft();
                IContainer Right(IContainer c) => CellStyle(c).AlignRight();

                table.Cell().Element(Center).Text(item.Number.ToString());
                table.Cell().Element(Left).Text(item.Name);
                table.Cell().Element(Center).Text($"{item.Quantity:G29}");
                table.Cell().Element(Center).Text(item.Unit);
                table.Cell().Element(Right).MoneyText(item.NetPrice, model.CurrencySymbol);
                table.Cell().Element(Right).MoneyText(item.NetValue, model.CurrencySymbol);

                var vatText = item.VatRate == 0 ? "NP" : $"{item.VatRate:P0}";
                table.Cell().Element(Center).Text(vatText);
                table.Cell().Element(Right).MoneyText(item.VatAmount, model.CurrencySymbol);
                table.Cell().Element(Right).MoneyText(item.GrossValue, model.CurrencySymbol);
            }

            if (showSummary)
            {
                IContainer SummaryHeader(IContainer c) => c.Background(colorDark).Border(1).BorderColor(colorDark).Padding(5).AlignMiddle();
                IContainer SummaryValue(IContainer c) => c.Background(colorDark).Border(1).BorderColor(colorDark).PaddingRight(5).AlignMiddle().AlignRight();
                IContainer SummaryCell(IContainer c) => c.Background(colorLight).Border(1).BorderColor(colorLight).Padding(5).AlignMiddle();

                table.Cell().ColumnSpan(2).Element(SummaryHeader).Text("RAZEM DO ZAPŁATY:").Bold().FontColor(Colors.White);

                table.Cell().ColumnSpan(2).Element(SummaryValue)
                     .MoneyText(model.Summary.TotalToPay, model.CurrencySymbol)
                     .Bold().FontColor(Colors.White);

                table.Cell().Element(SummaryCell).AlignCenter().Text("RAZEM:").Bold().FontColor(colorDark);
                table.Cell().Element(SummaryCell).AlignRight().MoneyText(model.Summary.TotalNet, model.CurrencySymbol).Bold().FontColor(colorDark);
                table.Cell().Element(SummaryCell);
                table.Cell().Element(SummaryCell).AlignRight().MoneyText(model.Summary.TotalVat, model.CurrencySymbol).Bold().FontColor(colorDark);
                table.Cell().Element(SummaryCell).AlignRight().MoneyText(model.Summary.TotalGross, model.CurrencySymbol).Bold().FontColor(colorDark);
            }
        });
    }
}

public class InvoiceCurrencySummaryComponent(InvoiceViewModel model) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            if (model.RequiresCurrencyConversion)
            {
                col.Item().LabelValueRow("Słownie w euro:", model.Summary.InWordsEuro);

                col.Item().Row(row => {
                    row.RelativeItem().AlignCenter().Text("Kwota VAT została przeliczona na złote polskie po kursie średnim NBP dla EUR, Tabela nr");
                });

                col.Item().Row(row => {
                    row.RelativeItem().AlignCenter().Text($"{model.ExchangeRateInfo.NbpTable} z {model.ExchangeRateInfo.NbpDate}.");
                });

                col.Item().PaddingTop(10).LabelValueRow("Cena euro:", $"{model.ExchangeRateInfo.ExchangeRate:F4} zł");
            }

            var suffix = model.RequiresCurrencyConversion ? " w PLN" : "";

            col.Item().PaddingTop(10).LabelValueRow($"Kwota VAT{suffix}:", $"{model.Summary.VatInPLN:N2} zł");
            col.Item().LabelValueRow($"Słownie{suffix}:", model.Summary.VatInPLNWords);

            col.Item().PaddingTop(10).LabelValueRow($"Cała kwota{suffix}:", $"{model.Summary.AllInPLN:N2} zł");
            col.Item().LabelValueRow($"Słownie{suffix}:", model.Summary.AllInPLNWords);
        });
    }
}

public class InvoiceFooterComponent() : IComponent
{
    public void Compose(IContainer container)
    {
        container.PaddingTop(20).Row(row =>
        {
            var borderColor = InvoiceStyles.PrimaryColorDark;
            var labelColor = InvoiceStyles.LabelColor;

            row.RelativeItem().Height(80).Border(1).BorderColor(borderColor).AlignBottom().PaddingBottom(5).AlignCenter()
                    .Text(t => t.Span("imię, nazwisko i podpis osoby upoważnionej do odebrania dokumentu").FontColor(labelColor).FontSize(7));

            row.ConstantItem(20);

            row.RelativeItem().Height(80).Border(1).BorderColor(borderColor).AlignBottom().PaddingBottom(5).AlignCenter()
                    .Text(t => t.Span("imię, nazwisko i podpis osoby upoważnionej do wystawienia dokumentu").FontColor(labelColor).FontSize(7));
        });
    }
}