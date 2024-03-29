using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

// .NET 8 Exception handler: https://www.milanjovanovic.tech/blog/global-error-handling-in-aspnetcore-8
namespace TrueVote.Api.Helpers
{
    [ExcludeFromCodeCoverage]
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<LoggerHelper> _logger;

        public GlobalExceptionHandler(ILogger<LoggerHelper> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server error",
                Detail = exception.Message
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
