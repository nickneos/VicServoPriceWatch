using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Maps;
using MauiLocation = Microsoft.Maui.Devices.Sensors.Location;

namespace VicServoPriceWatch.Android;

/// <summary>
/// Allows the user to select a location on a native map.
/// </summary>
public partial class LocationPickerPage : ContentPage
{
    private static readonly MauiLocation defaultLocation = new(-37.8136, 144.9631);
    private readonly TaskCompletionSource<MauiLocation?> selectionSource = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private MauiLocation? selectedLocation;

    /// <summary>
    /// Initializes the map-based location picker.
    /// </summary>
    public LocationPickerPage()
    {
        InitializeComponent();
        LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            defaultLocation,
            Distance.FromKilometers(25)));
    }

    /// <summary>
    /// Gets the task that completes when the user confirms or cancels the selection.
    /// </summary>
    public Task<MauiLocation?> SelectionTask => selectionSource.Task;

    /// <summary>
    /// Requests location access so the map can display its native current-location control.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (permissionStatus != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }
    }

    /// <summary>
    /// Places a pin when the user taps the map.
    /// </summary>
    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        SetSelectedLocation(e.Location, "Selected location");
    }

    /// <summary>
    /// Gets the device location, marks it on the map, and selects it immediately.
    /// </summary>
    private async void OnCurrentLocationClicked(object? sender, EventArgs e)
    {
        Button currentLocationButton = (Button)sender!;
        currentLocationButton.IsEnabled = false;
        SelectionStatusLabel.Text = "Getting your current location...";

        try
        {
            PermissionStatus permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                SelectionStatusLabel.Text = "Location access was not allowed.";
                return;
            }

            MauiLocation? location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(15)));

            if (location is null)
            {
                SelectionStatusLabel.Text = "The device did not provide a location.";
                return;
            }

            SetSelectedLocation(location, "Current location");
            LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1)));
        }
        catch (PermissionException)
        {
            SelectionStatusLabel.Text = "Location access was not allowed. Check Android permissions.";
        }
        catch (FeatureNotEnabledException)
        {
            SelectionStatusLabel.Text = "Turn on location services and try again.";
        }
        catch (FeatureNotSupportedException)
        {
            SelectionStatusLabel.Text = "Location is not available on this device.";
        }
        finally
        {
            currentLocationButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Searches for an address and selects the first matching location.
    /// </summary>
    private async void OnSearchAddressClicked(object? sender, EventArgs e)
    {
        string query = AddressSearchEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query))
        {
            SelectionStatusLabel.Text = "Enter an address to search.";
            return;
        }

        SearchAddressButton.IsEnabled = false;
        SelectionStatusLabel.Text = "Searching for the address...";

        try
        {
            IEnumerable<MauiLocation>? locations = await Geocoding.Default.GetLocationsAsync(query);
            MauiLocation? location = locations?.FirstOrDefault();

            if (location is null)
            {
                SelectionStatusLabel.Text = "No matching address was found.";
                return;
            }

            SetSelectedLocation(location, query);
            LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1)));
        }
        catch (Exception exception) when (exception is FeatureNotSupportedException or FeatureNotEnabledException)
        {
            SelectionStatusLabel.Text = "Address search is not available on this device.";
        }
        catch (HttpRequestException)
        {
            SelectionStatusLabel.Text = "The address search could not be completed. Check your connection and try again.";
        }
        finally
        {
            SearchAddressButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Confirms the selected map location.
    /// </summary>
    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (selectedLocation is null)
        {
            return;
        }

        await CompleteSelectionAsync(selectedLocation);
    }

    /// <summary>
    /// Cancels location selection.
    /// </summary>
    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await CompleteSelectionAsync(null);
    }

    /// <summary>
    /// Completes the selection task and closes the modal page.
    /// </summary>
    /// <param name="location">The selected location, or <see langword="null"/> when cancelled.</param>
    private async Task CompleteSelectionAsync(MauiLocation? location)
    {
        selectionSource.TrySetResult(location);
        await Navigation.PopModalAsync();
    }

    /// <summary>
    /// Completes the selection with no location when the page is dismissed by back navigation.
    /// </summary>
    protected override void OnDisappearing()
    {
        selectionSource.TrySetResult(null);
        base.OnDisappearing();
    }

    /// <summary>
    /// Updates the selected map pin and status text.
    /// </summary>
    /// <param name="location">The location to select.</param>
    /// <param name="label">The label shown on the map pin.</param>
    private void SetSelectedLocation(MauiLocation location, string label)
    {
        selectedLocation = location;
        LocationMap.Pins.Clear();
        LocationMap.Pins.Add(new Pin
        {
            Label = label,
            Location = location
        });

        SelectionStatusLabel.Text = $"Selected: {location.Latitude:F6}, {location.Longitude:F6}";
        ConfirmButton.IsEnabled = true;
    }
}
