using BuildingBlocks.Application.Services;
using BuildingBlocks.Messaging.RabbitMQ;
using BuildingBlocks.EfCore;
using IAM.Infrastructure.Configurations;
using IAM.Infrastructure.Data;
using IAM.Infrastructure.Repositories;
using IAM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using IAM.Application.Common.Interfaces;
using IAM.Domain.Aggregates.Users.Repositories;
using Polly;
using Polly.Extensions.Http;

namespace IAM.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AddDatabase(services, configuration);
            AddRepositories(services);
            AddSettings(services, configuration);
            AddRedis(services, configuration);
            AddApplicationServices(services, configuration);
            AddEventBus(services);

            return services;
        }

        private static void AddRedis(IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration["Redis:ConnectionString"];
            var withRetry = $"{redisConnectionString},abortConnect=false";
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(withRetry));
            services.AddScoped<IJwtBlacklistService, JwtBlacklistService>();
        }

        private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuditing(builder =>
            {
                builder
                    .AddCreatedAuditing()
                    .AddUpdatedAuditing()
                    .AddDeletedAuditing();
            });

            services.AddDbContext<IAMDbContext>((serviceProvider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString);
                options.AddAuditingInterceptors(serviceProvider);
            });

            services.AddUnitOfWork<IAMDbContext>();
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
        }

        private static void AddSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BCryptSettings>()
                .Bind(configuration.GetSection(BCryptSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .PostConfigure(LoadPemKeysFromFiles)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<SeedUsersSettings>()
                .Bind(configuration.GetSection(SeedUsersSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<JwksSettings>()
                .Bind(configuration.GetSection(JwksSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        private static void LoadPemKeysFromFiles(JwtSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.PrivateKeyPem) &&
                !string.IsNullOrWhiteSpace(settings.PrivateKeyPath) && File.Exists(settings.PrivateKeyPath))
            {
                settings.PrivateKeyPem = File.ReadAllText(settings.PrivateKeyPath);
            }

            if (string.IsNullOrWhiteSpace(settings.PublicKeyPem) &&
                !string.IsNullOrWhiteSpace(settings.PublicKeyPath) && File.Exists(settings.PublicKeyPath))
            {
                settings.PublicKeyPem = File.ReadAllText(settings.PublicKeyPath);
            }
        }

        private static void AddApplicationServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<ITokenHasher, TokenHasher>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ITokenClaimService, TokenClaimService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IJwtContext, CurrentUserService>();
            services.AddSingleton<IJwksProvider, JwksProvider>();

            services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();

            // Data seeding
            services.AddScoped<DatabaseSeeder>();
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services)
        {
            services.AddRabbitMQ("RabbitMQ");

            return services;
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(15)
                );
        }
    }
}