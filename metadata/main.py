import argparse
import base64
import json
import os
import sys
from mutagen.id3 import ID3, APIC, TIT2, TALB, TPE1, TXXX

# ---------- helpers ----------

DEFAULT_DELTA_META = {
    "format": "delta-mp3",
    "version": "1.0"
}

DEFAULT_ID3_VERSION = 4
VALID_THEME_MODES = {"dark", "light", "oled", "midnight"}
VALID_THEME_ACCENTS = {
    "amber",
    "ocean",
    "rose",
    "forest",
    "violet",
    "crimson",
    "cyan",
    "mint",
    "sunset",
    "gold"
}


def prompt(text, optional=False):
    while True:
        value = input(text).strip()
        if value or optional:
            return value

def read_text(path):
    if not path or not os.path.exists(path):
        return None
    with open(path, "r", encoding="utf-8") as f:
        return f.read()

def read_binary(path):
    if not path or not os.path.exists(path):
        return None
    with open(path, "rb") as f:
        return f.read()

def get_mime(path):
    ext = path.lower()
    if ext.endswith(".png"):
        return "image/png"
    elif ext.endswith(".jpg") or ext.endswith(".jpeg"):
        return "image/jpeg"
    else:
        raise ValueError("Cover must be .jpg or .png")


def parse_data_arguments(values):
    data_paths = {}
    for value in values or []:
        if "=" not in value:
            raise ValueError(f"Invalid --data value '{value}'. Expected REF=PATH.")

        reference_id, path = value.split("=", 1)
        reference_id = reference_id.strip()
        path = path.strip()
        if not reference_id or not path:
            raise ValueError(f"Invalid --data value '{value}'. Expected REF=PATH.")

        data_paths[reference_id] = path

    return data_paths

# ---------- metadata ----------

def clear_metadata(mp3_path):
    try:
        tags = ID3(mp3_path)
        tags.delete()
        tags.save(v2_version=3)
        print("[+] Metadata cleared")
    except Exception:
        print("[!] No metadata found or already clean")

def add_basic_tags(tags, title, artist, album):
    tags.add(TIT2(encoding=3, text=title))
    tags.add(TPE1(encoding=3, text=artist))
    tags.add(TALB(encoding=3, text=album))


def add_delta_meta(tags, format_name=DEFAULT_DELTA_META["format"], version=DEFAULT_DELTA_META["version"]):
    payload = {
        "format": format_name,
        "version": version
    }

    tags.add(TXXX(
        encoding=3,
        desc="DELTA_META",
        text=json.dumps(payload)
    ))

    print("[+] DELTA_META added")

def add_cover(tags, cover_path):
    if not cover_path:
        return

    data = read_binary(cover_path)
    if not data:
        print("[!] Failed to read cover")
        return

    tags.add(APIC(
        encoding=3,
        mime=get_mime(cover_path),
        type=3,
        desc="",
        data=data
    ))

    print("[+] Cover added")


def add_theme(tags, theme_path):
    content = read_text(theme_path)
    if not content:
        print("[!] Failed to read theme")
        return None

    try:
        theme = json.loads(content)
    except Exception as e:
        print(f"[!] Invalid theme JSON: {e}")
        return None

    theme_type = theme.get("type")
    if theme_type and str(theme_type).strip().lower() != "theme":
        print("[!] Theme metadata must use type=theme when 'type' is present")
        return None

    mode = str(theme.get("mode", theme.get("themeMode", ""))).strip()
    accent = str(theme.get("accent", theme.get("themeAccent", ""))).strip()
    if not mode:
        print("[!] Theme is missing mode/themeMode")
        return None

    if not accent:
        print("[!] Theme is missing accent/themeAccent")
        return None

    if mode.lower() not in VALID_THEME_MODES:
        print(f"[!] Invalid theme mode '{mode}'")
        return None

    if accent.lower() not in VALID_THEME_ACCENTS:
        print(f"[!] Invalid theme accent '{accent}'")
        return None

    tags.add(TXXX(
        encoding=3,
        desc="DELTA_THEME",
        text=json.dumps(theme)
    ))

    print("[+] Theme added")
    return theme

# ---------- SPEC: VISUALIZER ----------

def add_visualizer_module(tags, module_path):
    content = read_text(module_path)
    if not content:
        print("[!] Failed to read module")
        return None

    try:
        module = json.loads(content)
    except Exception as e:
        print(f"[!] Invalid module JSON: {e}")
        return None

    # enforce spec rules
    if module.get("type") != "visualizer":
        print("[!] Module must be type=visualizer")
        return None

    if module.get("runtime") != "wasm":
        print("[!] Visualizer must use runtime=wasm")
        return None

    if "binaryRef" not in module:
        print("[!] Missing binaryRef in module")
        return None

    tags.add(TXXX(
        encoding=3,
        desc="DELTA_MODULE_0",
        text=json.dumps(module)
    ))

    print("[+] Visualizer module added")
    return module

def add_binary(tags, key, path):
    data = read_binary(path)
    if not data:
        print(f"[!] Failed to read binary: {key}")
        return

    encoded = base64.b64encode(data).decode("utf-8")

    tags.add(TXXX(
        encoding=3,
        desc=f"DELTA_BIN_{key}",
        text=encoded
    ))

    print(f"[+] Binary added: {key}")

def add_data(tags, key, path):
    content = read_text(path)
    if not content:
        print(f"[!] Failed to read data: {key}")
        return

    tags.add(TXXX(
        encoding=3,
        desc=f"DELTA_DATA_{key}",
        text=content
    ))

    print(f"[+] Data added: {key}")

