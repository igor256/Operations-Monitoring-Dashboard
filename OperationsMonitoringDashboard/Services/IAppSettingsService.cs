using OperationsMonitoringDashboard.Models;

namespace OperationsMonitoringDashboard.Services;

/// <summary>
/// Provides in-memory application settings shared across view models and services.
/// </summary>
public interface IAppSettingsService
{
    DeploymentEnvironment Environment { get; set; }

    int AutoRefreshIntervalSeconds { get; set; }

    bool IsAutoRefreshEnabled { get; set; }

    bool ShowOnlyCriticalAlerts { get; set; }

    bool AreAlertNotificationsEnabled { get; set; }

    AppTheme Theme { get; set; }

    bool AreAnimationsEnabled { get; set; }
}
