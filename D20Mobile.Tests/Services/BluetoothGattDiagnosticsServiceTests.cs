using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class BluetoothGattDiagnosticsServiceTests
{
    [Fact]
    public void Constructor_WithNullTransport_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BluetoothGattDiagnosticsService(null!));
    }

    [Fact]
    public async Task DiscoverServicesAsync_ForwardsResultAndCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        IReadOnlyList<GattServiceInfo> expected =
            [new GattServiceInfo("uuid", "Serviço", true, [])];
        var transport = new RecordingDiscoveryTransport { Result = expected };
        var service = new BluetoothGattDiagnosticsService(transport);

        var result = await service.DiscoverServicesAsync(cancellation.Token);

        Assert.Same(expected, result);
        Assert.Equal(cancellation.Token, transport.CancellationToken);
    }

    private sealed class RecordingDiscoveryTransport : IGattServiceDiscoveryTransport
    {
        public IReadOnlyList<GattServiceInfo> Result { get; init; } = [];

        public CancellationToken CancellationToken { get; private set; }

        public Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
            CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
            return Task.FromResult(Result);
        }
    }
}
