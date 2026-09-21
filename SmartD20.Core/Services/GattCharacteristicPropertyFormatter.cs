using D20Mobile.Models;

namespace D20Mobile.Services;

public static class GattCharacteristicPropertyFormatter
{
    private static readonly (GattCharacteristicProperties Property, string Label)[] Labels =
    [
        (GattCharacteristicProperties.Read, "READ"),
        (GattCharacteristicProperties.Write, "WRITE"),
        (GattCharacteristicProperties.WriteWithoutResponse, "WRITE NO RESPONSE"),
        (GattCharacteristicProperties.Notify, "NOTIFY"),
        (GattCharacteristicProperties.Indicate, "INDICATE"),
        (GattCharacteristicProperties.Broadcast, "BROADCAST"),
        (GattCharacteristicProperties.AuthenticatedSignedWrites, "SIGNED WRITE"),
        (GattCharacteristicProperties.ExtendedProperties, "EXTENDED")
    ];

    public static string Format(GattCharacteristicProperties properties)
    {
        var labels = Labels
            .Where(item => properties.HasFlag(item.Property))
            .Select(item => item.Label)
            .ToArray();

        return labels.Length == 0 ? "SEM PROPRIEDADES" : string.Join(" · ", labels);
    }
}
