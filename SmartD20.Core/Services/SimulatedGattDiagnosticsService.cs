using D20Mobile.Models;

namespace D20Mobile.Services;

public sealed class SimulatedGattDiagnosticsService : IGattDiagnosticsService
{
    public async Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(350, cancellationToken);

        return
        [
            new GattServiceInfo(
                "0000180f-0000-1000-8000-00805f9b34fb",
                "Bateria",
                true,
                [
                    new GattCharacteristicInfo(
                        "00002a19-0000-1000-8000-00805f9b34fb",
                        "Nível da bateria",
                        GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify)
                ]),
            new GattServiceInfo(
                "0000180a-0000-1000-8000-00805f9b34fb",
                "Informações do dispositivo",
                true,
                [
                    new GattCharacteristicInfo(
                        "00002a26-0000-1000-8000-00805f9b34fb",
                        "Versão do firmware",
                        GattCharacteristicProperties.Read)
                ])
        ];
    }
}
