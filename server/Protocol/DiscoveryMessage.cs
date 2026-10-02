using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketController.Protocol;

/// <summary>What a server tells phones about itself during LAN discovery.</summary>
/// <param name="Name">PC name shown in the app.</param>
/// <param name="Port">Controller (game) port to connect to.</param>
/// <param name="Clients">Controller slots in use.</param>
/// <param name="Max">Total controller slots.</param>
/// <param name="Id">
/// Random per-run server ID. A PC with several network adapters (Wi‑Fi, WSL, VMware…) is heard
/// from several source addresses; the app uses this ID to show it once.
/// </param>
/// <param name="Addresses">All of the server's IPv4 addresses, so the app can pick one on its own subnet.</param>
public sealed record DiscoveryInfo(
    [property: JsonPropertyName("name")]    string Name,
    [property: JsonPropertyName("port")]    int Port,
    [property: JsonPropertyName("clients")] int Clients,
    [property: JsonPropertyName("max")]     int Max,
    [property: JsonPropertyName("id")]      string Id = "",
    [property: JsonPropertyName("ips")]     IReadOnlyList<string>? Addresses = null)
{
    // Records compare lists by reference; compare contents so equal announcements are equal.
    public bool Equals(DiscoveryInfo? other) =>
        other is not null &&
        Name == other.Name && Port == other.Port && Clients == other.Clients && Max == other.Max &&
        Id == other.Id && (Addresses ?? []).SequenceEqual(other.Addresses ?? []);

    public override int GetHashCode() => HashCode.Combine(Name, Port, Clients, Max, Id);
}

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
