using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MultiSTOP.Launcher.Models;
using MultiSTOP.Launcher.Services;
using MultiSTOP.Launcher.UI;

namespace MultiSTOP.Launcher;

public sealed partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var installations = InstallLocator.FindGameInstallations();
            if (installations.Count == 0)
            {
                var root = InstallLocator.GetRootFolder();
                var missingGameWindow = new NoticeWindow(
                    "SUPPORTED GAME NOT FOUND",
                    "MultiSTOP couldn't find a supported game executable.",
                    "MultiSTOP searched every subfolder beneath its own location for either:\n\n" +
                    "  • p2008.exe  —  F-STOP\n" +
                    "  • valve.exe  —  Aperture Camera / Aperture Sandbox\n\n" +
                    "Scan location:\n" + root +
                    "\n\nPlace the game's files somewhere inside this folder tree, then start MultiSTOP again.",
                    NoticeKind.Error,
                    "CLOSE LAUNCHER");

                desktop.MainWindow = missingGameWindow;
                missingGameWindow.Closed += (_, _) => desktop.Shutdown();
            }
            else
            {
                var settingsStore = new SettingsStore();
                var settings = settingsStore.Load(out var loadWarning);
                desktop.MainWindow = new MainWindow(installations, settings, settingsStore, loadWarning);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
