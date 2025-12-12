using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using ProcessMonitor.Domain.Exceptions;

namespace ProcessMonitor.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { Message = "Failed to analyze the request.", Details = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                if (_env.IsDevelopment())
                    await context.Response.WriteAsJsonAsync(new { Message = "A database error occurred.", Details = ex.ToString() });
                else
                    await context.Response.WriteAsJsonAsync(new { Message = "A database error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                if (_env.IsDevelopment())
                    await context.Response.WriteAsJsonAsync(new { Message = "An unexpected error occurred.", Details = ex.ToString() });
                else
                    await context.Response.WriteAsJsonAsync(new { Message = "An unexpected error occurred." });
            }
        }
    }
}
