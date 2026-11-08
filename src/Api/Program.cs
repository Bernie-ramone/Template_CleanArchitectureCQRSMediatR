using CleanCqrs.Api.Endpoints;
using CleanCqrs.Api.Errors;
using CleanCqrs.Application;
using CleanCqrs.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    await app.Services.EnsureDatabaseCreatedAsync();
}

app.MapProductEndpoints();

await app.RunAsync();

/// <summary>Entry point; public so integration tests can reference it from WebApplicationFactory.</summary>
public partial class Program;
