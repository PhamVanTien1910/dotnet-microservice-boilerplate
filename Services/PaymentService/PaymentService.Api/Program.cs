using BuildingBlocks.Api;
using BuildingBlocks.Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using PaymentService.API.Extensions;
using PaymentService.API.Middleware;
using PaymentService.Application;
using PaymentService.Infrastructure;
using BuildingBlocks.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplicationServices()
                .AddInfrastructureServices(builder.Configuration);
builder.Services.AddProblemDetails()
                .AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCustomApiVersioning(builder =>
{
    builder.AddCustomApiVersioning();
    builder.AddVersionedSwagger();
});
builder.Services.AddJwksAuthentication(builder.Configuration, options =>
{
    options.UserAgent = "Boilerplate-PaymentService/1.0";
    options.RequireHttpsMetadata = true;
    options.IncludeErrorDetails = false;
});
builder.Services.AddCustomAuthorization();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "database");

var app = builder.Build();

app.Services.ConfigureJwtBearerWithJwks();

await app.MigrateDatabaseAsync();

// Add Stripe webhook middleware FIRST to preserve raw body
app.UseMiddleware<StripeWebhookMiddleware>();

Console.WriteLine(app.Environment);
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseVersionedSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.MapHealthChecks("/health");

app.UseApiPipeline();
app.Run();
