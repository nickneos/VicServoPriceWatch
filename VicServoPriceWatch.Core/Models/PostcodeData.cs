using System.Text.Json.Serialization;

namespace VicServoPriceWatch.Core.Models;

/// <summary>
/// Represents an Australian postcode record from the Australian postcodes dataset.
/// </summary>
public sealed class PostcodeData
{
    /// <summary>Gets or sets the dataset record identifier.</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>Gets or sets the four-digit postcode.</summary>
    [JsonPropertyName("postcode")]
    public string Postcode { get; set; } = string.Empty;

    /// <summary>Gets or sets the locality name.</summary>
    [JsonPropertyName("locality")]
    public string Locality { get; set; } = string.Empty;

    /// <summary>Gets or sets the state or territory abbreviation.</summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    ///// <summary>Gets or sets the approximate longitude.</summary>
    //[JsonPropertyName("long")]
    //public double Longitude { get; set; }

    ///// <summary>Gets or sets the approximate latitude.</summary>
    //[JsonPropertyName("lat")]
    //public double Latitude { get; set; }

    ///// <summary>Gets or sets the delivery centre.</summary>
    //[JsonPropertyName("dc")]
    //public string DeliveryCentre { get; set; } = string.Empty;

    ///// <summary>Gets or sets the postcode type.</summary>
    //[JsonPropertyName("type")]
    //public string Type { get; set; } = string.Empty;

    ///// <summary>Gets or sets the record status.</summary>
    //[JsonPropertyName("status")]
    //public string Status { get; set; } = string.Empty;

    ///// <summary>Gets or sets the statistical area level 3 code.</summary>
    //[JsonPropertyName("sa3")]
    //public string Sa3 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the statistical area level 3 name.</summary>
    //[JsonPropertyName("sa3name")]
    //public string Sa3Name { get; set; } = string.Empty;

    ///// <summary>Gets or sets the statistical area level 4 code.</summary>
    //[JsonPropertyName("sa4")]
    //public string Sa4 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the statistical area level 4 name.</summary>
    //[JsonPropertyName("sa4name")]
    //public string Sa4Name { get; set; } = string.Empty;

    ///// <summary>Gets or sets the geographic region.</summary>
    //[JsonPropertyName("region")]
    //public string Region { get; set; } = string.Empty;

    /// <summary>Gets or sets the precise latitude.</summary>
    [JsonPropertyName("Lat_precise")]
    public double PreciseLatitude { get; set; }

    /// <summary>Gets or sets the precise longitude.</summary>
    [JsonPropertyName("Long_precise")]
    public double PreciseLongitude { get; set; }

    ///// <summary>Gets or sets the 2021 statistical area level 1 code.</summary>
    //[JsonPropertyName("SA1_CODE_2021")]
    //public string Sa1Code2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 statistical area level 1 name.</summary>
    //[JsonPropertyName("SA1_NAME_2021")]
    //public string Sa1Name2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 statistical area level 2 code.</summary>
    //[JsonPropertyName("SA2_CODE_2021")]
    //public string Sa2Code2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 statistical area level 2 name.</summary>
    //[JsonPropertyName("SA2_NAME_2021")]
    //public string Sa2Name2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 statistical area level 3 code.</summary>
    //[JsonPropertyName("SA3_CODE_2021")]
    //public string Sa3Code2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 statistical area level 3 name.</summary>
    //[JsonPropertyName("SA3_NAME_2021")]
    //public string Sa3Name2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 statistical area level 4 code.</summary>
    //[JsonPropertyName("SA4_CODE_2021")]
    //public string Sa4Code2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 statistical area level 4 name.</summary>
    //[JsonPropertyName("SA4_NAME_2021")]
    //public string Sa4Name2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2011 remoteness area classification.</summary>
    //[JsonPropertyName("RA_2011")]
    //public string RemotenessArea2011 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2016 remoteness area classification.</summary>
    //[JsonPropertyName("RA_2016")]
    //public string RemotenessArea2016 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 remoteness area classification.</summary>
    //[JsonPropertyName("RA_2021")]
    //public string RemotenessArea2021 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2021 remoteness area name.</summary>
    //[JsonPropertyName("RA_2021_NAME")]
    //public string RemotenessArea2021Name { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2015 modified Monash model classification.</summary>
    //[JsonPropertyName("MMM_2015")]
    //public string ModifiedMonashModel2015 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the 2019 modified Monash model classification.</summary>
    //[JsonPropertyName("MMM_2019")]
    //public string ModifiedMonashModel2019 { get; set; } = string.Empty;

    ///// <summary>Gets or sets the Commonwealth electoral division code.</summary>
    //[JsonPropertyName("ced")]
    //public string CommonwealthElectoralDivision { get; set; } = string.Empty;

    ///// <summary>Gets or sets the altitude, when available.</summary>
    //[JsonPropertyName("altitude")]
    //public string Altitude { get; set; } = string.Empty;

    ///// <summary>Gets or sets the charge zone.</summary>
    //[JsonPropertyName("chargezone")]
    //public string ChargeZone { get; set; } = string.Empty;

    ///// <summary>Gets or sets the primary health network code.</summary>
    //[JsonPropertyName("phn_code")]
    //public string PhnCode { get; set; } = string.Empty;

    ///// <summary>Gets or sets the primary health network name.</summary>
    //[JsonPropertyName("phn_name")]
    //public string PhnName { get; set; } = string.Empty;

    ///// <summary>Gets or sets the local government area name.</summary>
    //[JsonPropertyName("lgaregion")]
    //public string LocalGovernmentArea { get; set; } = string.Empty;

    ///// <summary>Gets or sets the local government area code.</summary>
    //[JsonPropertyName("lgacode")]
    //public string LocalGovernmentAreaCode { get; set; } = string.Empty;

    ///// <summary>Gets or sets the electorate name.</summary>
    //[JsonPropertyName("electorate")]
    //public string Electorate { get; set; } = string.Empty;

    ///// <summary>Gets or sets the electorate rating.</summary>
    //[JsonPropertyName("electoraterating")]
    //public string ElectorateRating { get; set; } = string.Empty;

    ///// <summary>Gets or sets the state electoral district code.</summary>
    //[JsonPropertyName("sed_code")]
    //public string StateElectoralDistrictCode { get; set; } = string.Empty;

    ///// <summary>Gets or sets the state electoral district name.</summary>
    //[JsonPropertyName("sed_name")]
    //public string StateElectoralDistrictName { get; set; } = string.Empty;

}
