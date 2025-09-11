#!/usr/bin/env bash
set -euo pipefail

# Run Unity PlayMode tests in batchmode.
# Usage:
#   tools/run_playmode_tests.sh
# Env:
#   UNITY_BIN: Optional path to Unity editor binary (defaults to first found in PATH)

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")"/.. && pwd)"
LOG_DIR="$PROJECT_DIR/Logs"
RESULTS_XML="$LOG_DIR/results.playmode.xml"
LOG_FILE="$LOG_DIR/tests.playmode.log"

mkdir -p "$LOG_DIR"

UNITY_BIN_DEFAULT=""
for c in Unity unity unity-editor; do
  if command -v "$c" >/dev/null 2>&1; then UNITY_BIN_DEFAULT="$c"; break; fi
done

UNITY_BIN="${UNITY_BIN:-$UNITY_BIN_DEFAULT}"

if [[ -z "$UNITY_BIN" ]]; then
  echo "Error: Unity CLI not found in PATH. Set UNITY_BIN or add 'Unity' to PATH." >&2
  exit 127
fi

echo "Running PlayMode tests with: $UNITY_BIN"
"$UNITY_BIN" \
  -batchmode \
  -projectPath "$PROJECT_DIR" \
  -runTests \
  -testPlatform PlayMode \
  -logFile "$LOG_FILE" \
  -testResults "$RESULTS_XML" \
  -quit

echo "\nTest run complete. Logs: $LOG_FILE, Results: $RESULTS_XML"
