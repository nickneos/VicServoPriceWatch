namespace VicServoPriceWatch.Core.Models;

/// <summary>
/// Represents user interface preferences persisted between application sessions.
/// </summary>
public sealed class UserPreferences
{
    /// <summary>
    /// Gets or sets the selected fuel type filter.
    /// </summary>
    public string FuelType { get; set; } = "All fuel types";

    /// <summary>
    /// Gets or sets the suburb filter text.
    /// </summary>
    public string Suburb { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postcode filter text.
    /// </summary>
    public string Postcode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected grid sort column name.
    /// </summary>
    public string? SortColumnName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the selected column is sorted ascending.
    /// </summary>
    public bool SortAscending { get; set; } = true;
}
