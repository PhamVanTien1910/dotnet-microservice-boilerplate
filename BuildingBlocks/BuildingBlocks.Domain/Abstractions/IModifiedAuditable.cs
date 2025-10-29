namespace BuildingBlocks.Domain.Abstractions;

public interface IModifiedAuditable
{
    DateTime? ModifiedAt { get; }
}