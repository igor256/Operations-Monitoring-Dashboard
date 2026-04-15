using System.Collections.ObjectModel;
using System.Windows.Input;
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
    private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromSeconds(8);

    private readonly IMonitoringDataService _monitoringDataService;
    private readonly RelayCommand _refreshCommand;
    private readonly RelayCommand _clearAlertsCommand;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly DispatcherTimer _autoRefreshTimer;

    private DeviceModel? _selectedDevice;
    private bool _isLoading;
    private string _selectedNavigationItem;
    private string _searchText = string.Empty;
    private SelectionOption<DeviceStatus?>? _selectedDeviceStatusFilter;
    private SelectionOption<AlertSeverity?>? _selectedAlertSeverityFilter;
    private SelectionOption<DeviceSortOption>? _selectedDeviceSortOption;

    /// <summary>
    /// Initializes the dashboard view model and starts the first data load.
    /// </summary>
    public MainViewModel()
    {
        _monitoringDataService = new MockMonitoringDataService();

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

        _selectedNavigationItem = ViewerTextResources.NavOverview;
        _selectedDeviceStatusFilter = DeviceStatusFilters.First();
        _selectedAlertSeverityFilter = AlertSeverityFilters.First();
        _selectedDeviceSortOption = DeviceSortOptions.First();

        Devices = new ObservableCollection<DeviceModel>();
        Alerts = new ObservableCollection<AlertModel>();

        _refreshCommand = new RelayCommand(async _ => await RefreshAsync(), _ => !IsLoading);
        _clearAlertsCommand = new RelayCommand(async _ => await ClearAlertsAsync(), _ => !IsLoading && Alerts.Count > 0);

        _autoRefreshTimer = new DispatcherTimer
        {
            Interval = AutoRefreshInterval
        };

        _autoRefreshTimer.Tick += AutoRefreshTimerOnTick;
        _autoRefreshTimer.Start();

        _ = RefreshAsync();
    }

    /// <summary>
    /// Gets the application title text.
    /// </summary>
    public string Title => ViewerTextResources.AppTitle;

    /// <summary>
    /// Gets the current deployment environment label.
    /// </summary>
    public string EnvironmentLabel => ViewerTextResources.EnvironmentProd;

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
        set => SetProperty(ref _selectedNavigationItem, value);
    }

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
        Alerts.Clear();

        foreach (var alert in updatedAlerts)
        {
            Alerts.Add(alert);
        }

        _clearAlertsCommand.RaiseCanExecuteChanged();
    }
}
