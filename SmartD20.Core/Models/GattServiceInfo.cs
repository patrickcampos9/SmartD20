namespace D20Mobile.Models;

public sealed record GattServiceInfo(
    string Uuid,
    string Name,
    bool IsPrimary,
    IReadOnlyList<GattCharacteristicInfo> Characteristics)
{
    public string TypeText => IsPrimary ? "Serviço primário" : "Serviço secundário";
}
