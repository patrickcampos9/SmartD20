using D20Mobile.Controllers;
using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Tests.Controllers;

public sealed class GattDiagnosticsControllerTests
{
    private static readonly GattServiceInfo Service = new("uuid", "Serviço", true, []);

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GattDiagnosticsController(null!));
    }

    [Theory]
    [InlineData("D20", "D20")]
    [InlineData(null, "Dispositivo conectado")]
    [InlineData("   ", "Dispositivo conectado")]
    public void SetDeviceName_UsesNameOrFallback(string? deviceName, string expected)
    {
        var controller = new GattDiagnosticsController(new StubDiagnosticsService());

        controller.SetDeviceName(deviceName);

        Assert.Equal(expected, controller.Model.DeviceName);
    }

    [Fact]
    public async Task RefreshAsync_WhenSuccessful_ReplacesServices()
    {
        var diagnostics = new StubDiagnosticsService { Result = [Service] };
        var controller = new GattDiagnosticsController(diagnostics);
        controller.Model.Services.Add(new GattServiceInfo("old", "Antigo", true, []));

        await controller.RefreshAsync();

        Assert.Equal([Service], controller.Model.Services);
        Assert.Equal("1 serviço(s) GATT encontrado(s).", controller.Model.StatusMessage);
        Assert.False(controller.Model.IsBusy);
    }

    [Fact]
    public async Task RefreshAsync_WithNoServices_ShowsEmptyStatus()
    {
        var controller = new GattDiagnosticsController(new StubDiagnosticsService());

        await controller.RefreshAsync();

        Assert.Empty(controller.Model.Services);
        Assert.Equal("Nenhum serviço GATT foi encontrado.", controller.Model.StatusMessage);
    }

    [Theory]
    [InlineData(true, "Descoberta de serviços cancelada.")]
    [InlineData(false, "Não foi possível descobrir os serviços: Falha")]
    public async Task RefreshAsync_WhenDiscoveryDoesNotComplete_ShowsExpectedStatus(
        bool canceled,
        string expected)
    {
        var diagnostics = new StubDiagnosticsService
        {
            Exception = canceled
                ? new OperationCanceledException()
                : new InvalidOperationException("Falha")
        };
        var controller = new GattDiagnosticsController(diagnostics);

        await controller.RefreshAsync();

        Assert.Equal(expected, controller.Model.StatusMessage);
        Assert.False(controller.Model.IsBusy);
    }

    [Fact]
    public async Task RefreshAsync_WhileBusy_DoesNotStartAnotherDiscovery()
    {
        var diagnostics = new StubDiagnosticsService();
        var controller = new GattDiagnosticsController(diagnostics);
        controller.Model.IsBusy = true;

        await controller.RefreshAsync();

        Assert.Equal(0, diagnostics.CallCount);
    }

    private sealed class StubDiagnosticsService : IGattDiagnosticsService
    {
        public IReadOnlyList<GattServiceInfo> Result { get; init; } = [];

        public Exception? Exception { get; init; }

        public int CallCount { get; private set; }

        public Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Exception is null
                ? Task.FromResult(Result)
                : Task.FromException<IReadOnlyList<GattServiceInfo>>(Exception);
        }
    }
}
