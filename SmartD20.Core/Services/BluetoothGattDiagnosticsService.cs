using D20Mobile.Models;

namespace D20Mobile.Services;

public sealed class BluetoothGattDiagnosticsService : IGattDiagnosticsService
{
    private readonly IGattServiceDiscoveryTransport _transport;

    public BluetoothGattDiagnosticsService(IGattServiceDiscoveryTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    public Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
        CancellationToken cancellationToken = default)
    {
        return _transport.DiscoverServicesAsync(cancellationToken);
    }
}
