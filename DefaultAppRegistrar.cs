using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace AudioPlayer;

public static class DefaultAppRegistrar
{
    private const string AppDisplayName = "Audio Player";
    private const string AppCapabilitiesPath = @"Software\AudioPlayer\Capabilities";
    private const string ProgId = "AudioPlayer.AudioFile";

    public static void RegisterCurrentUser()
    {
        var executablePath = Application.ExecutablePath;
        var executableName = Path.GetFileName(executablePath);
        var openCommand = $"\"{executablePath}\" \"%1\"";

        using var applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\Applications\{executableName}");
        applicationKey?.SetValue("FriendlyAppName", AppDisplayName);

        using (var commandKey = applicationKey?.CreateSubKey(@"shell\open\command"))
        {
            commandKey?.SetValue(string.Empty, openCommand);
        }

        using (var supportedTypesKey = applicationKey?.CreateSubKey("SupportedTypes"))
        {
            foreach (var extension in SupportedAudioFormats.Extensions)
            {
                supportedTypesKey?.SetValue(extension, string.Empty);
            }
        }

        using var progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}");
        progIdKey?.SetValue(string.Empty, $"{AppDisplayName} Audio File");

        using (var defaultIconKey = progIdKey?.CreateSubKey("DefaultIcon"))
        {
            defaultIconKey?.SetValue(string.Empty, $"\"{executablePath}\",0");
        }

        using (var progIdCommandKey = progIdKey?.CreateSubKey(@"shell\open\command"))
        {
            progIdCommandKey?.SetValue(string.Empty, openCommand);
        }

        using var capabilitiesKey = Registry.CurrentUser.CreateSubKey(AppCapabilitiesPath);
        capabilitiesKey?.SetValue("ApplicationName", AppDisplayName);
        capabilitiesKey?.SetValue(
            "ApplicationDescription",
            "Desktop audio player with a live visualizer, drag-and-drop support, and broad Windows codec compatibility.");

        using (var fileAssociationsKey = capabilitiesKey?.CreateSubKey("FileAssociations"))
        {
            foreach (var extension in SupportedAudioFormats.Extensions)
            {
                fileAssociationsKey?.SetValue(extension, ProgId);
            }
        }

        using var registeredApplicationsKey = Registry.CurrentUser.CreateSubKey(@"Software\RegisteredApplications");
        registeredApplicationsKey?.SetValue(AppDisplayName, AppCapabilitiesPath);
    }

    public static void OpenDefaultAppsSettings()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "ms-settings:defaultapps",
            UseShellExecute = true
        });
    }
}
