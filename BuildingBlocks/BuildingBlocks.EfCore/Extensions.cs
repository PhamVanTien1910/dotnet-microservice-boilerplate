using System.Linq.Expressions;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.EfCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.EfCore;

public static class Extensions
{
    public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
       where TContext : DbContext
    {
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        return services;
    }
    public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);

                entityType.SetQueryFilter(filter);
            }
        }
    }

    public static IServiceCollection AddAuditing(
        this IServiceCollection services,
        Action<IAuditBuilder> builderAction)
    {
        var auditBuilder = new AuditBuilder(services);
        builderAction(auditBuilder);
        return services;
    }

    public static DbContextOptionsBuilder AddAuditingInterceptors(
    this DbContextOptionsBuilder optionsBuilder,
    IServiceProvider serviceProvider)
    {
        // Get all registered IAuditInterceptor implementations from the service provider
        var auditInterceptors = serviceProvider.GetServices<IAuditInterceptor>().OfType<IInterceptor>();

        optionsBuilder.AddInterceptors(auditInterceptors);

        return optionsBuilder;
    }

    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<DispatchDomainEventInterceptor>();
        return services;
    }

    public static DbContextOptionsBuilder AddDomainEventInterceptor(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        var interceptor = serviceProvider.GetRequiredService<DispatchDomainEventInterceptor>();
        optionsBuilder.AddInterceptors(interceptor);

        return optionsBuilder;
    }
}