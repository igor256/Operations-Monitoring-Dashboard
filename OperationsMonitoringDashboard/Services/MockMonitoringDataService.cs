using OperationsMonitoringDashboard.Models;

namespace OperationsMonitoringDashboard.Services;

/// <summary>
/// Simulates a monitoring backend service using delayed asynchronous responses.
/// </summary>
public class MockMonitoringDataService : IMonitoringDataService
{
    private readonly object _syncRoot = new();
    private readonly Random _random = new();
    private readonly List<DeviceModel> _devices;
    private readonly List<AlertModel> _alerts;

    /// <summary>
    /// Initializes mock data and internal state used for simulated real-time updates.
    /// </summary>
    public MockMonitoringDataService()
    {
        DateTime now = DateTime.UtcNow;

        _devices = new List<DeviceModel>
        {
            new() { Name = "Signal Node B7", Type = "Signal Node", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-1), Region = "North Plant", Details = "Primary signal hub for northern process line.", ResponseTimeMs = 58 },
            new() { Name = "Pump Unit 4", Type = "Hydraulic Pump", Status = DeviceStatus.Warning, LastUpdateTime = now.AddMinutes(-4), Region = "West Annex", Details = "Pressure fluctuation detected in stage-2 intake.", ResponseTimeMs = 143 },
            new() { Name = "Station Gateway A12", Type = "Gateway", Status = DeviceStatus.Online, LastUpdateTime = now.AddSeconds(-45), Region = "Core DC", Details = "Routes telemetry traffic from station cluster A.", ResponseTimeMs = 42 },
            new() { Name = "Cooling Loop C3", Type = "Cooling Controller", Status = DeviceStatus.Maintenance, LastUpdateTime = now.AddMinutes(-15), Region = "South Plant", Details = "Scheduled firmware update in progress.", ResponseTimeMs = 0 },
            new() { Name = "Boiler Sensor H9", Type = "Thermal Sensor", Status = DeviceStatus.Offline, LastUpdateTime = now.AddHours(-2), Region = "Heat Processing", Details = "No heartbeat since maintenance window close.", ResponseTimeMs = 0 },
            new() { Name = "Compressor Rack D2", Type = "Compressor", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-2), Region = "West Annex", Details = "Compressor rack operating within tolerance.", ResponseTimeMs = 67 },
            new() { Name = "Valve Cluster M5", Type = "Valve Controller", Status = DeviceStatus.Warning, LastUpdateTime = now.AddMinutes(-6), Region = "Mixing Line", Details = "Intermittent actuator lag on valve 3.", ResponseTimeMs = 156 },
            new() { Name = "Transit Relay T1", Type = "Relay", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-1), Region = "Transit Yard", Details = "Relay link stable with low packet loss.", ResponseTimeMs = 73 },
            new() { Name = "Dock Sensor P8", Type = "Proximity Sensor", Status = DeviceStatus.Offline, LastUpdateTime = now.AddHours(-1).AddMinutes(-22), Region = "Loading Dock", Details = "Power interruption suspected at dock 2.", ResponseTimeMs = 0 },
            new() { Name = "Generator Core G1", Type = "Generator Controller", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-3), Region = "Power House", Details = "Generator control loop synchronized.", ResponseTimeMs = 51 },
            new() { Name = "Filter Module F6", Type = "Filter Controller", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-2), Region = "Water Treatment", Details = "Filter pressure and flow within target range.", ResponseTimeMs = 62 },
            new() { Name = "Security Gateway S4", Type = "Security Appliance", Status = DeviceStatus.Warning, LastUpdateTime = now.AddMinutes(-7), Region = "Core DC", Details = "Elevated failed login attempts detected.", ResponseTimeMs = 134 },
            new() { Name = "Conveyor Node C11", Type = "Conveyor Controller", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-1), Region = "Packaging", Details = "Conveyor speed stable at nominal rate.", ResponseTimeMs = 49 },
            new() { Name = "Telemetry Bridge R9", Type = "Bridge Unit", Status = DeviceStatus.Maintenance, LastUpdateTime = now.AddMinutes(-19), Region = "Remote Site East", Details = "Bridge patch deployment in validation stage.", ResponseTimeMs = 0 },
            new() { Name = "Flow Meter L2", Type = "Flow Meter", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-2), Region = "Mixing Line", Details = "Flow rates normal for active blend lines.", ResponseTimeMs = 64 },
            new() { Name = "Vacuum Controller V3", Type = "Vacuum Controller", Status = DeviceStatus.Warning, LastUpdateTime = now.AddMinutes(-9), Region = "Packaging", Details = "Vacuum pressure drifting near threshold.", ResponseTimeMs = 148 },
            new() { Name = "Warehouse Beacon W7", Type = "Beacon", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-1), Region = "Warehouse", Details = "RF beacon synchronization healthy.", ResponseTimeMs = 77 },
            new() { Name = "Batch Processor B4", Type = "Batch Processor", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-3), Region = "Blend Control", Details = "Batch cycle complete without anomalies.", ResponseTimeMs = 59 },
            new() { Name = "Water Pump Z5", Type = "Pump", Status = DeviceStatus.Offline, LastUpdateTime = now.AddHours(-3).AddMinutes(-11), Region = "Water Treatment", Details = "Pump unreachable following breaker trip.", ResponseTimeMs = 0 },
            new() { Name = "Intake Gateway I6", Type = "Gateway", Status = DeviceStatus.Online, LastUpdateTime = now.AddMinutes(-1), Region = "North Plant", Details = "Intake gateway linked to all upstream nodes.", ResponseTimeMs = 55 }
        };

        _alerts = new List<AlertModel>
        {
            new() { Message = "Pressure variance above safe range", Severity = AlertSeverity.Critical, DeviceName = "Pump Unit 4", Timestamp = now.AddMinutes(-5) },
            new() { Message = "Device heartbeat missing", Severity = AlertSeverity.Critical, DeviceName = "Boiler Sensor H9", Timestamp = now.AddMinutes(-18) },
            new() { Message = "Firmware update initiated", Severity = AlertSeverity.Info, DeviceName = "Cooling Loop C3", Timestamp = now.AddMinutes(-22) },
            new() { Message = "Unauthorized login attempts spiking", Severity = AlertSeverity.Warning, DeviceName = "Security Gateway S4", Timestamp = now.AddMinutes(-15) },
            new() { Message = "Telemetry latency elevated", Severity = AlertSeverity.Warning, DeviceName = "Valve Cluster M5", Timestamp = now.AddMinutes(-12) },
            new() { Message = "Remote bridge patch validation", Severity = AlertSeverity.Info, DeviceName = "Telemetry Bridge R9", Timestamp = now.AddMinutes(-26) },
            new() { Message = "Dock sensor power cycle failed", Severity = AlertSeverity.Critical, DeviceName = "Dock Sensor P8", Timestamp = now.AddMinutes(-31) },
            new() { Message = "Vacuum pressure near limit", Severity = AlertSeverity.Warning, DeviceName = "Vacuum Controller V3", Timestamp = now.AddMinutes(-9) },
            new() { Message = "Maintenance window completed", Severity = AlertSeverity.Info, DeviceName = "Batch Processor B4", Timestamp = now.AddMinutes(-41) },
            new() { Message = "Breaker trip detected", Severity = AlertSeverity.Critical, DeviceName = "Water Pump Z5", Timestamp = now.AddMinutes(-62) },
            new() { Message = "Signal jitter resolved", Severity = AlertSeverity.Info, DeviceName = "Signal Node B7", Timestamp = now.AddMinutes(-75) },
            new() { Message = "Gateway packet retries increasing", Severity = AlertSeverity.Warning, DeviceName = "Intake Gateway I6", Timestamp = now.AddMinutes(-14) }
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeviceModel>> GetDevicesAsync(DeviceQueryOptions options)
    {
        await Task.Delay(900);

        lock (_syncRoot)
        {
            SimulateDeviceChanges();
            return QueryDevices(options).ToList();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AlertModel>> GetAlertsAsync(AlertQueryOptions options)
    {
        await Task.Delay(700);

        lock (_syncRoot)
        {
            SimulateAlertChanges();
            return QueryAlerts(options).ToList();
        }
    }

    /// <inheritdoc />
    public Task ClearAlertsAsync()
    {
        lock (_syncRoot)
        {
            _alerts.Clear();
        }

        return Task.CompletedTask;
    }

    private IEnumerable<DeviceModel> QueryDevices(DeviceQueryOptions options)
    {
        IEnumerable<DeviceModel> devices = _devices.Select(CloneDevice);

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            string searchText = options.SearchText.Trim();
            devices = devices.Where(d =>
                d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                d.Type.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                d.Region.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        if (options.StatusFilter.HasValue)
        {
            devices = devices.Where(d => d.Status == options.StatusFilter.Value);
        }

        return options.SortBy switch
        {
            DeviceSortOption.Name => devices.OrderBy(d => d.Name),
            DeviceSortOption.LastUpdate => devices.OrderByDescending(d => d.LastUpdateTime),
            _ => devices.OrderByDescending(d => d.LastUpdateTime)
        };
    }

    private IEnumerable<AlertModel> QueryAlerts(AlertQueryOptions options)
    {
        IEnumerable<AlertModel> alerts = _alerts.Select(CloneAlert);

        if (options.SeverityFilter.HasValue)
        {
            alerts = alerts.Where(a => a.Severity == options.SeverityFilter.Value);
        }

        return alerts.OrderByDescending(a => a.Timestamp);
    }

    private void SimulateDeviceChanges()
    {
        DateTime now = DateTime.UtcNow;

        foreach (var device in _devices)
        {
            if (_random.NextDouble() < 0.55)
            {
                device.LastUpdateTime = now.AddSeconds(-_random.Next(5, 150));
            }

            if (device.Status == DeviceStatus.Maintenance)
            {
                continue;
            }

            if (_random.NextDouble() < 0.2)
            {
                device.Status = _random.Next(0, 100) < 72 ? DeviceStatus.Online : DeviceStatus.Warning;
            }

            if (device.Status == DeviceStatus.Offline)
            {
                device.ResponseTimeMs = 0;
                continue;
            }

            int jitter = _random.Next(-18, 20);
            int baseline = Math.Max(device.ResponseTimeMs == 0 ? _random.Next(45, 95) : device.ResponseTimeMs, 35);
            device.ResponseTimeMs = Math.Clamp(baseline + jitter, 30, 220);
        }
    }

    private void SimulateAlertChanges()
    {
        if (_devices.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;

        if (_random.NextDouble() < 0.28)
        {
            DeviceModel sourceDevice = _devices[_random.Next(_devices.Count)];
            AlertSeverity severity = GetSeverityForStatus(sourceDevice.Status);

            _alerts.Add(new AlertModel
            {
                DeviceName = sourceDevice.Name,
                Severity = severity,
                Message = BuildSimulatedAlertMessage(sourceDevice, severity),
                Timestamp = now
            });
        }

        _alerts.RemoveAll(alert => alert.Timestamp < now.AddHours(-4));
    }

    private AlertSeverity GetSeverityForStatus(DeviceStatus status)
    {
        return status switch
        {
            DeviceStatus.Offline => AlertSeverity.Critical,
            DeviceStatus.Warning => AlertSeverity.Warning,
            DeviceStatus.Maintenance => AlertSeverity.Info,
            _ => _random.NextDouble() < 0.2 ? AlertSeverity.Warning : AlertSeverity.Info
        };
    }

    private static string BuildSimulatedAlertMessage(DeviceModel device, AlertSeverity severity)
    {
        return severity switch
        {
            AlertSeverity.Critical => $"Critical state detected for {device.Type}",
            AlertSeverity.Warning => $"Performance warning raised on {device.Type}",
            _ => $"Informational event reported by {device.Type}"
        };
    }

    private static DeviceModel CloneDevice(DeviceModel device)
    {
        return new DeviceModel
        {
            Name = device.Name,
            Type = device.Type,
            Status = device.Status,
            LastUpdateTime = device.LastUpdateTime,
            Region = device.Region,
            Details = device.Details,
            ResponseTimeMs = device.ResponseTimeMs
        };
    }

    private static AlertModel CloneAlert(AlertModel alert)
    {
        return new AlertModel
        {
            Message = alert.Message,
            Severity = alert.Severity,
            DeviceName = alert.DeviceName,
            Timestamp = alert.Timestamp
        };
    }
}
