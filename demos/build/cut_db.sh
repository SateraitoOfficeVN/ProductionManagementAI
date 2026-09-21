#!/usr/bin/env bash
# Build the edited presentation cut of ScreenA_DB.mp4 (jump cuts, step captions).
# Run with the scratchpad as the working directory: font/text paths are relative
# because ffmpeg's filter parser chokes on the drive-letter colon in absolute paths.
set -euo pipefail

SRC="$1"          # absolute path to ScreenA_DB.mp4
OUT="$2"          # absolute path to ScreenA_DB_edited.mp4
CLIPS="clips"
TXT="txt"
rm -rf "$CLIPS" "$TXT"; mkdir -p "$CLIPS" "$TXT"

HUMAN=0xE8833A
AGENT=0x2FA8A0
CARDBG=0x22252A
TITLE=0xE9ECEF

# caption card, placed in the empty AI Assistant panel on the right
CX=1495; CY=150; CW=372; CH=142

# start | duration | step | kind(h/a) | jumped(0/1) | title | subtitle | extra
SEGS=(
"14|12|1|h|0|Prompt 1:|approve the plan, start DB design|"
"30|12|2|a|0|Claude reads first|database skill, rules, template, code|"
"64|8|2|a|1|Claude reads first|database skill, rules, template, code|"
"106|14|3|a|0|Output: DB-002|the production-order schema|"
"118|13|4|a|0|Claude corrects|its own draft|"
"145|16|5|a|0|Output: plan approved|status and evidence updated|"
"164|18|6|a|0|Turn 1 summary|three technical decisions|"
"205|11|7|h|0|Prompt 2:|Are there any open questions left?|"
"228|18|8|a|0|Five open items|three of them new|PAINT"
"254|9|9|h|0|Prompt 3:|Let's answer all of them|"
"264|10|10|h|0|Decision form|which plant timezone|"
"325|9|10|h|1|Decision form|Cancel with unsaved changes|"
"351|10|10|h|1|Decision form|how many demo products|"
"375|10|10|h|1|Decision form|database logins, then review|"
"392|12|11|a|0|Recording the answers|DEC-016 to DEC-020|"
"432|12|11|a|1|Recording the answers|DEC-016 to DEC-020|"
"528|18|11|a|1|Turn 3 summary|nothing left open|"
"542|13|12|h|0|Review: decisions.md|every decision recorded|"
"620|14|12|h|1|Review: DB-002|open decisions, all resolved|"
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
  vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arialbd.ttf:textfile=$TXT/${idx}_1.txt:x=$((CX+22)):y=$((CY+54)):fontsize=23:fontcolor=${TITLE}"
  vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arial.ttf:textfile=$TXT/${idx}_2.txt:x=$((CX+22)):y=$((CY+92)):fontsize=16:fontcolor=0xB9BEC4"

  if [ "$jumped" = "1" ]; then
    vf="$vf,drawbox=x=$CX:y=$((CY+CH+14)):w=200:h=40:color=${CARDBG}@0.94:t=fill:enable='lt(t\,2.6)'"
    vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arial.ttf:textfile=overlays/jumped.txt:x=$((CX+16)):y=$((CY+CH+25)):fontsize=18:fontcolor=${HUMAN}:enable='lt(t\,2.6)'"
  fi

  if [ "$extra" = "PAINT" ]; then
    # Repaint the two-line CSRF bullet: the presentation cut does not quote an earlier design pass.
    vf="$vf,drawbox=x=64:y=657:w=1842:h=40:color=0x181A1C:t=fill"
    vf="$vf,drawtext=fontfile=${FONTS:-fonts}/consola.ttf:textfile=overlays/csrf1.txt:x=70:y=660:fontsize=16:fontcolor=0xD0D3D6"
    vf="$vf,drawtext=fontfile=${FONTS:-fonts}/consola.ttf:textfile=overlays/csrf2.txt:x=70:y=678:fontsize=16:fontcolor=0xD0D3D6"
  fi

  ffmpeg -v error -y -ss "$ss" -t "$dur" -i "$SRC" -vf "$vf" \
    -r 30 -c:v libx264 -preset medium -crf 20 -pix_fmt yuv420p -an "$CLIPS/$idx.mp4"
  echo "clip $idx  step $step  ${ss}s +${dur}s  $t1"
done

: > "$CLIPS/list.txt"
for f in "$CLIPS"/[0-9][0-9].mp4; do echo "file '$(basename "$f")'" >> "$CLIPS/list.txt"; done
ffmpeg -v error -y -f concat -safe 0 -i "$CLIPS/list.txt" -c copy "$OUT"
echo "wrote $OUT"
