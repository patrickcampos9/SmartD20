using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Controllers;

public sealed class HomeController(ID20ConnectionService connectionService)
{
    public HomeScreenModel Model { get; } = new();

    public async Task ScanAsync(CancellationToken cancellationToken = default)
    {
        Model.IsBusy = true;
        Model.StatusMessage = "Procurando dispositivos próximos...";

        try
        {
            var devices = await connectionService.ScanAsync(cancellationToken);

            Model.Devices.Clear();
            foreach (var device in devices)
            {
                Model.Devices.Add(device);
            }

            Model.SelectedDevice = null;
            Model.StatusMessage = $"{devices.Count} dispositivo(s) encontrado(s).";
        }
        catch (OperationCanceledException)
        {
            Model.StatusMessage = "Busca cancelada.";
        }
        catch (Exception exception)
        {
            Model.StatusMessage = $"Não foi possível buscar dispositivos: {exception.Message}";
        }
        finally
        {
            Model.IsBusy = false;
        }
    }

    public void SelectDevice(D20Device? device)
    {
        Model.SelectedDevice = device;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (Model.SelectedDevice is null)
        {
            Model.StatusMessage = "Selecione um dispositivo antes de conectar.";
            return;
        }

        Model.IsBusy = true;
        Model.StatusMessage = $"Conectando a {Model.SelectedDevice.Name}...";

        try
        {
            await connectionService.ConnectAsync(Model.SelectedDevice, cancellationToken);
            Model.ConnectedDevice = Model.SelectedDevice;
            Model.StatusMessage = $"Conectado a {Model.ConnectedDevice.Name}.";
        }
        catch (OperationCanceledException)
        {
            Model.StatusMessage = "Conexão cancelada.";
        }
        catch (Exception exception)
        {
            Model.StatusMessage = $"Não foi possível conectar: {exception.Message}";
        }
        finally
        {
            Model.IsBusy = false;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (Model.ConnectedDevice is null)
        {
            return;
        }

        Model.IsBusy = true;
        Model.StatusMessage = "Desconectando...";

        try
        {
            await connectionService.DisconnectAsync(cancellationToken);
            Model.ConnectedDevice = null;
            Model.StatusMessage = "Dispositivo desconectado.";
        }
        catch (OperationCanceledException)
        {
            Model.StatusMessage = "Desconexão cancelada.";
        }
        catch (Exception exception)
        {
            Model.StatusMessage = $"Não foi possível desconectar: {exception.Message}";
        }
        finally
        {
            Model.IsBusy = false;
        }
    }
}
