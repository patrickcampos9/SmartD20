using D20Mobile.Models;
using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class GattCharacteristicPropertyFormatterTests
{
    [Fact]
    public void Format_WithNoProperties_ReturnsFallback()
    {
        var result = GattCharacteristicPropertyFormatter.Format(GattCharacteristicProperties.None);

        Assert.Equal("SEM PROPRIEDADES", result);
    }

    [Fact]
    public void Format_WithAllProperties_ReturnsLabelsInStableOrder()
    {
        const GattCharacteristicProperties properties =
            GattCharacteristicProperties.Read |
            GattCharacteristicProperties.Write |
            GattCharacteristicProperties.WriteWithoutResponse |
            GattCharacteristicProperties.Notify |
            GattCharacteristicProperties.Indicate |
            GattCharacteristicProperties.Broadcast |
            GattCharacteristicProperties.AuthenticatedSignedWrites |
            GattCharacteristicProperties.ExtendedProperties;

        var result = GattCharacteristicPropertyFormatter.Format(properties);

        Assert.Equal(
            "READ · WRITE · WRITE NO RESPONSE · NOTIFY · INDICATE · BROADCAST · SIGNED WRITE · EXTENDED",
            result);
    }
}
