import os
import json
import base64
from mutagen.id3 import ID3, APIC, TIT2, TALB, TPE1, TXXX

# ---------- helpers ----------

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

def main():
    print("=== DELTA VISUALIZER INJECTOR ===\n")

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

    print("\n--- Visualizer Module ---\n")

    module_path = prompt("Visualizer module JSON path: ")
    module = add_visualizer_module(tags, module_path)

    if not module:
        print("[!] Aborting: invalid module")
        return

    print("\n--- Binary (WASM) ---\n")

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

    # save with compatibility
    tags.save(mp3_path, v2_version=3)

    print("\n[+] Injection complete (spec compliant)")

# ---------- run ----------

if __name__ == "__main__":
    main()