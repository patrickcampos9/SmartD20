namespace D20Mobile.Services;

public sealed class BluetoothConnectionCompletion
{
    private readonly TaskCompletionSource _connected = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _disconnected = new(
        TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Connected => _connected.Task;

    public Task Disconnected => _disconnected.Task;

    public void ReportConnected()
    {
        _connected.TrySetResult();
    }

    public void ReportConnectionFailure(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        _disconnected.TrySetResult();
        _connected.TrySetException(exception);
    }

    public void ReportDisconnected()
    {
        _disconnected.TrySetResult();

        if (!_connected.Task.IsCompleted)
        {
            _connected.TrySetException(
                new InvalidOperationException(
                    "O dispositivo encerrou a conexão antes de concluí-la."));
        }
    }
}
