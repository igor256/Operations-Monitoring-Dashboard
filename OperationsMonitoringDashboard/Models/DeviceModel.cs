namespace OperationsMonitoringDashboard.Models;

/// <summary>
/// Represents a monitored edge or infrastructure device.
/// </summary>
public class DeviceModel
{
    /// <summary>
    /// Gets or sets the device display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the functional device type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current device runtime status.
    /// </summary>
    public DeviceStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the latest telemetry update timestamp.
    /// </summary>
    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the geographic or logical region identifier.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional details shown in the device details panel.
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latest response time for the device in milliseconds.
    /// </summary>
    public int ResponseTimeMs { get; set; }
}
