using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using OperationsMonitoringDashboard.Commands;
using OperationsMonitoringDashboard.Models;
using OperationsMonitoringDashboard.Resources;
using OperationsMonitoringDashboard.Services;

namespace OperationsMonitoringDashboard.ViewModels;

/// <summary>
/// Main view model that powers the operations monitoring dashboard shell and content.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IMonitoringDataService _monitoringDataService;
    private readonly IAppSettingsService _settingsService;
    private readonly RelayCommand _refreshCommand;
    private readonly RelayCommand _clearAlertsCommand;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly DispatcherTimer _autoRefreshTimer;

    private readonly List<AlertModel> _allAlerts = new();

    private DeviceModel? _selectedDevice;
    private bool _isLoading;
    private string _selectedNavigationItem;
    private string _searchText = string.Empty;
    private SelectionOption<DeviceStatus?>? _selectedDeviceStatusFilter;
    private SelectionOption<AlertSeverity?>? _selectedAlertSeverityFilter;
    private SelectionOption<DeviceSortOption>? _selectedDeviceSortOption;
    private SelectionOption<DeploymentEnvironment>? _selectedEnvironmentOption;
    private int _autoRefreshIntervalSeconds;
    private bool _isAutoRefreshEnabled;
    private bool _showOnlyCriticalAlerts;
    private bool _areAlertNotificationsEnabled;
    private SelectionOption<AppTheme>? _selectedThemeOption;
    private bool _areAnimationsEnabled;

    /// <summary>
    /// Initializes the dashboard view model and starts the first data load.
    /// </summary>
    public MainViewModel()
    {
        _monitoringDataService = new MockMonitoringDataService();
        _settingsService = new InMemoryAppSettingsService();

        NavigationItems = new ObservableCollection<string>
        {
            ViewerTextResources.NavOverview,
            ViewerTextResources.NavDevices,
            ViewerTextResources.NavAlerts,
            ViewerTextResources.NavLogs,
            ViewerTextResources.NavSettings
        };

        DeviceStatusFilters = new ObservableCollection<SelectionOption<DeviceStatus?>>
        {
            new() { Label = ViewerTextResources.AllStatuses, Value = null },
            new() { Label = DeviceStatus.Online.ToString(), Value = DeviceStatus.Online },
            new() { Label = DeviceStatus.Warning.ToString(), Value = DeviceStatus.Warning },
            new() { Label = DeviceStatus.Offline.ToString(), Value = DeviceStatus.Offline },
            new() { Label = DeviceStatus.Maintenance.ToString(), Value = DeviceStatus.Maintenance }
        };

        AlertSeverityFilters = new ObservableCollection<SelectionOption<AlertSeverity?>>
        {
            new() { Label = ViewerTextResources.AllSeverities, Value = null },
            new() { Label = AlertSeverity.Info.ToString(), Value = AlertSeverity.Info },
            new() { Label = AlertSeverity.Warning.ToString(), Value = AlertSeverity.Warning },
            new() { Label = AlertSeverity.Critical.ToString(), Value = AlertSeverity.Critical }
        };

        DeviceSortOptions = new ObservableCollection<SelectionOption<DeviceSortOption>>
        {
            new() { Label = ViewerTextResources.SortByLastUpdate, Value = DeviceSortOption.LastUpdate },
            new() { Label = ViewerTextResources.SortByName, Value = DeviceSortOption.Name }
        };

        EnvironmentOptions = new ObservableCollection<SelectionOption<DeploymentEnvironment>>
        {
            new() { Label = "DEV", Value = DeploymentEnvironment.Dev },
            new() { Label = "TEST", Value = DeploymentEnvironment.Test },
            new() { Label = "PROD", Value = DeploymentEnvironment.Prod }
        };

        ThemeOptions = new ObservableCollection<SelectionOption<AppTheme>>
        {
            new() { Label = ViewerTextResources.ThemeDark, Value = AppTheme.Dark },
            new() { Label = ViewerTextResources.ThemeLight, Value = AppTheme.Light }
        };

        _selectedNavigationItem = ViewerTextResources.NavOverview;
        _selectedDeviceStatusFilter = DeviceStatusFilters.First();
        _selectedAlertSeverityFilter = AlertSeverityFilters.First();
        _selectedDeviceSortOption = DeviceSortOptions.First();

        SelectedEnvironmentOption = EnvironmentOptions.First(o => o.Value == _settingsService.Environment);
        _autoRefreshIntervalSeconds = _settingsService.AutoRefreshIntervalSeconds;
        _isAutoRefreshEnabled = _settingsService.IsAutoRefreshEnabled;
        _showOnlyCriticalAlerts = _settingsService.ShowOnlyCriticalAlerts;
        _areAlertNotificationsEnabled = _settingsService.AreAlertNotificationsEnabled;
        SelectedThemeOption = ThemeOptions.First(o => o.Value == _settingsService.Theme);
        _areAnimationsEnabled = _settingsService.AreAnimationsEnabled;

        Devices = new ObservableCollection<DeviceModel>();
        Alerts = new ObservableCollection<AlertModel>();

        _refreshCommand = new RelayCommand(async _ => await RefreshAsync(), _ => !IsLoading);
        _clearAlertsCommand = new RelayCommand(async _ => await ClearAlertsAsync(), _ => !IsLoading && Alerts.Count > 0);

        _autoRefreshTimer = new DispatcherTimer();
        _autoRefreshTimer.Tick += AutoRefreshTimerOnTick;

        ApplyTheme();
        ApplyAutoRefreshConfiguration();

        _ = RefreshAsync();
    }

    /// <summary>
    /// Gets the current page title shown in the top bar.
    /// </summary>
    public string Title => IsSettingsViewActive ? ViewerTextResources.SettingsHeader : ViewerTextResources.AppTitle;

    /// <summary>
    /// Gets the currently selected environment label.
    /// </summary>
    public string EnvironmentLabel => SelectedEnvironmentOption?.Label ?? "PROD";

    /// <summary>
    /// Gets the navigation items shown in the left sidebar.
    /// </summary>
    public ObservableCollection<string> NavigationItems { get; }

    /// <summary>
    /// Gets the available device status filter options.
    /// </summary>
    public ObservableCollection<SelectionOption<DeviceStatus?>> DeviceStatusFilters { get; }

    /// <summary>
    /// Gets the available alert severity filter options.
    /// </summary>
    public ObservableCollection<SelectionOption<AlertSeverity?>> AlertSeverityFilters { get; }

    /// <summary>
    /// Gets the available device sort options.
    /// </summary>
    public ObservableCollection<SelectionOption<DeviceSortOption>> DeviceSortOptions { get; }

    /// <summary>
    /// Gets the available environment settings options.
    /// </summary>
    public ObservableCollection<SelectionOption<DeploymentEnvironment>> EnvironmentOptions { get; }

    /// <summary>
    /// Gets the available theme settings options.
    /// </summary>
    public ObservableCollection<SelectionOption<AppTheme>> ThemeOptions { get; }

    /// <summary>
    /// Gets the collection of devices shown on the dashboard.
    /// </summary>
    public ObservableCollection<DeviceModel> Devices { get; }

    /// <summary>
    /// Gets the collection of alerts shown on the dashboard.
    /// </summary>
    public ObservableCollection<AlertModel> Alerts { get; }

    /// <summary>
    /// Gets the command used to refresh dashboard data.
    /// </summary>
    public ICommand RefreshCommand => _refreshCommand;

    /// <summary>
    /// Gets the command used to clear active alerts.
    /// </summary>
    public ICommand ClearAlertsCommand => _clearAlertsCommand;

    /// <summary>
    /// Gets or sets the currently selected navigation item.
    /// </summary>
    public string SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetProperty(ref _selectedNavigationItem, value))
            {
                OnPropertyChanged(nameof(IsSettingsViewActive));
                OnPropertyChanged(nameof(IsDashboardViewActive));
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    /// <summary>
    /// Gets whether the settings view is active.
    /// </summary>
    public bool IsSettingsViewActive => SelectedNavigationItem == ViewerTextResources.NavSettings;

    /// <summary>
    /// Gets whether the dashboard view is active.
    /// </summary>
    public bool IsDashboardViewActive => !IsSettingsViewActive;

    /// <summary>
    /// Gets or sets search text entered by the user.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _ = RefreshAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets selected device status filter.
    /// </summary>
    public SelectionOption<DeviceStatus?>? SelectedDeviceStatusFilter
    {
        get => _selectedDeviceStatusFilter;
        set
        {
            if (SetProperty(ref _selectedDeviceStatusFilter, value))
            {
                _ = RefreshAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets selected alert severity filter.
    /// </summary>
    public SelectionOption<AlertSeverity?>? SelectedAlertSeverityFilter
    {
        get => _selectedAlertSeverityFilter;
        set
        {
            if (SetProperty(ref _selectedAlertSeverityFilter, value))
            {
                _ = RefreshAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets selected device sort option.
    /// </summary>
    public SelectionOption<DeviceSortOption>? SelectedDeviceSortOption
    {
        get => _selectedDeviceSortOption;
        set
        {
            if (SetProperty(ref _selectedDeviceSortOption, value))
            {
                _ = RefreshAsync();
            }
        }
    }

    /// <summary>
    /// Gets or sets selected deployment environment option.
    /// </summary>
    public SelectionOption<DeploymentEnvironment>? SelectedEnvironmentOption
    {
        get => _selectedEnvironmentOption;
        set
        {
            if (SetProperty(ref _selectedEnvironmentOption, value) && value != null)
            {
                _settingsService.Environment = value.Value;
                OnPropertyChanged(nameof(EnvironmentLabel));
            }
        }
    }

    /// <summary>
    /// Gets or sets the auto refresh interval in seconds.
    /// </summary>
    public int AutoRefreshIntervalSeconds
    {
        get => _autoRefreshIntervalSeconds;
        set
        {
            int safeValue = Math.Clamp(value, 1, 300);

            if (SetProperty(ref _autoRefreshIntervalSeconds, safeValue))
            {
                _settingsService.AutoRefreshIntervalSeconds = safeValue;
                ApplyAutoRefreshConfiguration();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether automatic refreshing is enabled.
    /// </summary>
    public bool IsAutoRefreshEnabled
    {
        get => _isAutoRefreshEnabled;
        set
        {
            if (SetProperty(ref _isAutoRefreshEnabled, value))
            {
                _settingsService.IsAutoRefreshEnabled = value;
                ApplyAutoRefreshConfiguration();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether only critical alerts should be shown.
    /// </summary>
    public bool ShowOnlyCriticalAlerts
    {
        get => _showOnlyCriticalAlerts;
        set
        {
            if (SetProperty(ref _showOnlyCriticalAlerts, value))
            {
                _settingsService.ShowOnlyCriticalAlerts = value;
                ApplyAlertProjection();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether alert notifications are enabled.
    /// </summary>
    public bool AreAlertNotificationsEnabled
    {
        get => _areAlertNotificationsEnabled;
        set
        {
            if (SetProperty(ref _areAlertNotificationsEnabled, value))
            {
                _settingsService.AreAlertNotificationsEnabled = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets selected theme option.
    /// </summary>
    public SelectionOption<AppTheme>? SelectedThemeOption
    {
        get => _selectedThemeOption;
        set
        {
            if (SetProperty(ref _selectedThemeOption, value) && value != null)
            {
                _settingsService.Theme = value.Value;
                ApplyTheme();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether animations are enabled.
    /// </summary>
    public bool AreAnimationsEnabled
    {
        get => _areAnimationsEnabled;
        set
        {
            if (SetProperty(ref _areAnimationsEnabled, value))
            {
                _settingsService.AreAnimationsEnabled = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected device used for the details panel.
    /// </summary>
    public DeviceModel? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetProperty(ref _selectedDevice, value))
            {
                OnPropertyChanged(nameof(SelectedDeviceStatusText));
                OnPropertyChanged(nameof(SelectedDeviceLastUpdateText));
                OnPropertyChanged(nameof(SelectedDeviceResponseTimeText));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether data refresh is in progress.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                _refreshCommand.RaiseCanExecuteChanged();
                _clearAlertsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets the count of devices currently in the online state.
    /// </summary>
    public int ActiveDevicesCount => Devices.Count(d => d.Status == DeviceStatus.Online);

    /// <summary>
    /// Gets the count of devices currently in the offline state.
    /// </summary>
    public int OfflineDevicesCount => Devices.Count(d => d.Status == DeviceStatus.Offline);

    /// <summary>
    /// Gets the count of active critical alerts.
    /// </summary>
    public int CriticalAlertsCount => Alerts.Count(a => a.Severity == AlertSeverity.Critical);

    /// <summary>
    /// Gets the average response time for active devices.
    /// </summary>
    public double AverageResponseTimeMs
    {
        get
        {
            var activeResponseTimes = Devices
                .Where(d => d.ResponseTimeMs > 0)
                .Select(d => d.ResponseTimeMs)
                .ToList();

            if (activeResponseTimes.Count == 0)
            {
                return 0;
            }

            return Math.Round(activeResponseTimes.Average(), 1);
        }
    }

    /// <summary>
    /// Gets the text for the selected device status field.
    /// </summary>
    public string SelectedDeviceStatusText => SelectedDevice?.Status.ToString() ?? ViewerTextResources.NotAvailable;

    /// <summary>
    /// Gets the text for the selected device last update field.
    /// </summary>
    public string SelectedDeviceLastUpdateText => SelectedDevice?.LastUpdateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? ViewerTextResources.NotAvailable;

    /// <summary>
    /// Gets the text for the selected device response time field.
    /// </summary>
    public string SelectedDeviceResponseTimeText => SelectedDevice == null ? ViewerTextResources.NotAvailable : $"{SelectedDevice.ResponseTimeMs} ms";

    /// <summary>
    /// Refreshes device and alert data using the monitoring data service.
    /// </summary>
    public async Task RefreshAsync()
    {
        if (!await _refreshLock.WaitAsync(0))
        {
            return;
        }

        IsLoading = true;

        try
        {
            var deviceTask = _monitoringDataService.GetDevicesAsync(new DeviceQueryOptions
            {
                SearchText = SearchText,
                StatusFilter = SelectedDeviceStatusFilter?.Value,
                SortBy = SelectedDeviceSortOption?.Value ?? DeviceSortOption.LastUpdate
            });

            var alertTask = _monitoringDataService.GetAlertsAsync(new AlertQueryOptions
            {
                SeverityFilter = SelectedAlertSeverityFilter?.Value
            });

            await Task.WhenAll(deviceTask, alertTask);

            UpdateDevices(deviceTask.Result);
            UpdateAlerts(alertTask.Result);

            if (SelectedDevice != null)
            {
                SelectedDevice = Devices.FirstOrDefault(d => d.Name == SelectedDevice.Name) ?? Devices.FirstOrDefault();
            }
            else
            {
                SelectedDevice = Devices.FirstOrDefault();
            }

            OnPropertyChanged(nameof(ActiveDevicesCount));
            OnPropertyChanged(nameof(OfflineDevicesCount));
            OnPropertyChanged(nameof(CriticalAlertsCount));
            OnPropertyChanged(nameof(AverageResponseTimeMs));
        }
        finally
        {
            IsLoading = false;
            _refreshLock.Release();
        }
    }

    private async Task ClearAlertsAsync()
    {
        if (!await _refreshLock.WaitAsync(0))
        {
            return;
        }

        IsLoading = true;

        try
        {
            await _monitoringDataService.ClearAlertsAsync();
            UpdateAlerts(Array.Empty<AlertModel>());
            OnPropertyChanged(nameof(CriticalAlertsCount));
        }
        finally
        {
            IsLoading = false;
            _refreshLock.Release();
        }
    }

    private void ApplyAutoRefreshConfiguration()
    {
        _autoRefreshTimer.Interval = TimeSpan.FromSeconds(AutoRefreshIntervalSeconds);

        if (IsAutoRefreshEnabled)
        {
            _autoRefreshTimer.Start();
        }
        else
        {
            _autoRefreshTimer.Stop();
        }
    }

    private void ApplyTheme()
    {
        if (SelectedThemeOption == null)
        {
            return;
        }

        var palette = SelectedThemeOption.Value switch
        {
            AppTheme.Light => new ThemePalette(
                Color.FromRgb(0xEF, 0xF3, 0xF9),
                Color.FromRgb(0xE2, 0xE8, 0xF0),
                Color.FromRgb(0xF8, 0xFA, 0xFD),
                Color.FromRgb(0xFF, 0xFF, 0xFF),
                Color.FromRgb(0xD1, 0xDA, 0xE6),
                Color.FromRgb(0x1A, 0x23, 0x33),
                Color.FromRgb(0x4F, 0x5F, 0x78)),
            _ => new ThemePalette(
                Color.FromRgb(0x10, 0x14, 0x1B),
                Color.FromRgb(0x17, 0x1C, 0x24),
                Color.FromRgb(0x1A, 0x20, 0x2A),
                Color.FromRgb(0x20, 0x27, 0x34),
                Color.FromRgb(0x2A, 0x33, 0x44),
                Color.FromRgb(0xF3, 0xF6, 0xFB),
                Color.FromRgb(0xA6, 0xB1, 0xC2))
        };

        SetBrushColor("BackgroundBrush", palette.Background);
        SetBrushColor("SidebarBrush", palette.Sidebar);
        SetBrushColor("TopBarBrush", palette.TopBar);
        SetBrushColor("CardBrush", palette.Card);
        SetBrushColor("BorderBrush", palette.Border);
        SetBrushColor("TextPrimaryBrush", palette.TextPrimary);
        SetBrushColor("TextSecondaryBrush", palette.TextSecondary);
    }

    private static void SetBrushColor(string key, Color color)
    {
        if (Application.Current.Resources[key] is SolidColorBrush brush)
        {
            if (brush.IsFrozen)
            {
                Application.Current.Resources[key] = new SolidColorBrush(color);
                return;
            }

            brush.Color = color;
        }
    }

    private void AutoRefreshTimerOnTick(object? sender, EventArgs e)
    {
        _ = RefreshAsync();
    }

    private void UpdateDevices(IEnumerable<DeviceModel> updatedDevices)
    {
        Devices.Clear();

        foreach (var device in updatedDevices)
        {
            Devices.Add(device);
        }
    }

    private void UpdateAlerts(IEnumerable<AlertModel> updatedAlerts)
    {
        _allAlerts.Clear();
        _allAlerts.AddRange(updatedAlerts);
        ApplyAlertProjection();
    }

    private void ApplyAlertProjection()
    {
        IEnumerable<AlertModel> projectedAlerts = _allAlerts;

        if (ShowOnlyCriticalAlerts)
        {
            projectedAlerts = projectedAlerts.Where(a => a.Severity == AlertSeverity.Critical);
        }

        Alerts.Clear();

        foreach (var alert in projectedAlerts)
        {
            Alerts.Add(alert);
        }

        _clearAlertsCommand.RaiseCanExecuteChanged();
        OnPropertyChanged(nameof(CriticalAlertsCount));
    }

    private sealed record ThemePalette(
        Color Background,
        Color Sidebar,
        Color TopBar,
        Color Card,
        Color Border,
        Color TextPrimary,
        Color TextSecondary);
}
