using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Controllers;

public sealed class GattDiagnosticsController
{
    private readonly IGattDiagnosticsService _diagnosticsService;

    public GattDiagnosticsController(IGattDiagnosticsService diagnosticsService)
    {
        _diagnosticsService = diagnosticsService
            ?? throw new ArgumentNullException(nameof(diagnosticsService));
    }

    public GattDiagnosticsScreenModel Model { get; } = new();

    public void SetDeviceName(string? deviceName)
    {
        Model.DeviceName = string.IsNullOrWhiteSpace(deviceName)
            ? "Dispositivo conectado"
            : deviceName;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (Model.IsBusy)
        {
            return;
        }

        Model.IsBusy = true;
        Model.StatusMessage = "Descobrindo serviços e características...";

        try
        {
            var services = await _diagnosticsService.DiscoverServicesAsync(cancellationToken);
            Model.Services.Clear();

            foreach (var service in services)
            {
                Model.Services.Add(service);
            }

            Model.StatusMessage = services.Count == 0
                ? "Nenhum serviço GATT foi encontrado."
                : $"{services.Count} serviço(s) GATT encontrado(s).";
        }
        catch (OperationCanceledException)
        {
            Model.StatusMessage = "Descoberta de serviços cancelada.";
        }
        catch (Exception exception)
        {
            Model.StatusMessage = $"Não foi possível descobrir os serviços: {exception.Message}";
        }
        finally
        {
            Model.IsBusy = false;
        }
    }
}
