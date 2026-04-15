namespace OperationsMonitoringDashboard.Models;

/// <summary>
/// Represents a system alert tied to a monitored device.
/// </summary>
public class AlertModel
{
    /// <summary>
    /// Gets or sets the alert message text.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alert severity level.
    /// </summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the name of the impacted device.
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alert creation timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
