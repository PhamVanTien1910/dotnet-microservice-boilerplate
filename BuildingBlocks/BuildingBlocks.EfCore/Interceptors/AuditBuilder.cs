using BuildingBlocks.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.EfCore.Interceptors;

public sealed class AuditBuilder : IAuditBuilder
{
    private readonly IServiceCollection _services;

    public AuditBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IAuditBuilder AddCreatedAuditing()
    {
        _services.AddSingleton<IAuditInterceptor, CreatedAuditingInterceptor>();
        return this;
    }

    public IAuditBuilder AddUpdatedAuditing()
    {
        _services.AddSingleton<IAuditInterceptor, UpdatedAuditingInterceptor>();
        return this;
    }

    public IAuditBuilder AddDeletedAuditing()
    {
        _services.AddSingleton<IAuditInterceptor, DeletedAuditingInterceptor>();
        return this;
    }

}