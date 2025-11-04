using BuildingBlocks.Domain.Exceptions;

namespace ShopManagement.Domain.Aggregates.ValueObjects
{
    public record Address
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        private Address(string street, string city, string state)
        {
            Street = street;
            City = city;
            State = state;
        }
        public static Address Create(string street, string city, string state)
        {
            if (string.IsNullOrWhiteSpace(street)) throw new BadRequestException("Street address cannot be empty.");
            if (string.IsNullOrWhiteSpace(city)) throw new BadRequestException("City cannot be empty.");
            if (string.IsNullOrWhiteSpace(state)) throw new BadRequestException("State cannot be empty.");
            return new Address(street, city, state);
        }
        public override string ToString()
        {
            return $"{Street}, {City}, {State}";
        }
    }
}