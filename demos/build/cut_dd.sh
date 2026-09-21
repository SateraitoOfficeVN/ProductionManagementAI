#!/usr/bin/env bash
# Build the edited presentation cut of ScreenA_DD.mp4 (jump cuts, step captions).
# Run with the scratchpad as the working directory: font/text paths are relative
# because ffmpeg's filter parser chokes on the drive-letter colon in absolute paths.
set -euo pipefail

SRC="$1"          # absolute path to ScreenA_DD.mp4
OUT="$2"          # absolute path to ScreenA_DD_edited.mp4
CLIPS="clips_dd"
TXT="txt_dd"
rm -rf "$CLIPS" "$TXT"; mkdir -p "$CLIPS" "$TXT"

HUMAN=0xE8833A
AGENT=0x2FA8A0
CARDBG=0x22252A
TITLE=0xE9ECEF

CX=1495; CY=150; CW=372; CH=142

# start | duration | step | kind(h/a) | jumped(0/1) | title | subtitle
SEGS=(
"24|12|1|h|0|Prompt 1:|DB design reviewed, start the DD|"
"40|12|2|a|0|Claude reads first|skills, DD templates, and the app's own code|"
"96|8|2|a|1|Claude reads first|skills, DD templates, and the app's own code|"
"107|18|3|h|0|Authorization|may the mockup be published?|"
"245|13|4|a|0|Output: DD-001|the detailed design|"
"292|13|5|a|0|Output: DD-001-API|split out for Screen B to reuse|"
"340|10|6|a|0|Loads artifact-design|before building the mockup|"
"400|12|6|a|1|Reuses the app's palette|not a generic wireframe|"
"452|13|7|a|0|Mockup published|private artifact, 11 states|"
"486|14|8|h|0|The published mockup|discard dialog and 404 states|"
"505|18|8|a|1|Turn 1 summary|DEC-021 to DEC-024, BD-001 rev 4|"
"560|14|9|h|0|Review: DD-001|rules, references, check parameters|"
"606|12|10|h|0|Prompt 2:|two DD templates produced no document|"
"634|15|11|a|0|Saves the preference|to memory, before fixing the output|"
"707|12|11|a|1|Output: DD-001-FN|the function design|"
"757|13|11|a|1|Output: DD-001-SPD|then DD-001 is rewired to both|"
"810|18|11|a|1|Turn 2 summary|shared templates need your review|"
"915|13|12|h|0|Review: the four DD docs|plus the mockup|"
)

i=0
for seg in "${SEGS[@]}"; do
  IFS='|' read -r ss dur step kind jumped t1 t2 extra <<< "$seg"
  i=$((i+1)); idx=$(printf "%02d" "$i")
  [ "$kind" = "h" ] && ACC=$HUMAN || ACC=$AGENT

  printf '%s' "$(printf 'STEP %02d OF 12' "$step")" > "$TXT/${idx}_l.txt"
  printf '%s' "$t1" > "$TXT/${idx}_1.txt"
  printf '%s' "$t2" > "$TXT/${idx}_2.txt"

  vf="drawbox=x=$CX:y=$CY:w=$CW:h=$CH:color=${CARDBG}@0.94:t=fill"
  vf="$vf,drawbox=x=$CX:y=$CY:w=5:h=$CH:color=${ACC}:t=fill"
  vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arialbd.ttf:textfile=$TXT/${idx}_l.txt:x=$((CX+22)):y=$((CY+20)):fontsize=19:fontcolor=${ACC}"
  vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arialbd.ttf:textfile=$TXT/${idx}_1.txt:x=$((CX+22)):y=$((CY+54)):fontsize=22:fontcolor=${TITLE}"
  vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arial.ttf:textfile=$TXT/${idx}_2.txt:x=$((CX+22)):y=$((CY+92)):fontsize=15:fontcolor=0xB9BEC4"

  if [ "$jumped" = "1" ]; then
    vf="$vf,drawbox=x=$CX:y=$((CY+CH+14)):w=200:h=40:color=${CARDBG}@0.94:t=fill:enable='lt(t\,2.6)'"
    vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arial.ttf:textfile=overlays/jumped.txt:x=$((CX+16)):y=$((CY+CH+25)):fontsize=18:fontcolor=${HUMAN}:enable='lt(t\,2.6)'"
  fi

  ffmpeg -v error -y -ss "$ss" -t "$dur" -i "$SRC" -vf "$vf" \
    -r 30 -c:v libx264 -preset medium -crf 20 -pix_fmt yuv420p -an "$CLIPS/$idx.mp4"
  echo "clip $idx  step $step  ${ss}s +${dur}s  $t1"
done

: > "$CLIPS/list.txt"
for f in "$CLIPS"/[0-9][0-9].mp4; do echo "file '$(basename "$f")'" >> "$CLIPS/list.txt"; done
ffmpeg -v error -y -f concat -safe 0 -i "$CLIPS/list.txt" -c copy "$OUT"
echo "wrote $OUT"
