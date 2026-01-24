using MudBlazor;
using PotoDocs.Shared.Models;

namespace PotoDocs.Blazor.Extensions;

public static class OrderExtensions
{
    public static Color GetStatusColor(this OrderDto order)
    {
        if (order.Invoice == null) return Color.Default;
        if (order.Invoice.HasPaid) return Color.Success;
        if (order.Invoice.SentDate == null) return Color.Warning;
        if (order.Invoice.DaysUntilDue < 0) return Color.Error;
        return Color.Info;
    }

    public static string GetStatusText(this OrderDto order)
    {
        if (order.Invoice == null) return "Brak faktury";
        if (order.Invoice.HasPaid) return "Zakończono";
        if (order.Invoice.SentDate == null) return "Do wysłania";
        if (order.Invoice.DaysUntilDue < 0) return $"Po terminie ({Math.Abs(order.Invoice.DaysUntilDue)} dni)";
        if (order.Invoice.DaysUntilDue == 0) return "Dziś";
        return $"Zostało {order.Invoice.DaysUntilDue} dni";
    }
}