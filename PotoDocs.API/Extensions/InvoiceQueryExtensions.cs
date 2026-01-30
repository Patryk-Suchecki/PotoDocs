using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;

namespace PotoDocs.API.Extensions;

public static class InvoiceQueryExtensions
{
    public static IQueryable<Invoice> IncludeFullDetails(this IQueryable<Invoice> query)
    {
        return query
            .Include(i => i.Items)
            .Include(i => i.Order)
            .Include(i => i.OriginalInvoice)
            .ThenInclude(oi => oi!.Items);
    }
}