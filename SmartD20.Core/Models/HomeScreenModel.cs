using System.Collections.ObjectModel;

namespace D20Mobile.Models;

public sealed class HomeScreenModel
{
    public ObservableCollection<WatchDevice> Devices { get; } = [];

    public WatchDevice? SelectedDevice { get; set; }

    public WatchDevice? ConnectedDevice { get; set; }

    public bool IsBusy { get; set; }

    public string StatusMessage { get; set; } = "Pronto para procurar dispositivos.";
}
