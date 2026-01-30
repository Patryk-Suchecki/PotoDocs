using PotoDocs.API.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PotoDocs.API.Invoices;

public static class InvoiceExtensions
{
    private const float LabelWidth = 130f;

    public static void LabelValueRow(this IContainer container, string label, string value)
    {
        container.Row(row =>
        {
            row.ConstantItem(LabelWidth).AlignRight().PaddingRight(5).Text(label).Style(InvoiceStyles.Label);
            row.RelativeItem().Text(value);
        });
    }

    public static TextSpanDescriptor MoneyText(this IContainer container, decimal value, string symbol)
    {
        return container.Text($"{value:N2} {symbol}");
    }
}