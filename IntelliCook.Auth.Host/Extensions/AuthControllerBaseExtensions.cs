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
        if (controller.ProblemDetailsFactory != null)
        {
            return controller.ProblemDetailsFactory.CreateValidationProblemDetails(
                controller.HttpContext,
                controller.ModelState,
                statusCode,
                title,
                type,
                detail,
                instance
            );
        }

        var problemDetails = new ValidationProblemDetails(controller.ModelState);
        problemDetails.Status = statusCode ?? problemDetails.Status;
        problemDetails.Title = title ?? problemDetails.Title;
        problemDetails.Type = type ?? problemDetails.Type;
        problemDetails.Detail = detail ?? problemDetails.Detail;
        problemDetails.Instance = instance ?? problemDetails.Instance;
        return problemDetails;
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
        if (controller.ProblemDetailsFactory != null)
        {
            return controller.ProblemDetailsFactory.CreateProblemDetails(
                controller.HttpContext,
                statusCode,
                title,
                type,
                detail,
                instance
            );
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };
        return problemDetails;
    }
}