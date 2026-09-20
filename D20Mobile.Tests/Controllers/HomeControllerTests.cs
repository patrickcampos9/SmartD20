using D20Mobile.Controllers;
using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Tests.Controllers;

public sealed class HomeControllerTests
{
    private static readonly WatchDevice Device = new("watch-1", "Relógio", -42);

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new HomeController(null!));
    }

    [Fact]
    public async Task ScanAsync_WhenSuccessful_ReplacesDevicesAndClearsSelection()
    {
        var service = new StubWatchConnectionService { ScanResult = [Device] };
        var controller = new HomeController(service);
        controller.Model.Devices.Add(new WatchDevice("old", "Antigo", -90));
        controller.SelectDevice(controller.Model.Devices[0]);

        await controller.ScanAsync();

        Assert.Equal([Device], controller.Model.Devices);
        Assert.Null(controller.Model.SelectedDevice);
        Assert.False(controller.Model.IsBusy);
        Assert.Equal("1 dispositivo(s) encontrado(s).", controller.Model.StatusMessage);
    }

    [Fact]
    public async Task ScanAsync_WhenCanceled_ShowsCanceledStatus()
    {
        var service = new StubWatchConnectionService
        {
            ScanException = new OperationCanceledException()
        };
        var controller = new HomeController(service);

        await controller.ScanAsync();

        Assert.False(controller.Model.IsBusy);
        Assert.Equal("Busca cancelada.", controller.Model.StatusMessage);
    }

    [Fact]
    public async Task ScanAsync_WhenServiceFails_ShowsFriendlyError()
    {
        var service = new StubWatchConnectionService
        {
            ScanException = new InvalidOperationException("Bluetooth desligado")
        };
        var controller = new HomeController(service);

        await controller.ScanAsync();

        Assert.False(controller.Model.IsBusy);
        Assert.Equal(
            "Não foi possível buscar dispositivos: Bluetooth desligado",
            controller.Model.StatusMessage);
    }

    [Fact]
    public async Task ScanAsync_WhileBusy_DoesNotStartAnotherScan()
    {
        var service = new StubWatchConnectionService();
        var controller = new HomeController(service);
        controller.Model.IsBusy = true;

        await controller.ScanAsync();

        Assert.Equal(0, service.ScanCallCount);
    }

    [Fact]
    public void SelectDevice_WhenIdle_SelectsDevice()
    {
        var controller = new HomeController(new StubWatchConnectionService());

        controller.SelectDevice(Device);

        Assert.Same(Device, controller.Model.SelectedDevice);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void SelectDevice_WhenInteractionIsBlocked_KeepsSelection(bool isBusy, bool isConnected)
    {
        var controller = new HomeController(new StubWatchConnectionService());
        controller.SelectDevice(Device);
        controller.Model.IsBusy = isBusy;
        controller.Model.ConnectedDevice = isConnected ? Device : null;

        controller.SelectDevice(new WatchDevice("watch-2", "Outro", -55));

        Assert.Same(Device, controller.Model.SelectedDevice);
    }

    [Fact]
    public async Task ConnectAsync_WithoutSelection_ShowsInstruction()
    {
        var service = new StubWatchConnectionService();
        var controller = new HomeController(service);

        await controller.ConnectAsync();

        Assert.Equal(0, service.ConnectCallCount);
        Assert.Equal("Selecione um dispositivo antes de conectar.", controller.Model.StatusMessage);
    }

    [Fact]
    public async Task ConnectAsync_WhenSuccessful_SetsConnectedDevice()
    {
        var service = new StubWatchConnectionService();
        var controller = new HomeController(service);
        controller.SelectDevice(Device);

        await controller.ConnectAsync();

        Assert.Same(Device, service.LastConnectedDevice);
        Assert.Same(Device, controller.Model.ConnectedDevice);
        Assert.False(controller.Model.IsBusy);
        Assert.Equal("Conectado a Relógio.", controller.Model.StatusMessage);
    }

    [Theory]
    [InlineData(true, "Conexão cancelada.")]
    [InlineData(false, "Não foi possível conectar: Falha")]
    public async Task ConnectAsync_WhenServiceDoesNotComplete_ShowsExpectedStatus(
        bool canceled,
        string expectedStatus)
    {
        var service = new StubWatchConnectionService
        {
            ConnectException = canceled
                ? new OperationCanceledException()
                : new InvalidOperationException("Falha")
        };
        var controller = new HomeController(service);
        controller.SelectDevice(Device);

        await controller.ConnectAsync();

        Assert.Null(controller.Model.ConnectedDevice);
        Assert.False(controller.Model.IsBusy);
        Assert.Equal(expectedStatus, controller.Model.StatusMessage);
    }

    [Fact]
    public async Task ConnectAsync_WhileBusy_DoesNotConnect()
    {
        var service = new StubWatchConnectionService();
        var controller = new HomeController(service);
        controller.SelectDevice(Device);
        controller.Model.IsBusy = true;

        await controller.ConnectAsync();

        Assert.Equal(0, service.ConnectCallCount);
    }

    [Fact]
    public async Task DisconnectAsync_WithoutConnection_DoesNothing()
    {
        var service = new StubWatchConnectionService();
        var controller = new HomeController(service);

        await controller.DisconnectAsync();

        Assert.Equal(0, service.DisconnectCallCount);
    }

    [Fact]
    public async Task DisconnectAsync_WhenSuccessful_ClearsConnectedDevice()
    {
        var service = new StubWatchConnectionService();
        var controller = await CreateConnectedControllerAsync(service);

        await controller.DisconnectAsync();

        Assert.Equal(1, service.DisconnectCallCount);
        Assert.Null(controller.Model.ConnectedDevice);
        Assert.False(controller.Model.IsBusy);
        Assert.Equal("Dispositivo desconectado.", controller.Model.StatusMessage);
    }

    [Theory]
    [InlineData(true, "Desconexão cancelada.")]
    [InlineData(false, "Não foi possível desconectar: Falha")]
    public async Task DisconnectAsync_WhenServiceDoesNotComplete_KeepsConnection(
        bool canceled,
        string expectedStatus)
    {
        var service = new StubWatchConnectionService();
        var controller = await CreateConnectedControllerAsync(service);
        service.DisconnectException = canceled
            ? new OperationCanceledException()
            : new InvalidOperationException("Falha");

        await controller.DisconnectAsync();

        Assert.Same(Device, controller.Model.ConnectedDevice);
        Assert.False(controller.Model.IsBusy);
        Assert.Equal(expectedStatus, controller.Model.StatusMessage);
    }

    [Fact]
    public async Task DisconnectAsync_WhileBusy_DoesNotDisconnect()
    {
        var service = new StubWatchConnectionService();
        var controller = await CreateConnectedControllerAsync(service);
        controller.Model.IsBusy = true;

        await controller.DisconnectAsync();

        Assert.Equal(0, service.DisconnectCallCount);
    }

    private static async Task<HomeController> CreateConnectedControllerAsync(
        StubWatchConnectionService service)
    {
        var controller = new HomeController(service);
        controller.SelectDevice(Device);
        await controller.ConnectAsync();
        service.ConnectCallCount = 0;
        return controller;
    }

    private sealed class StubWatchConnectionService : IWatchConnectionService
    {
        public IReadOnlyList<WatchDevice> ScanResult { get; init; } = [];

        public Exception? ScanException { get; init; }

        public Exception? ConnectException { get; init; }

        public Exception? DisconnectException { get; set; }

        public int ScanCallCount { get; private set; }

        public int ConnectCallCount { get; set; }

        public int DisconnectCallCount { get; private set; }

        public WatchDevice? LastConnectedDevice { get; private set; }

        public Task<IReadOnlyList<WatchDevice>> ScanAsync(CancellationToken cancellationToken = default)
        {
            ScanCallCount++;
            return ScanException is null
                ? Task.FromResult(ScanResult)
                : Task.FromException<IReadOnlyList<WatchDevice>>(ScanException);
        }

        public Task ConnectAsync(WatchDevice device, CancellationToken cancellationToken = default)
        {
            ConnectCallCount++;
            LastConnectedDevice = device;
            return ConnectException is null
                ? Task.CompletedTask
                : Task.FromException(ConnectException);
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            DisconnectCallCount++;
            return DisconnectException is null
                ? Task.CompletedTask
                : Task.FromException(DisconnectException);
        }
    }
}
