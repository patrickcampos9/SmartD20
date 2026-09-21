using D20Mobile.Models;

namespace D20Mobile.Services;

public interface IGattServiceDiscoveryTransport
{
    Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
        CancellationToken cancellationToken = default);
}
