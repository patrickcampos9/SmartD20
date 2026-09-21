using D20Mobile.Models;

namespace D20Mobile.Services;

public interface IGattDiagnosticsService
{
    Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
        CancellationToken cancellationToken = default);
}
