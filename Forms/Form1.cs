using System.Drawing;
using System.IO;

namespace AudioPlayer;

public partial class Form1 : Form
{
    private readonly AudioEngine engine = new();
    private readonly WindowsNowPlayingService nowPlaying = new();
    private readonly string? startupPath;

    private AppSettings appSettings;
    private ThemePalette themePalette;
    private ThemeMode appliedThemeMode;
    private ThemeAccent appliedThemeAccent;
    private bool hasAppliedTheme;
    private bool isUpdatingSeekBar;
    private bool isApplyingSettings;
    private bool showRemainingTime;
    private bool isMuted;
    private float preMuteVolume = 0.85f;
    private AudioTrackInfo? displayedArtworkTrack;
    private Image? visualizerAlbumArt;
    private long nextVisualizerCycleTick;

    public Form1(string? startupPath = null)
    {
        this.startupPath = startupPath;
        appSettings = AppSettingsStore.Load();
        themePalette = ThemePalette.Create(appSettings.ThemeMode, appSettings.ThemeAccent);

        InitializeComponent();
        HandleCreated += (_, _) =>
        {
            ApplyTheme();
            InitializeNowPlaying();
        };
        nextVisualizerCycleTick = Environment.TickCount64 + (long)VisualizerAutoCycleInterval.TotalMilliseconds;
        ApplyTheme();
        PopulateSettings();
        WireFileDrop(this);
        UpdateUiState();
    }

    private Color WindowBackColor => themePalette.WindowBackColor;
    private Color SurfaceBackColor => themePalette.SurfaceBackColor;
    private Color SurfaceAltBackColor => themePalette.SurfaceAltBackColor;
    private Color SurfaceRaisedColor => themePalette.SurfaceRaisedColor;
    private Color TextPrimaryColor => themePalette.TextPrimaryColor;
    private Color TextSecondaryColor => themePalette.TextSecondaryColor;
    private Color TextSoftColor => themePalette.TextSoftColor;
    private Color TextMutedColor => themePalette.TextMutedColor;
    private Color AccentPrimaryColor => themePalette.AccentPrimaryColor;
    private Color AccentSecondaryColor => themePalette.AccentSecondaryColor;
    private Color AccentSoftColor => themePalette.AccentSoftColor;
    private Color AccentContrastColor => themePalette.AccentContrastColor;
    private Color DangerColor => themePalette.DangerColor;
    private Color DangerTextColor => themePalette.DangerTextColor;
    private Color StatusBorderColor => themePalette.BorderStrongColor;
    private TimeSpan VisualizerAutoCycleInterval => TimeSpan.FromSeconds(appSettings.VisualizerCycleSeconds);

    private void Form1_Load(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(startupPath))
            return;

        if (File.Exists(startupPath))
        {
            LoadAudioFile(startupPath, appSettings.AutoPlayOnOpen);
            return;
        }

        ShowError(
            $"The startup file could not be found:{Environment.NewLine}{Environment.NewLine}{startupPath}",
            "Open Error");
    }
}
