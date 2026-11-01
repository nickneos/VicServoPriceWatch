using System.Net.Http.Json;
using System.Text.Json;

namespace VicServoPriceWatch;

internal class ServoApi
{
    private static readonly HttpClient client = new();
    private static readonly string url = "https://api.fuel.service.vic.gov.au/open-data/v1/fuel/prices";
    private static readonly string userAgent = "MyFuelApp/1.0 (DotNetClient)";

    internal static async Task<FuelPriceData> GetWebResource(string apiKey)
    {
        // 1. Create the request message
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // 2. Generate a random UUID for x-transactionid
        string transactionId = Guid.NewGuid().ToString();

        // 3. Add required custom and standard headers
        request.Headers.Add("x-consumer-id", apiKey);
        request.Headers.Add("x-transactionid", transactionId);
        request.Headers.UserAgent.ParseAdd(userAgent);

        // 4. Send the request
        using HttpResponseMessage response = await client.SendAsync(request);

        // 5. Throw an exception if HTTP status code indicates failure (e.g., 401, 403, 500)
        response.EnsureSuccessStatusCode();

        // 6. Read and deserialize the JSON response into FuelPriceData
        FuelPriceData? result = await response.Content.ReadFromJsonAsync<FuelPriceData>();

        return result ?? throw new JsonException("The response did not contain a valid fuel price response.");
    }
}
