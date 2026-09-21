#!/usr/bin/env bash
# Build the edited presentation cut of ScreenA_BD.mp4.
# Rebuilt to match the other three cuts' pacing, keeping the same material
# hidden as the original edit: the session setup commands (/clear, /model) and
# every reference to WI-002's earlier design pass.
# Run with the scratchpad as the working directory (relative font paths).
set -euo pipefail

SRC="$1"
OUT="$2"
CLIPS="clips_bd"
TXT="txt_bd"
rm -rf "$CLIPS" "$TXT"; mkdir -p "$CLIPS" "$TXT"

HUMAN=0xE8833A
AGENT=0x2FA8A0
CARDBG=0x22252A
TITLE=0xE9ECEF
BG=0x181A1C
FG=0xD0D3D6

CX=1495; CY=150; CW=372; CH=142

# start | duration | step | kind(h/a) | jumped(0/1) | title | subtitle | paint
SEGS=(
"5|14|1|h|0|Prompt 1:|start Screen A as per what we planned|A"
"30|12|2|a|0|Claude reads the harness|instructions, workflow, skills, template|"
"101|11|2|a|1|Claude reads the harness|instructions, workflow, skills, template|B"
"128|14|3|a|0|Output: brief.md|REQ-010 to REQ-018|C"
"160|16|4|a|0|Output: decisions.md|DEC-001 to DEC-009|D"
"240|18|5|a|0|Output: BD-001|339 lines of basic design|"
"282|16|6|a|0|Output: plan, status, evidence|and the docs indexes|"
"300|22|7|a|0|Turn 1 summary|one new business question, DEC-009|"
"368|12|8|h|0|Prompt 2:|are there still open decisions?|"
"402|11|9|h|0|Decision form|the due-date rule|"
"420|11|9|h|1|Decision form|concurrency, and which timezone|"
"437|11|9|h|1|Decision form|order-number format|"
"478|16|10|a|0|Recording the answers|DEC-009 to DEC-012|"
"510|12|10|a|1|Recording the answers|BD-001 revision 2, status Green|"
"537|22|11|a|0|Turn 2 summary|what is left for DB and DD|E"
"580|11|12|h|0|Review: BD-001|the screen, item by item|E"
)

i=0
for seg in "${SEGS[@]}"; do
  IFS='|' read -r ss dur step kind jumped t1 t2 paint <<< "$seg"
  i=$((i+1)); idx=$(printf "%02d" "$i")
  [ "$kind" = "h" ] && ACC=$HUMAN || ACC=$AGENT

  printf '%s' "$(printf 'STEP %02d OF 12' "$step")" > "$TXT/${idx}_l.txt"
  printf '%s' "$t1" > "$TXT/${idx}_1.txt"
  printf '%s' "$t2" > "$TXT/${idx}_2.txt"

  vf=""
  # --- hidden material, painted over before the caption card is drawn -------
  case "$paint" in
    A) # session setup commands: /clear, /model and its output
       vf="drawbox=x=50:y=636:w=1840:h=84:color=${BG}:t=fill," ;;
    B) # /model block + two sentences naming the earlier design pass
       vf="drawbox=x=50:y=548:w=1840:h=46:color=${BG}:t=fill"
       vf="$vf,drawbox=x=50:y=674:w=1840:h=26:color=${BG}:t=fill"
       vf="$vf,drawtext=fontfile=${FONTS:-fonts}/consola.ttf:textfile=overlays/bd_scrapped.txt:x=58:y=679:fontsize=16:fontcolor=${FG}"
       vf="$vf,drawbox=x=50:y=818:w=1840:h=26:color=${BG}:t=fill"
       vf="$vf,drawtext=fontfile=${FONTS:-fonts}/consola.ttf:textfile=overlays/bd_inputs.txt:x=58:y=823:fontsize=16:fontcolor=${FG}," ;;
    C) # brief.md revision note describing the restart
       vf="drawbox=x=50:y=552:w=1840:h=62:color=${BG}:t=fill"
       vf="$vf,drawtext=fontfile=${FONTS:-fonts}/consola.ttf:textfile=overlays/bd_rev.txt:x=58:y=557:fontsize=16:fontcolor=${FG}," ;;
    D) # decisions.md paragraph describing the restart
       vf="drawbox=x=50:y=642:w=1840:h=62:color=${BG}:t=fill"
       vf="$vf,drawtext=fontfile=${FONTS:-fonts}/consola.ttf:textfile=overlays/bd_log1.txt:x=58:y=647:fontsize=16:fontcolor=${FG}"
       vf="$vf,drawtext=fontfile=${FONTS:-fonts}/consola.ttf:textfile=overlays/bd_log2.txt:x=58:y=665:fontsize=16:fontcolor=${FG}," ;;
    E) # turn 2 summary line naming the earlier pass's CSRF conclusion
       vf="drawbox=x=50:y=812:w=1840:h=26:color=${BG}:t=fill"
       vf="$vf,drawtext=fontfile=${FONTS:-fonts}/consola.ttf:textfile=overlays/bd_csrf.txt:x=58:y=817:fontsize=16:fontcolor=${FG}," ;;
  esac

  vf="${vf}drawbox=x=$CX:y=$CY:w=$CW:h=$CH:color=${CARDBG}@0.94:t=fill"
  vf="$vf,drawbox=x=$CX:y=$CY:w=5:h=$CH:color=${ACC}:t=fill"
  vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arialbd.ttf:textfile=$TXT/${idx}_l.txt:x=$((CX+22)):y=$((CY+20)):fontsize=19:fontcolor=${ACC}"
  vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arialbd.ttf:textfile=$TXT/${idx}_1.txt:x=$((CX+22)):y=$((CY+54)):fontsize=21:fontcolor=${TITLE}"
  vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arial.ttf:textfile=$TXT/${idx}_2.txt:x=$((CX+22)):y=$((CY+92)):fontsize=14:fontcolor=0xB9BEC4"

  if [ "$jumped" = "1" ]; then
    vf="$vf,drawbox=x=$CX:y=$((CY+CH+14)):w=200:h=40:color=${CARDBG}@0.94:t=fill:enable='lt(t\,2.6)'"
    vf="$vf,drawtext=fontfile=${FONTS:-fonts}/arial.ttf:textfile=overlays/jumped.txt:x=$((CX+16)):y=$((CY+CH+25)):fontsize=18:fontcolor=${HUMAN}:enable='lt(t\,2.6)'"
  fi

  ffmpeg -v error -y -ss "$ss" -t "$dur" -i "$SRC" -vf "$vf" \
    -r 30 -c:v libx264 -preset medium -crf 20 -pix_fmt yuv420p -an "$CLIPS/$idx.mp4"
  echo "clip $idx  step $step  ${ss}s +${dur}s  paint=${paint:--}  $t1"
done

: > "$CLIPS/list.txt"
for f in "$CLIPS"/[0-9][0-9].mp4; do echo "file '$(basename "$f")'" >> "$CLIPS/list.txt"; done
ffmpeg -v error -y -f concat -safe 0 -i "$CLIPS/list.txt" -c copy "$OUT"
echo "wrote $OUT"
