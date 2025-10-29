namespace BuildingBlocks.Api
{
    public interface IApiBuilder
    {
        IApiBuilder AddCustomApiVersioning();
        IApiBuilder AddVersionedSwagger();
    }
}