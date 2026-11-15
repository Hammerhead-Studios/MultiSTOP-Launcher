namespace MultiSTOP.Launcher.Models;

public sealed class LauncherSettings
{
    public bool UseCustomResolution { get; set; }
    public int ResolutionWidth { get; set; } = 1920;
    public int ResolutionHeight { get; set; } = 1080;
    public string DisplayMode { get; set; } = "Default";
    public int RefreshRate { get; set; }

    public bool SkipIntro { get; set; } = true;
    public bool ShowConsole { get; set; }
    public bool DeveloperMode { get; set; }
    public bool DisableJoystick { get; set; }
    public bool DisableSound { get; set; }
    public bool HighPriority { get; set; }

    public string DirectXLevel { get; set; } = "Default";
    public string AutoRunCommands { get; set; } = string.Empty;
    public string ExtraArguments { get; set; } = string.Empty;

    // Intentionally unavailable until the multiplayer launch path is implemented.
    public bool ApplyToMultiplayer { get; set; }
}
