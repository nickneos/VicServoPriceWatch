using Microsoft.Extensions.Configuration;

namespace VicServoPriceWatch;

/// <summary>
/// Provides the application entry point and startup configuration.
/// </summary>
internal class Program
{
    [STAThread]
    /// <summary>
    /// Starts the Windows Forms application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            LoadConfiguration();
            Application.Run(new MainForm());
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.Message,
                "Vic Servo Price Watch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Loads the Servo API key from user secrets or environment variables.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no API key is configured.</exception>
    private static void LoadConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        Locals.ApiKey = configuration["FuelApiKey"] is { Length: > 0 } configuredApiKey
            ? configuredApiKey
            : throw new InvalidOperationException(
                "FuelApiKey is missing. Configure it with user secrets or the FuelApiKey environment variable.");
    }
}

/// <summary>
/// Stores application-wide local values.
/// </summary>
internal class Locals
{
    public static string ApiKey { get; set; } = "";
}

