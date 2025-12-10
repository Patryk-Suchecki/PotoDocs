using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Exceptions;
using System.Text.Json;

namespace PotoDocs.API;

public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            var problemDetails = ex switch
            {
                InvalidOperationException or BadRequestException =>
                    CreateProblemDetails("Niepoprawne żądanie", ex.Message, 400),

                KeyNotFoundException notFound =>
                    CreateProblemDetails("Nie znaleziono zasobu", notFound.Message, 404),

                UnauthorizedAccessException unauthorized =>
                    CreateProblemDetails("Błąd uwierzytelnienia", unauthorized.Message, 401),

                InvalidOperationException or BadRequestException or ValidationException =>
                    CreateProblemDetails("Niepoprawne żądanie", ex.Message, 400),

                _ =>
                    CreateProblemDetails("Wewnętrzny błąd serwera", "Wystąpił nieoczekiwany błąd. Skontaktuj się z administratorem.", 500)
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

    private ProblemDetails CreateProblemDetails(string title, string detail, int statusCode)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}"
        };
    }
}