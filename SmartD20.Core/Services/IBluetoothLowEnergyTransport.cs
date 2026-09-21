using D20Mobile.Models;

namespace D20Mobile.Services;

public interface IBluetoothLowEnergyTransport
{
    Task<IReadOnlyList<WatchDevice>> ScanAsync(CancellationToken cancellationToken = default);

    Task ConnectAsync(string deviceId, CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
