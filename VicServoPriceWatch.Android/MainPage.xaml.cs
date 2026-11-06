using Microsoft.Maui.Devices.Sensors;
using VicServoPriceWatch.Android.Services;

namespace VicServoPriceWatch.Android;

/// <summary>
/// Displays the Android application home page.
/// </summary>
public partial class MainPage : ContentPage
{
	private readonly AndroidLocationService locationService;

	/// <summary>
	/// Initializes the home page with the Android location service.
	/// </summary>
	/// <param name="locationService">The service used to obtain the device location.</param>
	public MainPage(AndroidLocationService locationService)
	{
		this.locationService = locationService;
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
			Location location = await locationService.GetCurrentLocationAsync();
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
}
