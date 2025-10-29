using YarpApiGateway.Extensions;
using YarpApiGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddRedisServices(builder.Configuration);
builder.Services.AddRateLimitingServices(builder.Configuration);
builder.Services.AddHealthCheckServices(builder.Configuration);
builder.Services.AddCustomCors(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");

app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();