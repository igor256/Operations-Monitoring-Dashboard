using OperationsMonitoringDashboard.Models;

namespace OperationsMonitoringDashboard.Services;

/// <summary>
/// Provides methods for retrieving and managing monitoring data from a data source.
/// </summary>
public interface IMonitoringDataService
{
    /// <summary>
    /// Retrieves devices using provided search, filter, and sort settings.
    /// </summary>
    Task<IReadOnlyList<DeviceModel>> GetDevicesAsync(DeviceQueryOptions options);

    /// <summary>
    /// Retrieves alerts using provided filter settings.
    /// Alerts are always returned newest-first.
    /// </summary>
    Task<IReadOnlyList<AlertModel>> GetAlertsAsync(AlertQueryOptions options);

    /// <summary>
    /// Clears active alerts from the underlying data source.
    /// </summary>
    Task ClearAlertsAsync();
}
