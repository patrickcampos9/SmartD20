namespace D20Mobile.Models;

public sealed record WatchDevice(
    string Id,
    string Name,
    int SignalStrength,
    bool IsSimulated = false)
{
    public string SignalText => $"{SignalStrength} dBm";

    public string SourceText => IsSimulated ? "Dispositivo simulado" : "Dispositivo Bluetooth";
}
