using D20Mobile.Models;

namespace D20Mobile.Services;

public sealed class SimulatedD20ConnectionService : ID20ConnectionService
{
    public async Task<IReadOnlyList<D20Device>> ScanAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(900, cancellationToken);

        return
        [
            new D20Device("SIM-D20-001", "D20 de demonstração", -48, true),
            new D20Device("SIM-BLE-002", "Pulseira BLE de teste", -71, true)
        ];
    }

    public Task ConnectAsync(
        D20Device device,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Delay(650, cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Delay(250, cancellationToken);
    }
}
