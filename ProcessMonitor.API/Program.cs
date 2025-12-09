using FluentValidation;
using ProcessMonitor.API.Middlewares;
using ProcessMonitor.API.Services;
using ProcessMonitor.API.Validators;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddValidatorsFromAssemblyContaining<AnalyzeRequestValidator>();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHttpClient<IAnalysisService, HuggingFaceAnalysisService>();

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Rate Limiting per IP address
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,                        
                Window = TimeSpan.FromMinutes(1),        
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2                           
            });
    });
});


var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
