using System.Drawing;

namespace AudioPlayer;

public sealed class LyricsViewControl : UserControl
{
    private static readonly Color SurfaceColor = Color.FromArgb(28, 24, 30);
    private static readonly Color EmphasisSurfaceColor = Color.FromArgb(36, 31, 37);
    private static readonly Color CaptionColor = Color.FromArgb(156, 122, 91);
    private static readonly Color BodyColor = Color.FromArgb(245, 237, 228);
    private static readonly Color MutedLineColor = Color.FromArgb(153, 136, 123);
    private static readonly Color PlayedSegmentColor = Color.FromArgb(226, 208, 186);
    private static readonly Color ActiveSegmentColor = Color.FromArgb(246, 183, 93);
    private static readonly Color UpcomingSegmentColor = Color.FromArgb(184, 164, 149);

    private readonly Font currentLineFont = new("Segoe UI Semibold", 16.5F, FontStyle.Bold, GraphicsUnit.Point);
    private readonly Font sideLineFont = new("Segoe UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point);
    private readonly Font emptyTitleFont = new("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point);
    private readonly Font emptyBodyFont = new("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

    private readonly TableLayoutPanel rootLayout;
    private readonly TableLayoutPanel headerLayout;
    private readonly Label lblCaption;
    private readonly Label lblSource;
    private readonly Label lblPrevious;
    private readonly Panel currentHostPanel;
    private readonly FlowLayoutPanel currentLinePanel;
    private readonly TableLayoutPanel emptyStateLayout;
    private readonly Label lblEmptyTitle;
    private readonly Label lblEmptyBody;
    private readonly Label lblNext;
    private readonly List<Label> segmentLabels = [];

    private AudioTrackInfo? currentTrack;
    private LyricsLine? displayedLine;
    private int displayedLineIndex = int.MinValue;
    private int displayedSegmentIndex = int.MinValue;
    private double currentPositionSeconds;

    public LyricsViewControl()
    {
        DoubleBuffered = true;
        BackColor = SurfaceColor;
        Padding = new Padding(18, 16, 18, 16);

        rootLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            RowCount = 4
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));

        headerLayout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            RowCount = 1
        };
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        lblCaption = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = CaptionColor,
            Margin = Padding.Empty,
            Text = "LYRICS"
        };

