using Scalar.AspNetCore;
using CleanArchitecture.VerticalSlice.Application;
using CleanArchitecture.VerticalSlice.Application.Extensions;
using CleanArchitecture.VerticalSlice.Infrastructure;
using CleanArchitecture.VerticalSlice.Api.Exceptions;
using CleanArchitecture.VerticalSlice.Api.Extensions;
using CleanArchitecture.VerticalSlice.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.AddSerilogLogging();

// Add services to the container.
builder.Services.AddOpenApi();

builder.AddOpenTelemetryConfiguration();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecksConfiguration();

builder.Services.AddExceptionHandler<CustomExceptionHandler>()
            .AddProblemDetails();

var app = builder.Build();

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Use correlation ID middleware
app.UseMiddleware<CorrelationIdMiddleware>();

app.MapApiEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHealthChecks();

app.MapScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.DeepSpace);
    options.WithDefaultHttpClient(
        ScalarTarget.CSharp,
        ScalarClient.HttpClient);
});

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

try
{
    Log.Information("Starting Clean Architecture application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
