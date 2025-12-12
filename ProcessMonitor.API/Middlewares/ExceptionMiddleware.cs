using Microsoft.EntityFrameworkCore;
using ProcessMonitor.Domain.Exceptions;

namespace ProcessMonitor.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AnalysisServiceException ex)
            {
                _logger.LogError(ex, "AnalysisServiceException occurred");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { Message = "Failed to analyze the request.", Details = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { Message = "A database error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { Message = "An unexpected error occurred." });
            }
        }
    }

}