        lblSource = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = CaptionColor,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.MiddleRight
        };

        lblPrevious = new Label
        {
            Dock = DockStyle.Fill,
            Font = sideLineFont,
            ForeColor = MutedLineColor,
            Margin = new Padding(0, 10, 0, 0),
            Padding = Padding.Empty,
            TextAlign = ContentAlignment.BottomLeft,
            UseMnemonic = false
        };

        currentHostPanel = new Panel
        {
            BackColor = EmphasisSurfaceColor,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 10),
            Padding = new Padding(18, 16, 18, 16)
        };

        currentLinePanel = new FlowLayoutPanel
        {
            BackColor = EmphasisSurfaceColor,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            WrapContents = true
        };

        emptyStateLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            RowCount = 2
        };
        emptyStateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        emptyStateLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        emptyStateLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        lblEmptyTitle = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Font = emptyTitleFont,
            ForeColor = BodyColor,
            Margin = Padding.Empty,
            Text = "No track loaded",
            UseMnemonic = false
        };

        lblEmptyBody = new Label
        {
            Dock = DockStyle.Fill,
            Font = emptyBodyFont,
            ForeColor = UpcomingSegmentColor,
            Margin = new Padding(0, 8, 0, 0),
            Text =
                "Open a song with embedded LRC data or a matching sidecar .lrc file to see synced lyrics here.",
            UseMnemonic = false
        };

        lblNext = new Label
        {
            Dock = DockStyle.Fill,
            Font = sideLineFont,
            ForeColor = MutedLineColor,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            TextAlign = ContentAlignment.TopLeft,
            UseMnemonic = false
        };

        headerLayout.Controls.Add(lblCaption, 0, 0);
        headerLayout.Controls.Add(lblSource, 1, 0);

        emptyStateLayout.Controls.Add(lblEmptyTitle, 0, 0);
        emptyStateLayout.Controls.Add(lblEmptyBody, 0, 1);

        currentHostPanel.Controls.Add(currentLinePanel);
        currentHostPanel.Controls.Add(emptyStateLayout);

        rootLayout.Controls.Add(headerLayout, 0, 0);
        rootLayout.Controls.Add(lblPrevious, 0, 1);
        rootLayout.Controls.Add(currentHostPanel, 0, 2);
        rootLayout.Controls.Add(lblNext, 0, 3);

        Controls.Add(rootLayout);

        currentHostPanel.SizeChanged += (_, _) => RefreshSegmentLabelBounds();
        ShowEmptyState();
    }

    public void UpdateState(AudioTrackInfo? track, double positionSeconds)
    {
        var trackChanged = !ReferenceEquals(currentTrack, track);
        currentTrack = track;
        currentPositionSeconds = Math.Max(0, positionSeconds);

        if (trackChanged)
        {
            displayedLine = null;
            displayedLineIndex = int.MinValue;
            displayedSegmentIndex = int.MinValue;
        }

        RefreshDisplay(trackChanged);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            currentLineFont.Dispose();
            sideLineFont.Dispose();
            emptyTitleFont.Dispose();
            emptyBodyFont.Dispose();
        }

        base.Dispose(disposing);
    }

    private void RefreshDisplay(bool forceRebuild)
    {
        var lyrics = currentTrack?.Lyrics;
        if (lyrics is null || !lyrics.HasLines)
        {
            ShowEmptyState();
            return;
        }

        ShowLyricsState(lyrics);

        var lineIndex = lyrics.FindLineIndex(currentPositionSeconds);
        var lineToShow = lineIndex >= 0
            ? lyrics.Lines[lineIndex]
            : lyrics.Lines[0];

        if (forceRebuild || displayedLineIndex != lineIndex || !ReferenceEquals(displayedLine, lineToShow))
        {
            displayedLineIndex = lineIndex;
            displayedLine = lineToShow;
            displayedSegmentIndex = int.MinValue;
            RebuildLineContext(lyrics, lineIndex, lineToShow);
        }

        UpdateSegmentHighlight(lineToShow, lineIndex);
    }

    private void ShowEmptyState()
    {
        lblCaption.Text = "LYRICS";
        lblSource.Text = string.Empty;

        lblPrevious.Visible = false;
        lblNext.Visible = false;
        currentLinePanel.Visible = false;
        emptyStateLayout.Visible = true;

        if (currentTrack is null)
        {
            lblEmptyTitle.Text = "No track loaded";
            lblEmptyBody.Text =
                "Open a song with embedded LRC data or a matching sidecar .lrc file to see synced lyrics here.";
        }
        else
        {
            lblEmptyTitle.Text = "No synced lyrics";
            lblEmptyBody.Text =
                "This track doesn't expose embedded LRC data and no matching sidecar .lrc file was found.";
        }

        currentLinePanel.Controls.Clear();
        segmentLabels.Clear();
        displayedLine = null;
        displayedLineIndex = int.MinValue;
        displayedSegmentIndex = int.MinValue;
    }

    private void ShowLyricsState(LyricsDocument lyrics)
    {
        lblCaption.Text = lyrics.HasWordTimings ? "ENHANCED LYRICS" : "SYNCED LYRICS";
        lblSource.Text = lyrics.SourceLabel.ToUpperInvariant();

        lblPrevious.Visible = true;
        lblNext.Visible = true;
        currentLinePanel.Visible = true;
        emptyStateLayout.Visible = false;
    }

    private void RebuildLineContext(LyricsDocument lyrics, int lineIndex, LyricsLine lineToShow)
    {
        lblPrevious.Text = lineIndex > 0
            ? lyrics.Lines[lineIndex - 1].Text
            : string.Empty;
        lblPrevious.Visible = lblPrevious.Text.Length > 0;

        lblNext.Text = lineIndex < 0
            ? lyrics.Lines.Count > 1 ? lyrics.Lines[1].Text : string.Empty
            : lineIndex < lyrics.Lines.Count - 1 ? lyrics.Lines[lineIndex + 1].Text : string.Empty;
        lblNext.Visible = lblNext.Text.Length > 0;

        currentLinePanel.SuspendLayout();
        currentLinePanel.Controls.Clear();
        segmentLabels.Clear();

        var segments = lineToShow.Segments.Count > 0
            ? lineToShow.Segments
            : [new LyricsSegment(lineToShow.StartTime, lineToShow.Text)];
        var wrapWidth = GetSegmentWrapWidth();

        foreach (var segment in segments)
        {
            var label = new Label
            {
                AutoSize = true,
                BackColor = EmphasisSurfaceColor,
                Font = currentLineFont,
                ForeColor = UpcomingSegmentColor,
                Margin = Padding.Empty,
                MaximumSize = new Size(wrapWidth, 0),
                Padding = Padding.Empty,
                Text = segment.Text,
                UseMnemonic = false
            };

            currentLinePanel.Controls.Add(label);
            segmentLabels.Add(label);
        }

        currentLinePanel.ResumeLayout();
        RefreshSegmentLabelBounds();
    }

    private void UpdateSegmentHighlight(LyricsLine lineToShow, int lineIndex)
    {
        var activeSegmentIndex = lineIndex >= 0
            ? lineToShow.FindActiveSegmentIndex(currentPositionSeconds)
            : -1;

        if (displayedSegmentIndex == activeSegmentIndex)
        {
            return;
        }

        displayedSegmentIndex = activeSegmentIndex;

        for (var i = 0; i < segmentLabels.Count; i++)
        {
            segmentLabels[i].ForeColor = lineIndex < 0
                ? UpcomingSegmentColor
                : i < activeSegmentIndex ? PlayedSegmentColor
                : i == activeSegmentIndex ? ActiveSegmentColor
                : UpcomingSegmentColor;
        }
    }

    private void RefreshSegmentLabelBounds()
    {
        if (segmentLabels.Count == 0)
        {
            return;
        }

        var wrapWidth = GetSegmentWrapWidth();
        foreach (var label in segmentLabels)
        {
            label.MaximumSize = new Size(wrapWidth, 0);
        }
    }

    private int GetSegmentWrapWidth() =>
        Math.Max(160, currentHostPanel.ClientSize.Width - currentHostPanel.Padding.Horizontal - 4);
}
