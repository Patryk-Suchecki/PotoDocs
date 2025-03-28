using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Exceptions;
using System.Text.Json;

namespace PotoDocs.API;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            var problemDetails = ex switch
            {
                BadRequestException badRequest => CreateProblemDetails(badRequest.Message, 400),
                KeyNotFoundException notFound => CreateProblemDetails(notFound.Message, 404),
                UnauthorizedAccessException unauthorized => CreateProblemDetails("Brak dostępu.", 403),
                _ => CreateProblemDetails("Wewnętrzny błąd serwera.", 500)
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status ?? 500;

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    private ProblemDetails CreateProblemDetails(string title, int statusCode)
    {
        return new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}"
        };
    }
}
