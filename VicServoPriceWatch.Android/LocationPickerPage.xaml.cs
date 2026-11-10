using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Maps;
using VicServoPriceWatch.Core.Models;
using VicServoPriceWatch.Core.Services;
using MauiLocation = Microsoft.Maui.Devices.Sensors.Location;

namespace VicServoPriceWatch.Android;

/// <summary>
/// Allows the user to select a location on a native map.
/// </summary>
public partial class LocationPickerPage : ContentPage
{
    private static readonly MauiLocation defaultLocation = new(-37.8136, 144.9631);
    private readonly ObservableCollection<string> addressSuggestions = new();
    private readonly TaskCompletionSource<MauiLocation?> selectionSource = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private IReadOnlyList<PostcodeData> postcodeData = Array.Empty<PostcodeData>();
    private Task? postcodeLoadTask;
    private MauiLocation? selectedLocation;

    /// <summary>
    /// Initializes the map-based location picker.
    /// </summary>
    public LocationPickerPage()
    {
        InitializeComponent();
        AddressSuggestionsView.ItemsSource = addressSuggestions;
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

        postcodeLoadTask ??= LoadPostcodeDataAsync();
        await postcodeLoadTask;
    }

    /// <summary>
    /// Places a pin when the user taps the map.
    /// </summary>
    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        SetSelectedLocation(e.Location, "Selected location");
    }

    /// <summary>
    /// Filters postcode suggestions as the user types.
    /// </summary>
    private void OnAddressSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateAddressSuggestions(e.NewTextValue);
    }

    /// <summary>
    /// Loads the cached or downloaded postcode data used by the address suggestions.
    /// </summary>
    private async Task LoadPostcodeDataAsync()
    {
        SelectionStatusLabel.Text = "Loading suburb suggestions...";

        try
        {
            IEnumerable<PostcodeData>? downloadedData = await PostcodeService.DownloadPostcodeDataAsync();
            postcodeData = downloadedData?
                .Where(record => !string.IsNullOrWhiteSpace(record.Locality) && !string.IsNullOrWhiteSpace(record.Postcode))
                .ToArray()
                ?? Array.Empty<PostcodeData>();

            SelectionStatusLabel.Text = "Tap the map or search for a suburb or postcode.";
        }
        catch (Exception)
        {
            SelectionStatusLabel.Text = "Suburb suggestions could not be loaded. You can still tap the map to select a location.";
        }
    }

    /// <summary>
    /// Updates the visible locality and postcode suggestions.
    /// </summary>
    /// <param name="searchText">The current search text.</param>
    private void UpdateAddressSuggestions(string? searchText)
    {
        addressSuggestions.Clear();
        string query = searchText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(query) || postcodeData.Count == 0)
        {
            AddressSuggestionsView.IsVisible = false;
            return;
        }

        foreach (string suggestion in postcodeData
            .Where(record => record.Locality.Contains(query, StringComparison.OrdinalIgnoreCase)
                || record.Postcode.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(record => $"{record.Locality}, {record.Postcode}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8))
        {
            addressSuggestions.Add(suggestion);
        }

        AddressSuggestionsView.IsVisible = addressSuggestions.Count > 0;
    }

    /// <summary>
    /// Selects a postcode suggestion and moves the map to its dataset coordinates.
    /// </summary>
    private void OnAddressSuggestionSelected(object? sender, SelectionChangedEventArgs e)
    {
        string? suggestion = e.CurrentSelection.FirstOrDefault() as string;
        if (suggestion is null)
        {
            return;
        }

        PostcodeData? postcode = postcodeData.FirstOrDefault(record =>
            string.Equals($"{record.Locality}, {record.Postcode}", suggestion, StringComparison.OrdinalIgnoreCase));
        AddressSearchEntry.Text = suggestion;
        AddressSuggestionsView.SelectedItem = null;
        AddressSuggestionsView.IsVisible = false;

        if (postcode is null)
        {
            return;
        }

        MauiLocation location = new(postcode.PreciseLatitude, postcode.PreciseLongitude);
        SetSelectedLocation(location, suggestion);
        LocationMap.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(1)));
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
