namespace D20Mobile.Models;

public sealed class HomeScreenModel
{
    public IList<D20Device> Devices { get; } = new List<D20Device>();

    public D20Device? SelectedDevice { get; set; }

    public D20Device? ConnectedDevice { get; set; }

    public bool IsBusy { get; set; }

    public string StatusMessage { get; set; } = "Pronto para procurar dispositivos.";
}
