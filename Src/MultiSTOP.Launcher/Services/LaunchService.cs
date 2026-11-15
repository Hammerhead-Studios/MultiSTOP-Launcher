using System.Diagnostics;
using System.Globalization;
using System.Text;
using MultiSTOP.Launcher.Models;

namespace MultiSTOP.Launcher.Services;

internal static class LaunchService
{
    public static string? Launch(string executablePath, LauncherSettings settings)
    {
        if (!File.Exists(executablePath))
            throw new FileNotFoundException("The selected game executable could not be found.", executablePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = Path.GetDirectoryName(executablePath)
                              ?? throw new InvalidOperationException("The game folder could not be resolved."),
            UseShellExecute = false
        };

        foreach (var argument in BuildArguments(settings))
            startInfo.ArgumentList.Add(argument);

        var process = Process.Start(startInfo)
                      ?? throw new InvalidOperationException("Windows did not return a game process.");

        if (!settings.HighPriority)
        {
            process.Dispose();
            return null;
        }

        try
        {
            process.PriorityClass = ProcessPriorityClass.High;
            process.Dispose();
            return null;
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or
                                   UnauthorizedAccessException)
        {
            process.Dispose();
            return "F-STOP started, but Windows didn't allow MultiSTOP to change its process priority.\n\n" + ex.Message;
        }
    }

    internal static IReadOnlyList<string> BuildArguments(LauncherSettings settings)
    {
        var result = new List<string>();

        if (settings.UseCustomResolution)
        {
            result.Add("-w");
            result.Add(settings.ResolutionWidth.ToString(CultureInfo.InvariantCulture));
            result.Add("-h");
            result.Add(settings.ResolutionHeight.ToString(CultureInfo.InvariantCulture));
        }

        switch (settings.DisplayMode)
        {
            case "Fullscreen":
                result.Add("-fullscreen");
                break;
            case "Windowed":
                result.Add("-windowed");
                break;
            case "Borderless windowed":
                result.Add("-windowed");
                result.Add("-noborder");
                break;
        }

        if (settings.RefreshRate > 0)
        {
            result.Add("-refresh");
            result.Add(settings.RefreshRate.ToString(CultureInfo.InvariantCulture));
        }

        if (settings.SkipIntro) result.Add("-novid");
        if (settings.ShowConsole) result.Add("-console");
        if (settings.DeveloperMode) result.Add("-dev");
        if (settings.DisableJoystick) result.Add("-nojoy");
        if (settings.DisableSound) result.Add("-nosound");

        if (!string.Equals(settings.DirectXLevel, "Default", StringComparison.OrdinalIgnoreCase))
        {
            result.Add("-dxlevel");
            result.Add(settings.DirectXLevel);
        }

        foreach (var commandLine in settings.AutoRunCommands
                     .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var command = ParseArguments(commandLine.Trim());
            if (command.Count == 0)
                continue;

            if (command[0].StartsWith("+", StringComparison.Ordinal))
            {
                result.AddRange(command);
            }
            else
            {
                result.Add("+" + command[0]);
                result.AddRange(command.Skip(1));
            }
        }

        result.AddRange(ParseArguments(settings.ExtraArguments));
        return result;
    }

    // Small Windows-style tokenizer so quoted values in the Extra Arguments box stay together.
    internal static List<string> ParseArguments(string input)
    {
        var arguments = new List<string>();
        var current = new StringBuilder();
        var quoted = false;

        for (var index = 0; index < input.Length; index++)
        {
            var character = input[index];

            if (character == '\\' && quoted && index + 1 < input.Length &&
                (input[index + 1] == '"' || input[index + 1] == '\\'))
            {
                current.Append(input[++index]);
                continue;
            }

            if (character == '"')
            {
                quoted = !quoted;
                continue;
            }

            if (char.IsWhiteSpace(character) && !quoted)
            {
                if (current.Length > 0)
                {
                    arguments.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(character);
        }

        if (current.Length > 0)
            arguments.Add(current.ToString());

        return arguments;
    }
}