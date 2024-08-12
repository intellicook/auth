using Microsoft.AspNetCore.Mvc;

namespace IntelliCook.Auth.Host.Extensions;

public static class AuthControllerBaseExtensions
{
    public static ValidationProblemDetails CreateValidationProblemDetails(
        this ControllerBase controller,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null
    )
    {
        return controller.ProblemDetailsFactory?.CreateValidationProblemDetails(
            controller.HttpContext,
            controller.ModelState,
            statusCode,
            title,
            type,
            detail,
            instance
        ) ?? new ValidationProblemDetails(controller.ModelState)
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };
    }

    public static ProblemDetails CreateProblemDetails(
        this ControllerBase controller,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null
    )
    {
        return controller.ProblemDetailsFactory?.CreateProblemDetails(
            controller.HttpContext,
            statusCode,
            title,
            type,
            detail,
            instance
        ) ?? new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };
    }
}