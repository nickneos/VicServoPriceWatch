using VicServoPriceWatch.Android.Services;

namespace VicServoPriceWatch.Android;

/// <summary>
/// Allows the user to manage the Android application's fuel-price API key.
/// </summary>
public partial class SettingsPage : ContentPage
{
    private readonly ApiKeyService apiKeyService;

    /// <summary>
    /// Initializes the settings page.
    /// </summary>
    /// <param name="apiKeyService">The service used to store the API key securely.</param>
    public SettingsPage(ApiKeyService apiKeyService)
    {
        this.apiKeyService = apiKeyService;
        InitializeComponent();
    }

    /// <summary>
    /// Loads the saved API key when the settings page appears.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            ApiKeyEntry.Text = await apiKeyService.GetApiKeyAsync();
            StatusLabel.Text = string.IsNullOrWhiteSpace(ApiKeyEntry.Text)
                ? "No API key is saved."
                : "A saved API key is available.";
        }
        catch (Exception exception)
        {
            StatusLabel.Text = $"Unable to read the saved API key: {exception.Message}";
        }
    }

    /// <summary>
    /// Saves the entered API key securely.
    /// </summary>
    private async void OnSaveApiKeyClicked(object? sender, EventArgs e)
    {
        string? apiKey = ApiKeyEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            StatusLabel.Text = "Enter an API key before saving.";
            return;
        }

        SaveButton.IsEnabled = false;

        try
        {
            await apiKeyService.SaveApiKeyAsync(apiKey);
            ApiKeyEntry.Text = string.Empty;
            StatusLabel.Text = "API key saved securely.";
        }
        catch (Exception exception)
        {
            StatusLabel.Text = $"Unable to save the API key: {exception.Message}";
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Removes the saved API key.
    /// </summary>
    private void OnClearApiKeyClicked(object? sender, EventArgs e)
    {
        try
        {
            apiKeyService.RemoveApiKey();
            ApiKeyEntry.Text = string.Empty;
            StatusLabel.Text = "Saved API key cleared.";
        }
        catch (Exception exception)
        {
            StatusLabel.Text = $"Unable to clear the API key: {exception.Message}";
        }
    }
}
