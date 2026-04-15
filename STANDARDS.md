# Standards

This file defines the expected format for extending Audio Player without reintroducing hardcoded lists, giant files, or theme drift.

These standards apply to:

- New visualizers
- New theme accents or theme modes
- New persisted settings
- New themed controls or popup surfaces
- New `Form1` behavior

## Core Rules

- Prefer registries over duplicated lists. If something is user-selectable, there should be one source of truth for the available options.
- Prefer data flow over control coupling. Renderers and controls should consume models/state, not query the form or audio engine directly.
- Keep renderers stateless. `VisualizerCatalog` stores renderer instances once, so renderer classes must not keep mutable per-track or per-frame state.
- Keep theme values centralized. Runtime colors should come from `ThemePalette`, `VisualizerTheme`, or a dedicated theme helper, not scattered `Color.FromArgb(...)` calls in forms.
- Keep handwritten files focused. If a file starts mixing multiple concerns, split it before it becomes the next mega file.
- Generated files are the exception. `*.Designer.cs` may be large; handwritten files should not follow that pattern.

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

## When To Update This File

Update `STANDARDS.md` whenever the extension workflow changes.

Examples:

- Visualizers stop being catalog-driven
- Theme registration moves out of `SettingsDialog`
- Settings move to a different persistence model
- File-size or partial-file conventions change
