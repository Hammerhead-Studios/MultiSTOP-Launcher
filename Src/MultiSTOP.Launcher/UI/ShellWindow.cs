using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using MultiSTOP.Launcher.Theme;

namespace MultiSTOP.Launcher.UI;

internal abstract class ShellWindow : Window
{
    protected ShellWindow(string title, double width, double height, bool canResize = false)
    {
        Title = title;
        Width = width;
        Height = height;
        MinWidth = width;
        MinHeight = height;
        CanResize = canResize;
        WindowDecorations = Avalonia.Controls.WindowDecorations.None;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Palette.Ink;
        ShowInTaskbar = true;

        try
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://MultiSTOP/Assets/MultiSTOP.ico")));
        }
        catch
        {
            // The executable still works if a shell icon cannot be decoded by the host OS.
        }
    }

    protected Border CreateTitleBar(string caption, bool showMinimize = true)
    {
        var columns = showMinimize ? "*,Auto,Auto" : "*,Auto";
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(columns),
            Height = 42,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        var name = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Spacing = 10,
            Margin = new Thickness(18, 0, 0, 0)
        };
        name.Children.Add(new TextBlock
        {
            Text = "◆",
            FontSize = 15,
            Foreground = Palette.Gold,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });
        name.Children.Add(new TextBlock
        {
            Text = caption,
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = Palette.Text,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            LetterSpacing = 1.1
        });
        Grid.SetColumn(name, 0);
        row.Children.Add(name);

        var column = 1;
        if (showMinimize)
        {
            var minimize = ChromeButton("—", "Minimize");
            minimize.Click += (_, _) => WindowState = WindowState.Minimized;
            Grid.SetColumn(minimize, column++);
            row.Children.Add(minimize);
        }

        var close = ChromeButton("×", "Close");
        close.Click += (_, _) => Close();
        Grid.SetColumn(close, column);
        row.Children.Add(close);

        // Keep drag handling on the title text area so the window controls remain clickable.
        name.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        return new Border
        {
            Background = Palette.Brush("#B80D0B08"),
            BorderBrush = Palette.Brush("#33FFE0A0"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = row
        };
    }

    protected static Button ChromeButton(string text, string tooltip)
    {
        var button = new Button
        {
            Content = text,
            Width = 42,
            Height = 32,
            Margin = new Thickness(0, 4, 5, 4),
            Padding = new Thickness(0),
            FontSize = 18,
            FontWeight = FontWeight.Normal,
            Background = Brushes.Transparent,
            Foreground = Palette.Text,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(5),
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        ToolTip.SetTip(button, tooltip);
        button.PointerEntered += (_, _) => button.Background = Palette.Brush("#55FFB13D");
        button.PointerExited += (_, _) => button.Background = Brushes.Transparent;
        return button;
    }

    protected static TextBlock MakeText(
        string text,
        double size = 14,
        IBrush? color = null,
        FontWeight? weight = null)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = size,
            Foreground = color ?? Palette.Text,
            FontWeight = weight ?? FontWeight.Normal,
            FontFamily = new FontFamily("Segoe UI"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    protected static Border MakeCard(Control child, Thickness? padding = null)
    {
        return new Border
        {
            Background = Palette.Panel,
            BorderBrush = Palette.Line,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(11),
            Padding = padding ?? new Thickness(18),
            Child = child
        };
    }

    protected static Button MakeActionButton(
        string text,
        bool primary = false,
        bool enabled = true,
        double height = 44)
    {
        var normal = primary ? Palette.Orange : Palette.Brush("#D52A241A");
        var hover = primary ? Palette.Gold : Palette.Brush("#EE3C3022");
        var button = new Button
        {
            Content = text,
            Height = height,
            Padding = new Thickness(18, 8),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Background = enabled ? normal : Palette.Brush("#88433D34"),
            Foreground = enabled ? (primary ? Palette.Ink : Palette.Text) : Palette.DisabledText,
            BorderBrush = enabled ? (primary ? Palette.Gold : Palette.Brush("#99D49A54")) : Palette.Brush("#334C463D"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            IsEnabled = enabled
        };

        if (enabled)
        {
            button.PointerEntered += (_, _) => button.Background = hover;
            button.PointerExited += (_, _) => button.Background = normal;
        }

        return button;
    }
}
