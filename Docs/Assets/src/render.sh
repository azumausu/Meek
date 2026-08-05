#!/usr/bin/env bash
# Renders the SVGs produced by build-event-order-diagrams.mjs into PNGs
# using headless Chrome at 2x. Run build-event-order-diagrams.mjs first.
set -euo pipefail

SRC_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT_DIR="$(cd "$SRC_DIR/.." && pwd)"
CHROME="/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"

while read -r name w h; do
  [ -z "$name" ] && continue
  "$CHROME" \
    --headless=new \
    --disable-gpu \
    --hide-scrollbars \
    --force-device-scale-factor=2 \
    --window-size="$w,$h" \
    --screenshot="$OUT_DIR/$name.png" \
    "file://$SRC_DIR/$name.html" 2>/dev/null
  echo "$name.png  $(sips -g pixelWidth -g pixelHeight "$OUT_DIR/$name.png" | awk '/pixel/{printf "%s ", $2}')"
done < "$SRC_DIR/manifest.txt"
