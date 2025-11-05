using System.Text.Json;
using ShopManagement.Application.Dtos;
using ShopManagement.Application.Interfaces;

namespace ShopManagement.Infrastructure.Services
{
    public class GpsService : IGpsService
    {
        private readonly HttpClient _httpClient;
        private const string NominatimBaseUrl = "https://nominatim.openstreetmap.org/search";

        public GpsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ClassManagementApp/1.0");
        }

        public async Task<GpsCoordinateDto?> GetGpsCoordinatesAsync(string street, string city, string state)
        {
            var address = $"{street}, {city}, {state}";
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{NominatimBaseUrl}?q={encodedAddress}&format=json&limit=1";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var nominatimResults = JsonSerializer.Deserialize<List<NominatimResult>>(response);

                if (nominatimResults != null && nominatimResults.Count > 0)
                {
                    var result = nominatimResults[0];
                    if (double.TryParse(result.lat, out double lat) && double.TryParse(result.lon, out double lon))
                    {
                        return new GpsCoordinateDto
                        {
                            Latitude = lat,
                            Longitude = lon
                        };
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Failed to retrieve GPS coordinates from the external service.");
            }

            return null;
        }

        private class NominatimResult
        {
            public string lat { get; set; } = null!;
            public string lon { get; set; } = null!;
        }
    }
}