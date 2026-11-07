using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using VicServoPriceWatch.Core.Utils;

namespace VicServoPriceWatch.Core.Models;

/// <summary>
/// Represents the collection of fuel-price details returned by the API.
/// </summary>
public sealed class FuelPriceData
{
    /// <summary>
    /// Gets or sets the fuel-price details for each station.
    /// </summary>
    [JsonPropertyName("fuelPriceDetails")]
    public List<FuelPriceDetail> FuelPriceDetails { get; set; } = [];

    /// <summary>
    /// Populates missing station postcodes and suburbs using each station's address.
    /// </summary>
    internal void PopulatePostcodesAndSuburbs()
    {
        foreach (FuelPriceDetail detail in FuelPriceDetails)
        {
            detail.FuelStation.GetPostcodesAndSuburb();
        }
    }
}

/// <summary>
/// Represents fuel prices and metadata for a single station.
/// </summary>
public sealed class FuelPriceDetail
{
    /// <summary>
    /// Gets or sets the station information.
    /// </summary>
    [JsonPropertyName("fuelStation")]
    public FuelStation FuelStation { get; set; } = new();

    /// <summary>
    /// Gets or sets the fuel prices reported by the station.
    /// </summary>
    [JsonPropertyName("fuelPrices")]
    public List<FuelPrice> FuelPrices { get; set; } = [];

    /// <summary>
    /// Gets or sets the time when the station data was updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Converts the station data into a list of display rows.
    /// One row per fuel type is created, even if the station has not reported a price for that fuel type.
    /// </summary>
    /// <param name="detail">The station-level API response.</param>
    /// <param name="price">The fuel price to display, if one is available.</param>
    /// <returns>A list of display rows.</returns>
    public List<PriceRow> CreatePriceRows()
    {
        if (this.FuelPrices.Count == 0) return [];
        
        List<PriceRow> rows = new List<PriceRow>();

        foreach (FuelPrice price in this.FuelPrices)
        {
            if (price.Price is null) continue;

            PriceRow row = new PriceRow
            {
                Station = this.FuelStation.Name,
                Address = this.FuelStation.Address,
                Suburb = this.FuelStation.Suburb ?? string.Empty,
                Postcode = this.FuelStation.Postcode ?? string.Empty,
                FuelType = FuelTypeDisplay.GetFuelTypeDisplayName(price.FuelType),
                PriceValue = price.Price,
                Price = $"{price.Price:F1}",
                UpdatedAtValue = this.UpdatedAt,
                UpdatedAt = this.UpdatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            };
            rows.Add(row);
        }
        return rows;
    }
}

/// <summary>
/// Represents a fuel station and its location and contact details.
/// </summary>
public sealed class FuelStation
{
    /// <summary>
    /// Gets or sets the station identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the station name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the station address.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the suburb where the station is located.
    /// </summary>
    [JsonPropertyName("suburb")]
    public string? Suburb { get; set; }

    /// <summary>
    /// Gets or sets the station postcode.
    /// </summary>
    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    /// <summary>
    /// Gets or sets the station brand identifier.
    /// </summary>
    [JsonPropertyName("brandId")]
    public string BrandId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the station contact phone number.
    /// </summary>
    [JsonPropertyName("contactPhone")]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Gets or sets the station geographic coordinates.
    /// </summary>
    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();

    /// <summary>
    /// Gets or sets the station opening hours by day.
    /// </summary>
    [JsonPropertyName("openingHours")]
    public Dictionary<string, OpeningHours>? OpeningHours { get; set; }

    /// <summary>
    /// Extracts missing postcode and suburb values from the station address.
    /// </summary>
    internal void GetPostcodesAndSuburb()
    {
        if (string.IsNullOrWhiteSpace(Postcode))
        {
            // Victorian postcodes are four digits at the end of the address.
            Postcode = Regex.Match(Address.TrimEnd(), @"\d{4}$").Value;
        }

        if (string.IsNullOrWhiteSpace(Suburb))
        {
            string addressBeforePostcode = Address;
            if (!string.IsNullOrWhiteSpace(Postcode))
            {
                int postcodeIndex = addressBeforePostcode.LastIndexOf(Postcode, StringComparison.OrdinalIgnoreCase);
                if (postcodeIndex >= 0)
                {
                    addressBeforePostcode = addressBeforePostcode[..postcodeIndex];
                }
            }

            // Remove the state suffix before selecting the final comma-separated segment.
            addressBeforePostcode = Regex.Replace(
                addressBeforePostcode,
                @"\b(?:VIC|Victoria)\b\s*$",
                string.Empty,
                RegexOptions.IgnoreCase).Trim(' ', ',', '-');

            string[] addressParts = addressBeforePostcode.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (addressParts.Length > 1)
            {
                Suburb = addressParts[^1].ToUpper();
            }
        }
    }
}

/// <summary>
/// Represents the latitude and longitude of a station.
/// </summary>
public sealed class Location
{
    /// <summary>
    /// Gets or sets the latitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

/// <summary>
/// Represents the opening hours for a station on a particular day.
/// </summary>
public sealed class OpeningHours
{
    /// <summary>
    /// Gets or sets the closing time.
    /// </summary>
    [JsonPropertyName("to")]
    public TimeOnly? To { get; set; }

    /// <summary>
    /// Gets or sets the opening time.
    /// </summary>
    [JsonPropertyName("from")]
    public TimeOnly? From { get; set; }

    /// <summary>
    /// Gets or sets the opening-hours state returned by the API.
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// Represents a fuel type and its current price at a station.
/// </summary>
public sealed class FuelPrice
{
    /// <summary>
    /// Gets or sets the API fuel-type code.
    /// </summary>
    [JsonPropertyName("fuelType")]
    public string FuelType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the fuel is currently available.
    /// </summary>
    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Gets or sets the reported price, if one is available.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Gets or sets the time when the fuel price was updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Represents one station/fuel-price row displayed in the results grid.
/// </summary>
public sealed class PriceRow
{
    public string Station { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Suburb { get; init; } = string.Empty;
    public string Postcode { get; init; } = string.Empty;
    public string FuelType { get; init; } = string.Empty;
    public string Price { get; init; } = string.Empty;
    public decimal? PriceValue { get; init; }
    public DateTimeOffset UpdatedAtValue { get; init; }
    public string UpdatedAt { get; init; } = string.Empty;
}