using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace VicServoPriceWatch;

public static class NominatimGeocoder
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string baseUrl = "https://nominatim.openstreetmap.org/search";
    private static readonly string userAgent = "Geocoder/1.0 (nicholas.neos@gmail.com)";

    public static async Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address)
    {
        // 1. URL encode the address parameter
        string encodedAddress = HttpUtility.UrlEncode(address);
        string requestUrl = $"{baseUrl}?q={encodedAddress}&format=json&limit=1";

        // 2. Nominatim requires a custom User-Agent header
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        try
        {
            HttpResponseMessage response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<NominatimResult[]>(json);

            if (results != null && results.Length > 0)
            {
                // Coordinates are returned as strings in JSON
                if (double.TryParse(results[0].Lat, out double lat) &&
                    double.TryParse(results[0].Lon, out double lon))
                {
                    return (lat, lon);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching coordinates: {ex.Message}");
        }

        return null;
    }
}

// Data model to deserialize Nominatim JSON response
public class NominatimResult
{
    [JsonPropertyName("lat")]
    public string Lat { get; set; } = string.Empty;

    [JsonPropertyName("lon")]
    public string Lon { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;
}
