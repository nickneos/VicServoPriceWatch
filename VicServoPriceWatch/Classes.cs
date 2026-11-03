using System.Text.Json.Serialization;

namespace VicServoPriceWatch;

public sealed class FuelPriceData
{
    [JsonPropertyName("fuelPriceDetails")]
    public List<FuelPriceDetail> FuelPriceDetails { get; set; } = [];
}

public sealed class FuelPriceDetail
{
    [JsonPropertyName("fuelStation")]
    public FuelStation FuelStation { get; set; } = new();

    [JsonPropertyName("fuelPrices")]
    public List<FuelPrice> FuelPrices { get; set; } = [];

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class FuelStation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("suburb")]
    public string? Suburb { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    [JsonPropertyName("brandId")]
    public string BrandId { get; set; } = string.Empty;

    [JsonPropertyName("contactPhone")]
    public string? ContactPhone { get; set; }

    [JsonPropertyName("location")]
    public Location Location { get; set; } = new();

    [JsonPropertyName("openingHours")]
    public Dictionary<string, OpeningHours>? OpeningHours { get; set; }
}

public sealed class Location
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

public sealed class OpeningHours
{
    [JsonPropertyName("to")]
    public TimeOnly? To { get; set; }

    [JsonPropertyName("from")]
    public TimeOnly? From { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
}

public sealed class FuelPrice
{
    [JsonPropertyName("fuelType")]
    public string FuelType { get; set; } = string.Empty;

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}
