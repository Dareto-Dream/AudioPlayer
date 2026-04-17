# Standards

This file defines the expected format for extending Audio Player without reintroducing hardcoded lists, giant files, or theme drift. It also establishes the secure, extensible framework for embedding rich visualizers, themes, and custom content into MP3 files for portable, self-contained audio experiences.

These standards apply to:

- New visualizers (built-in and embedded)
- New theme accents or theme modes
- New persisted settings
- New themed controls or popup surfaces
- New `Form1` behavior
- **Embedded visualizer modules and custom content**

## Core Rules

- Prefer registries over duplicated lists. If something is user-selectable, there should be one source of truth for the available options.
- Prefer data flow over control coupling. Renderers and controls should consume models/state, not query the form or audio engine directly.
- Keep renderers stateless. `VisualizerCatalog` stores renderer instances once, so renderer classes must not keep mutable per-track or per-frame state.
- Keep theme values centralized. Runtime colors should come from `ThemePalette`, `VisualizerTheme`, or a dedicated theme helper, not scattered `Color.FromArgb(...)` calls in forms.
- Keep handwritten files focused. If a file starts mixing multiple concerns, split it before it becomes the next mega file.
- Generated files are the exception. `*.Designer.cs` may be large; handwritten files should not follow that pattern.
- **Embedded modules must run in a complete sandbox with zero access to host system, network, or application state.**

## File Size And Organization

- Target less than 250 lines for most handwritten files.
- If a handwritten file passes roughly 350-400 lines and contains more than one concern, split it.
- Do not put new nested utility/rendering classes inside `Form1` unless they are truly form-private and tiny.
- Use partial form files by responsibility:
  - `Form1.cs` for core state and construction
  - `Form1.Events.cs` for event handlers
  - `Form1.Playback.cs` for playback/file actions
  - `Form1.Settings.cs` for settings plumbing
  - `Form1.Theme.cs` for theme application
  - `Form1.UI.cs` for UI state updates
  - `Form1.Artwork.cs` for album-art logic
- Use standalone files for reusable systems:
  - `*Catalog.cs` for registration/source-of-truth lists
  - `*Store.cs` for persistence
  - `*Renderer.cs` for a single visualizer implementation
  - `*Theme*.cs` for shared theme helpers

## Adding A Visualizer

### Required Files

At minimum, a new visualizer touches:

1. `VisualizerMode.cs`
2. A new renderer file such as `PulseRingVisualizerRenderer.cs`
3. `VisualizerCatalog.cs`

### Required Format

1. Add a new enum member to `VisualizerMode`.
2. Create one renderer class per visualizer.
3. Implement `IVisualizerRenderer`.
4. If the renderer needs shared background/HUD helpers, inherit from `VisualizerRendererBase`.
5. Register the visualizer exactly once in `VisualizerCatalog.Definitions`.

Minimal pattern:

```csharp
internal sealed class PulseRingVisualizerRenderer : VisualizerRendererBase, IVisualizerRenderer
{
    public void Draw(Graphics graphics, Rectangle bounds, VisualizerScene scene)
    {
        DrawBackground(graphics, bounds, scene.Theme);
        DrawHud(graphics, bounds, scene);

        // Custom rendering here.
    }
}
```

And then register it:

```csharp
new(VisualizerMode.PulseRing, "Pulse Ring", new PulseRingVisualizerRenderer())
```

### Album-Art-Dependent Visualizers

If a visualizer requires album art:

- Set `RequiresAlbumArt: true` in `VisualizerCatalog`
- Do not add custom fallback logic in `Form1`
- Let `VisualizerCatalog.GetOptions(...)` and `GetPreferredMode(...)` handle availability

This keeps:

- The main form dropdown
- The settings dialog
- Auto-cycle behavior

all in sync automatically.

### If A Visualizer Needs New Data

Do not let the renderer reach into `AudioEngine`, `Form1`, or control internals.

Instead:

1. Add the new field to `VisualizerScene`
2. Populate it in `SpectrumVisualizerControl.CreateScene(...)`
3. Update any smoothing/state handling in `SpectrumVisualizerControl.UpdateFrame(...)`
4. Consume that field inside the renderer

If the new visualizer needs new theme colors:

1. Add them to `VisualizerTheme`
2. Populate them in `SpectrumVisualizerControl.ApplyTheme(...)`
3. Read them from `scene.Theme`

Do not hardcode special renderer-only colors unless they are purely local math derived from the theme.

### Visualizer Anti-Patterns

Do not:

- Add `switch` logic back into `SpectrumVisualizerControl.OnPaint`
- Keep a second visualizer label list in `Form1` or `SettingsDialog`
- Store mutable animation state inside renderer instances
- Query the form from renderer code

## Adding A Theme Accent Or Theme Mode

### Source Of Truth

Theme availability is currently defined by:

- `ThemeMode` in `AppSettings.cs`
- `ThemeAccent` in `AppSettings.cs`
- `ThemePalette.Create(...)`
- `SettingsDialog.PopulateComboOptions(...)`

### Adding A New Accent

1. Add the enum member to `ThemeAccent`
2. Add the accent colors in `ThemePalette.GetAccentColors(...)`
3. Add the selection option in `SettingsDialog.PopulateComboOptions(...)`
4. Verify `AppSettingsStore.Normalize(...)` still handles invalid enum values correctly
5. Verify contrast still works through `GetReadableTextColor(...)`

