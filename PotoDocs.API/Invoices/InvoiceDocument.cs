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
        string[] headers = ["Lp.", "Nazwa", "Ilość", "JM", "Cena Netto", "Wartość netto", "Stawka VAT", "Kwota VAT", "Wartość brutto"];
        doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontFamily("Tahoma").FontSize(8));

            var colorLight = _model.PrimaryColorLight;
            var colorDark = _model.PrimaryColorDark;
            var secondarycolorLight = _model.SecondaryColorLight;
            var secondarycolorDark = _model.SecondaryColorDark;
            var labelColor = _model.LabelColor;

            page.Header().Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(_model.DocumentTitle).SemiBold().FontSize(16).FontColor(colorDark);
                    column.Item().Text(_model.DocumentNumber).FontColor(colorDark);
                });

                row.ConstantItem(100).Height(50).AlignRight().AlignTop().Image("wwwroot/images/logo.png");
            });

            page.Content().Column(column =>
            {
                // Sprzedawca i Nabywca
                column.Item().Row(row =>
                {
                    row.ConstantItem(130).AlignRight().PaddingRight(5).Text("Sprzedawca:").SemiBold().FontColor(labelColor);
                    row.RelativeItem().Text(_model.Seller.Name);

                    row.ConstantItem(130).AlignRight().PaddingRight(5).Text("Nabywca:").SemiBold().FontColor(labelColor);
                    row.RelativeItem().Text(_model.Buyer.Name);
                });

                column.Item().Row(row =>
                {
                    row.ConstantItem(130).AlignRight().PaddingRight(5).Text("Adres:").SemiBold().FontColor(labelColor);
                    row.RelativeItem().Text(_model.Seller.Address);

                    row.ConstantItem(130).AlignRight().PaddingRight(5).Text("Adres:").SemiBold().FontColor(labelColor);
                    row.RelativeItem().Text(_model.Buyer.Address);
                });

                column.Item().Row(row =>
                {
                    row.ConstantItem(130).AlignRight().PaddingRight(5).Text("NIP:").SemiBold().FontColor(labelColor);
                    row.RelativeItem().Text(_model.Seller.NIP);

                    row.ConstantItem(130).AlignRight().PaddingRight(5).Text("NIP:").SemiBold().FontColor(labelColor);
                    row.RelativeItem().Text(_model.Buyer.NIP);
                });

                column.Item().PaddingVertical(10).Element(x => x.Height(1).Background(colorDark));

                // Daty, miejsce, płatności
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Miejsce wystawienia:").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.PlaceOfIssue);
                        });

                        col.Item().PaddingTop(10).Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Data sprzedaży:").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.SaleDate.ToString("dd-MM-yyyy"));
                        });

                        col.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Data wystawienia:").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.IssueDate.ToString("dd-MM-yyyy"));
                        });

                        col.Item().PaddingTop(10).Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Sposób zapłaty:").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.PaymentMethod);
                        });

                        col.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Termin zapłaty:").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.PaymentDeadline);
                        });

                        col.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Przyczyna korekty: ").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.CorrectionReason);
                        });
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Bank:").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.Bank.BankName);
                        });

                        col.Item().PaddingTop(10).Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Nr. Konta (PLN):").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.Bank.AccountPLN);
                        });

                        col.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Nr. Konta (EUR):").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.Bank.AccountEUR);
                        });

                        col.Item().PaddingTop(10).Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("Nr IBAN:").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.Bank.IBAN);
                        });

                        col.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().PaddingRight(5).Text("SWIFT/BIC:").SemiBold().FontColor(labelColor);
                            r.RelativeItem().Text(_model.Bank.SWIFT);
                        });
                    });
                });

                // Uwagi
                column.Item().PaddingVertical(10).Element(container =>
                {
                    container
                        .Border(1)
                        .BorderColor(colorDark)
                        .Padding(5)
                        .Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Span("Uwagi:").SemiBold().FontColor(labelColor);
                                text.Span(" " + _model.Comments);
                            });
                        });
                });
                if (_model.IsCorrection)
                {
                    column.Item().PaddingTop(10).Text("Było:").Bold().FontSize(10).FontColor(colorDark);
                    ComposeTable(column.Item(), _model.OriginalItems, colorLight, colorDark, false);
                    column.Item().PaddingTop(10).Text("Powinno być:").Bold().FontSize(10).FontColor(colorDark);
                }
                ComposeTable(column.Item(), _model.Items, _model.IsCorrection ? secondarycolorLight : colorLight, _model.IsCorrection ? secondarycolorDark : colorDark, !_model.IsCorrection);

                if (_model.IsCorrection)
                {
                    column.Item().PaddingTop(10).Text("Podsumowanie:").Bold().FontSize(10).FontColor(colorDark);
                    ComposeTable(column.Item(), _model.DifferenceItems, colorLight, colorDark, true);
                }

                // Przeliczenia walutowe i słownie
                column.Item().PaddingTop(15).Column(col =>
                {
                    if (_model.RequiresCurrencyConversion)
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(130).AlignRight().PaddingRight(5).Text("Słownie w euro:").FontColor(labelColor).SemiBold();
                            row.RelativeItem().Text(_model.Summary.InWordsEuro);
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Text("Kwota VAT została przeliczona na złote polskie po kursie średnim NBP dla EUR, Tabela nr");
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Text($"{_model.ExchangeRateInfo.NbpTable} z {_model.ExchangeRateInfo.NbpDate}.");
                        });

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.ConstantItem(130).AlignRight().PaddingRight(5).Text("Cena euro:").FontColor(labelColor).SemiBold();
                            row.RelativeItem().Text(_model.ExchangeRateInfo.ExchangeRate);
                        });
                    }
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.ConstantItem(130).AlignRight().PaddingRight(5).Text(_model.RequiresCurrencyConversion ? "Kwota VAT w PLN:" : "Kwota VAT:").FontColor(labelColor).SemiBold();
                        row.RelativeItem().Text(_model.Summary.VatInPLN);
                    });

                    col.Item().Row(row =>
                    {
                        row.ConstantItem(130).AlignRight().PaddingRight(5).Text(_model.RequiresCurrencyConversion ? "Słownie w PLN:" : "Słownie:").FontColor(labelColor).SemiBold();
                        row.RelativeItem().Text(_model.Summary.VatInPLNWords);
                    });

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.ConstantItem(130).AlignRight().PaddingRight(5).Text(_model.RequiresCurrencyConversion ? "Cała kwota w PLN:" : "Cała kwota:").FontColor(labelColor).SemiBold();
                        row.RelativeItem().Text(_model.Summary.AllInPLN);
                    });

                    col.Item().Row(row =>
                    {
                        row.ConstantItem(130).AlignRight().PaddingRight(5).Text(_model.RequiresCurrencyConversion ? "Słownie w PLN:" : "Słownie:").FontColor(labelColor).SemiBold();
                        row.RelativeItem().Text(_model.Summary.AllInPLNWords);
                    });
                });

                column.Item().PaddingVertical(10).Element(x => x.Height(1).Background(colorDark));
            });

            // Stopka
            page.Footer().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Height(80).Border(1).BorderColor(colorDark).AlignBottom().PaddingBottom(5).AlignCenter()
                    .Text(t => t.Span("imię, nazwisko i podpis osoby upoważnionej do odebrania dokumentu").FontColor(labelColor).FontSize(7));

                row.ConstantItem(20);

                row.RelativeItem().Height(80).Border(1).BorderColor(colorDark).AlignBottom().PaddingBottom(5).AlignCenter()
                    .Text(t => t.Span("imię, nazwisko i podpis osoby upoważnionej do wystawienia dokumentu").FontColor(labelColor).FontSize(7));
            });
        });
    }
    private void ComposeTable(IContainer container, List<InvoiceItemViewModel> items, string colorLight, string colorDark, bool showSummary)
    {
        string[] headers = ["Lp.", "Nazwa", "Ilość", "JM", "Cena Netto", "Wartość netto", "Stawka VAT", "Kwota VAT", "Wartość brutto"];

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1);
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            // Nagłówki
            foreach (var header in headers)
            {
                table.Cell().Element(cell =>
                    cell.Background(colorLight).Border(1).BorderColor(colorDark).Padding(5).MinHeight(25).AlignMiddle().AlignCenter()
                ).Text(header).SemiBold();
            }

            // Wiersze
            foreach (var item in items)
            {
                table.Cell().Element(CellStyle).Text(item.Number);
                table.Cell().Element(CellStyle).Text(item.Name);
                table.Cell().Element(CellStyle).Text(item.Quantity);
                table.Cell().Element(CellStyle).Text(item.Unit);
                table.Cell().Element(CellStyle).Text(item.NetPrice);
                table.Cell().Element(CellStyle).Text(item.NetValue);
                table.Cell().Element(CellStyle).Text(item.VatRate);
                table.Cell().Element(CellStyle).Text(item.VatAmount);
                table.Cell().Element(CellStyle).Text(item.GrossValue);
            }
            if (showSummary)
            {
                table.Cell().ColumnSpan(2).Element(SummaryHeader).Text("RAZEM DO ZAPŁATY:").Bold().FontColor(Colors.White);
                table.Cell().ColumnSpan(2).Element(SummaryValue).Text(_model.Summary.TotalToPay).Bold().FontColor(Colors.White);

                table.Cell().Element(SummaryCell).Text("RAZEM:").Bold().FontColor(colorDark);
                table.Cell().Element(SummaryCell).Text(_model.Summary.TotalNet).Bold().FontColor(colorDark);
                table.Cell().Element(SummaryCell);
                table.Cell().Element(SummaryCell).Text(_model.Summary.TotalVat).Bold().FontColor(colorDark);
                table.Cell().Element(SummaryCell).Text(_model.Summary.TotalGross).Bold().FontColor(colorDark);
            }

            IContainer CellStyle(IContainer c) => c.Border(1).BorderColor(colorLight).Padding(5).AlignMiddle().AlignCenter();
            IContainer SummaryHeader(IContainer c) => c.Background(colorDark).Border(1).BorderColor(colorDark).Padding(5).AlignMiddle();
            IContainer SummaryValue(IContainer c) => c.Background(colorDark).Border(1).BorderColor(colorDark).PaddingRight(5).AlignMiddle().AlignRight();
            IContainer SummaryCell(IContainer c) => c.Background(colorLight).Border(1).BorderColor(colorLight).Padding(5).AlignCenter().AlignMiddle();
        });
    }
}
