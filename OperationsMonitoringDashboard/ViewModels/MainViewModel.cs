using System.Collections.ObjectModel;
using System.Windows.Input;
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
    private readonly RelayCommand _refreshCommand;

    private DeviceModel? _selectedDevice;
    private bool _isLoading;
    private string _selectedNavigationItem;
    private string _searchText = string.Empty;

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

        _selectedNavigationItem = ViewerTextResources.NavOverview;

        Devices = new ObservableCollection<DeviceModel>();
        Alerts = new ObservableCollection<AlertModel>();

        _refreshCommand = new RelayCommand(async _ => await RefreshAsync(), _ => !IsLoading);

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
        set => SetProperty(ref _searchText, value);
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
        IsLoading = true;

        try
        {
            var devices = await _monitoringDataService.GetDevicesAsync();
            var alerts = await _monitoringDataService.GetAlertsAsync();

            Devices.Clear();
            foreach (var device in devices)
            {
                Devices.Add(device);
            }

            Alerts.Clear();
            foreach (var alert in alerts.OrderByDescending(a => a.Timestamp))
            {
                Alerts.Add(alert);
            }

            SelectedDevice = Devices.FirstOrDefault();

            OnPropertyChanged(nameof(ActiveDevicesCount));
            OnPropertyChanged(nameof(OfflineDevicesCount));
            OnPropertyChanged(nameof(CriticalAlertsCount));
            OnPropertyChanged(nameof(AverageResponseTimeMs));
        }
        finally
        {
            IsLoading = false;
        }
    }
}