### Adding A New Theme Mode

If you add a mode beyond the current dark/light split:

1. Add the enum member to `ThemeMode`
2. Update `ThemePalette.Create(...)` so every palette property is defined for that mode
3. Verify all theme-driven controls still look correct through `ThemeControlStyler` and per-control `ApplyTheme(...)`
4. Verify `WindowChromeStyler.ApplyTheme(...)` still produces acceptable native window chrome
5. Add the mode to `SettingsDialog.PopulateComboOptions(...)`

### Theme Rules

- New controls should consume a `ThemePalette`, not invent their own palette format unless they are a full subsystem like the visualizer.
- If a complex surface needs its own derived colors, compute them from `ThemePalette`.
- Do not use `Color.White`, `SystemColors`, or default WinForms control colors for runtime surfaces that should be themed.
- If a native popup or form chrome ignores your colors, fix the host/native surface too. Current examples:
  - `WindowChromeStyler.cs`
  - `ModernComboBox.cs`

## Adding A New Persisted Setting

For any setting that survives app restarts, update all of these layers:

1. `AppSettings`
2. `AppSettings.Clone()`
3. `AppSettingsStore.Normalize(...)`
4. `SettingsDialog`
5. `Form1.Settings.cs` or the subsystem that applies the setting

### Expected Flow

- `AppSettings` defines the property and default value
- `AppSettingsStore` normalizes persisted input
- `SettingsDialog` edits the value
- `Form1` or the owning system applies the value at runtime

### UI Option Lists

If the setting is selected from a dropdown:

- Use `SelectionOption<T>`
- Keep the list in one helper method or catalog
- Do not duplicate the same option list in multiple screens

Examples already following this pattern:

- `GetSampleRateOptions()`
- `GetCycleDurationOptions()`
- `VisualizerCatalog.GetOptions(...)`

### Setting Anti-Patterns

Do not:

- Read/write the JSON settings file outside `AppSettingsStore`
- Add a new setting to the dialog without applying it at runtime
- Add runtime-only magic numbers without deciding whether they should be user-configurable

## Adding Or Updating Themed Controls

### Simple Controls

If a control can be themed through shared properties, use `ThemeControlStyler`.

Current shared styling lives in:

- `ThemeControlStyler.ApplyComboBoxTheme(...)`
- `ThemeControlStyler.ApplyPrimaryButtonTheme(...)`
- `ThemeControlStyler.ApplyGhostButtonTheme(...)`
- `ThemeControlStyler.ApplySliderTheme(...)`
- `ThemeControlStyler.ApplyCheckBoxTheme(...)`

### Complex Controls

If a control has its own drawing system, give it an `ApplyTheme(ThemePalette palette)` method.

Current examples:

- `SpectrumVisualizerControl.ApplyTheme(...)`
- `LyricsViewControl.ApplyTheme(...)`

### Native Surfaces

If the white/default Windows look leaks through:

- theme the control itself
- theme the popup/native child window if one exists
- theme the form chrome if the surface is a window/dialog

Do not stop after theming only the managed child controls.

## Form Standards

- New form behavior should go into the matching partial, not into `Form1.cs` by default.
- If a new concern does not fit an existing partial cleanly, create a new partial file.
- Keep business rules and option catalogs outside the form when possible.
- `Form1` should consume systems such as catalogs, renderers, settings stores, and theme helpers; it should not become the home for those systems.

## Review Checklist

Before finishing a feature like a new visualizer, theme, or setting, verify:

- There is one source of truth for the option list
- The feature is available in the settings UI if it is user-configurable
- Runtime behavior reads from normalized settings
- Theme colors come from the palette/system, not ad hoc colors
- No handwritten file became oversized or mixed unrelated concerns
- `dotnet build` succeeds

## Embedded Visualizers and Custom Content

Embedded visualizers allow artists and producers to ship rich, animated experiences directly inside MP3 files as portable, self-contained modules. This section defines the contract, capabilities, and strict security boundaries.

### Architecture Overview

Embedded content is stored in ID3v2 tags and loaded into isolated WASM runtime contexts:

- **Module**: A WASM binary + metadata defining a visualizer or renderer
- **Data Blocks**: JSON or binary payloads referenced by the module (config, images, themes)
- **Binary Assets**: Images, fonts, or other binary data (base64-encoded in ID3 tags)
- **Context**: The sandbox environment that binds modules to audio data and drawing surfaces

### Module Definition (DELTA_MODULE_ frame)

A module is declared as a JSON structure in an ID3v2 user text frame with description prefix `DELTA_MODULE_`:

```json
{
  "id": "my_visualizer",
  "type": "visualizer",
  "runtime": "wasm",
  "entry": "_start",
  "version": "1.0.0",
  "binaryRef": "my_visualizer_wasm",
  "dataRefs": {
    "config": "config_json",
    "theme": "theme_json",
    "background": "image_png"
  }
}
```

**Required fields:**
- `id`: Unique identifier (alphanumeric, dash, underscore only; max 64 chars). Used as display label.
- `type`: One of `"visualizer"`, `"html"`, `"markdown"`, `"video"`. Determines module behavior and rendering.
- `runtime`: Module type specifier. For WASM visualizers, must be exactly `"wasm"`. For HTML/Markdown/Video, set to the content type (e.g., `"html"`, `"markdown"`, `"h264"`).
- `entry`: Meaning depends on type:
  - WASM: Export name in the module (e.g., `_start`, `render`)
  - HTML: Not used; can be omitted
  - Markdown: Not used; can be omitted
  - Video: Codec/container (e.g., `"h264"`, `"vp9"`, `"av1"`)
