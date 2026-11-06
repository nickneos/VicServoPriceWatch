using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace VicServoPriceWatch.Android.Services;

/// <summary>
/// Provides the current device location through the Android/MAUI location APIs.
/// </summary>
public sealed class AndroidLocationService
{
    /// <summary>
    /// Requests location access when necessary and gets the current device location.
    /// </summary>
    /// <returns>The current device location.</returns>
    /// <exception cref="PermissionException">
    /// Thrown when the user does not grant location access.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the device cannot provide a location.
    /// </exception>
    public async Task<Location> GetCurrentLocationAsync()
    {
        PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (permissionStatus != PermissionStatus.Granted)
        {
            throw new PermissionException(
                "Location access was not allowed. Enable location permission in Android settings or enter an address instead.");
        }

        Location? location = await Geolocation.Default.GetLocationAsync(
            new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(15)));

        return location ?? throw new InvalidOperationException(
            "The device did not provide a location. Check that location services are enabled and try again.");
    }
}
