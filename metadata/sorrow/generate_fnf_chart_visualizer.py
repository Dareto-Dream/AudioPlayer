import argparse
import json
import re
import struct
from pathlib import Path


SCROLL_WINDOW_SECONDS = 1.1
PAST_WINDOW_SECONDS = 0.0
RECEPTOR_Y = 0.18
TRAVEL_HEIGHT = 0.72


def slugify(value: str) -> str:
    normalized = re.sub(r"[^a-zA-Z0-9]+", "-", value.strip().lower())
    return normalized.strip("-") or "chart"


def align4(value: int) -> int:
    return (value + 3) & ~3


def bytes_to_wat(data: bytes) -> str:
    return "".join(f"\\{byte:02x}" for byte in data)


def pack_f32(values):
    return b"".join(struct.pack("<f", float(value)) for value in values)


def pack_u8(values):
    return bytes(int(value) & 0xFF for value in values)


def pack_i8(values):
    return bytes(struct.pack("<b", int(value))[0] for value in values)


def choose_notes(chart: dict, difficulty: str | None):
    notes_by_difficulty = chart.get("notes", {})
    if difficulty and difficulty in notes_by_difficulty:
        return difficulty, notes_by_difficulty[difficulty]

    if "hard" in notes_by_difficulty:
        return "hard", notes_by_difficulty["hard"]

    if notes_by_difficulty:
        first_key = next(iter(notes_by_difficulty))
        return first_key, notes_by_difficulty[first_key]

    raise ValueError("Chart does not contain any notes.")


def build_note_payloads(notes: list[dict]):
    times = []
    holds = []
    meta = []

    for note in sorted(notes, key=lambda item: float(item.get("t", 0))):
        direction = int(note.get("d", 0))
        lane = direction % 4
        side = 0 if direction < 4 else 1
        special = 1 if str(note.get("k", "")).strip() else 0
        times.append(float(note.get("t", 0)) / 1000.0)
        holds.append(float(note.get("l", 0)) / 1000.0)
        meta.append(lane | (side << 2) | (special << 3))

    return times, holds, meta


def build_focus_payloads(events: list[dict]):
    focus_times = []
    focus_chars = []

    for event in sorted(events, key=lambda item: float(item.get("t", 0))):
        if event.get("e") != "FocusCamera":
            continue

        payload = event.get("v") or {}
        char = int(payload.get("char", -1))
        if char not in (-1, 0, 1):
            continue

        focus_times.append(float(event.get("t", 0)) / 1000.0)
        focus_chars.append(char)

    if not focus_times:
        focus_times.append(0.0)
        focus_chars.append(-1)

    return focus_times, focus_chars


