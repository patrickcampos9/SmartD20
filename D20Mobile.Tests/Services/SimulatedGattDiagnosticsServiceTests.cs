using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class SimulatedGattDiagnosticsServiceTests
{
    private readonly SimulatedGattDiagnosticsService _service = new();

    [Fact]
    public async Task DiscoverServicesAsync_ReturnsRepresentativeGattTree()
    {
        var services = await _service.DiscoverServicesAsync();

        Assert.Collection(
            services,
            service => Assert.Equal("Bateria", service.Name),
            service => Assert.Equal("Informações do dispositivo", service.Name));
        Assert.All(services, service => Assert.NotEmpty(service.Characteristics));
    }

    [Fact]
    public async Task DiscoverServicesAsync_WhenCanceled_ThrowsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.DiscoverServicesAsync(cancellation.Token));
    }
}
