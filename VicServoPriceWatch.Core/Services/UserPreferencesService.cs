using System.Text.Json;
using VicServoPriceWatch.Core.Models;
using VicServoPriceWatch.Core.Utils;

namespace VicServoPriceWatch.Core.Services;

/// <summary>
/// Loads and saves user interface preferences in the local application data directory.
/// </summary>
public static class UserPreferencesService
{
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Loads saved preferences or returns default preferences when they are unavailable.
    /// </summary>
    /// <returns>The saved preferences, or a new default preferences instance.</returns>
    public static UserPreferences Load()
    {
        try
        {
            if (!File.Exists(AppDataPaths.UserPreferencesFilePath))
            {
                return new UserPreferences();
            }

            string json = File.ReadAllText(AppDataPaths.UserPreferencesFilePath);
            return JsonSerializer.Deserialize<UserPreferences>(json, serializerOptions) ?? new UserPreferences();
        }
        catch (IOException)
        {
            return new UserPreferences();
        }
        catch (JsonException)
        {
            return new UserPreferences();
        }
    }

    /// <summary>
    /// Saves preferences using a temporary file before replacing the existing file.
    /// </summary>
    /// <param name="preferences">The preferences to save.</param>
    public static void Save(UserPreferences preferences)
    {
        string temporaryFilePath = Path.Combine(
            AppDataPaths.DirectoryPath,
            $"user-preferences-{Guid.NewGuid():N}.tmp");

        try
        {
            Directory.CreateDirectory(AppDataPaths.DirectoryPath);
            string json = JsonSerializer.Serialize(preferences, serializerOptions);
            File.WriteAllText(temporaryFilePath, json);
            File.Move(temporaryFilePath, AppDataPaths.UserPreferencesFilePath, overwrite: true);
        }
        catch (IOException)
        {
            DeleteTemporaryFile(temporaryFilePath);
        }
        catch (UnauthorizedAccessException)
        {
            DeleteTemporaryFile(temporaryFilePath);
        }
    }

    /// <summary>
    /// Deletes a temporary preferences file when a save does not complete.
    /// </summary>
    /// <param name="filePath">The temporary file path.</param>
    private static void DeleteTemporaryFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
