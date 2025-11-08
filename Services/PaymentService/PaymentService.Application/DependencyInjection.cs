using System.Reflection;
using BuildingBlocks.Application.Messaging.Abstractions;
using BuildingBlocks.MediatR;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.EventHandlers;
using Shared.IntegrationEvents;

namespace PaymentService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            ConfigureCqrs(services);
            ConfigureIntegrationEventHandlers(services);

            return services;
        }

        private static void ConfigureCqrs(IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
        }

        private static void ConfigureIntegrationEventHandlers(IServiceCollection services)
        {
            services
            .AddScoped<IIntegrationEventHandler<BookingCreatedIntegrationEvent>,
            BookingCreatedIntegrationEventHandler>();
        }
    }
}