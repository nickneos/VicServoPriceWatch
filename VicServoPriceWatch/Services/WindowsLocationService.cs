using Windows.Devices.Geolocation;

namespace VicServoPriceWatch;

/// <summary>
/// Provides the current Windows location for the application.
/// </summary>
internal static class WindowsLocationService
{
    /// <summary>
    /// Gets the current device location after requesting the user's permission.
    /// </summary>
    /// <returns>The current latitude and longitude.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when location access is denied or a location cannot be obtained.
    /// </exception>
    internal static async Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
    {
        GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();

        if (accessStatus != GeolocationAccessStatus.Allowed)
        {
            throw new InvalidOperationException(
                "Windows location access was not allowed. Enable Location Services or enter an address instead.");
        }

        Geolocator geolocator = new()
        {
            DesiredAccuracyInMeters = 1000
        };

        Geoposition position = await geolocator.GetGeopositionAsync(
            maximumAge: TimeSpan.FromMinutes(5),
            timeout: TimeSpan.FromSeconds(15));

        BasicGeoposition coordinates = position.Coordinate.Point.Position;
        return (coordinates.Latitude, coordinates.Longitude);
    }
}