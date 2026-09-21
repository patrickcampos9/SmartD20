using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using D20Mobile.Models;
using D20Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace D20Mobile.Platforms.Android;

public sealed class AndroidBluetoothLowEnergyTransport :
    IBluetoothLowEnergyTransport,
    IGattServiceDiscoveryTransport
{
    private static readonly TimeSpan ScanDuration = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan DisconnectionTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ServiceDiscoveryTimeout = TimeSpan.FromSeconds(12);

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

    public async Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsurePermissionAsync();
        await _operationLock.WaitAsync(cancellationToken);

        try
        {
            if (_connectedGatt is null || _connectionCallback is null)
            {
                throw new InvalidOperationException(
                    "Conecte um dispositivo antes de descobrir os serviços GATT.");
            }

            var discovery = _connectionCallback.BeginServiceDiscovery();

            if (!_connectedGatt.DiscoverServices())
            {
                _connectionCallback.ReportServiceDiscoveryFailure(
                    new InvalidOperationException(
                        "O dispositivo não aceitou a descoberta de serviços GATT."));
            }

            try
            {
                return await discovery.WaitAsync(ServiceDiscoveryTimeout, cancellationToken);
            }
            catch (TimeoutException exception)
            {
                _connectionCallback.AbandonServiceDiscovery();
                throw new TimeoutException(
                    "O dispositivo não respondeu à descoberta de serviços GATT.",
                    exception);
            }
            catch (OperationCanceledException)
            {
                _connectionCallback.AbandonServiceDiscovery();
                throw;
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
        private readonly object _discoveryLock = new();
        private TaskCompletionSource<IReadOnlyList<GattServiceInfo>>? _serviceDiscovery;

        public Task Connected => _completion.Connected;

        public Task Disconnected => _completion.Disconnected;

        public Task<IReadOnlyList<GattServiceInfo>> BeginServiceDiscovery()
        {
            lock (_discoveryLock)
            {
                if (_serviceDiscovery is { Task.IsCompleted: false })
                {
                    throw new InvalidOperationException(
                        "Já existe uma descoberta de serviços GATT em andamento.");
                }

                _serviceDiscovery = new TaskCompletionSource<IReadOnlyList<GattServiceInfo>>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                return _serviceDiscovery.Task;
            }
        }

        public void ReportServiceDiscoveryFailure(Exception exception)
        {
            lock (_discoveryLock)
            {
                _serviceDiscovery?.TrySetException(exception);
            }
        }

        public void AbandonServiceDiscovery()
        {
            lock (_discoveryLock)
            {
                _serviceDiscovery = null;
            }
        }

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
                ReportServiceDiscoveryFailure(
                    new InvalidOperationException(
                        "O dispositivo desconectou durante a descoberta de serviços."));
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt? gatt, GattStatus status)
        {
            TaskCompletionSource<IReadOnlyList<GattServiceInfo>>? discovery;

            lock (_discoveryLock)
            {
                discovery = _serviceDiscovery;
            }

            if (discovery is null)
            {
                return;
            }

            if (status != GattStatus.Success || gatt is null)
            {
                discovery.TrySetException(
                    new InvalidOperationException(
                        $"A descoberta de serviços GATT falhou ({status})."));
                return;
            }

            var services = (gatt.Services ?? [])
                .Select(MapService)
                .OrderBy(service => service.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
            discovery.TrySetResult(services);
        }

        private static GattServiceInfo MapService(BluetoothGattService service)
        {
            var uuid = service.Uuid?.ToString() ?? "UUID indisponível";
            var characteristics = (service.Characteristics ?? [])
                .Select(MapCharacteristic)
                .OrderBy(characteristic => characteristic.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

            return new GattServiceInfo(
                uuid,
                GattUuidNameResolver.GetServiceName(uuid),
                service.Type == GattServiceType.Primary,
                characteristics);
        }

        private static GattCharacteristicInfo MapCharacteristic(
            BluetoothGattCharacteristic characteristic)
        {
            var uuid = characteristic.Uuid?.ToString() ?? "UUID indisponível";
            return new GattCharacteristicInfo(
                uuid,
                GattUuidNameResolver.GetCharacteristicName(uuid),
                MapProperties(characteristic.Properties));
        }

        private static GattCharacteristicProperties MapProperties(GattProperty properties)
        {
            var result = GattCharacteristicProperties.None;

            AddIfPresent(GattProperty.Broadcast, GattCharacteristicProperties.Broadcast);
            AddIfPresent(GattProperty.Read, GattCharacteristicProperties.Read);
            AddIfPresent(GattProperty.WriteNoResponse, GattCharacteristicProperties.WriteWithoutResponse);
            AddIfPresent(GattProperty.Write, GattCharacteristicProperties.Write);
            AddIfPresent(GattProperty.Notify, GattCharacteristicProperties.Notify);
            AddIfPresent(GattProperty.Indicate, GattCharacteristicProperties.Indicate);
            AddIfPresent(GattProperty.SignedWrite, GattCharacteristicProperties.AuthenticatedSignedWrites);
            AddIfPresent(GattProperty.ExtendedProps, GattCharacteristicProperties.ExtendedProperties);

            return result;

            void AddIfPresent(
                GattProperty androidProperty,
                GattCharacteristicProperties mappedProperty)
            {
                if (properties.HasFlag(androidProperty))
                {
                    result |= mappedProperty;
                }
            }
        }
    }
}
