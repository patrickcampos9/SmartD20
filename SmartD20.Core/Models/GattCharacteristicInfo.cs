using D20Mobile.Services;

namespace D20Mobile.Models;

public sealed record GattCharacteristicInfo(
    string Uuid,
    string Name,
    GattCharacteristicProperties Properties)
{
    public string PropertyText => GattCharacteristicPropertyFormatter.Format(Properties);
}
