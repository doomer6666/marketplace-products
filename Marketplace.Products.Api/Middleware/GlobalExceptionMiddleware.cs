using System.Net;
using FluentValidation;

namespace Marketplace.Products.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = ex switch
        {
            KeyNotFoundException keyEx => (
                (int)HttpStatusCode.NotFound,
                (object)new { error = "Not Found", details = keyEx.Message }
            ),

            ValidationException valEx => (
                (int)HttpStatusCode.BadRequest,
                new
                {
                    error = "Validation Failed",
                    errors = valEx.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage))
                }
            ),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                new { error = "Internal Server Error", details = "An unexpected error occurred." }
            )
        };

        if (statusCode == 500)
        {
            logger.LogError(ex, "Необработанная ошибка сервера");
        }
        else
        {
            logger.LogWarning("Клиентская ошибка {StatusCode}: {Message}", statusCode, ex.Message);
        }

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(response);
    }
}