
using Microsoft.AspNetCore.Http.HttpResults;
using PotoDocs.API.Exceptions;

namespace PotoDocs.API;

public class ErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (BadRequestException e)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(e.Message);
        }
    }
}
