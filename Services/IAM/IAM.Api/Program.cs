using IAM.Application;
using IAM.Infrastructure;
using IAM.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add application layer and infra
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddEventBus();

// Add API services
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddCustomAuthorization();

var app = builder.Build();

await app.MigrateAndSeedDatabaseAsync();

app.UseApiPipeline();

app.Run();
