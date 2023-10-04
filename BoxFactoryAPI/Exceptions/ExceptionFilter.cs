using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Models.Exceptions;

namespace BoxFactoryAPI.Exceptions;

public class ExceptionFilter : IAsyncExceptionFilter
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(IHostEnvironment environment, ILogger<ExceptionFilter> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        const string baseErrorMessage = "Something went wrong";
        var trace = Activity.Current?.Id ?? context?.HttpContext.TraceIdentifier;
        var exception = context!.Exception;

        string errorCode;
        int statusCode;
        string errorMessage;

        if (exception is AppException appException)
        {
            switch (appException)
            {
                case NotFoundException notFoundException:
                    errorCode = "NotFound";
                    statusCode = StatusCodes.Status404NotFound;
                    errorMessage = notFoundException.Message;
                    context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    break;

                // This should never happen in production
                default:
                    errorCode = "AppExceptionNotHandled";
                    statusCode = StatusCodes.Status500InternalServerError;
                    errorMessage = "AppException not handled in exception filter";
                    break;
            }
        }
        else
        {
            // Unhandled error
            _logger.LogError(exception, "Unhandled exception");

            errorCode = string.Empty;
            statusCode = StatusCodes.Status500InternalServerError;
            errorMessage = baseErrorMessage;

            if (_environment.IsDevelopment() || _environment.IsStaging())
            {
                errorMessage = exception.Message;
            }
        }

        context.HttpContext.Response.StatusCode = statusCode;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(errorCode, statusCode, trace, errorMessage));
    }
}