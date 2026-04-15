using OperationsMonitoringDashboard.Models;

namespace OperationsMonitoringDashboard.Services;

/// <summary>
/// Provides methods for retrieving monitoring data from a data source.
/// </summary>
public interface IMonitoringDataService
{
    /// <summary>
    /// Retrieves the latest monitored device list.
    /// </summary>
    Task<IReadOnlyList<DeviceModel>> GetDevicesAsync();

    /// <summary>
    /// Retrieves the latest active alert list.
    /// </summary>
    Task<IReadOnlyList<AlertModel>> GetAlertsAsync();
}
