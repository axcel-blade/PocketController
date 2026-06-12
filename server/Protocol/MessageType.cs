namespace PocketConsole.Protocol;

public enum MessageType : byte
{
    Connect = 0,
    Disconnect = 1,
    Input = 2,
    Ping = 3
}