def create_wat(chart_name: str, notes: list[dict], events: list[dict], scroll_speed: float) -> str:
    note_times, hold_lengths, note_meta = build_note_payloads(notes)
    focus_times, focus_chars = build_focus_payloads(events)

    note_count = len(note_times)
    focus_count = len(focus_times)

    note_times_offset = 0
    hold_lengths_offset = note_times_offset + (note_count * 4)
    note_meta_offset = hold_lengths_offset + (note_count * 4)
    focus_times_offset = align4(note_meta_offset + note_count)
    focus_chars_offset = focus_times_offset + (focus_count * 4)

    note_times_blob = bytes_to_wat(pack_f32(note_times))
    hold_lengths_blob = bytes_to_wat(pack_f32(hold_lengths))
    note_meta_blob = bytes_to_wat(pack_u8(note_meta))
    focus_times_blob = bytes_to_wat(pack_f32(focus_times))
    focus_chars_blob = bytes_to_wat(pack_i8(focus_chars))

    chart_duration_seconds = max(
        (float(note.get("t", 0)) + float(note.get("l", 0))) / 1000.0
        for note in notes
    )

    return f"""(module
  (import "delta" "set_color" (func $set_color (param i32 i32 i32 i32)))
  (import "delta" "set_thickness" (func $set_thickness (param f32)))
  (import "delta" "line" (func $line (param f32 f32 f32 f32)))
  (import "delta" "rect" (func $rect (param f32 f32 f32 f32 i32)))
  (import "delta" "circle" (func $circle (param f32 f32 f32 i32)))
  (import "delta" "get_waveform" (func $get_waveform (param i32) (result f32)))
  (import "delta" "get_spectrum" (func $get_spectrum (param i32) (result f32)))

  (memory 1)
  (data (i32.const {note_times_offset}) "{note_times_blob}")
  (data (i32.const {hold_lengths_offset}) "{hold_lengths_blob}")
  (data (i32.const {note_meta_offset}) "{note_meta_blob}")
  (data (i32.const {focus_times_offset}) "{focus_times_blob}")
  (data (i32.const {focus_chars_offset}) "{focus_chars_blob}")

  (func $lane_x (param $side i32) (param $lane i32) (result f32)
    local.get $side
    if (result f32)
      f32.const 0.58
      local.get $lane
      f32.convert_i32_s
      f32.const 0.07
      f32.mul
      f32.add
    else
      f32.const 0.14
      local.get $lane
      f32.convert_i32_s
      f32.const 0.07
      f32.mul
      f32.add
    end
  )

  (func $note_y (param $delta_seconds f32) (result f32)
    f32.const {RECEPTOR_Y}
    local.get $delta_seconds
    f32.const {TRAVEL_HEIGHT}
    f32.mul
    f32.const {SCROLL_WINDOW_SECONDS}
    f32.div
    f32.add
  )

  (func $hold_height (param $start_y f32) (param $end_y f32) (result f32)
    local.get $end_y
    local.get $start_y
    f32.sub
  )

  (func $fract (param $value f32) (result f32)
    local.get $value
    local.get $value
    f32.floor
    f32.sub
  )

  (func $tri (param $value f32) (result f32)
    f32.const 1
    local.get $value
    call $fract
    f32.const 0.5
    f32.sub
    f32.abs
    f32.const 4
    f32.mul
    f32.sub
  )

  (func $pulse (param $value f32) (result f32)
    local.get $value
    call $tri
    f32.const 1
    f32.add
    f32.const 0.5
    f32.mul
  )

  (func $render (export "render") (param $time f32) (param $peak f32) (param $rms f32)
    (local $i i32)
    (local $focus i32)
    (local $left_alpha i32)
    (local $right_alpha i32)
    (local $progress f32)
    (local $pulse f32)
    (local $lane i32)
    (local $side i32)
    (local $special i32)
    (local $x f32)
    (local $y f32)
    (local $tail_y f32)
    (local $delta_seconds f32)
    (local $hold_seconds f32)
    (local $meta i32)
    (local $wave_y f32)
    (local $wave_prev_x f32)
    (local $wave_prev_y f32)
    (local $wave_x f32)
    (local $wave_sample f32)

    i32.const -1
    local.set $focus
    i32.const 0
    local.set $i
    block $focus_done
      loop $focus_loop
        local.get $i
        i32.const {focus_count}
        i32.ge_s
        br_if $focus_done

        local.get $time
        i32.const {focus_times_offset}
        local.get $i
        i32.const 4
        i32.mul
        i32.add
        f32.load
        f32.lt
        br_if $focus_done

        i32.const {focus_chars_offset}
        local.get $i
        i32.add
        i32.load8_s
        local.set $focus

        local.get $i
        i32.const 1
        i32.add
        local.set $i
        br $focus_loop
      end
    end

    local.get $focus
    i32.const 0
    i32.eq
    if
      i32.const 78
      local.set $left_alpha
      i32.const 28
      local.set $right_alpha
    else
      local.get $focus
      i32.const 1
      i32.eq
      if
        i32.const 28
        local.set $left_alpha
        i32.const 78
        local.set $right_alpha
      else
        i32.const 52
        local.set $left_alpha
        i32.const 52
        local.set $right_alpha
      end
    end

    local.get $time
    f32.const {chart_duration_seconds:.6f}
    f32.div
    f32.const 0
    f32.max
    f32.const 1
    f32.min
    local.set $progress

    local.get $time
    f32.const 1.35
    f32.mul
    call $pulse
    local.set $pulse

    i32.const 4
    i32.const 6
    i32.const 10
    i32.const 255
    call $set_color
    f32.const 0
    f32.const 0
    f32.const 1
    f32.const 1
    i32.const 1
    call $rect

    i32.const 8
    i32.const 12
    i32.const 18
    i32.const 220
    call $set_color
    f32.const 0.025
    f32.const 0.045
    f32.const 0.95
    f32.const 0.9
    i32.const 1
    call $rect

    i32.const 10
    i32.const 15
    i32.const 24
    i32.const 255
    call $set_color
    f32.const 0.04
    f32.const 0.06
    f32.const 0.92
    f32.const 0.86
    i32.const 1
    call $rect

    i32.const 18
    i32.const 25
    i32.const 36
    i32.const 188
    call $set_color
    f32.const 0.04
    f32.const 0.06
    f32.const 0.92
    f32.const 0.12
    i32.const 1
    call $rect

    i32.const 14
    i32.const 18
    i32.const 28
    i32.const 168
    call $set_color
    f32.const 0.04
    f32.const 0.84
    f32.const 0.92
    f32.const 0.08
    i32.const 1
    call $rect

    i32.const 26
    i32.const 42
    i32.const 42
    local.get $left_alpha
    i32.const 36
    i32.add
    call $set_color
    f32.const 0.07
    f32.const 0.11
    f32.const 0.36
    f32.const 0.69
    i32.const 1
    call $rect

    i32.const 42
    i32.const 28
    i32.const 50
    local.get $right_alpha
    i32.const 36
    i32.add
    call $set_color
    f32.const 0.57
    f32.const 0.11
    f32.const 0.36
    f32.const 0.69
    i32.const 1
    call $rect

    i32.const 122
    i32.const 255
    i32.const 228
    local.get $left_alpha
    i32.const 118
    i32.add
    call $set_color
    f32.const 1.6
    call $set_thickness
    f32.const 0.07
    f32.const 0.11
    f32.const 0.36
    f32.const 0.69
    i32.const 0
    call $rect

    i32.const 255
    i32.const 170
    i32.const 242
    local.get $right_alpha
    i32.const 118
    i32.add
    call $set_color
    f32.const 1.6
    call $set_thickness
    f32.const 0.57
    f32.const 0.11
    f32.const 0.36
    f32.const 0.69
    i32.const 0
    call $rect

    i32.const 122
    i32.const 255
    i32.const 228
    local.get $left_alpha
    i32.const 78
    i32.add
    call $set_color
    f32.const 0.09
    f32.const 0.09
    f32.const 0.22
    f32.const 0.03
    i32.const 1
    call $rect

    i32.const 255
    i32.const 170
    i32.const 242
    local.get $right_alpha
    i32.const 78
    i32.add
    call $set_color
    f32.const 0.69
    f32.const 0.09
    f32.const 0.22
    f32.const 0.03
    i32.const 1
    call $rect

    i32.const 244
    i32.const 190
    i32.const 118
    i32.const 76
    call $set_color
    f32.const 0.487
    f32.const 0.11
    f32.const 0.026
    f32.const 0.69
    i32.const 1
    call $rect

    i32.const 255
    i32.const 222
    i32.const 170
    i32.const 112
    call $set_color
    f32.const 0.5
    f32.const 0.5
    f32.const 0.024
    local.get $pulse
    f32.const 0.008
    f32.mul
    f32.add
    i32.const 0
    call $circle

    i32.const 46
    i32.const 90
    i32.const 88
    i32.const 34
    call $set_color
    i32.const 0
    local.set $i
    block $grid_done
      loop $grid_loop
        local.get $i
        i32.const 6
        i32.ge_s
        br_if $grid_done
        f32.const 0.09
        local.get $i
        f32.convert_i32_s
        f32.const 0.14
        f32.mul
        f32.add
        local.set $wave_y
        f32.const 0.08
        local.get $wave_y
        f32.const 0.92
        local.get $wave_y
        call $line
        local.get $i
        i32.const 1
        i32.add
        local.set $i
        br $grid_loop
      end
    end

    i32.const 122
    i32.const 255
    i32.const 224
    i32.const 90
    call $set_color
    f32.const 2.2
    call $set_thickness
    f32.const 0.09
    local.set $wave_prev_x
    f32.const 0.5
    i32.const 0
    call $get_waveform
    local.get $rms
    f32.const 0.07
    f32.mul
    f32.mul
    f32.add
    local.set $wave_prev_y
    i32.const 1
    local.set $i
    block $wave_done
      loop $wave_loop
        local.get $i
        i32.const 24
        i32.ge_s
        br_if $wave_done

        f32.const 0.09
        local.get $i
        f32.convert_i32_s
        f32.const 0.034
        f32.mul
        f32.add
        local.set $wave_x

        local.get $i
        i32.const 4
        i32.mul
        call $get_waveform
        local.set $wave_sample
        f32.const 0.5
        local.get $wave_sample
        local.get $rms
        f32.const 0.07
        f32.mul
        f32.mul
        f32.add
        local.set $wave_y

        local.get $wave_prev_x
        local.get $wave_prev_y
        local.get $wave_x
        local.get $wave_y
        call $line

        local.get $wave_x
        local.set $wave_prev_x
        local.get $wave_y
        local.set $wave_prev_y
        local.get $i
        i32.const 1
        i32.add
        local.set $i
        br $wave_loop
      end
    end

    i32.const 214
    i32.const 246
    i32.const 237
    i32.const 126
    call $set_color
    f32.const 0.5
    f32.const 0.5
    f32.const 0.008
    local.get $pulse
    f32.const 0.004
    f32.mul
    f32.add
    i32.const 1
    call $circle

    i32.const 255
    i32.const 176
    i32.const 216
    i32.const 80
    call $set_color
    f32.const 0.11
    f32.const 0.89
    f32.const 0.012
    local.get $peak
    f32.const 0.01
    f32.mul
    f32.add
    i32.const 0
    call $circle
    f32.const 0.89
    f32.const 0.89
    f32.const 0.012
    local.get $peak
    f32.const 0.01
    f32.mul
    f32.add
    i32.const 0
    call $circle

    i32.const 0
    local.set $i
    block $lane_done
      loop $lane_loop
        local.get $i
        i32.const 8
        i32.ge_s
        br_if $lane_done

        local.get $i
        i32.const 4
        i32.lt_s
        local.get $i
        i32.const 3
        i32.and
        call $lane_x
        local.set $x

        local.get $i
        i32.const 4
        i32.lt_s
        if
          i32.const 36
          i32.const 72
          i32.const 74
          i32.const 78
          call $set_color
        else
          i32.const 58
          i32.const 42
          i32.const 70
          i32.const 78
          call $set_color
        end
        local.get $x
        f32.const 0.027
        f32.sub
        f32.const 0.12
        f32.const 0.054
        f32.const 0.68
        i32.const 1
        call $rect

        local.get $i
        i32.const 4
        i32.lt_s
        if
          i32.const 120
          i32.const 255
          i32.const 228
          i32.const 86
          call $set_color
        else
          i32.const 255
          i32.const 170
          i32.const 244
          i32.const 86
          call $set_color
        end
        f32.const 1.2
        call $set_thickness
        local.get $x
        f32.const 0.14
        local.get $x
        f32.const 0.8
        call $line

        local.get $i
        i32.const 4
        i32.lt_s
        if
          i32.const 88
          i32.const 255
          i32.const 234
          i32.const 70
          call $set_color
        else
          i32.const 255
          i32.const 184
          i32.const 246
          i32.const 70
          call $set_color
        end
        local.get $x
        f32.const 0.017
        f32.sub
        f32.const 0.125
        f32.const 0.034
        f32.const 0.012
        i32.const 1
        call $rect

        local.get $i
        i32.const 4
        i32.lt_s
        if
          i32.const 78
          i32.const 255
          i32.const 230
          i32.const 86
          call $set_color
        else
          i32.const 255
          i32.const 188
          i32.const 250
          i32.const 86
          call $set_color
        end
        local.get $x
        f32.const {RECEPTOR_Y}
        f32.const 0.024
        local.get $pulse
        f32.const 0.006
        f32.mul
        f32.add
        i32.const 1
        call $circle

        local.get $i
        i32.const 4
        i32.lt_s
        if
          i32.const 126
          i32.const 255
          i32.const 230
          i32.const 150
          call $set_color
        else
          i32.const 255
          i32.const 188
          i32.const 250
          i32.const 150
          call $set_color
        end
        f32.const 1.7
        call $set_thickness
        local.get $x
        f32.const 0.024
        f32.sub
        f32.const {RECEPTOR_Y - 0.028}
        f32.const 0.048
        f32.const 0.056
        i32.const 0
        call $rect

        i32.const 16
        i32.const 22
        i32.const 30
        i32.const 200
        call $set_color
        local.get $x
        f32.const 0.016
        f32.sub
        f32.const {RECEPTOR_Y - 0.016}
        f32.const 0.032
        f32.const 0.032
        i32.const 1
        call $rect

        local.get $i
        i32.const 4
        i32.lt_s
        if
          i32.const 216
          i32.const 255
          i32.const 246
          i32.const 124
          call $set_color
        else
          i32.const 255
          i32.const 222
          i32.const 250
          i32.const 124
          call $set_color
        end
        local.get $x
        f32.const {RECEPTOR_Y}
        f32.const 0.018
        i32.const 0
        call $circle

        i32.const 255
        i32.const 238
        i32.const 214
        i32.const 96
        call $set_color
        f32.const 1.15
        call $set_thickness
        local.get $x
        f32.const 0.012
        f32.sub
        f32.const {RECEPTOR_Y}
        local.get $x
        f32.const 0.012
        f32.add
        f32.const {RECEPTOR_Y}
        call $line

        local.get $i
        i32.const 1
        i32.add
        local.set $i
        br $lane_loop
      end
    end

    i32.const 0
    local.set $i
    block $notes_done
      loop $notes_loop
        local.get $i
        i32.const {note_count}
        i32.ge_s
        br_if $notes_done

        i32.const {note_times_offset}
        local.get $i
        i32.const 4
        i32.mul
        i32.add
        f32.load
        local.get $time
        f32.sub
        local.set $delta_seconds

        local.get $delta_seconds
        f32.const {-PAST_WINDOW_SECONDS}
        f32.ge
        if
          local.get $delta_seconds
          f32.const {SCROLL_WINDOW_SECONDS}
          f32.le
          if
            i32.const {note_meta_offset}
            local.get $i
            i32.add
            i32.load8_u
            local.set $meta

            local.get $meta
            i32.const 3
            i32.and
            local.set $lane

            local.get $meta
            i32.const 2
            i32.shr_u
            i32.const 1
            i32.and
            local.set $side

            local.get $meta
            i32.const 3
            i32.shr_u
            i32.const 1
            i32.and
            local.set $special

            local.get $side
            local.get $lane
            call $lane_x
            local.set $x

            local.get $delta_seconds
            call $note_y
            local.set $y

            i32.const {hold_lengths_offset}
            local.get $i
            i32.const 4
            i32.mul
            i32.add
            f32.load
            local.set $hold_seconds

            local.get $special
            if
              i32.const 255
              i32.const 170
              i32.const 106
              i32.const 102
              call $set_color
            else
              local.get $side
              if
                i32.const 104
                i32.const 255
                i32.const 228
                i32.const 96
                call $set_color
              else
                i32.const 255
                i32.const 142
                i32.const 238
                i32.const 96
                call $set_color
              end
            end

            local.get $hold_seconds
            f32.const 0.05
            f32.gt
            if
              local.get $delta_seconds
              local.get $hold_seconds
              f32.add
              call $note_y
              local.set $tail_y
              local.get $x
              f32.const 0.018
              f32.sub
              local.get $y
              f32.const 0.014
              f32.add
              f32.const 0.036
              local.get $y
              local.get $tail_y
              call $hold_height
              i32.const 1
              call $rect

              local.get $special
              if
                i32.const 255
                i32.const 198
                i32.const 138
                i32.const 198
                call $set_color
              else
                local.get $side
                if
                  i32.const 154
                  i32.const 255
                  i32.const 238
                  i32.const 208
                  call $set_color
                else
                  i32.const 255
                  i32.const 190
                  i32.const 248
                  i32.const 208
                  call $set_color
                end
              end
              local.get $x
              f32.const 0.011
              f32.sub
              local.get $y
              f32.const 0.012
              f32.add
              f32.const 0.022
              local.get $y
              local.get $tail_y
              call $hold_height
              i32.const 1
              call $rect

              local.get $x
              local.get $tail_y
              f32.const 0.01
              i32.const 1
              call $circle
            end

            local.get $special
            if
              i32.const 255
              i32.const 178
              i32.const 102
              i32.const 92
              call $set_color
            else
              local.get $side
              if
                i32.const 86
                i32.const 255
                i32.const 228
                i32.const 84
                call $set_color
              else
                i32.const 255
                i32.const 146
                i32.const 238
                i32.const 84
                call $set_color
              end
            end
            local.get $x
            f32.const 0.029
            f32.sub
            local.get $y
            f32.const 0.029
            f32.sub
            f32.const 0.058
            f32.const 0.058
            i32.const 1
            call $rect

            local.get $special
            if
              i32.const 255
              i32.const 198
              i32.const 122
              i32.const 228
              call $set_color
            else
              local.get $side
              if
                i32.const 122
                i32.const 255
                i32.const 234
                i32.const 222
                call $set_color
              else
                i32.const 255
                i32.const 170
                i32.const 244
                i32.const 222
                call $set_color
              end
            end
            local.get $x
            f32.const 0.023
            f32.sub
            local.get $y
            f32.const 0.023
            f32.sub
            f32.const 0.046
            f32.const 0.046
            i32.const 1
            call $rect

            i32.const 15
            i32.const 22
            i32.const 28
            i32.const 154
            call $set_color
            local.get $x
            f32.const 0.015
            f32.sub
            local.get $y
            f32.const 0.015
            f32.sub
            f32.const 0.03
            f32.const 0.03
            i32.const 1
            call $rect

            local.get $special
            if
              i32.const 255
              i32.const 236
              i32.const 204
              i32.const 144
              call $set_color
            else
              i32.const 252
              i32.const 250
              i32.const 255
              i32.const 136
              call $set_color
            end
            local.get $x
            f32.const 0.016
            f32.sub
            local.get $y
            f32.const 0.018
            f32.sub
            f32.const 0.032
            f32.const 0.008
            i32.const 1
            call $rect

            f32.const 1.85
            call $set_thickness
            i32.const 10
            i32.const 14
            i32.const 18
            i32.const 188
            call $set_color
            local.get $x
            f32.const 0.023
            f32.sub
            local.get $y
            f32.const 0.023
            f32.sub
            f32.const 0.046
            f32.const 0.046
            i32.const 0
            call $rect

            local.get $special
            if
              i32.const 255
              i32.const 224
              i32.const 182
              i32.const 215
              call $set_color
            else
              i32.const 236
              i32.const 246
              i32.const 252
              i32.const 210
              call $set_color
            end
          end
        end

        local.get $i
        i32.const 1
        i32.add
        local.set $i
        br $notes_loop
      end
    end

    i32.const 96
    i32.const 182
    i32.const 170
    i32.const 92
    call $set_color
    f32.const 0.08
    f32.const 0.93
    f32.const 0.84
    f32.const 0.02
    i32.const 1
    call $rect

    i32.const 18
    i32.const 26
    i32.const 34
    i32.const 180
    call $set_color
    f32.const 0.082
    f32.const 0.934
    f32.const 0.836
    f32.const 0.012
    i32.const 1
    call $rect

    i32.const 255
    i32.const 198
    i32.const 122
    i32.const 188
    call $set_color
    f32.const 0.082
    f32.const 0.934
    local.get $progress
    f32.const 0.836
    f32.mul
    f32.const 0.012
    i32.const 1
    call $rect

    i32.const 255
    i32.const 234
    i32.const 202
    i32.const 172
    call $set_color
    f32.const 0.082
    local.get $progress
    f32.const 0.836
    f32.mul
    f32.add
    f32.const 0.94
    f32.const 0.012
    local.get $pulse
    f32.const 0.004
    f32.mul
    f32.add
    i32.const 1
    call $circle
  )
)"""


