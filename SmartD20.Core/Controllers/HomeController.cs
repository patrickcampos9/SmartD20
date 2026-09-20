using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Controllers;

public sealed class HomeController
{
    private readonly IWatchConnectionService _connectionService;

    public HomeController(IWatchConnectionService connectionService)
    {
        _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
    }

    public HomeScreenModel Model { get; } = new();

    public async Task ScanAsync(CancellationToken cancellationToken = default)
    {
        if (Model.IsBusy)
        {
            return;
        }

        await ExecuteAsync(
            "Procurando dispositivos próximos...",
            "Busca cancelada.",
            "Não foi possível buscar dispositivos",
            async () =>
            {
                var devices = await _connectionService.ScanAsync(cancellationToken);

                Model.Devices.Clear();
                foreach (var device in devices)
                {
                    Model.Devices.Add(device);
                }

                Model.SelectedDevice = null;
                Model.StatusMessage = $"{devices.Count} dispositivo(s) encontrado(s).";
            });
    }

    public void SelectDevice(WatchDevice? device)
    {
        if (!Model.IsBusy && Model.ConnectedDevice is null)
        {
            Model.SelectedDevice = device;
        }
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (Model.IsBusy)
        {
            return;
        }

        if (Model.SelectedDevice is null)
        {
            Model.StatusMessage = "Selecione um dispositivo antes de conectar.";
            return;
        }

        var selectedDevice = Model.SelectedDevice;

        await ExecuteAsync(
            $"Conectando a {selectedDevice.Name}...",
            "Conexão cancelada.",
            "Não foi possível conectar",
            async () =>
            {
                await _connectionService.ConnectAsync(selectedDevice, cancellationToken);
                Model.ConnectedDevice = selectedDevice;
                Model.StatusMessage = $"Conectado a {selectedDevice.Name}.";
            });
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (Model.IsBusy)
        {
            return;
        }

        if (Model.ConnectedDevice is null)
        {
            return;
        }

        await ExecuteAsync(
            "Desconectando...",
            "Desconexão cancelada.",
            "Não foi possível desconectar",
            async () =>
            {
                await _connectionService.DisconnectAsync(cancellationToken);
                Model.ConnectedDevice = null;
                Model.StatusMessage = "Dispositivo desconectado.";
            });
    }

    private async Task ExecuteAsync(
        string busyMessage,
        string canceledMessage,
        string errorPrefix,
        Func<Task> operation)
    {
        Model.IsBusy = true;
        Model.StatusMessage = busyMessage;

        try
        {
            await operation();
        }
        catch (OperationCanceledException)
        {
            Model.StatusMessage = canceledMessage;
        }
        catch (Exception exception)
        {
            Model.StatusMessage = $"{errorPrefix}: {exception.Message}";
        }
        finally
        {
            Model.IsBusy = false;
        }
    }
}
