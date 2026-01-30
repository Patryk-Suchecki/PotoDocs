using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace PotoDocs.API.Extensions;

public static class QueryableExtensions
{
    public static async Task<T> FirstOrThrowAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, string errorMessage = "Nie znaleziono zasobu.")
    {
        var entity = await query.FirstOrDefaultAsync(predicate);
        return entity == null ? throw new KeyNotFoundException(errorMessage) : entity;
    }
}