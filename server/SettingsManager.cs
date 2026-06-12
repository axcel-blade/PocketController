using System.Text.Json;

namespace PocketConsoleServer;

/// <summary>User-configurable server settings, persisted to <c>settings.json</c> next to the EXE.</summary>
public class AppSettings
{
    /// <summary>UDP port the server listens on. Defaults to <see cref="PocketConsole.Protocol.Constants.DefaultPort"/>.</summary>
    public int Port { get; set; } = PocketConsole.Protocol.Constants.DefaultPort;

    /// <summary>Maximum number of simultaneous clients. Defaults to <see cref="PocketConsole.Protocol.Constants.MaxClients"/>.</summary>
    public int MaxClients { get; set; } = PocketConsole.Protocol.Constants.MaxClients;
}

/// <summary>
/// Loads and saves <see cref="AppSettings"/> as indented JSON.
/// A missing or corrupt file silently falls back to defaults.
/// </summary>
public static class SettingsManager
{
    private static readonly string SettingsPath =
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    /// <summary>
    /// Loads settings from disk. Returns a default <see cref="AppSettings"/> instance
    /// if the file does not exist or cannot be parsed.
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { /* corrupt file — fall through to defaults */ }
        return new AppSettings();
    }

    /// <summary>Writes <paramref name="settings"/> to <c>settings.json</c> as indented JSON.</summary>
    public static void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}
