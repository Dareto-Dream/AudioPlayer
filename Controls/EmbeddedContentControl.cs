using System.Text;
using Markdig;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Spectrallis;

public sealed class EmbeddedContentControl : Control
{
    private WebView2? webView;
    private bool initializationFailed;
    private float currentVideoSeconds;

    public EmbeddedContentControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
        BackColor = Color.Black;
    }

    public bool IsReady => webView is not null && !initializationFailed;
    public bool HasContent { get; private set; }

    public async Task InitializeAsync()
    {
        if (webView is not null || initializationFailed)
        {
            return;
        }

        try
        {
            webView = new WebView2 { Dock = DockStyle.Fill };
            Controls.Add(webView);

            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Spectrallis",
                "WebView2Cache");

            Directory.CreateDirectory(userDataFolder);

            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView.EnsureCoreWebView2Async(env);

            // Security settings
            webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView.CoreWebView2.Settings.IsScriptEnabled = true;
            webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

            // Listen for navigation requests (block external URLs)
            webView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                if (!e.Uri.StartsWith("about:blank") && !e.Uri.StartsWith("data:"))
                {
                    e.Cancel = true;
                }
            };
        }
        catch
        {
            initializationFailed = true;
        }
    }

    public void LoadHtmlContent(EmbeddedHtmlContext context)
    {
        if (!IsReady || context is null)
        {
            return;
        }

        try
        {
            var html = Encoding.UTF8.GetString(context.HtmlBytes);
            var sanitized = SanitizeHtml(html);
            var withCsp = InjectContentSecurityPolicy(sanitized);

            webView!.CoreWebView2.NavigateToString(withCsp);
            HasContent = true;
        }
        catch
        {
            HasContent = false;
        }
    }

    public void LoadMarkdownContent(EmbeddedMarkdownContext context)
    {
        if (!IsReady || context is null)
        {
            return;
        }

        try
        {
            var markdown = Encoding.UTF8.GetString(context.MarkdownBytes);
            var html = Markdown.ToHtml(markdown);

            var template = new StringBuilder();
            template.AppendLine("<!DOCTYPE html>");
            template.AppendLine("<html>");
            template.AppendLine("<head>");
            template.AppendLine("  <meta charset=\"utf-8\">");
            template.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            template.AppendLine("  <style>");
            template.AppendLine("    body {");
            template.AppendLine("      font-family: Georgia, serif;");
            template.AppendLine("      line-height: 1.6;");
            template.AppendLine("      padding: 2em;");
            template.AppendLine("      color: #e0e0e0;");
            template.AppendLine("      background-color: #1e1e1e;");
            template.AppendLine("      margin: 0;");
            template.AppendLine("    }");
            template.AppendLine("    h1, h2, h3, h4, h5, h6 {");
            template.AppendLine("      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;");
            template.AppendLine("      color: #fff;");
            template.AppendLine("    }");
            template.AppendLine("    a { color: #4a9eff; }");
            template.AppendLine("    code { background-color: #2d2d2d; padding: 0.2em 0.4em; border-radius: 3px; }");
            template.AppendLine("    pre { background-color: #2d2d2d; padding: 1em; border-radius: 5px; overflow-x: auto; }");
            template.AppendLine("  </style>");

            if (!string.IsNullOrWhiteSpace(context.CssOverride))
            {
                template.AppendLine("  <style>");
                template.AppendLine(context.CssOverride);
                template.AppendLine("  </style>");
            }

            template.AppendLine("</head>");
            template.AppendLine("<body>");
            template.AppendLine(html);
            template.AppendLine("</body>");
            template.AppendLine("</html>");

            var finalHtml = InjectContentSecurityPolicy(template.ToString());
            webView!.CoreWebView2.NavigateToString(finalHtml);
            HasContent = true;
        }
        catch
        {
            HasContent = false;
        }
    }

    public void LoadVideoContent(EmbeddedVideoContext context)
    {
        if (!IsReady || context is null)
        {
            return;
        }

        try
        {
            var base64Video = Convert.ToBase64String(context.VideoBytes);
            var mimeType = GetMimeTypeForCodec(context.Codec);
            var dataUri = $"data:{mimeType};base64,{base64Video}";

            var width = context.Width ?? 1280;
            var height = context.Height ?? 720;
            var autoplay = context.Autoplay ? "autoplay" : "";
            var loop = context.Loop ? "loop" : "";

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("  <meta charset=\"utf-8\">");
            html.AppendLine("  <style>");
            html.AppendLine("    body { margin: 0; padding: 0; background-color: #000; display: flex; align-items: center; justify-content: center; min-height: 100vh; }");
            html.AppendLine("    video { max-width: 100%; max-height: 100vh; }");
            html.AppendLine("  </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"  <video width=\"{width}\" height=\"{height}\" controls {autoplay} {loop}>");
            html.AppendLine($"    <source src=\"{dataUri}\" type=\"{mimeType}\">");
            html.AppendLine("    Your browser does not support the video tag.");
            html.AppendLine("  </video>");
            html.AppendLine("  <script>");
            html.AppendLine("    window.currentVideoElement = document.querySelector('video');");
            html.AppendLine("    window.syncVideoPosition = function(seconds) {");
            html.AppendLine("      if (window.currentVideoElement) {");
            html.AppendLine("        window.currentVideoElement.currentTime = Math.max(0, Math.min(seconds, window.currentVideoElement.duration));");
            html.AppendLine("      }");
            html.AppendLine("    };");
            html.AppendLine("  </script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            var finalHtml = InjectContentSecurityPolicy(html.ToString());
            webView!.CoreWebView2.NavigateToString(finalHtml);
            HasContent = true;
        }
        catch
        {
            HasContent = false;
        }
    }

    public async Task SyncVideoPosition(float audioSeconds)
    {
        if (!IsReady || !HasContent)
        {
            return;
        }

        // Only update if position changed significantly (avoid too frequent updates)
        if (Math.Abs(audioSeconds - currentVideoSeconds) < 0.05f)
        {
            return;
        }

        currentVideoSeconds = audioSeconds;

        try
        {
            await webView!.CoreWebView2.ExecuteScriptAsync($"window.syncVideoPosition({audioSeconds});");
        }
        catch
        {
            // Ignore script execution errors
        }
    }

    public void Clear()
    {
        if (!IsReady)
        {
            return;
        }

        try
        {
            webView!.CoreWebView2.NavigateToString("<html><body></body></html>");
            HasContent = false;
            currentVideoSeconds = 0;
        }
        catch
        {
            // Ignore
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        webView?.Dispose();
        base.OnHandleDestroyed(e);
    }

    private static string SanitizeHtml(string html)
    {
        // Simple sanitization: remove script tags and on* attributes
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            html,
            @"<script[^>]*>.*?</script>",
            "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"\s+on\w+\s*=\s*[""']?[^""']*[""']?",
            "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return sanitized;
    }

    private static string InjectContentSecurityPolicy(string html)
    {
        var csp = "default-src 'none'; style-src 'unsafe-inline' 'self'; script-src 'unsafe-inline'; img-src data: blob:; font-src data:; video-src data:; source-src data:";
        var metaCsp = $"<meta http-equiv=\"Content-Security-Policy\" content=\"{csp}\">";

        if (html.Contains("</head>", StringComparison.OrdinalIgnoreCase))
        {
            return html.Replace("</head>", $"{metaCsp}\n</head>", StringComparison.OrdinalIgnoreCase);
        }

        if (html.Contains("<html>", StringComparison.OrdinalIgnoreCase))
        {
            return html.Replace("<html>", $"<html><head>{metaCsp}</head>", StringComparison.OrdinalIgnoreCase);
        }

        return $"<html><head>{metaCsp}</head><body>{html}</body></html>";
    }

    private static string GetMimeTypeForCodec(string codec)
    {
        return codec.ToLowerInvariant() switch
        {
            "h264" or "h.264" => "video/mp4",
            "h265" or "h.265" => "video/mp4",
            "vp9" => "video/webm",
            "av1" => "video/mp4",
            _ => "video/mp4"
        };
    }
}
