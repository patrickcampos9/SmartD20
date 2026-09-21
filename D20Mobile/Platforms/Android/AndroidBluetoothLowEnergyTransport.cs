using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using D20Mobile.Models;
using D20Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace D20Mobile.Platforms.Android;

public sealed class AndroidBluetoothLowEnergyTransport : IBluetoothLowEnergyTransport
{
    private static readonly TimeSpan ScanDuration = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan DisconnectionTimeout = TimeSpan.FromSeconds(5);

    private readonly Context _context = global::Android.App.Application.Context;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private BluetoothGatt? _connectedGatt;
    private GattConnectionCallback? _connectionCallback;

    public async Task<IReadOnlyList<WatchDevice>> ScanAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsurePermissionAsync();
        var adapter = GetEnabledAdapter();
        var scanner = adapter.BluetoothLeScanner
            ?? throw new InvalidOperationException("A busca Bluetooth não está disponível neste aparelho.");
        using var callback = new DeviceScanCallback();

        scanner.StartScan(callback);

        try
        {
            try
            {
                await callback.Failure.WaitAsync(ScanDuration, cancellationToken);
            }
            catch (TimeoutException)
            {
                return callback.GetDevices();
            }

            return callback.GetDevices();
        }
        finally
        {
            scanner.StopScan(callback);
        }
    }

    public async Task ConnectAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        await EnsurePermissionAsync();
        await _operationLock.WaitAsync(cancellationToken);

        try
        {
            if (_connectedGatt is not null)
            {
                throw new InvalidOperationException("Já existe um dispositivo Bluetooth conectado.");
            }

            var adapter = GetEnabledAdapter();
            var device = adapter.GetRemoteDevice(deviceId)
                ?? throw new InvalidOperationException("O dispositivo Bluetooth não foi encontrado.");
            var callback = new GattConnectionCallback();
            var gatt = device.ConnectGatt(_context, false, callback)
                ?? throw new InvalidOperationException("Não foi possível iniciar a conexão Bluetooth.");

            _connectionCallback = callback;
            _connectedGatt = gatt;

            try
            {
                await callback.Connected.WaitAsync(ConnectionTimeout, cancellationToken);
            }
            catch (TimeoutException exception)
            {
                CloseConnection();
                throw new TimeoutException("O dispositivo não respondeu à tentativa de conexão.", exception);
            }
            catch
            {
                CloseConnection();
                throw;
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await EnsurePermissionAsync();
        await _operationLock.WaitAsync(cancellationToken);

        try
        {
            if (_connectedGatt is null || _connectionCallback is null)
            {
                return;
            }

            _connectedGatt.Disconnect();

            try
            {
                var disconnected = _connectionCallback.Disconnected;
                var timeout = Task.Delay(DisconnectionTimeout, cancellationToken);
                var completed = await Task.WhenAny(disconnected, timeout);

                if (completed == timeout)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                CloseConnection();
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }

    private static async Task EnsurePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Bluetooth>();
        }

        if (status != PermissionStatus.Granted)
        {
            throw new UnauthorizedAccessException(
                "A permissão para encontrar e conectar dispositivos próximos foi negada.");
        }
    }

    private BluetoothAdapter GetEnabledAdapter()
    {
        if (!_context.PackageManager!.HasSystemFeature(PackageManager.FeatureBluetoothLe))
        {
            throw new NotSupportedException("Este aparelho não oferece suporte a Bluetooth Low Energy.");
        }

        var manager = _context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var adapter = manager?.Adapter
            ?? throw new NotSupportedException("O Bluetooth não está disponível neste aparelho.");

        if (!adapter.IsEnabled)
        {
            throw new InvalidOperationException("O Bluetooth está desligado.");
        }

        return adapter;
    }

    private void CloseConnection()
    {
        _connectedGatt?.Close();
        _connectedGatt?.Dispose();
        _connectionCallback?.Dispose();
        _connectedGatt = null;
        _connectionCallback = null;
    }

    private sealed class DeviceScanCallback : ScanCallback
    {
        private readonly BluetoothScanResultCollector _collector = new();
        private readonly TaskCompletionSource _failure = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Failure => _failure.Task;

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult? result)
        {
            if (result is not null)
            {
                AddOrUpdate(result);
            }
        }

        public override void OnBatchScanResults(IList<ScanResult>? results)
        {
            if (results is null)
            {
                return;
            }

            foreach (var result in results)
            {
                AddOrUpdate(result);
            }
        }

        public override void OnScanFailed(ScanFailure errorCode)
        {
            _failure.TrySetException(
                new InvalidOperationException($"A busca Bluetooth falhou ({errorCode})."));
        }

        public IReadOnlyList<WatchDevice> GetDevices()
        {
            return _collector.GetDevices();
        }

        private void AddOrUpdate(ScanResult result)
        {
            _collector.Add(
                result.Device?.Address,
                result.ScanRecord?.DeviceName,
                result.Device?.Name,
                result.Rssi);
        }
    }

    private sealed class GattConnectionCallback : BluetoothGattCallback
    {
        private readonly BluetoothConnectionCompletion _completion = new();

        public Task Connected => _completion.Connected;

        public Task Disconnected => _completion.Disconnected;

        public override void OnConnectionStateChange(
            BluetoothGatt? gatt,
            GattStatus status,
            ProfileState newState)
        {
            if (status != GattStatus.Success)
            {
                _completion.ReportConnectionFailure(
                    new InvalidOperationException(
                        $"O dispositivo recusou a conexão Bluetooth ({status})."));
                return;
            }

            if (newState == ProfileState.Connected)
            {
                _completion.ReportConnected();
                return;
            }

            if (newState == ProfileState.Disconnected)
            {
                _completion.ReportDisconnected();
            }
        }
    }
}
