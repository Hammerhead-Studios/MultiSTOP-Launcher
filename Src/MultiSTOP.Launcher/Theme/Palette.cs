using Avalonia.Media;

namespace MultiSTOP.Launcher.Theme;

internal static class Palette
{
    public static readonly IBrush Ink = Brush("#FF100E0B");
    public static readonly IBrush Panel = Brush("#F21B1711");
    public static readonly IBrush PanelSoft = Brush("#D9211B13");
    public static readonly IBrush PanelRaised = Brush("#FF292116");
    public static readonly IBrush Gold = Brush("#FFFFBD4A");
    public static readonly IBrush Orange = Brush("#FFFF8C32");
    public static readonly IBrush Text = Brush("#FFFFF4E4");
    public static readonly IBrush Muted = Brush("#FFB8AA96");
    public static readonly IBrush Line = Brush("#66FFC36B");
    public static readonly IBrush Disabled = Brush("#FF514B42");
    public static readonly IBrush DisabledText = Brush("#FF9C9386");

    public static SolidColorBrush Brush(string value) => new(Color.Parse(value));
}
