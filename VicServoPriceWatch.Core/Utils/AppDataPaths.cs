namespace VicServoPriceWatch.Core.Utils;

/// <summary>
/// Provides paths for application data stored in the user's local profile.
/// </summary>
internal static class AppDataPaths
{
    /// <summary>
    /// Gets the directory used for application-local data.
    /// </summary>
    internal static string DirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VicServoPriceWatch");

    /// <summary>
    /// Gets the path of the cached fuel-price response.
    /// </summary>
    internal static string FuelPricesCacheFilePath => Path.Combine(DirectoryPath, "fuel-prices.json");

    /// <summary>
    /// Gets the path of the user's saved preferences.
    /// </summary>
    internal static string UserPreferencesFilePath => Path.Combine(DirectoryPath, "user-preferences.json");
}
