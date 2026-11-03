using System.Drawing;

namespace VicServoPriceWatch;

/// <summary>
/// Provides the main user interface for searching, filtering, sorting, and displaying
/// Victorian service-station fuel prices.
/// </summary>
internal sealed class MainForm : Form
{
    private static readonly string[] fuelTypeDisplayOrder =
    [
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

    private static readonly Dictionary<string, string> fuelTypeDisplayNames = new(StringComparer.OrdinalIgnoreCase)
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

    // Input controls for the nearest-station search.
    private readonly TextBox addressTextBox = new();
    private readonly NumericUpDown stationCountInput = new();

    // Controls used to filter the displayed price data.
    private readonly ComboBox fuelTypeFilter = new();
    private readonly TextBox suburbFilter = new();
    private readonly TextBox postcodeFilter = new();
    private readonly Button clearFiltersButton = new();

    // Actions for loading station-price data.
    private readonly Button searchButton = new();
    private readonly Button currentLocationButton = new();
    private readonly Button refreshButton = new();
#if DEBUG
    private readonly Button testLocationButton = new();
#endif

    // Status and results controls.
    private readonly Label statusLabel = new();
    private readonly DataGridView pricesGrid = new();

    // The complete set of rows currently loaded from the data source.
    private List<PriceRow> loadedRows = [];

    // Text displayed before the current result count, such as "Ready" or "Search complete".
    private string statusPrefix = "Ready";

    // Current grid sort state. A null column means that no explicit sort has been applied.
    private string? sortColumnName;
    private bool sortAscending = true;

    // Preferences loaded from and saved to the local application data directory.
    private readonly UserPreferences userPreferences;

    /// <summary>
    /// Initializes the main window and configures its controls and layout.
    /// </summary>
    public MainForm()
    {
        userPreferences = UserPreferencesService.Load();

        Text = "Vic Servo Price Watch";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 500);
        Width = 1200;
        Height = 700;

        // Configure the address and station-count inputs.
        addressTextBox.PlaceholderText = "Enter an address";
        addressTextBox.Width = 260;

        stationCountInput.Minimum = 1;
        stationCountInput.Maximum = 100;
        stationCountInput.Value = 10;
        stationCountInput.Width = 60;

        // Configure filters and refresh the grid whenever a filter changes.
        fuelTypeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        fuelTypeFilter.Width = 130;
        fuelTypeFilter.SelectedIndexChanged += (_, _) => ApplyFiltersAndSort();

        suburbFilter.PlaceholderText = "Suburb";
        suburbFilter.Width = 130;
        suburbFilter.Text = userPreferences.Suburb;
        suburbFilter.TextChanged += (_, _) => ApplyFiltersAndSort();

        postcodeFilter.PlaceholderText = "Postcode";
        postcodeFilter.Width = 80;
        postcodeFilter.Text = userPreferences.Postcode;
        postcodeFilter.TextChanged += (_, _) => ApplyFiltersAndSort();

        clearFiltersButton.Text = "Clear filters";
        clearFiltersButton.AutoSize = true;
        clearFiltersButton.Click += (_, _) => ClearFilters();

        sortColumnName = userPreferences.SortColumnName;
        sortAscending = userPreferences.SortAscending;

        // Configure commands that load or search for price data.
        searchButton.Text = "Find nearest";
        searchButton.AutoSize = true;
        searchButton.Click += async (_, _) => await FindNearestPricesAsync(useCurrentLocation: false);

        currentLocationButton.Text = "Use current location";
        currentLocationButton.AutoSize = true;
        currentLocationButton.Click += async (_, _) => await FindNearestPricesAsync(useCurrentLocation: true);

#if DEBUG
        testLocationButton.Text = "Test location";
        testLocationButton.AutoSize = true;
        testLocationButton.Click += async (_, _) =>
        {
            try
            {
                (double latitude, double longitude) =
                    await WindowsLocationService.GetCurrentLocationAsync();

                MessageBox.Show(
                    this,
                    $"Latitude: {latitude}\nLongitude: {longitude}",
                    "Location test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    exception.Message,
                    "Location test failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        };
#endif

        refreshButton.Text = "Show all prices";
        refreshButton.AutoSize = true;
        refreshButton.Click += async (_, _) => await LoadAllPricesAsync();

        // Configure the status display.
        statusLabel.Text = "Ready";
        statusLabel.AutoSize = true;
        statusLabel.Padding = new Padding(12, 7, 0, 0);

        // Configure the read-only results grid.
        pricesGrid.Dock = DockStyle.Fill;
        pricesGrid.ReadOnly = true;
        pricesGrid.AllowUserToAddRows = false;
        pricesGrid.AllowUserToDeleteRows = false;
        pricesGrid.AutoGenerateColumns = false;
        pricesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        pricesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        pricesGrid.MultiSelect = false;
        pricesGrid.RowHeadersVisible = false;
        pricesGrid.ColumnHeaderMouseClick += PricesGrid_ColumnHeaderMouseClick;

        // Add the results grid columns.
        pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceRow.Station),
            DataPropertyName = nameof(PriceRow.Station),
            HeaderText = "Station",
            FillWeight = 18,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });
        pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceRow.Address),
            DataPropertyName = nameof(PriceRow.Address),
            HeaderText = "Address",
            FillWeight = 23,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });
        pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceRow.Suburb),
            DataPropertyName = nameof(PriceRow.Suburb),
            HeaderText = "Suburb",
            FillWeight = 14,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });
        pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceRow.Postcode),
            DataPropertyName = nameof(PriceRow.Postcode),
            HeaderText = "Postcode",
            FillWeight = 10,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });
        pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceRow.FuelType),
            DataPropertyName = nameof(PriceRow.FuelType),
            HeaderText = "Fuel type",
            FillWeight = 14,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });
        pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceRow.Price),
            DataPropertyName = nameof(PriceRow.Price),
            HeaderText = "Price",
            FillWeight = 10,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });
        pricesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceRow.UpdatedAt),
            DataPropertyName = nameof(PriceRow.UpdatedAt),
            HeaderText = "Updated",
            FillWeight = 18,
            SortMode = DataGridViewColumnSortMode.Programmatic
        });

        FlowLayoutPanel toolbar = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(8)
        };
        toolbar.Controls.Add(new Label { Text = "Address:", AutoSize = true, Padding = new Padding(0, 7, 4, 0) });
        toolbar.Controls.Add(addressTextBox);
        toolbar.Controls.Add(new Label { Text = "Stations:", AutoSize = true, Padding = new Padding(12, 7, 4, 0) });
        toolbar.Controls.Add(stationCountInput);
        toolbar.Controls.Add(searchButton);
        toolbar.Controls.Add(currentLocationButton);
