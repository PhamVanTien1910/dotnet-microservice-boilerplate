using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Model;
using ShopManagement.Domain.Aggregates.ValueObjects;

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

        private Shop()
        {
        }

        public static Shop Create(string name, string? description, string? logoUrl)
        {
            return new Shop
            {
                Name = name,
                Description = description,
                LogoUrl = logoUrl,
                CreatedAt = DateTime.UtcNow
            };
        }

        public Location AddLocation(string name, string? phoneNumber, string street, string city, string state, double? latitude = null, double? longitude = null)
        {
            var isExistedName = _locations.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (isExistedName)
                throw new ConflictException($"Location with name {name} already exists.");
            var location = Location.Create(Id, name, PhoneNumber.Create(phoneNumber!),
                Address.Create(street, city, state), GpsCoordinate.Create(latitude, longitude));
            _locations.Add(location);
            return location;
        }
    }
}