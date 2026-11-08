using System.Text;
using System.Text.Json;
using VicServoPriceWatch.Core.Models;
using VicServoPriceWatch.Core.Services;
using VicServoPriceWatch.Core.Utils;
using VicServoPriceWatch.Android.Services;
using MauiLocation = Microsoft.Maui.Devices.Sensors.Location;

namespace VicServoPriceWatch.Android;

/// <summary>
/// Displays the Android application home page.
/// </summary>
public partial class MainPage : ContentPage
{
	private readonly ApiKeyService apiKeyService;
	private MauiLocation? selectedLocation;

	/// <summary>
	/// Initializes the home page with the Android location service.
	/// </summary>
	/// <param name="apiKeyService">The service used to retrieve the saved API key.</param>
	public MainPage(ApiKeyService apiKeyService)
	{
		this.apiKeyService = apiKeyService;
		InitializeComponent();
		FuelTypePicker.ItemsSource = new[] { "All fuel types" }
			.Concat(FuelTypeDisplay.displayOrder)
			.ToArray();
	}

	/// <summary>
	/// Opens the map picker and stores the location selected by the user.
	/// </summary>
	private async void OnSelectLocationClicked(object? sender, EventArgs e)
	{
		SelectLocationButton.IsEnabled = false;

		try
		{
			var pickerPage = new LocationPickerPage();
			await Navigation.PushModalAsync(pickerPage);

			MauiLocation? location = await pickerPage.SelectionTask;
			if (location is not null)
			{
				selectedLocation = location;
				LocationStatusLabel.Text = $"Selected location:\n{location.Latitude:F6}, {location.Longitude:F6}";
				SemanticScreenReader.Announce(LocationStatusLabel.Text);
			}
		}
		finally
		{
			SelectLocationButton.IsEnabled = true;
		}
	}

	/// <summary>
	/// Gets the nearest service stations using the saved API key and current location.
	/// </summary>
	private async void OnFindServosClicked(object? sender, EventArgs e)
	{
		FindServosButton.IsEnabled = false;
		ResultsLabel.Text = "Finding nearby servos...";

		try
		{
			string? apiKey = await apiKeyService.GetApiKeyAsync();
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				ResultsLabel.Text = "Save your API key in Settings before looking up fuel prices.";
				return;
			}

			if (selectedLocation is null)
			{
				ResultsLabel.Text = "Select a location on the map before looking up fuel prices.";
				return;
			}

			MauiLocation location = selectedLocation;
			LocationStatusLabel.Text = $"Latitude: {location.Latitude:F6}\nLongitude: {location.Longitude:F6}";

			FuelPriceData fuelPrices = await ServoApiClient.GetFuelPricesAsync(apiKey);
			string selectedFuelType = FuelTypePicker.SelectedItem?.ToString() ?? "All fuel types";
			string fuelType = FuelTypeDisplay.GetFuelTypeApiCode(selectedFuelType);
            fuelPrices = FuelDataUtils.FilterFuelType(fuelPrices, fuelType);
            fuelPrices = FuelDataUtils.FilterNearestServos(
				fuelPrices,
				(location.Latitude, location.Longitude),
				(int)NumberOfServosSlider.Value);

			ResultsLabel.Text = FormatResults(fuelPrices);
			SemanticScreenReader.Announce("Nearby servo results loaded.");
		}
		catch (PermissionException exception)
		{
			ResultsLabel.Text = exception.Message;
		}
		catch (InvalidOperationException exception)
		{
			ResultsLabel.Text = exception.Message;
		}
		catch (HttpRequestException)
		{
			ResultsLabel.Text = "The fuel-price service could not be reached. Check your internet connection and try again.";
		}
		catch (JsonException)
		{
			ResultsLabel.Text = "The fuel-price service returned an invalid response.";
		}
		finally
		{
			FindServosButton.IsEnabled = true;
		}
	}
    /// <summary>
    /// Updates the displayed number of nearby service stations.
    /// </summary>
    private async void OnNumberOfServosSliderValueChanged(object? sender, ValueChangedEventArgs e)
    {
        int newValue = (int)e.NewValue;
        NumberOfServosLabel.Text = $"Number of Servos: {newValue}";
        await Task.CompletedTask;
    }

    /// <summary>
    /// Formats nearby service-station data for display on the home page.
    /// </summary>
    /// <param name="fuelPriceData">The nearest service-station data.</param>
    /// <returns>A readable summary of the nearest service stations.</returns>
    private static string FormatResults(FuelPriceData fuelPriceData)
	{
		if (fuelPriceData.FuelPriceDetails.Count == 0)
		{
			return "No nearby servo results were found.";
		}

		var results = new StringBuilder();

		foreach (FuelPriceDetail detail in fuelPriceData.FuelPriceDetails)
		{
			results.AppendLine(detail.FuelStation.Name);
			results.AppendLine(detail.FuelStation.Address);

			foreach (FuelPrice price in detail.FuelPrices)
			{
                if (!price.IsAvailable || !price.Price.HasValue)
				{
					continue;
				}
				results.AppendLine($"{FuelTypeDisplay.GetFuelTypeDisplayName(price.FuelType)}: {price.Price.Value:F2}");
			}
			results.AppendLine();
		}

		return results.ToString().TrimEnd();
	}
}


