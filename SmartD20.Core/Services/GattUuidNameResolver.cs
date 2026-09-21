namespace D20Mobile.Services;

public static class GattUuidNameResolver
{
    private static readonly IReadOnlyDictionary<string, string> ServiceNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["1800"] = "Acesso genérico",
            ["1801"] = "Atributos genéricos",
            ["180A"] = "Informações do dispositivo",
            ["180D"] = "Frequência cardíaca",
            ["180F"] = "Bateria"
        };

    private static readonly IReadOnlyDictionary<string, string> CharacteristicNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["2A00"] = "Nome do dispositivo",
            ["2A01"] = "Aparência",
            ["2A05"] = "Serviços alterados",
            ["2A19"] = "Nível da bateria",
            ["2A24"] = "Número do modelo",
            ["2A25"] = "Número de série",
            ["2A26"] = "Versão do firmware",
            ["2A27"] = "Versão do hardware",
            ["2A29"] = "Fabricante",
            ["2A37"] = "Medição de frequência cardíaca"
        };

    public static string GetServiceName(string uuid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
        var assignedNumber = GetAssignedNumber(uuid);
        return assignedNumber is not null && ServiceNames.TryGetValue(assignedNumber, out var name)
            ? name
            : "Serviço desconhecido";
    }

    public static string GetCharacteristicName(string uuid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
        var assignedNumber = GetAssignedNumber(uuid);
        return assignedNumber is not null && CharacteristicNames.TryGetValue(assignedNumber, out var name)
            ? name
            : "Característica desconhecida";
    }

    private static string? GetAssignedNumber(string uuid)
    {
        var normalized = uuid.Trim();

        if (normalized.Length == 4)
        {
            return normalized.ToUpperInvariant();
        }

        const string bluetoothBaseSuffix = "-0000-1000-8000-00805f9b34fb";
        return normalized.Length == 36 &&
               normalized.EndsWith(bluetoothBaseSuffix, StringComparison.OrdinalIgnoreCase) &&
               normalized.StartsWith("0000", StringComparison.OrdinalIgnoreCase)
            ? normalized[4..8].ToUpperInvariant()
            : null;
    }
}
