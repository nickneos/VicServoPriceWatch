namespace VicServoPriceWatch;

internal class ServoUtils
{
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

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