def create_config(chart_name: str, notes: list[dict], scroll_speed: float) -> dict:
    special_count = sum(1 for note in notes if str(note.get("k", "")).strip())
    return {
        "name": f"{chart_name} FNF Highway",
        "color": "#7FF8E0",
        "accent": "#F6B36A",
        "thickness": 2,
        "amplitude": 72,
        "sampleCount": 128,
        "scrollSpeed": scroll_speed,
        "noteCount": len(notes),
        "specialNoteCount": special_count,
        "scrollWindowSeconds": SCROLL_WINDOW_SECONDS,
        "pastWindowSeconds": PAST_WINDOW_SECONDS,
        "style": "fnf-chart"
    }


def create_module(module_id: str, binary_ref: str, config_ref: str, chart_ref: str) -> dict:
    return {
        "id": module_id,
        "type": "visualizer",
        "runtime": "wasm",
        "entry": "render",
        "binaryRef": binary_ref,
        "dataRefs": {
            "config": config_ref,
            "chart": chart_ref
        },
        "version": "1.0.0"
    }


def main():
    parser = argparse.ArgumentParser(description="Generate an FNF chart visualizer module from a chart JSON.")
    parser.add_argument("--chart", required=True, help="Path to the source chart JSON.")
    parser.add_argument("--output-dir", required=True, help="Directory to write the generated files to.")
    parser.add_argument("--name", help="Base name for generated files and ids.")
    parser.add_argument("--difficulty", help="Difficulty key to render. Defaults to hard or the first available.")
    args = parser.parse_args()

    chart_path = Path(args.chart)
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    chart = json.loads(chart_path.read_text(encoding="utf-8"))
    difficulty, notes = choose_notes(chart, args.difficulty)

    base_name = args.name or chart_path.stem
    slug = slugify(base_name)
    scroll_speed = float(chart.get("scrollSpeed", {}).get(difficulty, chart.get("scrollSpeed", {}).get("default", 1)))

    chart_name = base_name.replace("-", " ").replace("_", " ").title()
    module_id = f"{slug}-fnf-highway"
    binary_ref = f"{slug}_fnf_vis"
    config_ref = f"{slug}_fnf_config"
    chart_ref = f"{slug}_chart"

    wat = create_wat(chart_name, notes, chart.get("events", []), scroll_speed)
    config = create_config(chart_name, notes, scroll_speed)
    module = create_module(module_id, binary_ref, config_ref, chart_ref)

    wat_path = output_dir / f"{slug}_visualizer.wasm"
    config_path = output_dir / f"{slug}_config.json"
    module_path = output_dir / f"{slug}_module.json"

    wat_path.write_text(wat, encoding="utf-8")
    config_path.write_text(json.dumps(config, indent=2), encoding="utf-8")
    module_path.write_text(json.dumps(module, indent=2), encoding="utf-8")

    print(f"[+] Difficulty: {difficulty}")
    print(f"[+] Generated {module_path}")
    print(f"[+] Generated {config_path}")
    print(f"[+] Generated {wat_path}")


if __name__ == "__main__":
    main()
