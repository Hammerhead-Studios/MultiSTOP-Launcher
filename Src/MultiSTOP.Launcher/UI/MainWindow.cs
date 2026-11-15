using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MultiSTOP.Launcher.Models;
using MultiSTOP.Launcher.Services;
using MultiSTOP.Launcher.Theme;

namespace MultiSTOP.Launcher.UI;

internal sealed class MainWindow : ShellWindow
{
    private readonly IReadOnlyList<GameInstallation> _installations;
    private readonly SettingsStore _settingsStore;
    private LauncherSettings _settings;
    private readonly string? _loadWarning;
    private readonly TextBlock _statusText;
    private GameInstallation _selectedInstallation;

    public MainWindow(
        IReadOnlyList<GameInstallation> installations,
        LauncherSettings settings,
        SettingsStore settingsStore,
        string? loadWarning)
        : base("MultiSTOP", 800, 600)
    {
        if (installations.Count == 0)
            throw new ArgumentException("At least one supported game installation is required.", nameof(installations));

        _installations = installations;
        _selectedInstallation = installations[0];
        _settings = settings;
        _settingsStore = settingsStore;
        _loadWarning = loadWarning;

        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("42,*,40")
        };

        var background = new Image
        {
            Source = new Bitmap(AssetLoader.Open(new Uri("avares://MultiSTOP/Assets/MultiSTOP-Background.png"))),
            Stretch = Stretch.UniformToFill
        };
        Grid.SetRowSpan(background, 3);
        root.Children.Add(background);

        var shade = new Border { Background = Palette.Brush("#71100B06") };
        Grid.SetRowSpan(shade, 3);
        root.Children.Add(shade);

        root.Children.Add(CreateTitleBar("HAMMERHEAD STUDIOS  /  MULTISTOP"));

