using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MultiSTOP.Launcher.Models;
using MultiSTOP.Launcher.Services;
using MultiSTOP.Launcher.Theme;

namespace MultiSTOP.Launcher.UI;

internal sealed class OptionsWindow : ShellWindow
{
    private readonly SettingsStore _store;
    private readonly Action<LauncherSettings> _onSaved;
    private readonly GameInstallation _selectedInstallation;
    private LauncherSettings _workingSettings;
    private readonly ContentControl _pageHost = new();
    private readonly Dictionary<string, Button> _navigationButtons = new();
    private readonly Dictionary<string, Control> _pageCache = new(StringComparer.Ordinal);

    private CheckBox _customResolution = new();
    private TextBox _width = new();
    private TextBox _height = new();
    private TextBox _refresh = new();
    private ComboBox _displayMode = new();
    private CheckBox _skipIntro = new();
    private CheckBox _console = new();
    private CheckBox _developer = new();
    private CheckBox _disableJoystick = new();
    private CheckBox _disableSound = new();
    private CheckBox _highPriority = new();
    private ComboBox _directX = new();
    private TextBox _commands = new();
    private TextBox _extraArguments = new();

    public OptionsWindow(
        LauncherSettings settings,
        SettingsStore store,
        GameInstallation selectedInstallation,
        Action<LauncherSettings> onSaved)
        : base("MultiSTOP | Launch Options", 980, 720)
    {
        _workingSettings = Copy(settings);
        _store = store;
        _selectedInstallation = selectedInstallation;
        _onSaved = onSaved;
        PopulateControls();

        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("42,*,66")
        };
        root.Children.Add(CreateTitleBar("MULTISTOP  /  LAUNCH OPTIONS"));

