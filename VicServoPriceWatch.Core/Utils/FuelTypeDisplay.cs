namespace VicServoPriceWatch.Core.Utils
{
    public class FuelTypeDisplay
    {
        public static readonly string[] displayOrder = [
                "U91",
                "U95",
                "U98",
                "Diesel",
                "Premium Diesel",
                "E10",
                "E85",
                "Biodiesel 20",
                "LPG",
                "LNG",
                "CNG"
        ];

        public static readonly Dictionary<string, string> displayNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["U91"] = "U91",
            ["P95"] = "U95",
            ["P98"] = "U98",
            ["DSL"] = "Diesel",
            ["PDSL"] = "Premium Diesel",
            ["E10"] = "E10",
            ["E85"] = "E85",
            ["B20"] = "Biodiesel 20",
            ["LPG"] = "LPG",
            ["LNG"] = "LNG",
            ["CNG"] = "CNG"
        };

        /// <summary>
        /// Converts an API fuel-type code to the name shown in the user interface.
        /// </summary>
        /// <param name="fuelType">The API fuel-type code or display name.</param>
        /// <returns>The user-facing fuel-type name.</returns>
        public static string GetFuelTypeDisplayName(string fuelType)
        {
            return FuelTypeDisplay.displayNames.TryGetValue(fuelType, out string? displayName)
                ? displayName
                : fuelType;
        }

        /// <summary>
        /// Converts a user-facing fuel-type name to the corresponding API fuel-type code.
        /// </summary>
        /// <param name="fuelDisplayName">The user-facing fuel-type name.</param>
        /// <returns>The API fuel-type code.</returns>
        public static string GetFuelTypeApiCode(string fuelDisplayName)
        {
            foreach (var kvp in FuelTypeDisplay.displayNames)
            {
                if (kvp.Value.Equals(fuelDisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Key;
                }
            }

            return fuelDisplayName;
        }


    }
}
