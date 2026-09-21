using D20Mobile.Controllers;
using D20Mobile.Models;

namespace D20Mobile.Views;

public partial class MainPage : ContentPage
{
    private readonly HomeController _controller;
    private readonly GattDiagnosticsPage _diagnosticsPage;

    public MainPage(HomeController controller, GattDiagnosticsPage diagnosticsPage)
    {
        InitializeComponent();
        _controller = controller;
        _diagnosticsPage = diagnosticsPage;
        DevicesList.ItemsSource = _controller.Model.Devices;
        Render();
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        var scan = _controller.ScanAsync();
        Render();
        await scan;

        DevicesList.SelectedItem = null;
        Render();
    }

    private void OnDeviceSelected(object? sender, SelectionChangedEventArgs e)
    {
        _controller.SelectDevice(e.CurrentSelection.FirstOrDefault() as WatchDevice);
        Render();
    }

    private async void OnConnectionClicked(object? sender, EventArgs e)
    {
        Task operation;

        if (_controller.Model.ConnectedDevice is null)
        {
            operation = _controller.ConnectAsync();
        }
        else
        {
            operation = _controller.DisconnectAsync();
        }

        Render();
        await operation;
        Render();
    }

    private async void OnDiagnosticsClicked(object? sender, EventArgs e)
    {
        var connectedDevice = _controller.Model.ConnectedDevice;

        if (connectedDevice is null)
        {
            return;
        }

        _diagnosticsPage.SetDeviceName(connectedDevice.Name);
        await Navigation.PushAsync(_diagnosticsPage);
        await _diagnosticsPage.RefreshAsync();
    }

    private void Render()
    {
        var model = _controller.Model;
        var isConnected = model.ConnectedDevice is not null;

        BusyIndicator.IsVisible = model.IsBusy;
        BusyIndicator.IsRunning = model.IsBusy;
        StatusLabel.Text = model.StatusMessage;
        ScanButton.IsEnabled = !model.IsBusy && !isConnected;
        ConnectionButton.Text = isConnected ? "Desconectar" : "Conectar";
        ConnectionButton.IsEnabled = !model.IsBusy && (isConnected || model.SelectedDevice is not null);
        DevicesList.IsEnabled = !model.IsBusy && !isConnected;
        DiagnosticsButton.IsVisible = isConnected;
        DiagnosticsButton.IsEnabled = !model.IsBusy && isConnected;
    }
}
