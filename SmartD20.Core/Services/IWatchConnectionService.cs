using D20Mobile.Models;

namespace D20Mobile.Services;

public interface IWatchConnectionService
{
    Task<IReadOnlyList<WatchDevice>> ScanAsync(CancellationToken cancellationToken = default);

    Task ConnectAsync(WatchDevice device, CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
