using D20Mobile.Models;

namespace D20Mobile.Tests.Models;

public sealed class WatchDeviceTests
{
    [Theory]
    [InlineData(-48, "-48 dBm")]
    [InlineData(0, "0 dBm")]
    public void SignalText_FormatsSignalStrength(int signalStrength, string expected)
    {
        var device = new WatchDevice("id", "Relógio", signalStrength);

        Assert.Equal(expected, device.SignalText);
    }

    [Theory]
    [InlineData(true, "Dispositivo simulado")]
    [InlineData(false, "Dispositivo Bluetooth")]
    public void SourceText_DescribesDeviceOrigin(bool isSimulated, string expected)
    {
        var device = new WatchDevice("id", "Relógio", -50, isSimulated);

        Assert.Equal(expected, device.SourceText);
    }
}
