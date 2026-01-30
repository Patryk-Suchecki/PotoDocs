using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;

namespace PotoDocs.API.Extensions;

public static class OrderQueryExtensions
{
    public static IQueryable<Order> IncludeFullDetails(this IQueryable<Order> query)
    {
        return query
            .Include(o => o.Driver)
            .Include(o => o.Files)
            .Include(o => o.Company)
            .Include(o => o.Stops)
            .Include(o => o.Invoice);
    }
}