#if DEBUG
        toolbar.Controls.Add(testLocationButton);
#endif
        toolbar.Controls.Add(refreshButton);
        toolbar.Controls.Add(statusLabel);

        FlowLayoutPanel filterToolbar = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(8, 0, 8, 8)
        };
        filterToolbar.Controls.Add(new Label { Text = "Fuel type:", AutoSize = true, Padding = new Padding(0, 5, 4, 0) });
        filterToolbar.Controls.Add(fuelTypeFilter);
        filterToolbar.Controls.Add(suburbFilter);
        filterToolbar.Controls.Add(postcodeFilter);
        filterToolbar.Controls.Add(clearFiltersButton);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(filterToolbar, 0, 1);
        layout.Controls.Add(pricesGrid, 0, 2);

        Controls.Add(layout);
        Shown += (_, _) => addressTextBox.Focus();
        FormClosing += (_, _) => SaveUserPreferences();
    }

    /// <summary>
    /// Loads all available station prices and displays them in the results grid.
    /// </summary>
    private async Task LoadAllPricesAsync()
    {
        SetBusy(true);
        statusLabel.Text = "Loading current prices...";

        try
        {
            FuelPriceData response = await ServoApiClient.GetFuelPricesAsync(Locals.ApiKey);
            DisplayPrices(response, $"Showing all prices from {response.FuelPriceDetails.Count:N0} stations. Last refreshed {DateTime.Now:HH:mm:ss}.");
        }
        catch (Exception exception)
        {
            ShowError("Could not load servo prices", exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>
    /// Gets the search coordinates from the address or current Windows location,
    /// finds the requested number of nearest stations, and displays their prices.
    /// </summary>
    /// <param name="useCurrentLocation">Whether to use the current Windows location instead of geocoding the address.</param>
    private async Task FindNearestPricesAsync(bool useCurrentLocation)
    {
        string address = addressTextBox.Text.Trim();
        int stationCount = (int)stationCountInput.Value;

        if (!useCurrentLocation && string.IsNullOrWhiteSpace(address))
        {
            MessageBox.Show(this, "Enter an address or use your current location.", "Location required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            addressTextBox.Focus();
            return;
        }

        SetBusy(true);

        try
        {
            (double Latitude, double Longitude) location;
            string locationDescription;

            if (useCurrentLocation)
            {
                statusLabel.Text = "Getting current location...";
                location = await WindowsLocationService.GetCurrentLocationAsync();
                locationDescription = "your current location";
            }
            else
            {
                statusLabel.Text = "Finding address...";
                (double Latitude, double Longitude)? coordinates = await NominatimGeocoder.GeocodeAddressAsync(address);

                if (coordinates is not { } addressLocation)
                {
                    throw new InvalidOperationException("The address could not be located. Try a more specific address.");
                }

                location = addressLocation;
                locationDescription = address;
            }

            statusLabel.Text = "Loading current prices...";
            FuelPriceData response = await ServoApiClient.GetFuelPricesAsync(Locals.ApiKey);
            FuelPriceData nearestStations = ServoUtils.FilterNearestServos(
                response,
                location.Latitude,
                location.Longitude,
                stationCount);

            DisplayPrices(nearestStations, $"Showing {nearestStations.FuelPriceDetails.Count:N0} nearest stations to {locationDescription}. Last refreshed {DateTime.Now:HH:mm:ss}.");
        }
        catch (Exception exception)
        {
            ShowError("Could not find nearby servo prices", exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>
    /// Converts the API response into display rows and applies the current view settings.
    /// </summary>
    /// <param name="response">The station and price data to display.</param>
    /// <param name="status">The status message to show before the result count.</param>
    private void DisplayPrices(FuelPriceData response, string status)
    {
        loadedRows = [];

        foreach (FuelPriceDetail detail in response.FuelPriceDetails)
        {
            if (detail.FuelPrices.Count == 0)
            {
                loadedRows.Add(CreatePriceRow(detail, null));
                continue;
            }

            loadedRows.AddRange(detail.FuelPrices.Select(price => CreatePriceRow(detail, price)));
        }

        PopulateFuelTypeFilter();
        statusPrefix = status;
        ApplyFiltersAndSort();
    }

    /// <summary>
    /// Rebuilds the fuel-type filter options from the currently loaded rows.
    /// </summary>
    private void PopulateFuelTypeFilter()
    {
        string selectedFuelType = fuelTypeFilter.SelectedItem?.ToString()
            ?? GetFuelTypeDisplayName(userPreferences.FuelType);

        fuelTypeFilter.BeginUpdate();
        fuelTypeFilter.Items.Clear();
        fuelTypeFilter.Items.Add("All fuel types");

        foreach (string fuelType in fuelTypeDisplayOrder)
        {
            fuelTypeFilter.Items.Add(fuelType);
        }

        foreach (string fuelType in loadedRows
            .Select(row => row.FuelType)
            .Where(fuelType => !string.IsNullOrWhiteSpace(fuelType) && fuelType != "No price reported")
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Where(fuelType => !fuelTypeDisplayOrder.Contains(fuelType, StringComparer.CurrentCultureIgnoreCase))
            .OrderBy(fuelType => fuelType, StringComparer.CurrentCultureIgnoreCase))
        {
            fuelTypeFilter.Items.Add(fuelType);
        }

        fuelTypeFilter.SelectedItem = fuelTypeFilter.Items.Contains(selectedFuelType)
            ? selectedFuelType
            : "All fuel types";
        fuelTypeFilter.EndUpdate();
    }

    /// <summary>
    /// Applies the active filters and sort order, then refreshes the grid binding.
    /// </summary>
    private void ApplyFiltersAndSort()
    {
        string selectedFuelType = fuelTypeFilter.SelectedItem?.ToString() ?? "All fuel types";
        string suburb = suburbFilter.Text.Trim();
        string postcode = postcodeFilter.Text.Trim();

        // Filters are applied to the complete loaded set before sorting the visible rows.
        IEnumerable<PriceRow> filteredRows = loadedRows.Where(row =>
            (selectedFuelType == "All fuel types" || string.Equals(row.FuelType, selectedFuelType, StringComparison.CurrentCultureIgnoreCase)) &&
            (suburb.Length == 0 || row.Suburb.Contains(suburb, StringComparison.CurrentCultureIgnoreCase)) &&
            (postcode.Length == 0 || row.Postcode.Contains(postcode, StringComparison.CurrentCultureIgnoreCase)));

        // Use the raw numeric and timestamp values so sorting is not based on display strings.
        IEnumerable<PriceRow> sortedRows = sortColumnName switch
        {
            nameof(PriceRow.Station) => filteredRows.OrderBy(row => row.Station, StringComparer.CurrentCultureIgnoreCase),
            nameof(PriceRow.Address) => filteredRows.OrderBy(row => row.Address, StringComparer.CurrentCultureIgnoreCase),
            nameof(PriceRow.Suburb) => filteredRows.OrderBy(row => row.Suburb, StringComparer.CurrentCultureIgnoreCase),
            nameof(PriceRow.Postcode) => filteredRows.OrderBy(row => row.Postcode, StringComparer.CurrentCultureIgnoreCase),
            nameof(PriceRow.FuelType) => filteredRows.OrderBy(row => row.FuelType, StringComparer.CurrentCultureIgnoreCase),
            nameof(PriceRow.Price) => filteredRows.OrderBy(row => row.PriceValue),
            nameof(PriceRow.UpdatedAt) => filteredRows.OrderBy(row => row.UpdatedAtValue),
            _ => filteredRows
        };

        if (sortColumnName is not null && !sortAscending)
        {
            sortedRows = sortedRows.Reverse();
        }

        List<PriceRow> rows = sortedRows.ToList();
        pricesGrid.DataSource = null;
        pricesGrid.DataSource = rows;
        statusLabel.Text = $"{statusPrefix} Showing {rows.Count:N0} of {loadedRows.Count:N0} prices.";
        UpdateSortGlyphs();
        SaveUserPreferences();
    }

    /// <summary>
    /// Updates the selected column and direction when a grid header is clicked.
    /// </summary>
    /// <param name="sender">The grid that raised the event.</param>
    /// <param name="e">The clicked column information.</param>
    private void PricesGrid_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        string columnName = pricesGrid.Columns[e.ColumnIndex].Name;

        if (sortColumnName == columnName)
        {
            sortAscending = !sortAscending;
        }
        else
        {
            sortColumnName = columnName;
            sortAscending = true;
        }

        ApplyFiltersAndSort();
    }

    /// <summary>
    /// Updates the visual sort indicator on each grid column.
    /// </summary>
    private void UpdateSortGlyphs()
    {
        foreach (DataGridViewColumn column in pricesGrid.Columns)
        {
            column.HeaderCell.SortGlyphDirection = column.Name == sortColumnName
                ? (sortAscending ? SortOrder.Ascending : SortOrder.Descending)
                : SortOrder.None;
        }
    }

    /// <summary>
    /// Captures the current filter and sort state and persists it locally.
    /// </summary>
    private void SaveUserPreferences()
    {
        userPreferences.FuelType = fuelTypeFilter.SelectedItem?.ToString() ?? userPreferences.FuelType;
        userPreferences.Suburb = suburbFilter.Text.Trim();
        userPreferences.Postcode = postcodeFilter.Text.Trim();
        userPreferences.SortColumnName = sortColumnName;
        userPreferences.SortAscending = sortAscending;
        UserPreferencesService.Save(userPreferences);
    }

    /// <summary>
    /// Resets all filters to their default values.
    /// </summary>
    private void ClearFilters()
    {
        suburbFilter.Clear();
        postcodeFilter.Clear();
        if (fuelTypeFilter.Items.Count > 0)
        {
            fuelTypeFilter.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Enables or disables controls while an asynchronous data operation is running.
    /// </summary>
    /// <param name="isBusy">Whether the form is currently processing an operation.</param>
    private void SetBusy(bool isBusy)
    {
        addressTextBox.Enabled = !isBusy;
        stationCountInput.Enabled = !isBusy;
        fuelTypeFilter.Enabled = !isBusy;
        suburbFilter.Enabled = !isBusy;
        postcodeFilter.Enabled = !isBusy;
        clearFiltersButton.Enabled = !isBusy;
        searchButton.Enabled = !isBusy;
        currentLocationButton.Enabled = !isBusy;
#if DEBUG
        testLocationButton.Enabled = !isBusy;
#endif
        refreshButton.Enabled = !isBusy;
        Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
    }

    /// <summary>
    /// Displays an operation error and updates the form status.
    /// </summary>
    /// <param name="title">The error dialog and status title.</param>
    /// <param name="exception">The exception containing the error details.</param>
    private void ShowError(string title, Exception exception)
    {
        statusLabel.Text = title + ".";
        MessageBox.Show(this, exception.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Creates a display row for a station and one of its fuel prices.
    /// </summary>
    /// <param name="detail">The station-level API response.</param>
    /// <param name="price">The fuel price to display, if one is available.</param>
    /// <returns>A formatted row for the results grid.</returns>
    private static PriceRow CreatePriceRow(FuelPriceDetail detail, FuelPrice? price)
    {
        return new PriceRow
        {
            Station = detail.FuelStation.Name,
            Address = detail.FuelStation.Address,
            Suburb = detail.FuelStation.Suburb ?? string.Empty,
            Postcode = detail.FuelStation.Postcode ?? string.Empty,
            FuelType = price is null ? "No price reported" : GetFuelTypeDisplayName(price.FuelType),
            PriceValue = price?.Price,
            Price = price?.Price is decimal value ? $"{value:F1}" : "Unavailable",
            UpdatedAtValue = price?.UpdatedAt ?? detail.UpdatedAt,
            UpdatedAt = price?.UpdatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? detail.UpdatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
        };
    }

    /// <summary>
    /// Converts an API fuel-type code to the name shown in the user interface.
    /// </summary>
    /// <param name="fuelType">The API fuel-type code or display name.</param>
    /// <returns>The user-facing fuel-type name.</returns>
    private static string GetFuelTypeDisplayName(string fuelType)
    {
        return fuelTypeDisplayNames.TryGetValue(fuelType, out string? displayName)
            ? displayName
            : fuelType;
    }

    /// <summary>
    /// Represents one station/fuel-price row displayed in the results grid.
    /// </summary>
    private sealed class PriceRow
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
}
