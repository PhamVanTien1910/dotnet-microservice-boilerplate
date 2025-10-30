using BuildingBlocks.Api;
using BuildingBlocks.Api.Middlewares;

namespace IAM.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddCustomApiVersioning(builder =>
            {
                builder.AddCustomApiVersioning();
                builder.AddVersionedSwagger();
            });

            services.AddJwtAuthenticationExtension(configuration);

            services.AddAuthorization();


            var redisConn = configuration.GetSection("Redis:ConnectionString").Value;
            if (!string.IsNullOrWhiteSpace(redisConn))
            {
                services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
            }

            services.AddHealthChecks();

            return services;
        }
    }
}