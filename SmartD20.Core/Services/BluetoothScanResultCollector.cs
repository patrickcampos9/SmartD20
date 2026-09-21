using System.Collections.Concurrent;
using D20Mobile.Models;

namespace D20Mobile.Services;

public sealed class BluetoothScanResultCollector
{
    private readonly ConcurrentDictionary<string, WatchDevice> _devices = new();

    public void Add(
        string? identifier,
        string? advertisedName,
        string? deviceName,
        int signalStrength)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return;
        }

        var name = !string.IsNullOrWhiteSpace(advertisedName)
            ? advertisedName
            : deviceName;
        var device = new WatchDevice(
            identifier,
            string.IsNullOrWhiteSpace(name) ? "Dispositivo sem nome" : name,
            signalStrength);

        _devices.AddOrUpdate(
            identifier,
            device,
            (_, current) => device.SignalStrength > current.SignalStrength ? device : current);
    }

    public IReadOnlyList<WatchDevice> GetDevices()
    {
        return _devices.Values
            .OrderByDescending(device => device.SignalStrength)
            .ToArray();
    }
}