def add_lrc(tags, lrc_path):
    if not lrc_path:
        return

    content = read_text(lrc_path)
    if not content:
        print("[!] Failed to read LRC")
        return

    tags.add(TXXX(
        encoding=3,
        desc="LRC_SYNC",
        text=content
    ))

    print("[+] LRC fallback added")

# ---------- main ----------

def save_tags(tags, mp3_path, id3_version=DEFAULT_ID3_VERSION):
    tags.save(mp3_path, v2_version=id3_version)
    print(f"\n[+] Injection complete (saved as ID3v2.{id3_version})")


def run_interactive():
    print("=== DELTA METADATA INJECTOR ===\n")

    mp3_path = prompt("MP3 file path: ")
    if not os.path.exists(mp3_path):
        print("File not found")
        return

    clear_metadata(mp3_path)

    tags = ID3()
    tags.clear()

    print("\n--- Basic Metadata ---\n")

    title = prompt("Title: ")
    artist = prompt("Artist: ")
    album = prompt("Album: ")
    cover = prompt("Cover path (.jpg/.png optional): ", optional=True)

    add_basic_tags(tags, title, artist, album)
    add_cover(tags, cover)
    add_delta_meta(tags)

    print("\n--- Embedded Theme (Optional) ---\n")

    theme_path = prompt("Theme JSON path (optional): ", optional=True)
    if theme_path:
        theme = add_theme(tags, theme_path)
        if not theme:
            print("[!] Aborting: invalid theme")
            return

    print("\n--- Visualizer Module (Optional) ---\n")

    module_path = prompt("Visualizer module JSON path (optional): ", optional=True)
    if module_path:
        module = add_visualizer_module(tags, module_path)
        if not module:
            print("[!] Aborting: invalid module")
            return

        print("\n--- Binary (WASM/WAT) ---\n")

        binary_key = module["binaryRef"]
        binary_path = prompt(f"Binary file for '{binary_key}': ")
        add_binary(tags, binary_key, binary_path)

        print("\n--- Data Blocks ---\n")

        data_refs = module.get("dataRefs", {})
        for key, ref in data_refs.items():
            path = prompt(f"Data file for '{ref}' ({key}): ", optional=True)
            if path:
                add_data(tags, ref, path)

    print("\n--- LRC Fallback ---\n")

    lrc_path = prompt("LRC file (optional): ", optional=True)
    add_lrc(tags, lrc_path)

    save_tags(tags, mp3_path)


def create_parser():
    parser = argparse.ArgumentParser(description="Inject DELTA metadata into an MP3 file.")
    parser.add_argument("--mp3", dest="mp3_path", help="Path to the MP3 file to update.")
    parser.add_argument("--title", help="Track title.")
    parser.add_argument("--artist", help="Track artist.")
    parser.add_argument("--album", help="Track album.")
    parser.add_argument("--cover", help="Path to cover art (.png/.jpg).")
    parser.add_argument("--theme", dest="theme_path", help="Optional path to embedded theme JSON.")
    parser.add_argument("--module", dest="module_path", help="Optional path to the visualizer module JSON.")
    parser.add_argument("--binary", dest="binary_path", help="Optional path to the WASM/WAT visualizer payload.")
    parser.add_argument(
        "--data",
        action="append",
        default=[],
        metavar="REF=PATH",
        help="Bind a DELTA data reference id to a file path. Repeat for multiple refs. Only used with --module."
    )
    parser.add_argument("--lrc", dest="lrc_path", help="Optional path to an LRC lyrics file.")
    parser.add_argument(
        "--id3-version",
        type=int,
        choices=(3, 4),
        default=DEFAULT_ID3_VERSION,
        help="ID3 version to save. Use v2.4 to preserve UTF-8 DELTA frames."
    )
    return parser


def run_cli(args):
    required_fields = {
        "mp3": args.mp3_path,
        "title": args.title,
        "artist": args.artist,
        "album": args.album
    }

    missing = [name for name, value in required_fields.items() if not value]
    if missing:
        print(f"[!] Missing required arguments: {', '.join(missing)}")
        return 1

    if not os.path.exists(args.mp3_path):
        print(f"[!] MP3 file not found: {args.mp3_path}")
        return 1

    try:
        data_paths = parse_data_arguments(args.data)
    except ValueError as error:
        print(f"[!] {error}")
        return 1

    if args.binary_path and not args.module_path:
        print("[!] --binary requires --module")
        return 1

    if args.data and not args.module_path:
        print("[!] --data requires --module")
        return 1

    clear_metadata(args.mp3_path)

    tags = ID3()
    tags.clear()

    add_basic_tags(tags, args.title, args.artist, args.album)
    add_cover(tags, args.cover)
    add_delta_meta(tags)

    if args.theme_path:
        theme = add_theme(tags, args.theme_path)
        if not theme:
            print("[!] Aborting: invalid theme")
            return 1

    if args.module_path:
        if not args.binary_path:
            print("[!] --module requires --binary")
            return 1

        module = add_visualizer_module(tags, args.module_path)
        if not module:
            print("[!] Aborting: invalid module")
            return 1

        add_binary(tags, module["binaryRef"], args.binary_path)

        for _, reference_id in module.get("dataRefs", {}).items():
            path = data_paths.get(reference_id)
            if path:
                add_data(tags, reference_id, path)
            else:
                print(f"[!] Missing data file for ref '{reference_id}'")

    add_lrc(tags, args.lrc_path)
    save_tags(tags, args.mp3_path, args.id3_version)
    return 0

def main():
    if len(sys.argv) == 1:
        run_interactive()
        return

    parser = create_parser()
    args = parser.parse_args()
    raise SystemExit(run_cli(args))

# ---------- run ----------

if __name__ == "__main__":
    main()
