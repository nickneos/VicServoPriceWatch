using System.Net.Http.Json;
using System.Text.Json;
using VicServoPriceWatch.Core.Models;
using VicServoPriceWatch.Core.Utils;

namespace VicServoPriceWatch.Core.Services;

/// <summary>
/// Retrieves fuel-price data from the Victorian fuel-price API and manages the local response cache.
/// </summary>
public class ServoApiClient
{
    // A shared client avoids creating a new connection for every request.
    private static readonly HttpClient client = new();
    private static readonly string url = "https://api.fuel.service.vic.gov.au/open-data/v1/fuel/prices";
    private static readonly string userAgent = "MyFuelApp/1.0 (DotNetClient)";

    // Cached responses are reused until they exceed this age.
    private static readonly TimeSpan cacheLifetime = TimeSpan.FromMinutes(15);
    private static readonly string cacheDirectory = AppDataPaths.DirectoryPath;
    private static readonly string cacheFilePath = AppDataPaths.FuelPricesCacheFilePath;
    private static readonly JsonSerializerOptions cacheSerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Gets fuel-price data from a fresh local cache or, when necessary, from the remote API.
    /// </summary>
    /// <param name="apiKey">The API consumer key used to authenticate the request.</param>
    /// <returns>The normalized fuel-price response.</returns>
    public static async Task<FuelPriceData> GetFuelPricesAsync(string apiKey)
    {
        FuelPriceData? cachedResult = await TryReadFreshCacheAsync();
        if (cachedResult is not null)
        {
            return cachedResult;
        }

        // Create the request and attach the headers required by the API.
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        string transactionId = Guid.NewGuid().ToString();

        request.Headers.Add("x-consumer-id", apiKey);
        request.Headers.Add("x-transactionid", transactionId);
        request.Headers.UserAgent.ParseAdd(userAgent);

        using HttpResponseMessage response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        FuelPriceData? result = await response.Content.ReadFromJsonAsync<FuelPriceData>();

        if (result is null)
        {
            throw new JsonException("The response did not contain a valid fuel price response.");
        }

        // Normalize before caching so cached responses are ready for all consumers.
        result.PopulatePostcodesAndSuburbs();
        await WriteCacheAsync(result);

        return result;
    }

    /// <summary>
    /// Reads the cached response when the cache exists and has not expired.
    /// </summary>
    /// <returns>The cached response, or <see langword="null"/> when it is unavailable or invalid.</returns>
    private static async Task<FuelPriceData?> TryReadFreshCacheAsync()
    {
        if (!File.Exists(cacheFilePath) || DateTime.UtcNow - File.GetLastWriteTimeUtc(cacheFilePath) > cacheLifetime)
        {
            return null;
        }

        try
        {
            await using FileStream stream = File.OpenRead(cacheFilePath);
            FuelPriceData? cachedResult = await JsonSerializer.DeserializeAsync<FuelPriceData>(stream);

            if (cachedResult is not null)
            {
                // This also supports cache files created before address normalization was added.
                cachedResult.PopulatePostcodesAndSuburbs();
            }

            return cachedResult;
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Writes a response to the cache using a temporary file before replacing the old cache.
    /// </summary>
    /// <param name="result">The normalized response to cache.</param>
    private static async Task WriteCacheAsync(FuelPriceData result)
    {
        Directory.CreateDirectory(cacheDirectory);
        string temporaryFilePath = Path.Combine(cacheDirectory, $"fuel-prices-{Guid.NewGuid():N}.tmp");

        try
        {
            await using (FileStream stream = new(temporaryFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(stream, result, cacheSerializerOptions);
                await stream.FlushAsync();
            }

            // Replace the cache only after serialization completes to avoid leaving a partial cache file.
            File.Move(temporaryFilePath, cacheFilePath, true);
        }
        catch (IOException)
        {
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }
    }
}
