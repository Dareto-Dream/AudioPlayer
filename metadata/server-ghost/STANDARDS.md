# DELTA MP3 METADATA PACKING STANDARD

This document defines how audio files are structured and packaged to support embedded visualizers, data, and runtime modules within MP3 metadata.

The goal is to treat MP3 files as **self-contained experience containers** while preserving compatibility with standard media players.

---

# Core Principles

* Standard audio playback must always work without modification.
* All extended functionality must exist entirely within metadata.
* Unknown metadata must be safely ignored by external players.
* The player must remain stable even if metadata is missing or invalid.
* Metadata acts as a transport layer, not a direct execution layer.

---

# File Structure Overview

A compliant file is structured as:

```text
MP3 FILE
├── Audio Stream (standard MP3 frames)
├── ID3v2 Tags
│   ├── Standard Tags (Title, Artist, Album, Cover)
│   └── DELTA_* Tags (custom metadata system)
```

---

# ID3 Requirements

* Version: **ID3v2.3**
* Encoding: **UTF-8 (encoding = 3)**
* Custom data stored using:

  * Frame type: `TXXX` (User Defined Text)

All DELTA metadata must exist inside `TXXX` frames.

---

# Namespacing Rules

All custom metadata must follow:

```text
DELTA_<CATEGORY>_<IDENTIFIER>
```

Categories:

* `META` → global metadata
* `MODULE` → behavior definitions
* `DATA` → structured or raw data
* `BIN` → binary payloads

Examples:

```text
DELTA_META
DELTA_MODULE_0
DELTA_DATA_chart_0
DELTA_BIN_tri_vis
```

---

# Standard Metadata (Required)

The following tags must be present for compatibility:

* `TIT2` → Title
* `TPE1` → Artist
* `TALB` → Album
* `APIC` → Cover Art

Cover art must:

* Use MIME `image/jpeg` or `image/png`
* Use type `3` (front cover)
* Use empty description (`desc=""`)

---

# DELTA_META (Required)

Defines global metadata for the file.

```text
Description: DELTA_META
```

```json
{
  "format": "delta-mp3",
  "version": "1.0"
}
```

---

# Visualizer Module

Defines the embedded visualizer.

```text
Description: DELTA_MODULE_0
```

```json
{
  "id": "unique-id",
  "type": "visualizer",

  "runtime": "wasm",
  "entry": "render",

  "binaryRef": "binary_id",

  "dataRefs": {
    "key": "data_id"
  },

  "version": "1.0.0"
}
```

## Rules

* Only one visualizer module is allowed
* If multiple exist, only the first valid one is used
* Invalid modules must be ignored

---

# Binary Payloads

Binary data is stored as base64.

```text
Description: DELTA_BIN_<id>
```

Example:

```text
DELTA_BIN_tri_vis
```

Rules:

* Must be base64 encoded
* Must resolve from `binaryRef`
* Intended for WASM or executable runtime payloads

---

# Data Blocks

Used for charts, config, lyrics, or other structured data.

```text
Description: DELTA_DATA_<id>
```

Examples:

```text
DELTA_DATA_chart_0
DELTA_DATA_config
```

Rules:

* May contain JSON or raw text
* Must not contain executable code
* Referenced via `dataRefs`

---

# Lyrics (Optional)

Fallback lyrics support:

```text
Description: LRC_SYNC
```

Contains:

* Raw LRC formatted text

Used only if no structured lyrics data is provided.

---

# Encoding Rules

* JSON must be valid UTF-8
* Binary must be base64 encoded
* Large payloads should be minimized where possible

---

# Size Guidelines

Recommended limits:

* Total metadata size: < 10 MB
* Individual binary payload: < 5 MB

These are guidelines, not strict limits.

---

# Packing Order

Recommended order of frames:

1. Standard tags
2. DELTA_META
3. DELTA_MODULE_0
4. DELTA_DATA_*
5. DELTA_BIN_*
6. LRC_SYNC (optional)

Order is not required but improves debugging.

---

# Compatibility Rules

* Files must play normally in standard media players
* Unknown DELTA tags must not affect playback
* Removing DELTA tags must not corrupt the audio

---

# Failure Handling

If any metadata is invalid:

* Ignore the affected module/data
* Continue playback
* Fall back to built-in visualizers

The player must never crash due to metadata.

---

# Security Rules

* Binary payloads must be treated as untrusted
* Execution must be sandboxed (e.g. WASM runtime)
* No direct system access allowed
* No reflection into host application

---

# Versioning

* `DELTA_META.version` defines format version
* Modules may include their own version field
* Player must remain backward-compatible when possible

---

# Summary

A compliant DELTA MP3 file:

* Plays as a normal audio file in any player
* Contains structured metadata for enhanced behavior
* Optionally includes a fully embedded visualizer
* Remains safe, portable, and self-contained

This standard enables MP3 files to act as:

* audio tracks
* visualizer definitions
* data containers
* interactive experience packages
