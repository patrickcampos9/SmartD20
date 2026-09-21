using D20Mobile.Models;

namespace D20Mobile.Services;

public sealed class BluetoothWatchConnectionService : IWatchConnectionService
{
    private readonly IBluetoothLowEnergyTransport _transport;

    public BluetoothWatchConnectionService(IBluetoothLowEnergyTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    public Task<IReadOnlyList<WatchDevice>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return _transport.ScanAsync(cancellationToken);
    }

    public Task ConnectAsync(
        WatchDevice device,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(device);

        if (string.IsNullOrWhiteSpace(device.Id))
        {
            throw new ArgumentException("O dispositivo não possui um identificador válido.", nameof(device));
        }

        return _transport.ConnectAsync(device.Id, cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return _transport.DisconnectAsync(cancellationToken);
    }
}
