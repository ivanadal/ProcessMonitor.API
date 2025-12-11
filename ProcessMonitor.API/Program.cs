using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProcessMonitor.API.Middlewares;
using ProcessMonitor.API.Validators;
using ProcessMonitor.Data;
using ProcessMonitor.Domain.Interfaces;
using ProcessMonitor.Domain.Services;
using ProcessMonitor.Infrastructure.Repositories;
using ProcessMonitor.Infrastructure.Services;
using Serilog;
using System;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddValidatorsFromAssemblyContaining<AnalyzeRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<HistoryQueryValidator>();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<IAnalysisService, AnalysisDomainService>();

builder.Services.AddHttpClient<HuggingFaceAnalysisService>((sp,client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var apiKey = builder.Configuration["HuggingFaceApiKey"];
    if (string.IsNullOrWhiteSpace(apiKey))
        throw new InvalidOperationException("Environment variable 'HuggingFaceApiKey' is missing.");

   
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiKey);
});

builder.Services.AddScoped<IAIAnalysisService>(sp =>
    sp.GetRequiredService<HuggingFaceAnalysisService>());

builder.Services.AddScoped<IAnalysisRepository, AnalysisRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=processmonitor.db"));

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddScoped<IAnalysisService, AnalysisDomainService>();

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

// Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

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
