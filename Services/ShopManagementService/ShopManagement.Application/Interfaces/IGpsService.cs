using ShopManagement.Application.Dtos;

namespace ShopManagement.Application.Interfaces
{
    public interface IGpsService
    {
        Task<GpsCoordinateDto?> GetGpsCoordinatesAsync(string street, string city, string state);
    }
}