using System.Collections.ObjectModel;

namespace D20Mobile.Models;

public sealed class GattDiagnosticsScreenModel
{
    public ObservableCollection<GattServiceInfo> Services { get; } = [];

    public string DeviceName { get; set; } = "Dispositivo conectado";

    public string StatusMessage { get; set; } = "Pronto para descobrir serviços GATT.";

    public bool IsBusy { get; set; }
}
