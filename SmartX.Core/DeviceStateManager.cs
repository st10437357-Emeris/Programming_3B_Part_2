using System;
using System.Collections.Generic;

namespace SmartX.Core.Management
{
    // Mock classes for context
    public class DeviceProfile { public string MacAddress { get; set; } public string Category { get; set; } }
    public class TelemetryLog { public DateTime Timestamp { get; set; } public string Payload { get; set; } }

    public class DeviceStateManager
    {
        // 1. Dictionaries (Dictionary<TKey, TValue>)
        // Stores live, registered devices using the MAC address as the key for instantaneous O(1) lookups.
        private readonly Dictionary<string, DeviceProfile> _registeredDevices;

        // 2. Sorted Dictionaries
        // Organises historical sensor logs sequentially by timestamp for efficient frontend timeline rendering.
        private readonly SortedDictionary<DateTime, TelemetryLog> _historicalLogs;

        // 3. Hash Sets (HashSet<T>)
        // Maintains a list of currently disconnected nodes to prevent processing duplicate disconnect alerts.
        private readonly HashSet<string> _activeErrorStates;

        public DeviceStateManager()
        {
            _registeredDevices = new Dictionary<string, DeviceProfile>();
            _historicalLogs = new SortedDictionary<DateTime, TelemetryLog>();

            // Using StringComparer prevents casing issues with MAC addresses
            _activeErrorStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a new device for O(1) lookups.
        /// </summary>
        public void RegisterDevice(DeviceProfile device)
        {
            if (_registeredDevices.TryAdd(device.MacAddress, device))
            {
                Console.WriteLine($"Device {device.MacAddress} registered successfully.");
            }
        }

        /// <summary>
        /// Retrieves a device instantly O(1) when a telemetry packet arrives.
        /// </summary>
        public DeviceProfile GetDevice(string macAddress)
        {
            _registeredDevices.TryGetValue(macAddress, out var device);
            return device; // Returns null if not found
        }

        /// <summary>
        /// Logs telemetry data sequentially by timestamp.
        /// </summary>
        public void AddHistoricalLog(DateTime timestamp, TelemetryLog log)
        {
            // TryAdd ensures it doesn't crash if two logs hit the exact same millisecond
            _historicalLogs.TryAdd(timestamp, log);
        }

        /// <summary>
        /// Flags a device as disconnected, ensuring duplicates are ignored.
        /// </summary>
        public void ReportDeviceDisconnect(string macAddress)
        {
            // HashSet.Add returns false if the item already exists, naturally preventing duplicate alerts
            if (_activeErrorStates.Add(macAddress))
            {
                Console.WriteLine($"ALERT: Device {macAddress} has disconnected. Triggering system warning.");
                // Logic to trigger the actual API warning goes here
            }
            else
            {
                Console.WriteLine($"Device {macAddress} is already in an error state. Ignoring duplicate alert.");
            }
        }

        /// <summary>
        /// Clears the error state when a device reconnects.
        /// </summary>
        public void ResolveDeviceError(string macAddress)
        {
            if (_activeErrorStates.Remove(macAddress))
            {
                Console.WriteLine($"Device {macAddress} error resolved. System restored.");
            }
        }
    }
}