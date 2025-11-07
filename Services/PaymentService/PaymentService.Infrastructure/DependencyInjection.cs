using BuildingBlocks.Application.Services;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.EfCore;
using BuildingBlocks.Messaging.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Options;
// using PaymentService.Application.Common.Interfaces;
// using PaymentService.Application.Settings;
using PaymentService.Domain.Aggregates.PaymentAggregate.Repositories;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Data.Repositories;
using PaymentService.Infrastructure.Services;
using PaymentService.Infrastructure.Settings;

namespace PaymentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuditing(builder =>
        {
            builder.AddCreatedAuditing();
            builder.AddUpdatedAuditing();
        });

        services.AddDomainEventDispatcher();

        services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddAuditingInterceptors(serviceProvider);
            options.AddDomainEventInterceptor(serviceProvider);
        });

        //services.Configure<PaymentSettings>(configuration.GetSection("PaymentSettings"));

        services.Configure<StripeSettings>(options =>
        {
            var stripeSection = configuration.GetSection("StripeSettings");
            options.PublishableKey = stripeSection.GetValue<string>("PublishableKey") ?? "";

            // Read SecretKey from environment variable STRIPE_API_KEY, fallback to appsettings
            options.SecretKey = Environment.GetEnvironmentVariable("STRIPE_API_KEY")
                                ?? stripeSection.GetValue<string>("SecretKey")
                                ?? "";

            // Read WebhookSecret from environment variable STRIPE_WEBHOOK_SECRET, fallback to appsettings
            options.WebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")
                                    ?? stripeSection.GetValue<string>("WebhookSecret")
                                    ?? "";
        });

        services.AddUnitOfWork<PaymentDbContext>();

        // Repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Services
        services.AddHttpContextAccessor();
        //services.AddScoped<IPaymentGatewayService, StripeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // RabbitMQ Event Bus
        services.AddRabbitMQ("RabbitMQ");

        return services;
    }
}
