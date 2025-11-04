using BuildingBlocks.Domain.Exceptions;

namespace ShopManagement.Domain.Aggregates.ValueObjects
{
    public record GpsCoordinate
    {
        public double Latitude { get; }
        public double Longitude { get; }
        private GpsCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        public static GpsCoordinate? Create(double? latitude, double? longitude)
        {
            if (latitude == null || longitude == null)
                return null;
            if (latitude < -90 || latitude > 90)
                    throw new BadRequestException("Latitude must be between -90 and 90 degrees.");
            if (longitude < -180 || longitude > 180)
                throw new BadRequestException("Longitude must be between -180 and 180 degrees.");
            return new GpsCoordinate(latitude.Value, longitude.Value);
        }
        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }
    }
}