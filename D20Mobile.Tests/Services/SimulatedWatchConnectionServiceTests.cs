using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class SimulatedWatchConnectionServiceTests
{
    private readonly SimulatedWatchConnectionService _service = new();

    [Fact]
    public async Task ScanAsync_ReturnsSimulatedDevices()
    {
        var devices = await _service.ScanAsync();

        Assert.Collection(
            devices,
            device =>
            {
                Assert.Equal("SIM-D20-001", device.Id);
                Assert.True(device.IsSimulated);
            },
            device =>
            {
                Assert.Equal("SIM-BLE-002", device.Id);
                Assert.True(device.IsSimulated);
            });
    }

    [Fact]
    public async Task ScanAsync_WhenCanceled_ThrowsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.ScanAsync(cancellation.Token));
    }

    [Fact]
    public async Task ConnectAsync_WithDevice_Completes()
    {
        var device = new WatchDevice("id", "Relógio", -50);

        await _service.ConnectAsync(device);
    }

    [Fact]
    public async Task ConnectAsync_WithNullDevice_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.ConnectAsync(null!));
    }

    [Fact]
    public async Task ConnectAsync_WhenCanceled_ThrowsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.ConnectAsync(new WatchDevice("id", "Relógio", -50), cancellation.Token));
    }

    [Fact]
    public async Task DisconnectAsync_Completes()
    {
        await _service.DisconnectAsync();
    }

    [Fact]
    public async Task DisconnectAsync_WhenCanceled_ThrowsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.DisconnectAsync(cancellation.Token));
    }
}
