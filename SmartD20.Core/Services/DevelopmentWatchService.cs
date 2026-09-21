using D20Mobile.Models;

namespace D20Mobile.Services;

public sealed class DevelopmentWatchService : IWatchConnectionService, IGattDiagnosticsService
{
    private readonly IWatchConnectionService _bluetoothConnection;
    private readonly IWatchConnectionService _simulatedConnection;
    private readonly IGattDiagnosticsService _bluetoothDiagnostics;
    private readonly IGattDiagnosticsService _simulatedDiagnostics;
    private IWatchConnectionService? _activeConnection;
    private IGattDiagnosticsService? _activeDiagnostics;

    public DevelopmentWatchService(
        IWatchConnectionService bluetoothConnection,
        IWatchConnectionService simulatedConnection,
        IGattDiagnosticsService bluetoothDiagnostics,
        IGattDiagnosticsService simulatedDiagnostics)
    {
        _bluetoothConnection = bluetoothConnection
            ?? throw new ArgumentNullException(nameof(bluetoothConnection));
        _simulatedConnection = simulatedConnection
            ?? throw new ArgumentNullException(nameof(simulatedConnection));
        _bluetoothDiagnostics = bluetoothDiagnostics
            ?? throw new ArgumentNullException(nameof(bluetoothDiagnostics));
        _simulatedDiagnostics = simulatedDiagnostics
            ?? throw new ArgumentNullException(nameof(simulatedDiagnostics));
    }

    public async Task<IReadOnlyList<WatchDevice>> ScanAsync(
        CancellationToken cancellationToken = default)
    {
        var bluetoothDevices = await _bluetoothConnection.ScanAsync(cancellationToken);
        var simulatedDevices = await _simulatedConnection.ScanAsync(cancellationToken);
        return bluetoothDevices.Concat(simulatedDevices).ToArray();
    }

    public async Task ConnectAsync(
        WatchDevice device,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(device);
        var connection = device.IsSimulated ? _simulatedConnection : _bluetoothConnection;
        var diagnostics = device.IsSimulated ? _simulatedDiagnostics : _bluetoothDiagnostics;

        await connection.ConnectAsync(device, cancellationToken);
        _activeConnection = connection;
        _activeDiagnostics = diagnostics;
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_activeConnection is null)
        {
            return;
        }

        await _activeConnection.DisconnectAsync(cancellationToken);
        _activeConnection = null;
        _activeDiagnostics = null;
    }

    public Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_activeDiagnostics is null)
        {
            throw new InvalidOperationException(
                "Conecte um dispositivo antes de descobrir os serviços GATT.");
        }

        return _activeDiagnostics.DiscoverServicesAsync(cancellationToken);
    }
}
