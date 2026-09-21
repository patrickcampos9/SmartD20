using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class BluetoothWatchConnectionServiceTests
{
    private static readonly WatchDevice Device = new("device-id", "Relógio", -45);

    [Fact]
    public void Constructor_WithNullTransport_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BluetoothWatchConnectionService(null!));
    }

    [Fact]
    public async Task ScanAsync_ForwardsResultAndCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        IReadOnlyList<WatchDevice> expected = [Device];
        var transport = new RecordingBluetoothTransport { ScanResult = expected };
        var service = new BluetoothWatchConnectionService(transport);

        var result = await service.ScanAsync(cancellation.Token);

        Assert.Same(expected, result);
        Assert.Equal(cancellation.Token, transport.LastCancellationToken);
    }

    [Fact]
    public async Task ConnectAsync_ForwardsIdentifierAndCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        var transport = new RecordingBluetoothTransport();
        var service = new BluetoothWatchConnectionService(transport);

        await service.ConnectAsync(Device, cancellation.Token);

        Assert.Equal(Device.Id, transport.ConnectedDeviceId);
        Assert.Equal(cancellation.Token, transport.LastCancellationToken);
    }

    [Fact]
    public async Task ConnectAsync_WithNullDevice_ThrowsArgumentNullException()
    {
        var service = new BluetoothWatchConnectionService(new RecordingBluetoothTransport());

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.ConnectAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ConnectAsync_WithInvalidIdentifier_ThrowsArgumentException(string identifier)
    {
        var service = new BluetoothWatchConnectionService(new RecordingBluetoothTransport());
        var device = new WatchDevice(identifier, "Relógio", -45);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ConnectAsync(device));
    }

    [Fact]
    public async Task DisconnectAsync_ForwardsCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        var transport = new RecordingBluetoothTransport();
        var service = new BluetoothWatchConnectionService(transport);

        await service.DisconnectAsync(cancellation.Token);

        Assert.True(transport.DisconnectCalled);
        Assert.Equal(cancellation.Token, transport.LastCancellationToken);
    }

    private sealed class RecordingBluetoothTransport : IBluetoothLowEnergyTransport
    {
        public IReadOnlyList<WatchDevice> ScanResult { get; init; } = [];

        public string? ConnectedDeviceId { get; private set; }

        public bool DisconnectCalled { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<IReadOnlyList<WatchDevice>> ScanAsync(CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(ScanResult);
        }

        public Task ConnectAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            ConnectedDeviceId = deviceId;
            LastCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            DisconnectCalled = true;
            LastCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
