namespace OperationsMonitoringDashboard.Models;

/// <summary>
/// Carries filter options used to query alerts.
/// </summary>
public class AlertQueryOptions
{
    /// <summary>
    /// Gets or sets optional severity filter. Null means all severities.
    /// </summary>
    public AlertSeverity? SeverityFilter { get; set; }
}
