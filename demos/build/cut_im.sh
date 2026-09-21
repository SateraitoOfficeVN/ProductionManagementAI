#!/usr/bin/env bash
# Build the edited presentation cut of ScreenA_Implementaion.mp4.
# 50 min 44 s of source condensed to ~5 min: jump cuts, step captions.
# Run with the scratchpad as the working directory (relative font paths).
set -euo pipefail

SRC="$1"
OUT="$2"
CLIPS="clips_im"
TXT="txt_im"
rm -rf "$CLIPS" "$TXT"; mkdir -p "$CLIPS" "$TXT"

HUMAN=0xE8833A
AGENT=0x2FA8A0
CARDBG=0x22252A
TITLE=0xE9ECEF

CX=1495; CY=150; CW=372; CH=142

# start | duration | step | kind(h/a) | jumped(0/1) | title | subtitle
SEGS=(
"193|12|1|h|0|Decision form|which end-to-end test tool|"
"208|10|1|h|1|Decision form|how accessibility checks run|"
"227|10|1|h|1|Decision form|where the harness change goes|"
"378|16|2|a|0|Plan revision 2|and what to know before approving|"
"442|14|2|h|1|The plan itself|14 steps, each with a deliverable and a check|"
"456|14|2|h|1|and who owns what|roles, authorizations, risks|"
"496|12|3|h|0|Prompt 1:|keep the design plan so we can back-track|"
"568|14|3|a|1|Revision 1 restored|but placed below revision 2|"
"606|12|4|h|0|Prompt 2:|put it in chronological order|"
"658|14|4|a|1|The rule first|then the file is reordered|"
"716|12|5|h|0|Prompt 3:|approved, and update the harness|"
"790|12|5|a|1|RFC 0002 written|the plan gates push, PR and merge|"
"922|12|6|a|0|Branches and worktrees|two branches, harness PR opened|"
"1016|17|7|a|0|35/35 unit tests pass|CI blocked by a billing lock|"
"1212|11|7|a|1|Decisions inside the code|DEC-011 and DEC-017 in a comment|"
"1436|17|7|a|1|A failure prevented|CRLF would break the init script|"
"1866|12|8|a|0|REQ-019 in the component|native dialog, focus trap, Escape|"
"2114|17|8|a|1|A real bug, found|focus never fired on Keep editing|"
"2216|12|9|a|0|E2E: the stack|volume wiped only as authorized|"
"2340|11|9|a|1|A harness rule in config|no silent retries of flaky journeys|"
"2412|17|9|a|1|8 journeys pass|and pmai_app is denied DELETE|"
"2566|11|9|a|1|All tests pass|test plan written, PR next|"
"2762|28|9|a|1|Cooked for 34m 0s|what was built, caught and left open|"
"2884|18|10|h|0|The screen exists|the mockup, now running|"
"2990|24|11|a|0|Reconciling master|two things still need your call|"
)

i=0
for seg in "${SEGS[@]}"; do
  IFS='|' read -r ss dur step kind jumped t1 t2 extra <<< "$seg"
  i=$((i+1)); idx=$(printf "%02d" "$i")
  [ "$kind" = "h" ] && ACC=$HUMAN || ACC=$AGENT

  printf '%s' "$(printf 'STEP %02d OF 11' "$step")" > "$TXT/${idx}_l.txt"
  printf '%s' "$t1" > "$TXT/${idx}_1.txt"
  printf '%s' "$t2" > "$TXT/${idx}_2.txt"

  vf="drawbox=x=$CX:y=$CY:w=$CW:h=$CH:color=${CARDBG}@0.94:t=fill"
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
  echo "clip $idx  step $step  ${ss}s +${dur}s  $t1"
done

: > "$CLIPS/list.txt"
for f in "$CLIPS"/[0-9][0-9].mp4; do echo "file '$(basename "$f")'" >> "$CLIPS/list.txt"; done
ffmpeg -v error -y -f concat -safe 0 -i "$CLIPS/list.txt" -c copy "$OUT"
echo "wrote $OUT"