- `binaryRef`: ID of the binary in a `DELTA_BIN_` frame. Validated to exist.

**Optional fields:**
- `version`: Semantic version string (e.g., `"1.0.0"`). Used for compatibility checks.
- `dataRefs`: Object mapping binding names to data block IDs. For HTML/Markdown, often references stylesheets or assets.
- `width`, `height`: For HTML/Video, pixel dimensions (e.g., `1920`, `1080`). Default: fill available space.
- `autoplay`: For video, boolean flag to play on load. Default: `false`.
- `loop`: For video, boolean flag to repeat. Default: `true` (videos loop with audio).

### Module Types

#### Type: `visualizer` (WASM)

See [WASM Module Requirements](#wasm-module-requirements) below.

#### Type: `html` (Rich HTML Content)

HTML modules embed styled web content directly in the player. Use for album art overlays, lyric displays, interactive UI, or animated backgrounds.

**Example module:**

```json
{
  "id": "album_art_overlay",
  "type": "html",
  "runtime": "html",
  "entry": "",
  "version": "1.0.0",
  "binaryRef": "html_content",
  "dataRefs": {
    "style": "stylesheet_css",
    "images": "images_data"
  }
}
```

**Content model:**
- The binary referenced by `binaryRef` is UTF-8 HTML.
- HTML is parsed and rendered in a **sandboxed iframe** with restrictions.
- Maximum HTML size: **512 KB** (includes all embedded content).

**Allowed HTML elements:**
- Structural: `div`, `p`, `section`, `article`, `header`, `footer`, `main`, `aside`
- Text: `h1`–`h6`, `span`, `strong`, `em`, `code`, `pre`, `blockquote`, `ul`, `ol`, `li`
- Media: `img`, `video` (restricted; see below), `audio` (restricted)
- Form elements (read-only): `input[type=text]`, `button` (disabled)
- Canvas & SVG: `canvas`, `svg` (for custom graphics)

**Forbidden elements:**
- Scripts: `script`, `style` (use CSS data block instead)
- Forms: `form`, `input[type=submit]`, `input[type=file]`
- Iframes: `iframe` (no nested frames)
- Objects: `object`, `embed`, `applet`
- Any element with `on*` event attributes

**CSS restrictions:**
- Inline styles are **stripped**. Use `<style>` tag or external stylesheet only.
- Allowed properties: layout (`display`, `flex`, `grid`, `margin`, `padding`), colors (`color`, `background-color`), typography (`font-size`, `font-family`, `font-weight`), transforms (`transform`, `opacity`)
- Forbidden properties: `position: fixed`, `position: absolute` (can escape bounds), `z-index` > 1000, `@import` URLs, `url()` with non-data URIs
- Media queries: Allowed (respond to player dimensions)
- Animations: Allowed (`animation`, `transition`; keyframes must be defined locally)

**Data references:**
- `style`: Optional CSS stylesheet (data block)
- `images`, `assets`: Optional image/media data blocks

**Interactive behavior:**
- HTML can use JavaScript **within the sandbox**. JavaScript is restricted to:
  - DOM manipulation (no access to parent window)
  - Local storage (sandboxed per module)
  - Canvas/SVG rendering
  - Audio context playback (read-only, synced to player)
  - No `eval`, no dynamic script loading
- Click/input events are isolated; modules cannot affect the player UI

**Size limits:**
- Total HTML + CSS + inline SVG: ≤ 512 KB
- Each image asset: ≤ 512 KB
- All assets combined: ≤ 2 MB

**Rendering:**
- HTML content is rendered alongside the visualizer or as a fullscreen overlay (user choice in settings)
- Viewport is device-dependent (phone: 360×640, desktop: variable)
- Responsive design is required; modules should use `viewport` meta tag and media queries

#### Type: `markdown` (Formatted Text Content)

Markdown modules embed formatted text for lyrics, liner notes, credits, or annotations.

**Example module:**

```json
{
  "id": "lyrics_and_credits",
  "type": "markdown",
  "runtime": "markdown",
  "entry": "",
  "version": "1.0.0",
  "binaryRef": "markdown_content",
  "dataRefs": {
    "style": "custom_css"
  }
}
```

**Content model:**
- The binary referenced by `binaryRef` is UTF-8 Markdown (CommonMark dialect).
- Markdown is converted to HTML, then rendered in a sandboxed context.
- Maximum Markdown size: **256 KB**.

**Supported Markdown features:**
- Headings (h1–h6)
- Paragraphs, line breaks
- Bold, italic, strikethrough
- Lists (ordered, unordered, nested)
- Code blocks (no syntax highlighting by default)
- Blockquotes
- Tables
- Links (HTTP/HTTPS only; external)
- Inline images (must reference embedded assets)
- Horizontal rules
- HTML entities and Unicode

**Restricted features:**
- No raw HTML blocks (use HTML module type instead)
- No footnotes or references
- No custom containers or extensions
- Links must be `http://` or `https://` (no `file://`, `javascript://`, `data://`)

**Rendering:**
- Output HTML is rendered in a sandboxed iframe (same restrictions as `html` type)
- Scroll behavior is automatic; content fills available vertical space
- Default typography: readable serif for body, sans-serif for headings

**Data references:**
- `style`: Optional CSS override for default Markdown styles

#### Type: `video` (Embedded Video Content)

Video modules embed short video clips synced to audio playback (album art, visualizer, music video, performance footage).

**Example module:**

```json
{
  "id": "music_video",
  "type": "video",
  "runtime": "h264",
  "entry": "h264",
  "version": "1.0.0",
  "binaryRef": "video_data",
  "width": 1920,
  "height": 1080,
  "autoplay": true,
  "loop": true
}
```

**Content model:**
- The binary referenced by `binaryRef` is video data (base64-encoded or raw).
- Video is decoded and played back synchronously with audio.
- Maximum video size: **16 MB** (enforced at load time).
- Maximum duration: Unlimited (synced to audio track).

**Supported video codecs:**
- **H.264** (AVC): `runtime: "h264"`, widely supported, good compression
- **VP9**: `runtime: "vp9"`, excellent compression, slower decode
- **AV1**: `runtime: "av1"`, best compression, requires modern hardware
- **H.265** (HEVC): `runtime: "h265"`, good compression, patent concerns in some regions

**Supported containers:**
- MP4 (`.mp4`): H.264, H.265
- WebM (`.webm`): VP9, AV1
- Matroska (`.mkv`): Any codec

**Playback behavior:**
- Video starts paused; user clicks to play (or `autoplay: true` for automatic playback)
- Video loop is synchronized to audio: when audio loops/repeats, video resets to start
- If video is shorter than audio, video loops and repeats
- If video is longer than audio, playback stops at audio end
- Audio is the sync source; video follows

**Dimensions:**
- `width` and `height` are target display dimensions (aspect ratio)
- Video is letterboxed if aspect ratio doesn't match
- If not specified, video fills available space while maintaining aspect ratio

**Restrictions:**
- No subtitles/captions embedded in video (use Markdown module for lyrics)
- No multiple tracks (audio, data streams)
- No live streaming (must be pre-encoded)
- Thumbnail extraction is optional; player can show first frame

**Size limits:**
- Single video file: ≤ 16 MB
- Recommended: ≤ 10 MB for fast metadata reading
- Compression: Use modern codecs (H.264 minimum, VP9/AV1 preferred)

### Data Blocks (DELTA_DATA_ frame)

Data blocks store configuration, theme overrides, and metadata in ID3v2 user text frames:

```
Description: DELTA_DATA_config_json
Text: { "color": "#FF00AA", "thickness": 2.5, "sampleCount": 96 }
```

or for raw text:

```
Description: DELTA_DATA_theme_name
Text: dark_mode
```

**Validation:**
- Frame encoding must be UTF-8.
- Content is treated as JSON if it parses; otherwise as raw text.
- `EmbeddedDataBlock.TryGetNumber()` and `TryGetString()` handle both JSON and raw text gracefully.
- Maximum data block size: **64 KB** (enforced at load time).

### Binary Assets (DELTA_BIN_ frame)

Binary data (images, fonts, compiled code) is base64-encoded in ID3v2 user text frames:

```
Description: DELTA_BIN_my_visualizer_wasm
Text: <base64-encoded WASM binary>
```

**Validation:**
- Content must be valid base64.
- Decoded size must not exceed **256 KB** (enforced at load time).
- For WASM binaries specifically: must parse as valid WASM and declare entry export.
- For images: must be PNG, JPEG, or WebP; max dimensions 1024x1024.

**Supported asset types:**
- WASM modules (`.wasm`)
- Images (`.png`, `.jpg`, `.webp`)
- Future: fonts (`.woff2`), compressed data

### Drawing Instruction Set

The embedded renderer supports a rich set of 2D drawing primitives. All coordinates are normalized to `[0, 1]` range (relative to canvas).

#### Basic Shapes

**Line**
```csharp
record EmbeddedLineInstruction(
    float X1, float Y1,      // Start point (normalized 0-1)
    float X2, float Y2,      // End point (normalized 0-1)
    Color Color,             // Stroke color (ARGB)
    float Thickness);        // Stroke width in pixels (clamped 1-20)
```

**Rectangle**
```csharp
record EmbeddedRectangleInstruction(
    float X, float Y,        // Top-left corner (normalized 0-1)
    float Width, float Height, // Dimensions (normalized 0-1)
    Color Color,             // Stroke or fill color
    float Thickness,         // Stroke width (clamped 1-20)
    bool Filled);            // true = fill, false = stroke only
```

**Circle**
```csharp
record EmbeddedCircleInstruction(
    float CenterX, float CenterY, // Center point (normalized 0-1)
    float Radius,            // Radius as fraction of canvas (clamped 0-1)
    Color Color,             // Stroke or fill color
    float Thickness,         // Stroke width (clamped 1-20)
    bool Filled);            // true = fill, false = stroke only
```

#### Advanced Shapes (Planned)

The following will be added in v1.1:

- **Arc**: Circular arc with start/end angles
- **Bezier**: Cubic bezier curves for smooth paths
- **Polygon**: Multi-point filled or stroked shapes
- **Path**: Composable drawing paths with fill/stroke rules

#### Text Rendering (Planned)

v1.1 will add text support:

```csharp
record EmbeddedTextInstruction(
    float X, float Y,        // Baseline position (normalized 0-1)
    string Text,             // UTF-8 text (max 256 chars)
    string FontName,         // System font or embedded font ID
    float FontSize,          // Size in pixels (clamped 12-72)
    Color Color,             // Text color
    bool Bold, bool Italic); // Style flags
```

#### Image and Texture Rendering (Planned)

v1.1 will add image support:

```csharp
record EmbeddedImageInstruction(
    float X, float Y,        // Top-left corner (normalized 0-1)
    float Width, float Height, // Dimensions (normalized 0-1)
    string ImageRef,         // Data block ID of image asset
    float Opacity,           // Alpha 0-1 (clamped)
    float Rotation);         // Rotation in degrees
```

#### Gradient Fills (Planned)

v1.1 will support gradient fills:

```csharp
record EmbeddedGradientFill(
    string Id,               // Reusable gradient ID
    EmbeddedGradientStop[] Stops, // Color stops
    bool IsRadial);          // false = linear, true = radial
```

### Color Format

Colors are specified as `System.Drawing.Color` (ARGB format):

- HTML hex notation: `"#RRGGBB"` or `"#AARRGGBB"` (parsed by `ColorTranslator.FromHtml()`)
- Integer ARGB: `255, 0, 255, 170` (magenta with full opacity)
- Named colors: `"Red"`, `"Blue"`, etc. (standard .NET color names)

Default colors:
- Stroke: `#FF00FFAA` (bright magenta)
- Fill: `#FF0055FF` (bright blue)

All colors are clamped to valid ARGB range; invalid colors fall back to defaults without error.

### WASM Module Requirements

#### Execution Sandbox

WASM modules are instantiated with a **zero-capability sandbox**:

- **No system calls**: No access to file system, network, process execution, or OS APIs.
- **No host bindings**: The WASM environment exports no host functions beyond those in the `EmbeddedVisualizerContext`.
- **Memory isolation**: Linear memory is isolated per module; no shared memory or cross-module access.
- **No dynamic code**: No eval, no code generation, no plugin loading.
- **Timeout enforcement**: Long-running computations are interrupted after **500 ms** per frame.
- **Deterministic execution**: Modules must produce identical output for identical input (no system time, random, or state queries).

#### Allowed Imports

Modules may only import from:

- `env.audio_sample`: Read a single audio sample (input: index, output: float32)
- `env.random_uint32`: Pseudo-random number (no real randomness; seeded by frame)
- `env.time_ms`: Elapsed time since track start (milliseconds, no real wall-clock)

No other imports are resolved. Modules that attempt to import restricted functions fail validation.

#### Forbidden Operations

Modules **must not**:

- Call `syscall` or `sysenter` directly
- Use memory mapping or shared memory
- Access the DOM (this is audio, not a web browser)
- Make network requests (no sockets, no HTTP, no DNS)
- Read or write files
- Fork or spawn processes
- Access environment variables or configuration
- Call into native code (no indirect calls outside the module itself)

These constraints are enforced by:
1. Validating the WASM module's imports at load time.
2. Restricting the import namespace to safe, read-only functions.
3. Isolating memory and ensuring bounds checks.
4. Running WASM in an interpreter (not JIT) to maintain determinism and safety.

### Configuration Pattern

Modules read runtime configuration from data blocks. The standard pattern:

```csharp
var config = context.GetDataByBinding("config");
var strokeColor = TryReadColor(config?.TryGetString("color", "strokeColor", "lineColor")) 
    ?? Color.FromArgb(255, 0, 255, 170);
var thickness = config is not null && config.TryGetNumber("thickness", out var t)
    ? Math.Clamp(t, 1f, 10f)
    : 2.2f;
```

**Best practices:**
- Always provide sensible defaults.
- Clamp numeric values to safe ranges.
- Fall back gracefully on missing or malformed data.
- Use `TryGetString()` with multiple property name aliases for flexibility.

### Session Management

Each track that uses an embedded visualizer gets its own `EmbeddedVisualizerSession`:

```csharp
public sealed class EmbeddedVisualizerSession
{
    public EmbeddedVisualizerContext Context { get; }
    public byte[] WasmMemory { get; }
    public EmbeddedVisualizerHostState HostState { get; }
    // ... rendering state
}
```

Sessions are created on-demand and held by `SpectrumVisualizerControl`. They are disposed when:
- A new track is loaded
- The user switches visualizer modes
- The application exits

**Memory management:**
- Each session allocates WASM linear memory (typically 1-4 MB per module).
- Sessions are cached per track to avoid re-instantiation.
- Large embedded modules (>256 KB) are rejected at parse time.

### Validation and Error Handling

#### Load-Time Validation

When reading embedded metadata:

**All types:**
1. Module JSON parses successfully.
2. `type` is one of: `"visualizer"`, `"html"`, `"markdown"`, `"video"` (case-insensitive).
3. `binaryRef` points to an existing binary asset.
4. All data block sizes are ≤ 64 KB (except for video; see below).
5. All referenced data blocks exist or are optional.

**WASM visualizer-specific:**
6. `runtime` is exactly `"wasm"` (case-insensitive).
7. `entry` export exists in the WASM binary and is callable.
8. Binary asset is valid WASM (parses and validates).
9. WASM module size is ≤ 512 KB compiled.

**HTML-specific:**
6. `runtime` is exactly `"html"` (case-insensitive).
7. Binary asset is valid UTF-8 HTML (parsed and sanitized).
8. HTML is stripped of forbidden elements, scripts, and unsafe CSS.
9. Total HTML + assets: ≤ 2 MB.

**Markdown-specific:**
6. `runtime` is exactly `"markdown"` (case-insensitive).
7. Binary asset is valid UTF-8 Markdown (CommonMark compliant).
8. Markdown is converted to safe HTML; external links are extracted for security audit.
9. Total Markdown size: ≤ 256 KB.

**Video-specific:**
6. `runtime` is one of: `"h264"`, `"h265"`, `"vp9"`, `"av1"` (case-insensitive).
7. Binary asset is valid video (container format detected, codec matches runtime).
8. Video duration ≤ audio track duration (or auto-loops).
9. Video size: ≤ 16 MB (enforced at load time).

If any check fails, the embedded module is silently skipped and the track plays with defaults (built-in visualizers, no overlay).

#### Runtime Validation

During playback:

**WASM visualizer:**
1. Drawing instructions are validated (coordinates clamped to [0, 1], colors valid, thickness/radius clamped).
2. Missing data blocks treated as `null` (no error).
3. Malformed JSON in data blocks falls back to raw text parsing.
4. WASM memory access is bounds-checked.

**HTML:**
1. DOM is kept isolated; no access to parent window, document, or opener.
2. All network requests (XHR, fetch, image load) are intercepted and logged (allowed only for pre-loaded assets).
3. Dynamically injected scripts are blocked.
4. CSS rules are validated; unsafe properties are stripped.

**Markdown:**
1. Links are validated (only HTTP/HTTPS allowed).
2. Image references are checked against embedded assets; missing images are replaced with placeholder.
3. Heading IDs and table of contents can be auto-generated (optional).

**Video:**
1. Playback is synchronized to audio track (see [Video Playback Sync](#video-playback-sync)).
2. Seek operations are synced to audio position.
3. Video frame is updated only on audio frame boundary (no tearing).
4. Out-of-sync frames are silently dropped; playback continues.

**Failure modes:**
- Rendering error (invalid WASM instruction, HTML parse error): Instruction is skipped; rendering continues.
- WASM trap (division by zero, out-of-bounds memory): Frame is marked invalid; previous frame is re-displayed.
- Timeout (WASM exceeds 500 ms): Frame is abandoned; previous frame is re-displayed; module flagged for future skipping.
- HTML sandbox violation (script injection attempt): Offending element is removed; rendering continues.
- Video decode error (corrupted frame): Frame is dropped; last valid frame displayed.
- Video sync loss (decoder lag): Playback catches up; no audio skipping.

### Video Playback Sync

Video modules require precise synchronization with audio:

1. **Sync source**: Audio playback position is authoritative. Video follows.
2. **Frame alignment**: Video frames are updated only when audio buffer is refreshed (typically 44.1 kHz or 48 kHz samples).
3. **Buffering**: Video decoder maintains 2–3 frame buffer to smooth out decode latency.
4. **Seek behavior**: When user seeks in audio, video seek position is set to the corresponding frame. Both are synced after seek completes.
5. **Frame skipping**: If video decoder lags, frame is skipped (not rendered); audio continues unaffected.
6. **Loop sync**: When audio loops (repeat, A-B loop, gapless next track), video seeks to start and loops simultaneously.

**Target latency**: Video output within 100 ms of audio (tolerance for human perception).

### HTML/Markdown Rendering and Security

**Sandboxing:**
- HTML and Markdown are rendered in isolated iframe with `sandbox` attribute
- Sandbox directives: `allow-same-origin allow-scripts allow-popups-to-escape-sandbox`
  - `allow-same-origin`: Allow local font and image loading
  - `allow-scripts`: Allow JavaScript (but no network access)
  - `allow-popups-to-escape-sandbox`: Allow links to open in external browser
  - Denied: `allow-forms`, `allow-top-navigation`, `allow-pointer-lock`, `allow-presentation`
- Iframe has `referrerpolicy="no-referrer"` to prevent leaking user info

**XSS Prevention (HTML type):**
1. All user-injected content is HTML-entity-escaped.
2. Forbidden tags (`script`, `iframe`, `object`, `embed`) are stripped at parse time.
3. Event attributes (`on*`) are removed.
4. URLs in `href` and `src` are validated:
   - Allowed: `http://`, `https://`, `data:image/` (pre-embedded images only)
   - Forbidden: `javascript://`, `data:text/html`, `file://`
5. CSS `url()` functions are intercepted; only `data:` URIs are allowed (pre-embedded).
6. Inline styles are stripped; use `<style>` tag or data block instead.

**Content Security Policy (HTML type):**
CSP header for iframe: `default-src 'self' data:; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:`
- Blocks external script loads
- Blocks external stylesheet loads
- Allows inline styles and scripts (module author can be trusted)
- Allows only embedded images and fonts

**Markdown Security (Markdown type):**
1. Markdown is parsed with link validation enabled.
2. Link targets are restricted:
   - Allowed: `http://`, `https://` (external links open in browser)
   - Forbidden: `javascript://`, `data://`, `file://`
3. HTML output is sanitized (same as HTML type).
4. Image references are checked against embedded assets; broken images use placeholder.

**JavaScript Isolation (HTML type):**
- Scripts can access: `window` (sandboxed), `document` (iframe DOM), `console`, `fetch` (restricted)
- Scripts cannot access: `parent`, `top`, `window.opener`, native APIs
- `fetch()` and XHR are intercepted; only requests to `data:` URIs succeed
- `localStorage` and `sessionStorage` are isolated per module (not shared with app)

### Performance Considerations

**WASM visualizers:**
- Run at player's render FPS (typically 60 FPS).
- Target module latency: **< 5 ms** per frame.
- Modules exceeding **20 ms** are warned (debug builds) but not stopped.
- Modules exceeding **500 ms** are interrupted; frame is dropped.

**HTML/Markdown:**
- Rendered in iframe; repaint occurs only on DOM changes or window resize.
- Target page load: **< 200 ms**.
- Scrolling and animations should maintain **≥ 30 FPS**.
- Heavy scripts can block the main thread; use `requestAnimationFrame` for smooth updates.

**Video:**
- Decoded at real-time playback speed (no pre-buffering entire video).
- Target decode time: **< 10 ms** per frame (depends on codec and hardware).
- Sync tolerance: **≤ 100 ms** audio/video drift acceptable.
- Bitrate: Recommend ≤ 5 Mbps for fast streaming (metadata reading should be fast).

### Versioning and Compatibility

The `version` field in module metadata enables forward/backward compatibility:

- Current version: `"1.0.0"` (basic shapes, no advanced features)
- Version `1.1.0`: Adds text, gradients, images, arcs, beziers, polygons
- Version `2.0.0`: Might add 3D rendering or physics

**Compatibility rules:**
- Major version mismatch (1.x vs 2.x): Module is rejected.
- Minor version mismatch (1.0 vs 1.1): Module is accepted but advanced features silently degrade.
- Patch version mismatch (1.0.1 vs 1.0.2): Module is accepted as-is.

### Embedding Content in MP3 Files

#### WASM Visualizer Workflow

1. **Create a WASM module** (using Rust, C, or AssemblyScript) that exports your render function.
2. **Prepare data blocks** (JSON config, images, themes).
3. **Encode binary assets** as base64.
4. **Add ID3v2 frames** to the MP3:
   - `DELTA_MODULE_*`: Module metadata (JSON with `type: "visualizer"`, `runtime: "wasm"`)
   - `DELTA_BIN_*`: WASM binary (base64-encoded)
   - `DELTA_DATA_*`: Configuration (JSON or text)
5. **Test** with AudioPlayer.

Example:
```json
DELTA_MODULE_my_vis = {
  "id": "my_visualizer",
  "type": "visualizer",
  "runtime": "wasm",
  "entry": "_start",
  "version": "1.0.0",
  "binaryRef": "my_visualizer_wasm",
  "dataRefs": { "config": "config_json" }
}
DELTA_BIN_my_visualizer_wasm = base64(wasm_binary)
DELTA_DATA_config_json = { "color": "#FF00AA", "thickness": 2.5 }
```

#### HTML Content Workflow

1. **Create HTML** with optional CSS (inline or in `<style>` tag) and assets (images, fonts).
2. **Sanitize HTML**: Ensure no `<script>`, `on*` attributes, or forbidden tags.
3. **Prepare assets** (images as PNG/JPEG, ≤ 512 KB each).
4. **Encode binary assets** as base64.
5. **Add ID3v2 frames**:
   - `DELTA_MODULE_*`: Module metadata (JSON with `type: "html"`, `runtime: "html"`)
   - `DELTA_BIN_*`: HTML binary (UTF-8 encoded, base64 wrapped)
   - `DELTA_BIN_*`: Image assets (base64-encoded)
   - `DELTA_DATA_*`: Optional CSS overrides (data block)
6. **Test** in AudioPlayer (check responsive design, no console errors).

Example:
```json
DELTA_MODULE_album_art = {
  "id": "album_art_overlay",
  "type": "html",
  "runtime": "html",
  "version": "1.0.0",
  "binaryRef": "html_content",
  "width": 800,
  "height": 600,
  "dataRefs": { "images": "image_png" }
}
DELTA_BIN_html_content = base64("<html>...</html>")
DELTA_BIN_image_png = base64(png_binary)
```

#### Markdown Content Workflow

1. **Write Markdown** (CommonMark dialect) with optional front-matter (YAML or TOML).
2. **Embed images** as references to data blocks or external URLs (HTTPS only).
3. **Validate links** (only HTTP/HTTPS allowed).
4. **Prepare assets** (images as PNG/JPEG, ≤ 512 KB each).
5. **Encode binary assets** as base64.
6. **Add ID3v2 frames**:
   - `DELTA_MODULE_*`: Module metadata (JSON with `type: "markdown"`, `runtime: "markdown"`)
   - `DELTA_BIN_*`: Markdown binary (UTF-8 encoded, base64 wrapped)
   - `DELTA_BIN_*`: Image assets (base64-encoded)
   - `DELTA_DATA_*`: Optional CSS overrides (data block)
7. **Test** in AudioPlayer (check rendering, link validation, image loading).

Example:
```json
DELTA_MODULE_lyrics = {
  "id": "lyrics_and_credits",
  "type": "markdown",
  "runtime": "markdown",
  "version": "1.0.0",
  "binaryRef": "markdown_content",
  "dataRefs": { "style": "custom_css" }
}
DELTA_BIN_markdown_content = base64("# Song Title\n\n## Verse 1\n...")
DELTA_DATA_custom_css = "h1 { color: #FF00AA; }"
```

#### Video Embedding Workflow

1. **Encode video** in one of the supported codecs (H.264, VP9, AV1, H.265).
2. **Target compression**: ≤ 5 Mbps bitrate for fast metadata reading.
3. **Sync with audio**: Ensure video duration matches or is shorter than audio track.
4. **Encode to base64** or leave as binary.
5. **Add ID3v2 frames**:
   - `DELTA_MODULE_*`: Module metadata (JSON with `type: "video"`, `runtime: "h264"|"vp9"|"av1"|"h265"`)
   - `DELTA_BIN_*`: Video binary (base64-encoded)
6. **Test** in AudioPlayer (check sync, playback, looping).

Example:
```json
DELTA_MODULE_music_video = {
  "id": "music_video",
  "type": "video",
  "runtime": "h264",
  "entry": "h264",
  "version": "1.0.0",
  "binaryRef": "video_data",
  "width": 1920,
  "height": 1080,
  "autoplay": true,
  "loop": true
}
DELTA_BIN_video_data = base64(mp4_binary)
```

#### Multi-Module Example

A single track can embed multiple modules (e.g., WASM visualizer + HTML overlay + video):

```json
DELTA_MODULE_wasm_viz = { "type": "visualizer", "runtime": "wasm", ... }
DELTA_MODULE_html_overlay = { "type": "html", "runtime": "html", ... }
DELTA_MODULE_music_video = { "type": "video", "runtime": "h264", ... }
DELTA_BIN_* = (corresponding binaries)
DELTA_DATA_* = (corresponding data)
```

The player renders all modules in layers: WASM visualizer → HTML overlay → video (or as configured by user).

### Anti-Patterns for Embedded Modules

#### WASM Visualizers

Do not:

- Attempt file I/O (modules cannot access file system)
- Make network requests (modules are air-gapped)
- Store persistent state (each frame is independent)
- Use system time or randomness for deterministic output (use seeded RNG)
- Embed executable code outside WASM (e.g., raw x86-64 or script)
- Exceed size limits (module > 512 KB)
- Assume host OS or architecture (WASM is cross-platform; behavior is deterministic)

#### HTML Content

Do not:

- Include `<script>` tags with inline code or external URLs (not executed; use `<style>` instead)
- Use `onclick`, `onload`, `on*` event attributes (stripped; use `addEventListener` in `<script>`)
- Attempt to set `position: fixed` or `position: absolute` with large z-index (escapes bounds; use relative layout)
- Embed uncompressed images (use PNG with compression or JPEG; max 512 KB each)
- Load external resources via `<link rel="stylesheet" href="https://...">` (requests blocked; embed CSS)
- Use `localStorage` for state persistence across sessions (isolated per module; data lost on player restart)
- Create very long pages without pagination (user must scroll; consider truncating or pagination)
- Assume dark theme colors without respecting player theme (use CSS `var()` custom properties if available)

#### Markdown Content

Do not:

- Use raw HTML blocks (not supported; use HTML module type instead)
- Include external resource URLs in images (only embedded images work; external HTTPS links for text are OK)
- Create deeply nested lists or tables (can be slow to render)
- Assume specific font or heading sizes (player provides default typography)
- Link to `javascript://` URLs or other unsafe schemes (blocked at validation)

#### Video Embedding

Do not:

- Embed videos longer than the audio track without looping enabled (video will cut off)
- Use variable bitrate encoding without understanding sync impact (constant bitrate preferred for stability)
- Assume specific video player controls or UI (player provides basic play/pause/seek)
- Encode in unsupported codecs (use H.264, VP9, AV1, or H.265 only)
- Embed multiple audio tracks (audio from video is ignored; player audio is used)
- Create videos larger than 16 MB (will be rejected; use compression)

#### General Best Practices

Do:

- **Test with real audio**: Verify modules work with actual music files, not mock audio data.
- **Provide fallbacks**: If an image or asset is missing, provide a sensible placeholder or graceful degradation.
- **Optimize for performance**: Keep WASM modules < 100 KB if possible; minify HTML/CSS; compress images and video.
- **Document metadata**: In module `id` or version, include creation date and compatibility notes.
- **Version your modules**: Use semantic versioning (e.g., `1.0.0`); increment on changes.
- **Validate before embedding**: Use tools to lint HTML (remove scripts), validate Markdown, check video codecs.
- **Consider mobile**: Design responsive HTML; keep video bitrate reasonable for streaming devices.

## When To Update This File

Update `STANDARDS.md` whenever the extension workflow or embedded module specification changes.

Examples:

- Visualizers stop being catalog-driven
- Theme registration moves out of `SettingsDialog`
- Settings move to a different persistence model
- File-size or partial-file conventions change
- **Embedded module capabilities are extended (new drawing primitives, new data formats)**
- **WASM sandbox rules change or new security constraints are added**
- **New module types are added (e.g., `shader`, `animation`)**
- **HTML/Markdown/Video rendering behavior changes**
- **Video codec support is added or removed**
- **Size limits are adjusted for performance or compatibility reasons**
- **Security restrictions are tightened or loosened (CSP, sandbox attributes, allowed APIs)**
