using Microsoft.Maui.Storage;

namespace VicServoPriceWatch.Android.Services;

/// <summary>
/// Stores and retrieves the fuel-price API key using platform-secure storage.
/// </summary>
public sealed class ApiKeyService
{
    private const string ApiKeyStorageName = "fuel-price-api-key";

    /// <summary>
    /// Gets the saved API key, if one exists.
    /// </summary>
    /// <returns>The saved API key, or <see langword="null"/> when none is saved.</returns>
    public Task<string?> GetApiKeyAsync()
    {
        return SecureStorage.Default.GetAsync(ApiKeyStorageName);
    }

    /// <summary>
    /// Saves the API key in platform-secure storage.
    /// </summary>
    /// <param name="apiKey">The API key to save.</param>
    public Task SaveApiKeyAsync(string apiKey)
    {
        return SecureStorage.Default.SetAsync(ApiKeyStorageName, apiKey);
    }

    /// <summary>
    /// Removes the saved API key from secure storage.
    /// </summary>
    public void RemoveApiKey()
    {
        SecureStorage.Default.Remove(ApiKeyStorageName);
    }
}
