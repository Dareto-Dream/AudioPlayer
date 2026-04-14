import os
from mutagen.id3 import ID3, APIC, TIT2, TALB, TPE1, TXXX

# ---------- helpers ----------

def prompt(text, optional=False):
    while True:
        value = input(text).strip()
        if value or optional:
            return value

def read_file(path, mode="r", encoding="utf-8"):
    if not path or not os.path.exists(path):
        return None
    with open(path, mode, encoding=None if "b" in mode else encoding) as f:
        return f.read()

def get_mime(path):
    ext = path.lower()
    if ext.endswith(".png"):
        return "image/png"
    elif ext.endswith(".jpg") or ext.endswith(".jpeg"):
        return "image/jpeg"
    else:
        raise ValueError("Cover must be .jpg or .png")

# ---------- core logic ----------

def clear_metadata(mp3_path):
    try:
        tags = ID3(mp3_path)
        tags.delete()
        tags.save(v2_version=3)
        print("[+] Existing metadata removed")
    except Exception:
        print("[!] No metadata found or already clean")

def write_metadata(mp3_path, title, artist, album, cover_path, lrc_path):
    tags = ID3()
    tags.clear()  # ensure clean state

    # basic tags
    tags.add(TIT2(encoding=3, text=title))
    tags.add(TPE1(encoding=3, text=artist))
    tags.add(TALB(encoding=3, text=album))

    # ---------- cover art (FIXED) ----------
    if cover_path:
        cover_data = read_file(cover_path, mode="rb")
        if cover_data:
            tags.add(APIC(
                encoding=3,
                mime=get_mime(cover_path),  # MUST be correct
                type=3,                     # front cover
                desc="",                    # IMPORTANT: empty for compatibility
                data=cover_data
            ))
            print("[+] Cover art added")
        else:
            print("[!] Failed to load cover art")

    # ---------- LRC (for your visualizer) ----------
    if lrc_path:
        lrc_data = read_file(lrc_path)
        if lrc_data:
            tags.add(TXXX(
                encoding=3,
                desc="LRC_SYNC",
                text=lrc_data
            ))
            print("[+] LRC embedded (TXXX:LRC_SYNC)")
        else:
            print("[!] Failed to load LRC file")

    # ---------- CRITICAL: SAVE AS ID3v2.3 ----------
    tags.save(mp3_path, v2_version=3)
    print("[+] Metadata written successfully (ID3v2.3)")

# ---------- main CLI ----------

def main():
    print("=== MP3 Metadata Tool ===\n")

    mp3_path = prompt("MP3 file path: ")
    if not os.path.exists(mp3_path):
        print("File does not exist.")
        return

    # wipe everything
    clear_metadata(mp3_path)

    print("\nEnter new metadata:\n")

    title = prompt("Song Title: ")
    artist = prompt("Artist: ")
    album = prompt("Album: ")

    cover_path = prompt("Cover image path (.jpg/.png) [optional]: ", optional=True)
    lrc_path = prompt("LRC file path [optional]: ", optional=True)

    try:
        write_metadata(mp3_path, title, artist, album, cover_path, lrc_path)
    except Exception as e:
        print(f"[!] Error: {e}")

    print("\nDone.")

# ---------- run ----------

if __name__ == "__main__":
    main()