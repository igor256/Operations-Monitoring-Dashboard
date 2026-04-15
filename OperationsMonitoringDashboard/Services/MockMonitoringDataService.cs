using OperationsMonitoringDashboard.Models;

namespace OperationsMonitoringDashboard.Services;

/// <summary>
/// Simulates a monitoring backend service using delayed asynchronous responses.
/// </summary>
public class MockMonitoringDataService : IMonitoringDataService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<DeviceModel>> GetDevicesAsync()
    {
        await Task.Delay(900);

        DateTime now = DateTime.UtcNow;

        return new List<DeviceModel>
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
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AlertModel>> GetAlertsAsync()
    {
        await Task.Delay(700);

        DateTime now = DateTime.UtcNow;

        return new List<AlertModel>
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
}
