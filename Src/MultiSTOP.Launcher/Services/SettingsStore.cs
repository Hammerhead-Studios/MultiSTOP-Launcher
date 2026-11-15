using System.Text.Json;
using MultiSTOP.Launcher.Models;

namespace MultiSTOP.Launcher.Services;

internal sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string SettingsPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Hammerhead Studios",
        "MultiSTOP",
        "launcher-settings.json");

    public LauncherSettings Load(out string? warning)
    {
        warning = null;
        if (!File.Exists(SettingsPath))
            return new LauncherSettings();

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<LauncherSettings>(json, JsonOptions)
                   ?? new LauncherSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            warning = "The saved launch settings couldn't be read. MultiSTOP will use the default settings for now.\n\n" + ex.Message;
            return new LauncherSettings();
        }
    }

    public void Save(LauncherSettings settings)
    {
        var directory = Path.GetDirectoryName(SettingsPath)
                        ?? throw new InvalidOperationException("The settings folder could not be resolved.");
        Directory.CreateDirectory(directory);

        var temporaryPath = SettingsPath + ".tmp";
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, SettingsPath, true);
    }
}
