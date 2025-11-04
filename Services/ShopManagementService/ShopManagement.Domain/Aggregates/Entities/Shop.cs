using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Model;

namespace ShopManagement.Domain.Aggregates.Entities
{
    public class Shop : AggregateRoot, ICreatedAuditable, IModifiedAuditable, ISoftDeletable
    {
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public string? LogoUrl { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ModifiedAt { get; private set; }
        private readonly List<Location> _locations = new();
        public IReadOnlyCollection<Location> Locations => _locations.AsReadOnly();
    }
}