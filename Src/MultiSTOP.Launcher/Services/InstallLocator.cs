using MultiSTOP.Launcher.Models;

namespace MultiSTOP.Launcher.Services;

internal static class InstallLocator
{
    public static string GetRootFolder() => Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);

    /// <summary>
    /// Searches every folder below the directory containing MultiSTOP.exe for supported game executables.
    /// Reparse points are skipped so directory junctions and symbolic links cannot create scan loops.
    /// </summary>
    public static IReadOnlyList<GameInstallation> FindGameInstallations()
    {
        var root = GetRootFolder();
        var found = new List<GameInstallation>();

        if (!Directory.Exists(root))
            return found;

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            ReturnSpecialDirectories = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };

        try
        {
            foreach (var file in Directory.EnumerateFiles(root, "*", options))
            {
                var fileName = Path.GetFileName(file);
                string displayName;

                if (string.Equals(fileName, "p2008.exe", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = "F-STOP";
                }
                else if (string.Equals(fileName, "valve.exe", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = "Aperture Camera / Sandbox";
                }
                else
                {
                    continue;
                }

                var relativePath = Path.GetRelativePath(root, file);
                found.Add(new GameInstallation(displayName, Path.GetFullPath(file), relativePath));
            }
        }
        catch (IOException)
        {
            // Keep any valid matches collected before a folder became inaccessible.
        }
        catch (UnauthorizedAccessException)
        {
            // Keep any valid matches collected before an access restriction was hit.
        }

        return found
            .OrderBy(item => string.Equals(item.DisplayName, "F-STOP", StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
