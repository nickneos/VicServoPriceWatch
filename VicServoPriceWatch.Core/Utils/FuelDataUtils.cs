using VicServoPriceWatch.Core.Models;

namespace VicServoPriceWatch.Core.Utils;

public class FuelDataUtils
{
    /// <summary>
    /// Filters the fuel price data by the specified fuel type.
    /// </summary>
    /// <param name="fuelPriceData">The fuel price data to filter.</param>
    /// <param name="fuelType">The fuel type to filter by, or "All fuel types" to include all fuel types.</param>
    /// <returns>A new <see cref="FuelPriceData"/> instance containing the filtered stations.</returns>
    public static FuelPriceData FilterFuelType(FuelPriceData fuelPriceData, string fuelType = "All fuel types")
    {
        bool includeAllFuelTypes = fuelType.Equals("All fuel types", StringComparison.OrdinalIgnoreCase);

        var filteredData = fuelPriceData.FuelPriceDetails
            .Where(detail =>
                detail.FuelStation is not null &&
                (includeAllFuelTypes ||
                 detail.FuelPrices.Any(price => price.FuelType == fuelType)))
            .Select(detail => new FuelPriceDetail
            {
                FuelStation = detail.FuelStation!,
                FuelPrices = detail.FuelPrices
                    .Where(price => includeAllFuelTypes || price.FuelType == fuelType)
                    .ToList(),
                UpdatedAt = detail.UpdatedAt
            })
            .ToList();
        return new FuelPriceData
        {
            FuelPriceDetails = filteredData
        };
    }
    /// <summary>
    /// Finds the nearest service stations to the specified coordinates.
    /// </summary>
    /// <param name="fuelPriceData">The fuel price data.</param>
    /// <param name="latitude">The latitude of the reference location.</param>
    /// <param name="longitude">The longitude of the reference location.</param>
    /// <param name="limitTo">The maximum number of nearest stations to return.</param>
    /// <param name="fuelType">The fuel type to filter by, or "All fuel types" to include all fuel types.</param>
    /// <returns>A new <see cref="FuelPriceData"/> instance containing the nearest stations.</returns>
    public static FuelPriceData FilterNearestServos(FuelPriceData fuelPriceData, (double latitude, double longitude) geoCoordinates, int limitTo = 10)
    {
        var filteredData = fuelPriceData.FuelPriceDetails
            .Where(detail => detail.FuelStation is not null)
            .Select(detail => new
            {
                Detail = detail,
                Distance = GeoHelpers.CalculateDistanceInKilometres(
                    geoCoordinates.latitude,
                    geoCoordinates.longitude,
                    detail.FuelStation!.Location.Latitude,
                    detail.FuelStation.Location.Longitude)
            })
            .OrderBy(item => item.Distance)
            .Take(limitTo)
            .Select(item => item.Detail)
            .ToList();

        return new FuelPriceData
        {
            FuelPriceDetails = filteredData
        };
    }
}