        var center = new StackPanel
        {
            Width = 390,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 32, 0, 0)
        };
        center.Children.Add(MakeText("F-STOP  /  APERTURE CAMERA  /  APERTURE SANDBOX", 9, Palette.Gold, FontWeight.SemiBold));
        center.Children.Add(MakeText("Choose what you want to play.", 17, Palette.Text, FontWeight.SemiBold));
        center.Children.Add(MakeText("Start a detected local build or adjust its launch settings.", 11, Palette.Muted));

        if (_installations.Count > 1)
        {
            center.Children.Add(MakeText("GAME INSTALLATION", 9, Palette.Gold, FontWeight.Bold));
            var selector = new ComboBox
            {
                ItemsSource = _installations.Select(item => item.DisplayLabel).ToArray(),
                SelectedIndex = 0,
                MinHeight = 36,
                Background = Palette.Brush("#F51A1712"),
                Foreground = Palette.Text,
                BorderBrush = Palette.Brush("#B8D99D4E"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(7),
                Padding = new Thickness(10, 5),
                FontSize = 11
            };
            selector.SelectionChanged += (_, _) =>
            {
                if (selector.SelectedIndex >= 0 && selector.SelectedIndex < _installations.Count)
                {
                    _selectedInstallation = _installations[selector.SelectedIndex];
                    UpdateStatus();
                }
            };
            center.Children.Add(selector);
        }

        center.Children.Add(CreateMenuButton("▶", "LAUNCH", "Start the selected game with your saved launch settings.", true, true));
        center.Children.Add(CreateMenuButton("◉", "LAUNCH  [MULTIPLAYER]", "Multiplayer is still being worked on.", false, false));
        center.Children.Add(CreateMenuButton("⚙", "LAUNCH OPTIONS", "Resolution, startup switches and console commands.", false, true));
        center.Children.Add(CreateMenuButton("×", "EXIT", "Close MultiSTOP.", false, true));

        var status = new Border
        {
            Background = Palette.Brush("#BA17130E"),
            BorderBrush = Palette.Brush("#66FFC15D"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 3, 0, 0)
        };
        var statusRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 9 };
        statusRow.Children.Add(MakeText("●", 10, Palette.Gold));
        _statusText = MakeText(string.Empty, 9, Palette.Text, FontWeight.SemiBold);
        statusRow.Children.Add(_statusText);
        status.Child = statusRow;
        center.Children.Add(status);
        Grid.SetRow(center, 1);
        root.Children.Add(center);

        var footer = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        var copyright = MakeText("© 2026 HAMMERHEAD STUDIOS  ·  MULTISTOP 0.1.0 BETA", 9, Palette.Brush("#D7F2E4CC"));
        copyright.VerticalAlignment = VerticalAlignment.Center;
        copyright.Margin = new Thickness(15, 0);
        footer.Children.Add(copyright);

        var about = MakeActionButton("ABOUT  +", height: 29);
        about.FontSize = 10;
        about.Padding = new Thickness(13, 3);
        about.Margin = new Thickness(0, 3, 12, 4);
        about.Click += async (_, _) => await new AboutWindow().ShowDialog(this);
        Grid.SetColumn(about, 1);
        footer.Children.Add(about);
        Grid.SetRow(footer, 2);
        root.Children.Add(footer);

        Content = root;
        UpdateStatus();

        if (!string.IsNullOrWhiteSpace(_loadWarning))
        {
            Dispatcher.UIThread.Post(async () =>
            {
                await new NoticeWindow(
                    "SETTINGS WARNING",
                    "MultiSTOP couldn't load the previous launch settings.",
                    _loadWarning,
                    NoticeKind.Warning).ShowDialog(this);
            });
        }
    }

    private void UpdateStatus()
    {
        _statusText.Text = _selectedInstallation.StatusLabel;
    }

    private Button CreateMenuButton(string icon, string title, string subtitle, bool primary, bool enabled)
    {
        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("34,*,Auto"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            VerticalAlignment = VerticalAlignment.Center
        };
        var glyph = MakeText(icon, 21, enabled && primary ? Palette.Ink : (enabled ? Palette.Gold : Palette.DisabledText), FontWeight.SemiBold);
        glyph.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetRowSpan(glyph, 2);
        content.Children.Add(glyph);

        var heading = MakeText(title, 12, enabled && primary ? Palette.Ink : (enabled ? Palette.Text : Palette.DisabledText), FontWeight.Bold);
        heading.LetterSpacing = 0.8;
        Grid.SetColumn(heading, 1);
        content.Children.Add(heading);

        var description = MakeText(subtitle, 10, enabled && primary ? Palette.Brush("#FF49321B") : Palette.Muted);
        Grid.SetColumn(description, 1);
        Grid.SetRow(description, 1);
        content.Children.Add(description);

        var marker = MakeText(enabled ? "›" : "LOCKED", enabled ? 22 : 8, enabled && primary ? Palette.Ink : Palette.Gold, FontWeight.SemiBold);
        marker.VerticalAlignment = VerticalAlignment.Center;
        marker.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetColumn(marker, 2);
        Grid.SetRowSpan(marker, 2);
        content.Children.Add(marker);

        var button = new Button
        {
            Content = content,
            Height = 58,
            Padding = new Thickness(15, 8),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = enabled
                ? (primary ? Palette.Orange : Palette.Brush("#D51A1712"))
                : Palette.Brush("#AA39352E"),
            Foreground = enabled ? Palette.Text : Palette.DisabledText,
            BorderBrush = enabled
                ? (primary ? Palette.Gold : Palette.Brush("#88E3A64F"))
                : Palette.Brush("#554C453B"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            IsEnabled = enabled
        };
        if (enabled)
        {
            var normal = primary ? Palette.Orange : Palette.Brush("#D51A1712");
            var hover = primary ? Palette.Gold : Palette.Brush("#EE3D3021");
            button.PointerEntered += (_, _) => button.Background = hover;
            button.PointerExited += (_, _) => button.Background = normal;
        }

        if (title == "LAUNCH")
            button.Click += (_, _) => LaunchGame();
        else if (title == "LAUNCH OPTIONS")
            button.Click += async (_, _) => await new OptionsWindow(_settings, _settingsStore, _selectedInstallation, updated => _settings = updated).ShowDialog(this);
        else if (title == "EXIT")
            button.Click += (_, _) => Close();

        return button;
    }

    private async void LaunchGame()
    {
        try
        {
            var warning = LaunchService.Launch(_selectedInstallation.ExecutablePath, _settings);
            _statusText.Text = $"{_selectedInstallation.DisplayName.ToUpperInvariant()} STARTED";
            if (!string.IsNullOrWhiteSpace(warning))
            {
                await new NoticeWindow(
                    "LAUNCH NOTE",
                    $"{_selectedInstallation.DisplayName} was started, with one setting left unapplied.",
                    warning,
                    NoticeKind.Warning).ShowDialog(this);
            }
        }
        catch (Exception ex)
        {
            await new NoticeWindow(
                "COULDN'T START GAME",
                $"MultiSTOP wasn't able to start {_selectedInstallation.DisplayName}.",
                ex.Message,
                NoticeKind.Error).ShowDialog(this);
        }
    }
}
