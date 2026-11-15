using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MultiSTOP.Launcher.Theme;

namespace MultiSTOP.Launcher.UI;

internal sealed class AboutWindow : ShellWindow
{
    private readonly ContentControl _content = new();
    private readonly Dictionary<string, Button> _tabs = new();

    private static readonly IReadOnlyDictionary<string, (string Subtitle, string Body)> Sections =
        new Dictionary<string, (string, string)>
        {
            ["MultiSTOP"] = (
                "A front door for supported builds on your machine.",
                "MultiSTOP is a community-made launcher for F-STOP and compatible Aperture Camera / Aperture Sandbox installations, put together under the Hammerhead Studios name. The idea is simple: keep the usual way of starting a local build close at hand, and put the less frequently changed launch settings somewhere easier to manage than a shortcut full of command-line switches.\n\n" +
                "When it starts, MultiSTOP scans the folders beneath its own location for supported executables. It recognises p2008.exe as F-STOP and valve.exe as an Aperture Camera / Aperture Sandbox installation. When more than one match is found, the main screen lets you choose which one to launch. The selected program is started from its own folder, with the options saved in Launch Options. Those options cover display size and mode, refresh rate, common startup switches, an optional DirectX level, extra arguments, and console commands to run as the game starts.\n\n" +
                "The Multiplayer button is part of the main screen, but it is deliberately unavailable at this stage. The button is there because multiplayer is part of the project's plans; it is not a promise that the current launcher can connect to a working multiplayer session. That work is still in progress, so the control stays disabled until there is something ready to launch.\n\n" +
                "MultiSTOP is intended to sit in a folder above the supported game's files. It does not bundle F-STOP or Aperture Camera / Aperture Sandbox, replace their executables, or install game content. Your launcher preferences are kept separately in your Windows user profile, so they can be changed without editing any game directory. The goal is to keep the launcher out of the way and let the selected game remain the main event."),

            ["Hammerhead Studios"] = (
                "The name behind MultiSTOP.",
                "Hammerhead Studios is the project name attached to MultiSTOP and its presentation. It is used here as the creator credit for the launcher, its interface, and the custom artwork prepared for this project.\n\n" +
                "This launcher is being built around a specific community project, so the focus is on making the everyday parts feel finished: a clear start screen, settings that are easy to find, useful error messages when something is missing, and a layout that does not get in the way of the game. Small details matter too. Buttons should tell you what they do, unavailable features should look unavailable, and a failed launch should explain what went wrong instead of leaving you guessing.\n\n" +
                "The studio credit does not mean that MultiSTOP is an official Valve product. Hammerhead Studios is an independent name used for this work, and this launcher is not endorsed by or affiliated with Valve. Names and marks belonging to their respective owners remain theirs.\n\n" +
                "© 2026 Hammerhead Studios. MultiSTOP launcher code, interface design, and original launcher artwork are credited to Hammerhead Studios unless a component is identified otherwise."),

            ["F-STOP"] = (
                "The game this launcher was made to start.",
                "F-STOP is an unreleased Valve game. MultiSTOP is being made as an independent launcher for people who already have a local copy of its files and want a straightforward way to start it. The launcher looks for F-STOP's p2008.exe anywhere beneath the folder where MultiSTOP.exe is placed, so the game's folders do not have to use one exact layout.\n\n" +
                "This project does not include the game itself. It does not claim ownership of F-STOP, its code, its characters, its artwork, or any other Valve material. The game and the names connected to it remain the property of their respective owners. MultiSTOP is a separate community utility and is not an official Valve release.\n\n" +
                "F-STOP's own behavior can depend on the particular build being used. Some launch switches may not be supported by every version, and a setting that works on one build may have no visible effect on another. For that reason, the launcher leaves most options under your control and provides a way to pass additional arguments when a build needs something specific.\n\n" +
                "The Multiplayer entry in MultiSTOP is not active yet. Until the multiplayer work is ready, the launcher starts the normal game executable selected on the main screen. Any future multiplayer support will be handled as a separate launch path rather than pretending that the disabled button already works.")
        };

