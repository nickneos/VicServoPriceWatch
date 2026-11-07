using System.Text.RegularExpressions;
using VicServoPriceWatch.Core.Models;

namespace VicServoPriceWatch.Core.Utils;

public class ServoLocator
{
    /// <summary>
    /// Finds the nearest service stations to the specified coordinates.
    /// </summary>
    /// <param name="fuelPriceData">The fuel price data.</param>
    /// <param name="latitude">The latitude of the reference location.</param>
    /// <param name="longitude">The longitude of the reference location.</param>
    /// <param name="limitTo">The maximum number of nearest stations to return.</param>
    /// <param name="fuelType">The fuel type to filter by, or "All fuel types" to include all fuel types.</param>
    /// <returns>A new <see cref="FuelPriceData"/> instance containing the nearest stations.</returns>
    public static FuelPriceData FindNearestServos(FuelPriceData fuelPriceData, double latitude, double longitude, int limitTo = 10, string fuelType = "All fuel types")
    {
        bool includeAllFuelTypes = fuelType.Equals("All fuel types", StringComparison.OrdinalIgnoreCase);

        var filteredData = fuelPriceData.FuelPriceDetails
            .Where(detail =>
                detail.FuelStation is not null &&
                (includeAllFuelTypes ||
                 detail.FuelPrices.Any(price => price.FuelType == fuelType)))
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
            .Select(item => new FuelPriceDetail
            {
                FuelStation = item.Detail.FuelStation!,
                FuelPrices = item.Detail.FuelPrices
                    .Where(price => includeAllFuelTypes || price.FuelType == fuelType)
                    .ToList(),
                UpdatedAt = item.Detail.UpdatedAt
            })
            .ToList();

        return new FuelPriceData
        {
            FuelPriceDetails = filteredData
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
