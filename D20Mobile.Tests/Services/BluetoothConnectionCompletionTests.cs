using D20Mobile.Services;

namespace D20Mobile.Tests.Services;

public sealed class BluetoothConnectionCompletionTests
{
    [Fact]
    public async Task ReportConnected_CompletesConnection()
    {
        var completion = new BluetoothConnectionCompletion();

        completion.ReportConnected();

        await completion.Connected;
        Assert.False(completion.Disconnected.IsCompleted);
    }

    [Fact]
    public async Task ReportConnectionFailure_FailsConnectionAndCompletesDisconnection()
    {
        var completion = new BluetoothConnectionCompletion();
        var expected = new InvalidOperationException("Falha");

        completion.ReportConnectionFailure(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await completion.Connected);
        Assert.Same(expected, actual);
        await completion.Disconnected;
    }

    [Fact]
    public void ReportConnectionFailure_WithNullException_ThrowsArgumentNullException()
    {
        var completion = new BluetoothConnectionCompletion();

        Assert.Throws<ArgumentNullException>(() => completion.ReportConnectionFailure(null!));
    }

    [Fact]
    public async Task ReportDisconnected_BeforeConnection_FailsConnection()
    {
        var completion = new BluetoothConnectionCompletion();

        completion.ReportDisconnected();

        await completion.Disconnected;
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await completion.Connected);
        Assert.Equal(
            "O dispositivo encerrou a conexão antes de concluí-la.",
            exception.Message);
    }

    [Fact]
    public async Task ReportDisconnected_AfterConnection_PreservesSuccessfulConnection()
    {
        var completion = new BluetoothConnectionCompletion();
        completion.ReportConnected();

        completion.ReportDisconnected();

        await completion.Connected;
        await completion.Disconnected;
    }
}
