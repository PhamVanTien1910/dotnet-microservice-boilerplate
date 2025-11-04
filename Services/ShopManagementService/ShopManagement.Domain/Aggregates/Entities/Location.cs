using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.Models;
using ShopManagement.Domain.Aggregates.ValueObjects;

namespace ShopManagement.Domain.Aggregates.Entities
{
    public class Location : Entity
    {
        public Guid ShopId { get; private set; }
        public string Name { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public Address Address { get; private set; }
        public GpsCoordinate? GpsCoordinate { get; private set; }
        public bool IsActive { get; private set; } = true;
        public virtual Shop? Shop { get; private set; }

        private Location()
        {
            Name = null!;
            PhoneNumber = null!;
            Address = null!;
        }
        private Location(Guid shopId, string name, PhoneNumber phoneNumber, Address address, GpsCoordinate? gpsCoordinate = null)
        {
            Id = Guid.Empty;
            ShopId = shopId;
            Name = name;
            PhoneNumber = phoneNumber;
            Address = address;
            GpsCoordinate = gpsCoordinate;
        }
        
        public static Location Create(Guid shopId, string name, PhoneNumber phoneNumber, Address address, GpsCoordinate? gpsCoordinate = null)
        {
            if (shopId == Guid.Empty)
                throw new BadRequestException("Shop ID cannot be empty.");
            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException("Location name cannot be empty.");

            return new Location(shopId, name, phoneNumber, address, gpsCoordinate);
        }
    }
}