using Microsoft.Extensions.Configuration;
using Geocoder;

namespace VicServoPriceWatch;

internal class Program
{
    private static void Main(string[] args)
    {
        LoadConfiguration();

        FuelPriceData fuelResponse = ServoApi.GetWebResource(Locals.ApiKey).Result;

        if (fuelResponse is null) return;

        // Filter and print to console the closest fuel stations to a given address
        FilterAndPrint(fuelResponse);
    }

    private static void FilterAndPrint(FuelPriceData fuelResponse)
    {
        // Prompt the user for an address
        Console.Write("Enter an address: ");
        string? address = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(address))
        {
            Console.WriteLine("An address is required.");
            return;
        }

        // Prompt the user for a number of results to return
        Console.Write("Enter the number of results to return: ");

        if (!int.TryParse(Console.ReadLine(), out int resultCount) || resultCount <= 0)
        {
            Console.WriteLine("A positive number of results is required.");
            return;
        }
        Console.WriteLine();

        // Geocode the address to get latitude and longitude
        (double Lat, double Long)? loc = NominatimApi.GeocodeAddressAsync(address).Result;

        if (loc is not { Lat: var latitude, Long: var longitude })
        {
            Console.WriteLine("Could not geocode the address.");
            return;
        }

        // Filter the servos to get the closest ones to address provided by the user
        FuelPriceData Filtered = ServoUtils.FilterNearestServos(fuelResponse, latitude, longitude, resultCount);

        // Print the filtered results to the console
        foreach (FuelPriceDetail entry in Filtered.FuelPriceDetails)
        {
            Console.WriteLine($"{entry.FuelStation?.Name} - {entry.FuelStation?.Address}");

            foreach (FuelPrice price in entry.FuelPrices)
            {
                if (price.Price.HasValue)
                {
                    Console.WriteLine($"\t{price.FuelType}:\t${price.Price.Value:F1}\tUpdated {price.UpdatedAt:dd/MM/yyyy HH:mm}");
                }
            }
            Console.WriteLine();
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

