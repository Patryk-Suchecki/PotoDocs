using MudBlazor;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Extensions;

public static class InvoiceExtensions
{
    public static Color GetStatusColor(this InvoiceDto invoice)
    {
        if (invoice.HasPaid == true) return Color.Success;
        if (invoice.DaysUntilDue < 0) return Color.Error;
        if (invoice.DaysUntilDue == 0) return Color.Warning;
        return Color.Info;
    }

    public static string GetStatusText(this InvoiceDto invoice)
    {
        if (invoice.HasPaid == true) return "Zapłacono";
        if (invoice.DaysUntilDue < 0) return $"Po terminie ({Math.Abs(invoice.DaysUntilDue)} dni)";
        if (invoice.SentDate == null) return "Do wysłania";
        if (invoice.DaysUntilDue == 0) return "Dziś";
        return $"Zostało {invoice.DaysUntilDue} dni";
    }

}