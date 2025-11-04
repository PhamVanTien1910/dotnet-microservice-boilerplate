using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.EfCore;
using ShopManagement.Infrastructure.Configurations;
using Minio;
using ShopManagement.Application.Interfaces;
using ShopManagement.Infrastructure.Services;
using ShopManagement.Domain.Aggregates.Repositories;
using ShopManagement.Domain.Aggregates.Entities;
using ShopManagement.Infrastructure.Repositories;


namespace ShopManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ShopDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("PostgreSql"));
                options.AddAuditingInterceptors(serviceProvider);
            });

            services.AddUnitOfWork<ShopDbContext>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            
           services.Configure<MinioSettings>(configuration.GetSection(MinioSettings.SectionName));
            services.AddSingleton(sp =>
            {
                var config = configuration.GetSection("Minio");
                return new MinioClient()
                    .WithEndpoint(config["Endpoint"])
                    .WithCredentials(config["AccessKey"], config["SecretKey"])
                    .WithSSL(config.GetValue<bool>("UseSSL"))
                    .Build();
            });
            services.AddScoped<IUploadService, MinioService>();
            services.AddScoped<IShopRepository, ShopRepository>();
    
            return services;
        }
    }
}