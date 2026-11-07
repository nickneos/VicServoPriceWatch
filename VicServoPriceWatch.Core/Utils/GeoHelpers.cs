namespace VicServoPriceWatch.Core.Utils
{
    public class GeoHelpers
    {
        /// <summary>
        /// Calculates the great-circle distance between two geographic coordinates.
        /// </summary>
        /// <param name="latitude1">The latitude of the first location.</param>
        /// <param name="longitude1">The longitude of the first location.</param>
        /// <param name="latitude2">The latitude of the second location.</param>
        /// <param name="longitude2">The longitude of the second location.</param>
        /// <returns>The distance between the locations in kilometres.</returns>
        public static double CalculateDistanceInKilometres(double latitude1, double longitude1, double latitude2, double longitude2)
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
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Determines whether the specified input is a valid set of GPS coordinates.
        /// </summary>
        /// <param name="input">The input string to validate.</param>
        /// <returns>true if the input is a valid set of GPS coordinates; otherwise, false.</returns>
        public static bool isGpsCoordinates(string input)
        {
            string[] parts = input.Replace(" ", "").Split(',');
            if (parts.Length != 2)
            {
                return false;
            }
            return double.TryParse(parts[0].Trim(), out double latitude) &&
                   double.TryParse(parts[1].Trim(), out double longitude) &&
                   latitude >= -90 && latitude <= 90 &&
                   longitude >= -180 && longitude <= 180;
        }
    }
}
