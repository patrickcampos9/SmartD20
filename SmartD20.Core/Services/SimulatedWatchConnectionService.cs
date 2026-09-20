using D20Mobile.Models;

namespace D20Mobile.Services;

public sealed class SimulatedWatchConnectionService : IWatchConnectionService
{
    public async Task<IReadOnlyList<WatchDevice>> ScanAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(900, cancellationToken);

        return
        [
            new WatchDevice("SIM-D20-001", "D20 de demonstração", -48, true),
            new WatchDevice("SIM-BLE-002", "Pulseira BLE de teste", -71, true)
        ];
    }

    public Task ConnectAsync(
        WatchDevice device,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(device);
        return Task.Delay(650, cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.Delay(250, cancellationToken);
    }
}
