using Microsoft.Extensions.Configuration;

namespace VicServoPriceWatch;

internal class Program
{
    [STAThread]
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

    private static void LoadConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        Locals.ApiKey = configuration["ServoApi:ApiKey"] is { Length: > 0 } configuredApiKey
            ? configuredApiKey
            : throw new InvalidOperationException("ServoApi:ApiKey is missing from appsettings.json.");
    }
}

internal class Locals
{
    public static string ApiKey { get; set; } = "";
}

