using System.Text.RegularExpressions;

namespace VicServoPriceWatch;

internal class ServoUtils
{
    /// <summary>
    /// Normalizes station addresses by extracting missing postcodes and suburbs
    /// from the station address text.
    /// </summary>
    /// <param name="fuelResponse">
    /// The fuel price response containing station details to normalize.
    /// </param>
    internal static void NormalizeStationAddresses(FuelPriceData fuelResponse)
    {
        foreach (FuelPriceDetail detail in fuelResponse.FuelPriceDetails)
        {
            FuelStation station = detail.FuelStation;
            string address = station.Address ?? string.Empty;

            if (string.IsNullOrWhiteSpace(station.Postcode))
            {
                station.Postcode = Regex.Match(address, @"\b\d{4}\b").Value;
            }

            if (string.IsNullOrWhiteSpace(station.Suburb))
            {
                string addressBeforePostcode = address;
                if (!string.IsNullOrWhiteSpace(station.Postcode))
                {
                    int postcodeIndex = addressBeforePostcode.LastIndexOf(station.Postcode, StringComparison.OrdinalIgnoreCase);
                    if (postcodeIndex >= 0)
                    {
                        addressBeforePostcode = addressBeforePostcode[..postcodeIndex];
                    }
                }

                addressBeforePostcode = Regex.Replace(
                    addressBeforePostcode,
                    @"\b(?:VIC|Victoria)\b\s*$",
                    string.Empty,
                    RegexOptions.IgnoreCase).Trim(' ', ',', '-');

                string[] addressParts = addressBeforePostcode.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (addressParts.Length > 1)
                {
                    station.Suburb = addressParts[^1];
                }
            }
        }
    }

    /// <summary>
    /// Filters the fuel stations to return the nearest stations to the specified coordinates.
    /// </summary>
    /// <param name="fuelResponse">The fuel price data containing fuel station details.</param>
    /// <param name="latitude">The latitude of the reference location.</param>
    /// <param name="longitude">The longitude of the reference location.</param>
    /// <param name="limitTo">The maximum number of nearest stations to return.</param>
    /// <returns>A new <see cref="FuelPriceData"/> instance containing the nearest stations.</returns>
    internal static FuelPriceData FilterNearestServos(FuelPriceData fuelResponse, double latitude, double longitude, int limitTo = 3)
    {
        var closestDetails = fuelResponse.FuelPriceDetails
            .Where(detail => detail.FuelStation is not null)
            .Select(detail => new
            {
                Detail = detail,
                Distance = CalculateDistanceInKilometres(
                    latitude,
                    longitude,
                    detail.FuelStation!.Location.Latitude,
                    detail.FuelStation.Location.Longitude)
            })
            .OrderBy(item => item.Distance)
            .Take(limitTo)
            .Select(item => item.Detail)
            .ToList();

        return new FuelPriceData
        {
            FuelPriceDetails = closestDetails
        };
    }

    /// <summary>
    /// Calculates the great-circle distance between two geographic coordinates.
    /// </summary>
    /// <param name="latitude1">The latitude of the first location.</param>
    /// <param name="longitude1">The longitude of the first location.</param>
    /// <param name="latitude2">The latitude of the second location.</param>
    /// <param name="longitude2">The longitude of the second location.</param>
    /// <returns>The distance between the locations in kilometres.</returns>
    private static double CalculateDistanceInKilometres(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double earthRadiusKilometres = 6371;

        double latitudeDifference = DegreesToRadians(latitude2 - latitude1);
        double longitudeDifference = DegreesToRadians(longitude2 - longitude1);

        double a =
            Math.Sin(latitudeDifference / 2) *
            Math.Sin(latitudeDifference / 2) +
            Math.Cos(DegreesToRadians(latitude1)) *
            Math.Cos(DegreesToRadians(latitude2)) *
            Math.Sin(longitudeDifference / 2) *
            Math.Sin(longitudeDifference / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKilometres * c;
    }

    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The equivalent angle in radians.</returns>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
