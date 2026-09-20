using System.Collections.ObjectModel;
using D20Mobile.Controllers;
using D20Mobile.Models;

namespace D20Mobile.Views;

public partial class MainPage : ContentPage
{
    private readonly HomeController _controller;
    private readonly ObservableCollection<D20Device> _devices = [];

    public MainPage(HomeController controller)
    {
        InitializeComponent();
        _controller = controller;
        DevicesList.ItemsSource = _devices;
        Render();
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        RenderBusy("Procurando dispositivos próximos...");
        await _controller.ScanAsync();

        _devices.Clear();
        foreach (var device in _controller.Model.Devices)
        {
            _devices.Add(device);
        }

        DevicesList.SelectedItem = null;
        Render();
    }

    private void OnDeviceSelected(object? sender, SelectionChangedEventArgs e)
    {
        _controller.SelectDevice(e.CurrentSelection.FirstOrDefault() as D20Device);
        Render();
    }

    private async void OnConnectionClicked(object? sender, EventArgs e)
    {
        if (_controller.Model.ConnectedDevice is null)
        {
            RenderBusy("Conectando...");
            await _controller.ConnectAsync();
        }
        else
        {
            RenderBusy("Desconectando...");
            await _controller.DisconnectAsync();
        }

        Render();
    }

    private void RenderBusy(string message)
    {
        BusyIndicator.IsVisible = true;
        BusyIndicator.IsRunning = true;
        StatusLabel.Text = message;
        ScanButton.IsEnabled = false;
        ConnectionButton.IsEnabled = false;
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
    }
}
