using D20Mobile.Controllers;

namespace D20Mobile.Views;

public partial class GattDiagnosticsPage : ContentPage
{
    private readonly GattDiagnosticsController _controller;

    public GattDiagnosticsPage(GattDiagnosticsController controller)
    {
        InitializeComponent();
        _controller = controller;
        ServicesList.ItemsSource = _controller.Model.Services;
        Render();
    }

    public void SetDeviceName(string? deviceName)
    {
        _controller.SetDeviceName(deviceName);
        Render();
    }

    public async Task RefreshAsync()
    {
        var refresh = _controller.RefreshAsync();
        Render();
        await refresh;
        Render();
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await RefreshAsync();
    }

    private void Render()
    {
        var model = _controller.Model;
        DeviceNameLabel.Text = model.DeviceName;
        StatusLabel.Text = model.StatusMessage;
        BusyIndicator.IsVisible = model.IsBusy;
        BusyIndicator.IsRunning = model.IsBusy;
        RefreshButton.IsEnabled = !model.IsBusy;
    }
}
