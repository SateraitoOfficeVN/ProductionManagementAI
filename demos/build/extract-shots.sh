#!/usr/bin/env bash
# Re-extract the screenshots each deck embeds, from its edited cut.
# Usage:  ./extract-shots.sh [basic-design|database-design|detailed-design|implementation|all]
#
# Run from demos/build/. Timestamps are positions in the *edited* video, so
# rebuild the cut first (cut_*.sh) if you changed its segment list.
set -euo pipefail

WHICH="${1:-all}"
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

shot() { # <deck> <name> <seconds> <ffmpeg crop>
  ffmpeg -v error -y -ss "$3" -i "$VIDEO" -frames:v 1 -vf "crop=$4" -q:v 3 "decks/$1/img/$2.jpg"
}

if [ "$WHICH" = "basic-design" ] || [ "$WHICH" = "all" ]; then
  VIDEO="$ROOT/demos/01-basic-design/WI-002/ScreenA_BD_edited.mp4"
  d=basic-design; mkdir -p decks/$d/img
  shot $d prompt1    7 "1440:300:40:700"
  shot $d read      31 "1860:300:40:560"
  shot $d brief     44 "1860:300:40:540"
  shot $d decisions 59 "1860:300:40:540"
  shot $d bd001     76 "1860:280:40:560"
  shot $d plan      93 "1860:280:40:560"
  shot $d sum1     112 "1860:290:40:630"
  shot $d prompt2  129 "1440:220:40:820"
  shot $d form1    140 "1500:300:40:640"
  shot $d form3    162 "1500:300:40:640"
  shot $d rec      176 "1860:280:40:560"
  shot $d sum2     207 "1860:250:40:670"
  shot $d review   223 "1035:520:430:120"
  echo "basic-design shots extracted"
fi

if [ "$WHICH" = "database-design" ] || [ "$WHICH" = "all" ]; then
  VIDEO="$ROOT/demos/03-database-design/WI-002/ScreenA_DB_edited.mp4"
  d=database-design; mkdir -p decks/$d/img
  shot $d prompt1    6 "1440:300:40:700"
  shot $d read      18 "1860:200:40:755"
  shot $d write     39 "1860:240:40:655"
  shot $d fix       52 "1860:210:40:650"
  shot $d plan      67 "1860:340:40:575"
  shot $d sum1      84 "1860:320:40:595"
  shot $d prompt2   98 "1440:330:40:620"
  shot $d items    113 "1860:270:40:605"
  shot $d formtz   136 "1250:270:40:690"
  shot $d formpr   155 "1500:210:40:685"
  shot $d rec      176 "1860:290:40:605"
  shot $d sum3     203 "1860:310:40:595"
  shot $d decmd    218 "1035:430:430:85"
  shot $d db002    232 "1035:545:430:85"
  echo "database-design shots extracted"
fi

if [ "$WHICH" = "detailed-design" ] || [ "$WHICH" = "all" ]; then
  VIDEO="$ROOT/demos/02-detailed-design/WI-002/ScreenA_DD_edited.mp4"
  d=detailed-design; mkdir -p decks/$d/img
  shot $d prompt1    6 "1440:300:40:700"
  shot $d read      18 "1860:200:40:770"
  shot $d auth      41 "1860:300:40:690"
  shot $d dd001     56 "1860:200:40:670"
  shot $d api       69 "1860:200:40:670"
  shot $d skill     81 "1860:210:40:730"
  shot $d palette   92 "1860:190:40:740"
  shot $d publish  104 "1860:200:40:720"
  shot $d mockup   118 "1050:560:440:440"
  shot $d sum1     134 "1860:220:40:670"
  shot $d ddrev    150 "1440:520:460:120"
  shot $d prompt2  163 "1440:330:40:620"
  shot $d memory   176 "1860:200:40:690"
  shot $d spd      202 "1860:200:40:670"
  shot $d sum2     218 "1860:230:40:670"
  shot $d tree     233 "420:420:60:120"
  echo "detailed-design shots extracted"
fi

if [ "$WHICH" = "implementation" ] || [ "$WHICH" = "all" ]; then
  VIDEO="$ROOT/demos/04-implementation/WI-002/ScreenA_Implementaion_edited.mp4"
  d=implementation; mkdir -p decks/$d/img
  shot $d form        6 "1860:300:40:690"
  shot $d plan       40 "1860:230:40:700"
  shot $d plandoc    55 "1430:285:470:205"
  shot $d planroles  69 "900:300:470:310"
  shot $d chrono    121 "1860:230:40:670"
  shot $d rfc2      146 "1860:200:40:670"
  shot $d tests     172 "1860:240:40:560"
  shot $d crlf      200 "1860:250:40:560"
  shot $d bug       229 "1860:200:40:560"
  shot $d perm      269 "1860:200:40:560"
  shot $d sum       303 "1860:300:40:560"
  shot $d planout   303 "1400:265:470:295"
  shot $d app       326 "1000:560:560:160"
  shot $d recon     347 "1860:300:40:560"
  echo "implementation shots extracted"
fi
