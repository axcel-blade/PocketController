using System.Text.Json;

namespace PocketConsoleServer;

public class AppSettings
{
    public int Port { get; set; } = PocketConsole.Protocol.Constants.DefaultPort;
    public int MaxClients { get; set; } = PocketConsole.Protocol.Constants.MaxClients;
}

public static class SettingsManager
{
    private static readonly string SettingsPath =
        Path.Combine(AppContext.BaseDirectory, "settings.json");

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
        catch { }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}
