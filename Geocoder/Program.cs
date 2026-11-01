namespace Geocoder;

internal class Program
{
    static async Task Main(string[] args)
    {
        string address = "1600 Amphitheatre Parkway, Mountain View, CA";

        var location = await NominatimApi.GeocodeAddressAsync(address);

        if (location != null)
        {
            Console.WriteLine($"Address: {address}");
            Console.WriteLine($"Latitude: {location.Value.Latitude}");
            Console.WriteLine($"Longitude: {location.Value.Longitude}");
        }
        else
        {
            Console.WriteLine("No coordinates found for the provided address.");
        }
    }
}
