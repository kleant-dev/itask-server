using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace slender_server.API.Middleware;

/// <summary>
/// Handles FluentValidation ValidationException and returns a 400 Bad Request Problem Details response.
/// Use ValidateAndThrowAsync in controllers to automatically trigger this handler.
/// </summary>
public sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        // Group errors by property name (camelCase for consistency with API JSON settings)
        var errors = validationException.Errors
            .GroupBy(e => ToCamelCase(e.PropertyName))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Validation failed",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            }
        };

        context.ProblemDetails.Extensions.Add("errors", errors);

        return await problemDetailsService.TryWriteAsync(context);
    }

    private static string ToCamelCase(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName ?? string.Empty;

        if (propertyName.Length == 1)
            return propertyName.ToLowerInvariant();

        return char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
    }
}