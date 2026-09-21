namespace D20Mobile.Models;

[Flags]
public enum GattCharacteristicProperties
{
    None = 0,
    Broadcast = 1 << 0,
    Read = 1 << 1,
    WriteWithoutResponse = 1 << 2,
    Write = 1 << 3,
    Notify = 1 << 4,
    Indicate = 1 << 5,
    AuthenticatedSignedWrites = 1 << 6,
    ExtendedProperties = 1 << 7
}