        var main = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("205,*"),
            Margin = new Thickness(0)
        };

        var sidebar = new Border
        {
            Background = Palette.Brush("#FF17140F"),
            BorderBrush = Palette.Brush("#55D99D4E"),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(16, 20)
        };
        var sideStack = new StackPanel { Spacing = 7 };
        sideStack.Children.Add(MakeText("SETTINGS", 10, Palette.Gold, FontWeight.Bold));
        sideStack.Children.Add(MakeText("Choose a section to configure how F-STOP starts.", 11, Palette.Muted));
        sideStack.Children.Add(new Border { Height = 1, Background = Palette.Brush("#55D99D4E"), Margin = new Thickness(0, 9, 0, 9) });
        AddNavigation(sideStack, "Display", "Screen size and display mode");
        AddNavigation(sideStack, "Startup", "Startup switches and behavior");
        AddNavigation(sideStack, "Commands", "Commands and extra arguments");
        AddNavigation(sideStack, "Advanced", "Engine and launcher settings");
        sideStack.Children.Add(new Border { Height = 1, Background = Palette.Brush("#55D99D4E"), Margin = new Thickness(0, 12, 0, 8) });
        sideStack.Children.Add(MakeText("Changes are saved on this PC and used by the Launch button for the selected installation.", 10, Palette.Muted));
        sidebar.Child = sideStack;
        main.Children.Add(sidebar);

        var hostBorder = new Border
        {
            Background = Palette.Brush("#FF211C15"),
            Padding = new Thickness(24, 18, 24, 14),
            Child = _pageHost
        };
        Grid.SetColumn(hostBorder, 1);
        main.Children.Add(hostBorder);
        Grid.SetRow(main, 1);
        root.Children.Add(main);

        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"),
            Background = Palette.Brush("#FF17140F"),
            Margin = new Thickness(0),
            ColumnSpacing = 9
        };
        var footerNote = MakeText("MULTISTOP SETTINGS  ·  LOCAL CONFIGURATION", 9, Palette.Muted);
        footerNote.VerticalAlignment = VerticalAlignment.Center;
        footerNote.Margin = new Thickness(18, 0);
        footer.Children.Add(footerNote);

        var reset = MakeActionButton("RESET DEFAULTS", height: 38);
        reset.FontSize = 10;
        reset.Click += (_, _) => ResetDefaults();
        Grid.SetColumn(reset, 1);
        footer.Children.Add(reset);

        var cancel = MakeActionButton("CANCEL", height: 38);
        cancel.FontSize = 10;
        cancel.Click += (_, _) => Close();
        Grid.SetColumn(cancel, 2);
        footer.Children.Add(cancel);

        var save = MakeActionButton("SAVE SETTINGS", primary: true, height: 38);
        save.FontSize = 10;
        save.Click += async (_, _) => await SaveSettingsAsync();
        Grid.SetColumn(save, 3);
        footer.Children.Add(save);

        Grid.SetRow(footer, 2);
        root.Children.Add(footer);
        Content = root;

        SelectPage("Display");
    }

    private void AddNavigation(Panel parent, string name, string description)
    {
        var content = new StackPanel { Spacing = 3 };
        content.Children.Add(MakeText(name.ToUpperInvariant(), 11, Palette.Text, FontWeight.Bold));
        content.Children.Add(MakeText(description, 9, Palette.Muted));

        var button = new Button
        {
            Content = content,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = Palette.Brush("#0017140F"),
            Foreground = Palette.Text,
            BorderBrush = Palette.Brush("#003D3324"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Padding = new Thickness(10, 9),
            Margin = new Thickness(0, 1),
            MinHeight = 54
        };
        button.Click += (_, _) => SelectPage(name);
        _navigationButtons[name] = button;
        parent.Children.Add(button);
    }

    private void SelectPage(string page)
    {
        foreach (var pair in _navigationButtons)
        {
            var selected = pair.Key == page;
            pair.Value.Background = selected ? Palette.Brush("#FF3A2B1A") : Palette.Brush("#0017140F");
            pair.Value.BorderBrush = selected ? Palette.Brush("#CCF3A94D") : Palette.Brush("#003D3324");
        }

        // Each page owns controls stored in fields. Keep its visual tree and reuse it
        // when the user returns instead of trying to parent those controls a second time.
        if (!_pageCache.TryGetValue(page, out var pageView))
        {
            pageView = page switch
            {
                "Startup" => BuildStartupPage(),
                "Commands" => BuildCommandsPage(),
                "Advanced" => BuildAdvancedPage(),
                _ => BuildDisplayPage()
            };
            _pageCache.Add(page, pageView);
        }

        if (!ReferenceEquals(_pageHost.Content, pageView))
        {
            // Explicitly detach the old page before attaching a cached page view.
            _pageHost.Content = null;
            _pageHost.Content = pageView;
        }
    }

    private Control BuildDisplayPage()
    {
        var stack = CreatePageStack(
            "Display & Resolution",
            "Choose how the game should open. The game's own video settings may still take precedence on some builds.");

        stack.Children.Add(BuildGroup("SCREEN SIZE", "Set a custom width and height for the game window.",
            new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    _customResolution,
                    FormRow("Width", _width, "pixels"),
                    FormRow("Height", _height, "pixels")
                }
            }));

        stack.Children.Add(BuildGroup("DISPLAY MODE", "Pick the launch mode and, optionally, request a refresh rate.",
            new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    FormRow("Window mode", _displayMode),
                    FormRow("Refresh rate", _refresh, "Hz  ·  0 uses the current default")
                }
            }));

        return WrapPage(stack);
    }

    private Control BuildStartupPage()
    {
        var stack = CreatePageStack(
            "Startup & Behavior",
            $"These switches are added to {_selectedInstallation.DisplayName} when you press Launch.");

        stack.Children.Add(BuildGroup("STARTUP SWITCHES", "Common Source-engine options. Leave a switch off unless you need it.",
            new StackPanel
            {
                Spacing = 11,
                Children =
                {
                    ToggleRow(_skipIntro, "Skip the intro video", "Adds -novid. Useful when you want to get straight to the menu."),
                    ToggleRow(_console, "Open the developer console", "Adds -console. The console can be used for map and debug commands."),
                    ToggleRow(_developer, "Enable developer mode", "Adds -dev. Some developer output and tools may become available."),
                    ToggleRow(_disableJoystick, "Disable joystick support", "Adds -nojoy."),
                    ToggleRow(_disableSound, "Disable game sound", "Adds -nosound. This is mainly useful for troubleshooting."),
                    ToggleRow(_highPriority, "Start with high process priority", "Asks Windows to give the game a higher process priority after it starts.")
                }
            }));

        return WrapPage(stack);
    }

    private Control BuildCommandsPage()
    {
        var stack = CreatePageStack(
            "Auto-run Commands",
            "Enter Source console commands that should be sent to the game as it starts.");

        var commandEditor = new StackPanel { Spacing = 8 };
        commandEditor.Children.Add(MakeText("ONE COMMAND PER LINE", 10, Palette.Gold, FontWeight.Bold));
        commandEditor.Children.Add(_commands);
        commandEditor.Children.Add(MakeText("Example: map test_map  ·  exec autoexec.cfg  ·  sv_cheats 1\nMultiSTOP adds the leading + required by Source's startup-command syntax. You can also type the + yourself.", 10, Palette.Muted));
        stack.Children.Add(BuildGroup("CONSOLE COMMANDS", $"Commands are passed to {_selectedInstallation.DisplayName}; MultiSTOP does not execute them itself.", commandEditor));

        var extra = new StackPanel { Spacing = 8 };
        extra.Children.Add(MakeText("ADDITIONAL COMMAND-LINE ARGUMENTS", 10, Palette.Gold, FontWeight.Bold));
        extra.Children.Add(_extraArguments);
        extra.Children.Add(MakeText("For switches not listed on the other pages. Separate arguments with spaces; put values containing spaces in quotation marks.", 10, Palette.Muted));
        stack.Children.Add(BuildGroup("EXTRA ARGUMENTS", "These are appended to the options generated above.", extra));

        return WrapPage(stack);
    }

    private Control BuildAdvancedPage()
    {
        var stack = CreatePageStack(
            "Advanced Settings",
            "A few lower-level options for builds that need a specific startup environment.");

        stack.Children.Add(BuildGroup("DIRECTX", "A forced DirectX level is passed with -dxlevel. Default lets the game choose.",
            new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    FormRow("DirectX level", _directX),
                    MakeText("Changing this can cause Source to rebuild its video settings. Use Default unless your particular build needs a fixed level.", 10, Palette.Muted)
                }
            }));

        var multiplayerRow = new StackPanel { Spacing = 7 };
        var multiplayer = new CheckBox
        {
            Content = "Also apply these settings to Launch [Multiplayer]",
            IsChecked = false,
            IsEnabled = false,
            Foreground = Palette.DisabledText,
            FontSize = 12
        };
        multiplayerRow.Children.Add(multiplayer);
        multiplayerRow.Children.Add(MakeText("Unavailable for now. Multiplayer launching is still disabled while that part of MultiSTOP is being worked on.", 10, Palette.Muted));
        stack.Children.Add(BuildGroup("MULTIPLAYER SETTINGS", "This control is intentionally locked until the multiplayer launcher is ready.", multiplayerRow));

        var paths = new StackPanel { Spacing = 7 };
        paths.Children.Add(MakeText("GAME EXECUTABLE", 9, Palette.Gold, FontWeight.Bold));
        paths.Children.Add(MakeText(_selectedInstallation.ExecutablePath, 11, Palette.Text));
        paths.Children.Add(MakeText("SETTINGS FILE", 9, Palette.Gold, FontWeight.Bold));
        paths.Children.Add(MakeText(_store.SettingsPath, 11, Palette.Text));
        stack.Children.Add(BuildGroup("INSTALLATION DETAILS", "This is the executable selected on the main screen.", paths));

        return WrapPage(stack);
    }

    private static StackPanel CreatePageStack(string title, string description)
    {
        var stack = new StackPanel { Spacing = 15, Margin = new Thickness(0, 2, 4, 14) };
        stack.Children.Add(MakeText(title, 23, Palette.Text, FontWeight.SemiBold));
        stack.Children.Add(MakeText(description, 11, Palette.Muted));
        return stack;
    }

    private static Control WrapPage(Control content)
    {
        return new ScrollViewer
        {
            Content = content,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };
    }

    private static Border BuildGroup(string heading, string description, Control content)
    {
        var stack = new StackPanel { Spacing = 11 };
        stack.Children.Add(MakeText(heading, 10, Palette.Gold, FontWeight.Bold));
        stack.Children.Add(MakeText(description, 10, Palette.Muted));
        stack.Children.Add(new Border { Height = 1, Background = Palette.Brush("#44E9B15E") });
        stack.Children.Add(content);
        return new Border
        {
            Background = Palette.Brush("#FF29231A"),
            BorderBrush = Palette.Brush("#66D99D4E"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(15),
            Child = stack
        };
    }

    private static Control FormRow(string label, Control field, string? suffix = null)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("145,*,Auto"),
            VerticalAlignment = VerticalAlignment.Center,
            ColumnSpacing = 9
        };
        var labelText = MakeText(label, 11, Palette.Text);
        labelText.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(labelText);
        Grid.SetColumn(field, 1);
        row.Children.Add(field);
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            var hint = MakeText(suffix, 9, Palette.Muted);
            hint.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(hint, 2);
            row.Children.Add(hint);
        }
        return row;
    }

    private static Control ToggleRow(CheckBox toggle, string title, string description)
    {
        var stack = new StackPanel { Spacing = 3 };
        toggle.Content = title;
        toggle.FontSize = 12;
        toggle.Foreground = Palette.Text;
        stack.Children.Add(toggle);
        stack.Children.Add(MakeText(description, 10, Palette.Muted));
        return stack;
    }

    private static TextBox Input(string value, string watermark = "", bool multiline = false)
    {
        return new TextBox
        {
            Text = value,
            PlaceholderText = watermark,
            AcceptsReturn = multiline,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
            VerticalContentAlignment = multiline ? VerticalAlignment.Top : VerticalAlignment.Center,
            Height = multiline ? 180 : 36,
            MinHeight = multiline ? 180 : 36,
            Background = Palette.Brush("#FF17140F"),
            Foreground = Palette.Text,
            BorderBrush = Palette.Brush("#99C98C4F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 7),
            FontSize = 12
        };
    }

    private static ComboBox Choice(IEnumerable<string> items, string selected)
    {
        return new ComboBox
        {
            ItemsSource = items.ToArray(),
            SelectedItem = selected,
            MinHeight = 36,
            Background = Palette.Brush("#FF17140F"),
            Foreground = Palette.Text,
            BorderBrush = Palette.Brush("#99C98C4F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 5),
            FontSize = 12
        };
    }

    private void PopulateControls()
    {
        _customResolution = new CheckBox
        {
            Content = "Use a custom resolution",
            IsChecked = _workingSettings.UseCustomResolution,
            Foreground = Palette.Text,
            FontSize = 12
        };
        _width = Input(_workingSettings.ResolutionWidth.ToString(CultureInfo.InvariantCulture), "1920");
        _height = Input(_workingSettings.ResolutionHeight.ToString(CultureInfo.InvariantCulture), "1080");
        _refresh = Input(_workingSettings.RefreshRate.ToString(CultureInfo.InvariantCulture), "0");
        _customResolution.IsCheckedChanged += (_, _) => UpdateResolutionFields();
        UpdateResolutionFields();
        _displayMode = Choice(new[] { "Default", "Fullscreen", "Windowed", "Borderless windowed" }, _workingSettings.DisplayMode);

        _skipIntro = new CheckBox { IsChecked = _workingSettings.SkipIntro };
        _console = new CheckBox { IsChecked = _workingSettings.ShowConsole };
        _developer = new CheckBox { IsChecked = _workingSettings.DeveloperMode };
        _disableJoystick = new CheckBox { IsChecked = _workingSettings.DisableJoystick };
        _disableSound = new CheckBox { IsChecked = _workingSettings.DisableSound };
        _highPriority = new CheckBox { IsChecked = _workingSettings.HighPriority };

        _directX = Choice(new[] { "Default", "70", "80", "81", "90", "95", "98" }, _workingSettings.DirectXLevel);
        _commands = Input(_workingSettings.AutoRunCommands, "map test_map\nexec autoexec.cfg\nsv_cheats 1", true);
        _extraArguments = Input(_workingSettings.ExtraArguments, "-novid -console -windowed");
    }

    private void UpdateResolutionFields()
    {
        var enabled = _customResolution.IsChecked == true;
        _width.IsEnabled = enabled;
        _height.IsEnabled = enabled;
    }

    private async Task SaveSettingsAsync()
    {
        var width = _workingSettings.ResolutionWidth;
        var height = _workingSettings.ResolutionHeight;
        if (_customResolution.IsChecked == true)
        {
            if (!int.TryParse(_width.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out width) || width < 640 || width > 7680)
            {
                await ShowValidationNotice("Enter a width between 640 and 7680 pixels.");
                return;
            }

            if (!int.TryParse(_height.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out height) || height < 480 || height > 4320)
            {
                await ShowValidationNotice("Enter a height between 480 and 4320 pixels.");
                return;
            }
        }

        if (!int.TryParse(_refresh.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var refresh) || refresh < 0 || refresh > 1000)
        {
            await ShowValidationNotice("Enter a refresh rate from 0 to 1000. Use 0 to leave it unchanged.");
            return;
        }

        var updated = new LauncherSettings
        {
            UseCustomResolution = _customResolution.IsChecked == true,
            ResolutionWidth = width,
            ResolutionHeight = height,
            DisplayMode = _displayMode.SelectedItem?.ToString() ?? "Default",
            RefreshRate = refresh,
            SkipIntro = _skipIntro.IsChecked == true,
            ShowConsole = _console.IsChecked == true,
            DeveloperMode = _developer.IsChecked == true,
            DisableJoystick = _disableJoystick.IsChecked == true,
            DisableSound = _disableSound.IsChecked == true,
            HighPriority = _highPriority.IsChecked == true,
            DirectXLevel = _directX.SelectedItem?.ToString() ?? "Default",
            AutoRunCommands = _commands.Text ?? string.Empty,
            ExtraArguments = _extraArguments.Text ?? string.Empty,
            ApplyToMultiplayer = false
        };

        try
        {
            _store.Save(updated);
            _workingSettings = updated;
            _onSaved(updated);
            Close();
        }
        catch (Exception ex)
        {
            await new NoticeWindow(
                "COULDN'T SAVE SETTINGS",
                "MultiSTOP couldn't write the settings file.",
                ex.Message,
                NoticeKind.Error).ShowDialog(this);
        }
    }

    private async Task ShowValidationNotice(string message)
    {
        await new NoticeWindow("CHECK YOUR SETTINGS", message, "The current values have not been saved.", NoticeKind.Warning).ShowDialog(this);
    }

    private void ResetDefaults()
    {
        // Detach the current view before replacing the controls it contains.
        _pageHost.Content = null;
        _pageCache.Clear();
        _workingSettings = new LauncherSettings();
        PopulateControls();
        SelectPage("Display");
    }

    private static LauncherSettings Copy(LauncherSettings source)
    {
        return new LauncherSettings
        {
            UseCustomResolution = source.UseCustomResolution,
            ResolutionWidth = source.ResolutionWidth,
            ResolutionHeight = source.ResolutionHeight,
            DisplayMode = source.DisplayMode,
            RefreshRate = source.RefreshRate,
            SkipIntro = source.SkipIntro,
            ShowConsole = source.ShowConsole,
            DeveloperMode = source.DeveloperMode,
            DisableJoystick = source.DisableJoystick,
            DisableSound = source.DisableSound,
            HighPriority = source.HighPriority,
            DirectXLevel = source.DirectXLevel,
            AutoRunCommands = source.AutoRunCommands,
            ExtraArguments = source.ExtraArguments,
            ApplyToMultiplayer = false
        };
    }
}
