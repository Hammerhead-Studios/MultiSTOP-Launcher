namespace MultiSTOP.Launcher.Models;

/// <summary>Represents a supported game executable found below the launcher folder.</summary>
internal sealed record GameInstallation(
    string DisplayName,
    string ExecutablePath,
    string RelativePath)
{
    public string DisplayLabel => $"{DisplayName}  —  {RelativePath}";

    public string StatusLabel => DisplayName.ToUpperInvariant() switch
    {
        "F-STOP" => "F-STOP FOUND  ·  p2008.exe",
        _ => "APERTURE CAMERA / SANDBOX FOUND  ·  valve.exe"
    };
}