    public AboutWindow() : base("MultiSTOP | About", 900, 620)
    {
        var root = new Grid { RowDefinitions = new RowDefinitions("42,*,42") };
        root.Children.Add(CreateTitleBar("MULTISTOP  /  ABOUT"));

        var main = new Grid { ColumnDefinitions = new ColumnDefinitions("190,*") };
        var sidebar = new Border
        {
            Background = Palette.Brush("#FF17140F"),
            BorderBrush = Palette.Brush("#55D99D4E"),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(16, 20)
        };
        var sideStack = new StackPanel { Spacing = 8 };
        sideStack.Children.Add(MakeText("ABOUT MULTISTOP", 10, Palette.Gold, FontWeight.Bold));
        sideStack.Children.Add(MakeText("A little more about the launcher, the people behind it, and the game it starts.", 10, Palette.Muted));
        sideStack.Children.Add(new Border { Height = 1, Background = Palette.Brush("#55D99D4E"), Margin = new Thickness(0, 8, 0, 8) });
        AddTab(sideStack, "MultiSTOP");
        AddTab(sideStack, "Hammerhead Studios");
        AddTab(sideStack, "F-STOP");
        sidebar.Child = sideStack;
        main.Children.Add(sidebar);

        var contentBorder = new Border
        {
            Background = Palette.Brush("#FF211C15"),
            Padding = new Thickness(27, 25),
            Child = _content
        };
        Grid.SetColumn(contentBorder, 1);
        main.Children.Add(contentBorder);
        Grid.SetRow(main, 1);
        root.Children.Add(main);

        var footer = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        var credit = MakeText("MULTISTOP 0.1.0 BETA  ·  © 2026 HAMMERHEAD STUDIOS", 9, Palette.Muted);
        credit.VerticalAlignment = VerticalAlignment.Center;
        credit.Margin = new Thickness(17, 0);
        footer.Children.Add(credit);
        var done = MakeActionButton("DONE", primary: true, height: 30);
        done.FontSize = 10;
        done.MinWidth = 86;
        done.Margin = new Thickness(0, 4, 12, 4);
        done.Click += (_, _) => Close();
        Grid.SetColumn(done, 1);
        footer.Children.Add(done);
        Grid.SetRow(footer, 2);
        root.Children.Add(footer);

        Content = root;
        SelectTab("MultiSTOP");
    }

    private void AddTab(Panel parent, string name)
    {
        var button = new Button
        {
            Content = name.ToUpperInvariant(),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = Palette.Brush("#0017140F"),
            Foreground = Palette.Text,
            BorderBrush = Palette.Brush("#003D3324"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Padding = new Thickness(11, 12),
            FontSize = 10,
            FontWeight = FontWeight.SemiBold
        };
        button.Click += (_, _) => SelectTab(name);
        _tabs[name] = button;
        parent.Children.Add(button);
    }

    private void SelectTab(string name)
    {
        foreach (var tab in _tabs)
        {
            var selected = tab.Key == name;
            tab.Value.Background = selected ? Palette.Brush("#FF3A2B1A") : Palette.Brush("#0017140F");
            tab.Value.BorderBrush = selected ? Palette.Brush("#CCF3A94D") : Palette.Brush("#003D3324");
        }

        var section = Sections[name];
        var stack = new StackPanel { Spacing = 16 };
        stack.Children.Add(MakeText(name.ToUpperInvariant(), 10, Palette.Gold, FontWeight.Bold));
        stack.Children.Add(MakeText(section.Subtitle, 22, Palette.Text, FontWeight.SemiBold));
        stack.Children.Add(new Border { Height = 1, Background = Palette.Brush("#55D99D4E") });
        stack.Children.Add(new TextBlock
        {
            Text = section.Body,
            FontSize = 13,
            LineHeight = 22,
            Foreground = Palette.Brush("#FFE1D5C3"),
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Segoe UI")
        });
        _content.Content = new ScrollViewer
        {
            Content = stack,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };
    }
}
