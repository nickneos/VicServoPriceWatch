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
	private readonly AndroidLocationService locationService;
	private readonly ApiKeyService apiKeyService;

	/// <summary>
	/// Initializes the home page with the Android location service.
	/// </summary>
	/// <param name="locationService">The service used to obtain the device location.</param>
	/// <param name="apiKeyService">The service used to retrieve the saved API key.</param>
	public MainPage(AndroidLocationService locationService, ApiKeyService apiKeyService)
	{
		this.locationService = locationService;
		this.apiKeyService = apiKeyService;
		InitializeComponent();
	}

	/// <summary>
	/// Gets the current device location and displays its coordinates.
	/// </summary>
	private async void OnGetLocationClicked(object? sender, EventArgs e)
	{
		LocationButton.IsEnabled = false;
		LocationStatusLabel.Text = "Getting your location...";

		try
		{
			MauiLocation location = await locationService.GetCurrentLocationAsync();
			LocationStatusLabel.Text = $"Latitude: {location.Latitude:F6}\nLongitude: {location.Longitude:F6}";
			SemanticScreenReader.Announce(LocationStatusLabel.Text);
		}
		catch (PermissionException exception)
		{
			LocationStatusLabel.Text = exception.Message;
			SemanticScreenReader.Announce(LocationStatusLabel.Text);
		}
		catch (InvalidOperationException exception)
		{
			LocationStatusLabel.Text = exception.Message;
			SemanticScreenReader.Announce(LocationStatusLabel.Text);
		}
		finally
		{
			LocationButton.IsEnabled = true;
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

			MauiLocation location = await locationService.GetCurrentLocationAsync();
			LocationStatusLabel.Text = $"Latitude: {location.Latitude:F6}\nLongitude: {location.Longitude:F6}";

			FuelPriceData fuelPrices = await ServoApiClient.GetFuelPricesAsync(apiKey);
			FuelPriceData nearestServos = ServoLocator.FindNearestServos(
				fuelPrices,
				location.Latitude,
				location.Longitude);

			ResultsLabel.Text = FormatResults(nearestServos);
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

			string prices = string.Join(
				", ",
				detail.FuelPrices
					.Where(price => price.IsAvailable && price.Price.HasValue)
					.Select(price => $"{price.FuelType}: {price.Price!.Value:C}"));

			results.AppendLine(string.IsNullOrWhiteSpace(prices) ? "No available prices" : prices);
			results.AppendLine();
		}

		return results.ToString().TrimEnd();
	}
}
