namespace BuildingBlocks.Domain.Abstractions;

public interface IAuditBuilder
{
    IAuditBuilder AddCreatedAuditing();
    IAuditBuilder AddUpdatedAuditing();
    IAuditBuilder AddDeletedAuditing();
}