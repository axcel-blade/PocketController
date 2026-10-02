using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketController.Protocol;

/// <summary>What a server tells phones about itself during LAN discovery.</summary>
public sealed record DiscoveryInfo(
    [property: JsonPropertyName("name")]    string Name,
    [property: JsonPropertyName("port")]    int Port,
    [property: JsonPropertyName("clients")] int Clients,
    [property: JsonPropertyName("max")]     int Max);

/// <summary>
/// Text packets used for LAN discovery. They are kept separate from the 48-byte
/// <see cref="GamepadMessage"/> format and use their own ports
/// (<see cref="Constants.DiscoveryPort"/> and <see cref="Constants.AnnouncePort"/>).
/// <list type="bullet">
/// <item>Probe, phone → server: <c>PCTRL?1|&lt;device name&gt;</c></item>
/// <item>Announcement, server → phone: <c>PCTRL!1|{"name":…,"port":…,"clients":…,"max":…}</c></item>
/// </list>
/// </summary>
public static class DiscoveryMessage
{
    public const string ProbePrefix    = "PCTRL?1|";
    public const string AnnouncePrefix = "PCTRL!1|";
    private const int MaxDeviceNameLength = 64;

    public static byte[] BuildProbe(string deviceName)
        => Encoding.UTF8.GetBytes(ProbePrefix + Truncate(deviceName.Replace('|', ' ')));

    /// <summary>Returns the phone's device name if <paramref name="data"/> is a probe; otherwise <c>null</c>.</summary>
    public static string? ParseProbe(ReadOnlySpan<byte> data)
    {
        if (data.Length > 512) return null;
        var text = Encoding.UTF8.GetString(data);
        if (!text.StartsWith(ProbePrefix, StringComparison.Ordinal)) return null;
        var name = Truncate(text[ProbePrefix.Length..].Trim());
        return name.Length == 0 ? "Unknown device" : name;
    }

    public static byte[] BuildAnnouncement(DiscoveryInfo info)
        => Encoding.UTF8.GetBytes(AnnouncePrefix + JsonSerializer.Serialize(info));

    /// <summary>Returns the server info if <paramref name="data"/> is an announcement; otherwise <c>null</c>.</summary>
    public static DiscoveryInfo? ParseAnnouncement(ReadOnlySpan<byte> data)
    {
        if (data.Length > 1024) return null;
        var text = Encoding.UTF8.GetString(data);
        if (!text.StartsWith(AnnouncePrefix, StringComparison.Ordinal)) return null;
        try
        {
            var info = JsonSerializer.Deserialize<DiscoveryInfo>(text[AnnouncePrefix.Length..]);
            return info is { Port: > 0 and <= 65535 } ? info : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Truncate(string s) => s.Length <= MaxDeviceNameLength ? s : s[..MaxDeviceNameLength];
}
