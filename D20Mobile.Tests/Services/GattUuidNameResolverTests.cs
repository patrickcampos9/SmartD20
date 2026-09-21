using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class GattUuidNameResolverTests
{
    [Theory]
    [InlineData("180F", "Bateria")]
    [InlineData("0000180a-0000-1000-8000-00805f9b34fb", "Informações do dispositivo")]
    [InlineData("6e400001-b5a3-f393-e0a9-e50e24dcca9e", "Serviço desconhecido")]
    public void GetServiceName_ResolvesKnownAndCustomUuids(string uuid, string expected)
    {
        Assert.Equal(expected, GattUuidNameResolver.GetServiceName(uuid));
    }

    [Theory]
    [InlineData("2A19", "Nível da bateria")]
    [InlineData("00002a26-0000-1000-8000-00805f9b34fb", "Versão do firmware")]
    [InlineData("6e400002-b5a3-f393-e0a9-e50e24dcca9e", "Característica desconhecida")]
    public void GetCharacteristicName_ResolvesKnownAndCustomUuids(string uuid, string expected)
    {
        Assert.Equal(expected, GattUuidNameResolver.GetCharacteristicName(uuid));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetServiceName_WithInvalidUuid_ThrowsArgumentException(string? uuid)
    {
        Assert.ThrowsAny<ArgumentException>(() => GattUuidNameResolver.GetServiceName(uuid!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetCharacteristicName_WithInvalidUuid_ThrowsArgumentException(string? uuid)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => GattUuidNameResolver.GetCharacteristicName(uuid!));
    }
}
