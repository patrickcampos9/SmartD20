using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class DevelopmentWatchServiceTests
{
    private static readonly WatchDevice BluetoothDevice = new("ble", "Bluetooth", -40);
    private static readonly WatchDevice SimulatedDevice = new("sim", "Simulado", -50, true);

    [Fact]
    public void Constructor_WithNullDependency_ThrowsArgumentNullException()
    {
        var connection = new RecordingConnectionService();
        var diagnostics = new RecordingDiagnosticsService();

        Assert.Throws<ArgumentNullException>(
            () => new DevelopmentWatchService(null!, connection, diagnostics, diagnostics));
        Assert.Throws<ArgumentNullException>(
            () => new DevelopmentWatchService(connection, null!, diagnostics, diagnostics));
        Assert.Throws<ArgumentNullException>(
            () => new DevelopmentWatchService(connection, connection, null!, diagnostics));
        Assert.Throws<ArgumentNullException>(
            () => new DevelopmentWatchService(connection, connection, diagnostics, null!));
    }

    [Fact]
    public async Task ScanAsync_CombinesBluetoothAndSimulatedDevices()
    {
        using var cancellation = new CancellationTokenSource();
        var bluetooth = new RecordingConnectionService { ScanResult = [BluetoothDevice] };
        var simulated = new RecordingConnectionService { ScanResult = [SimulatedDevice] };
        var service = CreateService(bluetooth, simulated);

        var devices = await service.ScanAsync(cancellation.Token);

        Assert.Equal([BluetoothDevice, SimulatedDevice], devices);
        Assert.Equal(cancellation.Token, bluetooth.CancellationToken);
        Assert.Equal(cancellation.Token, simulated.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConnectAndDiscover_RouteToSelectedDeviceType(bool simulatedDevice)
    {
        using var cancellation = new CancellationTokenSource();
        var bluetooth = new RecordingConnectionService();
        var simulated = new RecordingConnectionService();
        var bluetoothDiagnostics = new RecordingDiagnosticsService();
        var simulatedDiagnostics = new RecordingDiagnosticsService();
        var service = new DevelopmentWatchService(
            bluetooth,
            simulated,
            bluetoothDiagnostics,
            simulatedDiagnostics);
        var device = simulatedDevice ? SimulatedDevice : BluetoothDevice;

        await service.ConnectAsync(device, cancellation.Token);
        await service.DiscoverServicesAsync(cancellation.Token);

        Assert.Equal(simulatedDevice ? 0 : 1, bluetooth.ConnectCallCount);
        Assert.Equal(simulatedDevice ? 1 : 0, simulated.ConnectCallCount);
        Assert.Equal(simulatedDevice ? 0 : 1, bluetoothDiagnostics.CallCount);
        Assert.Equal(simulatedDevice ? 1 : 0, simulatedDiagnostics.CallCount);
    }

    [Fact]
    public async Task ConnectAsync_WithNullDevice_ThrowsArgumentNullException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.ConnectAsync(null!));
    }

    [Fact]
    public async Task DisconnectAsync_WithoutConnection_DoesNothing()
    {
        var bluetooth = new RecordingConnectionService();
        var simulated = new RecordingConnectionService();
        var service = CreateService(bluetooth, simulated);

        await service.DisconnectAsync();

        Assert.Equal(0, bluetooth.DisconnectCallCount);
        Assert.Equal(0, simulated.DisconnectCallCount);
    }

    [Fact]
    public async Task DisconnectAsync_AfterConnection_RoutesAndClearsSelection()
    {
        var bluetooth = new RecordingConnectionService();
        var service = CreateService(bluetooth);
        await service.ConnectAsync(BluetoothDevice);

        await service.DisconnectAsync();

        Assert.Equal(1, bluetooth.DisconnectCallCount);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DiscoverServicesAsync());
    }

    [Fact]
    public async Task DiscoverServicesAsync_WithoutConnection_ThrowsInvalidOperationException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DiscoverServicesAsync());
    }

    private static DevelopmentWatchService CreateService(
        RecordingConnectionService? bluetooth = null,
        RecordingConnectionService? simulated = null)
    {
        return new DevelopmentWatchService(
            bluetooth ?? new RecordingConnectionService(),
            simulated ?? new RecordingConnectionService(),
            new RecordingDiagnosticsService(),
            new RecordingDiagnosticsService());
    }

    private sealed class RecordingConnectionService : IWatchConnectionService
    {
        public IReadOnlyList<WatchDevice> ScanResult { get; init; } = [];

        public int ConnectCallCount { get; private set; }

        public int DisconnectCallCount { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<IReadOnlyList<WatchDevice>> ScanAsync(CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
            return Task.FromResult(ScanResult);
        }

        public Task ConnectAsync(WatchDevice device, CancellationToken cancellationToken = default)
        {
            ConnectCallCount++;
            CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            DisconnectCallCount++;
            CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingDiagnosticsService : IGattDiagnosticsService
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult<IReadOnlyList<GattServiceInfo>>([]);
        }
    }
}
