using OperationsMonitoringDashboard.Models;

namespace OperationsMonitoringDashboard.Services;

/// <summary>
/// Stores dashboard settings for the current application session.
/// </summary>
public class InMemoryAppSettingsService : IAppSettingsService
{
    public DeploymentEnvironment Environment { get; set; } = DeploymentEnvironment.Prod;

    public int AutoRefreshIntervalSeconds { get; set; } = 8;

    public bool IsAutoRefreshEnabled { get; set; } = true;

    public bool ShowOnlyCriticalAlerts { get; set; }

    public bool AreAlertNotificationsEnabled { get; set; } = true;

    public AppTheme Theme { get; set; } = AppTheme.Dark;

    public bool AreAnimationsEnabled { get; set; } = true;
}
