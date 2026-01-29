using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PotoDocs.API.Invoices;

public static class InvoiceStyles
{
    public static readonly string PrimaryColorDark = "#D9534F";
    public static readonly string PrimaryColorLight = "#E68E8C";
    public static readonly string SecondaryColorDark = "#000000";
    public static readonly string SecondaryColorLight = "#D9D9D9";
    public static readonly string LabelColor = "#616161";

    public static TextStyle Base => TextStyle.Default.FontFamily("Tahoma").FontSize(8).FontColor("#000000");
    public static TextStyle Header => Base.FontSize(16).SemiBold().FontColor(PrimaryColorDark);
    public static TextStyle Label => Base.SemiBold().FontColor(LabelColor);
    public static TextStyle TableHeader => Base.SemiBold().FontSize(7);
}