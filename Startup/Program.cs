using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Squirrel;

namespace AudioPlayer;

internal static class Program
{
    private const string AppId = "Spectralis";
    private const string AppExecutableName = "Spectralis.exe";
    private const string UpdateFeedUrl = "https://cdn.deltavdevs.com/spectralis";

    [STAThread]
    static void Main(string[] args)
    {
        SquirrelAwareApp.HandleEvents(
            onInitialInstall: _ => WithUpdateManager(manager =>
                manager.CreateShortcutsForExecutable(
                    AppExecutableName,
                    ShortcutLocation.StartMenu | ShortcutLocation.Desktop,
                    false)),
            onAppUpdate: _ => WithUpdateManager(manager =>
                manager.CreateShortcutsForExecutable(
                    AppExecutableName,
                    ShortcutLocation.StartMenu | ShortcutLocation.Desktop,
                    true)),
            onAppObsoleted: _ => { },
            onAppUninstall: _ => WithUpdateManager(manager =>
                manager.RemoveShortcutsForExecutable(
                    AppExecutableName,
                    ShortcutLocation.StartMenu | ShortcutLocation.Desktop)),
            onFirstRun: () => { },
            arguments: args);

        var filteredArgs = args
            .Where(static argument => !argument.StartsWith("--squirrel", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var startupPath = filteredArgs
            .Select(TryGetExistingFilePath)
            .FirstOrDefault(static path => path is not null);

        ApplicationConfiguration.Initialize();

        var mainForm = new Form1(startupPath);
        var updateCheckStarted = false;
        mainForm.Shown += (_, _) =>
        {
            if (updateCheckStarted)
                return;

            updateCheckStarted = true;
            _ = CheckForUpdatesAsync();
        };

        Application.Run(mainForm);
    }

    private static void WithUpdateManager(Action<UpdateManager> action)
    {
        using var manager = new UpdateManager(UpdateFeedUrl, AppId, null, null);
        action(manager);
    }

    private static async Task CheckForUpdatesAsync()
    {
        try
        {
            using var manager = new UpdateManager(UpdateFeedUrl, AppId, null, null);
            if (!manager.IsInstalledApp)
                return;

            var updateInfo = await manager.CheckForUpdate();
            if (updateInfo.ReleasesToApply.Count == 0)
                return;

            await manager.DownloadReleases(updateInfo.ReleasesToApply);
            await manager.ApplyReleases(updateInfo);
            UpdateManager.RestartApp();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Squirrel update check failed: {ex}");
        }
    }

    private static string? TryGetExistingFilePath(string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return null;

        try
        {
            var candidatePath = Path.GetFullPath(argument.Trim('"'));
            return File.Exists(candidatePath) ? candidatePath : null;
        }
        catch
        {
            return null;
        }
    }
}
