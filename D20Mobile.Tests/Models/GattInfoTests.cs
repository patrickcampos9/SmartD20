using D20Mobile.Models;

namespace D20Mobile.Tests.Models;

public sealed class GattInfoTests
{
    [Fact]
    public void CharacteristicPropertyText_FormatsProperties()
    {
        var characteristic = new GattCharacteristicInfo(
            "uuid",
            "Nome",
            GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify);

        Assert.Equal("uuid", characteristic.Uuid);
        Assert.Equal("Nome", characteristic.Name);
        Assert.Equal("READ · NOTIFY", characteristic.PropertyText);
    }

    [Theory]
    [InlineData(true, "Serviço primário")]
    [InlineData(false, "Serviço secundário")]
    public void ServiceTypeText_DescribesServiceType(bool isPrimary, string expected)
    {
        var service = new GattServiceInfo("uuid", "Nome", isPrimary, []);

        Assert.Equal("uuid", service.Uuid);
        Assert.Equal(expected, service.TypeText);
    }
}
