using BuildingBlocks.Api;
using BuildingBlocks.Api.Middlewares;
using ShopManagement.Api.Extensions;
using ShopManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
                .AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails()
                .AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCustomApiVersioning(builder =>
{
    builder.AddCustomApiVersioning();
    builder.AddVersionedSwagger();
});

builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

var app = builder.Build();
await app.Services.ApplyMigrationAsync();
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

app.UseApiPipeline();
app.Run();

