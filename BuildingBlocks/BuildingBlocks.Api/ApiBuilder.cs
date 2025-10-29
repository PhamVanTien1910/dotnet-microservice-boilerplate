using BuildingBlocks.Api.ApiVersioning;
using BuildingBlocks.Api.VersionedSwagger;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Api
{
    internal sealed class ApiBuilder(IServiceCollection services) : IApiBuilder
    {
        private IServiceCollection Services { get; } = services;

        public IApiBuilder AddCustomApiVersioning()
        {
            Services.AddCustomApiVersioning();
            return this;
        }

        public IApiBuilder AddVersionedSwagger()
        {
            Services.AddVersionedSwagger();
            return this;
        }
    }
}