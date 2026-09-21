using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class BluetoothScanResultCollectorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_WithInvalidIdentifier_IgnoresResult(string? identifier)
    {
        var collector = new BluetoothScanResultCollector();

        collector.Add(identifier, "Nome", "Alternativo", -50);

        Assert.Empty(collector.GetDevices());
    }

    [Theory]
    [InlineData("Anunciado", "Sistema", "Anunciado")]
    [InlineData(null, "Sistema", "Sistema")]
    [InlineData("", "", "Dispositivo sem nome")]
    public void Add_SelectsBestAvailableName(
        string? advertisedName,
        string? deviceName,
        string expectedName)
    {
        var collector = new BluetoothScanResultCollector();

        collector.Add("id", advertisedName, deviceName, -50);

        Assert.Equal(expectedName, Assert.Single(collector.GetDevices()).Name);
    }

    [Fact]
    public void Add_ForSameDevice_KeepsStrongestSignal()
    {
        var collector = new BluetoothScanResultCollector();

        collector.Add("id", "Primeiro", null, -40);
        collector.Add("id", "Mais fraco", null, -80);
        collector.Add("id", "Mais forte", null, -30);

        var device = Assert.Single(collector.GetDevices());
        Assert.Equal("Mais forte", device.Name);
        Assert.Equal(-30, device.SignalStrength);
    }

    [Fact]
    public void GetDevices_OrdersByStrongestSignal()
    {
        var collector = new BluetoothScanResultCollector();
        collector.Add("weak", "Fraco", null, -80);
        collector.Add("strong", "Forte", null, -30);

        var devices = collector.GetDevices();

        Assert.Equal(["strong", "weak"], devices.Select(device => device.Id));
    }
}
