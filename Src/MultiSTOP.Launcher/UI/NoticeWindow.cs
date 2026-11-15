using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using MultiSTOP.Launcher.Theme;

namespace MultiSTOP.Launcher.UI;

internal enum NoticeKind
{
    Information,
    Warning,
    Error
}

internal sealed class NoticeWindow : ShellWindow
{
    public NoticeWindow(
        string title,
        string message,
        string details,
        NoticeKind kind = NoticeKind.Information,
        string actionText = "GOT IT")
        : base("MultiSTOP | " + title, 540, 360)
    {
        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("42,*,Auto")
        };
        root.Children.Add(CreateTitleBar("MULTISTOP  /  " + title));

        var content = new StackPanel
        {
            Margin = new Thickness(28, 26, 28, 18),
            Spacing = 13
        };
        content.Children.Add(new TextBlock
        {
            Text = kind switch
            {
                NoticeKind.Error => "!",
                NoticeKind.Warning => "△",
                _ => "i"
            },
            FontSize = 25,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = kind == NoticeKind.Error ? Palette.Orange : Palette.Gold
        });
        content.Children.Add(MakeText(message, 19, Palette.Text, Avalonia.Media.FontWeight.SemiBold));
        content.Children.Add(new Border
        {
            Background = Palette.Brush("#A817140F"),
            BorderBrush = Palette.Brush("#66D5A04D"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Padding = new Thickness(12),
            Child = MakeText(details, 12, Palette.Muted)
        });
        Grid.SetRow(content, 1);
        root.Children.Add(content);

        var footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(20, 0, 22, 18)
        };
        var close = MakeActionButton(actionText, primary: true, height: 40);
        close.MinWidth = 130;
        close.Click += (_, _) => Close();
        footer.Children.Add(close);
        Grid.SetRow(footer, 2);
        root.Children.Add(footer);

        Content = root;
    }
}
