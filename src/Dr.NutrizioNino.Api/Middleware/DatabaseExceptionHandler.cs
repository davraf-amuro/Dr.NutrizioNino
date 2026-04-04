using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Dr.NutrizioNino.Api.Middleware;

public class DatabaseExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!IsDbConnectionError(exception))
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Base Dati non pronta",
            Detail = "Il database non è al momento raggiungibile.",
            Status = StatusCodes.Status503ServiceUnavailable
        }, cancellationToken);

        return true;
    }

    private static bool IsDbConnectionError(Exception exception)
    {
        if (exception is SqlException)
        {
            return true;
        }

        if (exception is InvalidOperationException && IsSqlRelated(exception))
        {
            return true;
        }

        if (exception.InnerException is not null)
        {
            return IsDbConnectionError(exception.InnerException);
        }

        return false;
    }

    private static bool IsSqlRelated(Exception exception) =>
        exception.Message.Contains("sql", StringComparison.OrdinalIgnoreCase) ||
        exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
        exception.Message.Contains("database", StringComparison.OrdinalIgnoreCase);
}
