namespace OperationsMonitoringDashboard.Models;

/// <summary>
/// Carries search, filter, and sort options used to query devices.
/// </summary>
public class DeviceQueryOptions
{
    /// <summary>
    /// Gets or sets search text matched against name, type, and region.
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional status filter. Null means all statuses.
    /// </summary>
    public DeviceStatus? StatusFilter { get; set; }

    /// <summary>
    /// Gets or sets the selected device sort mode.
    /// </summary>
    public DeviceSortOption SortBy { get; set; } = DeviceSortOption.LastUpdate;
}
