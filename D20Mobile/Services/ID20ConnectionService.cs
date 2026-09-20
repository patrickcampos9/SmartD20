using D20Mobile.Models;

namespace D20Mobile.Services;

public interface ID20ConnectionService
{
    Task<IReadOnlyList<D20Device>> ScanAsync(CancellationToken cancellationToken = default);

    Task ConnectAsync(D20Device device, CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
