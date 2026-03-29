#!/usr/bin/env bash
# Regenerates the Kiota-generated REST client from the StreamElements OpenAPI spec.
#
# Prerequisites:
#   - kiota CLI: https://aka.ms/kiota/docs/installation
#
# Usage:
#   ./scripts/regen-kiota.sh [--fetch]
#
#   --fetch   Download the latest OpenAPI spec from StreamElements before regenerating.
#
# The StreamElements API spec can be fetched from:
#   https://raw.githubusercontent.com/StreamElements/api-docs/main/api.yaml
#
# Or browse interactively at:
#   https://dev.streamelements.com/docs/api-docs/bcd899e16ac9a-se-api-docs
#
# NOTE: The generated client lives in src/StreamElements.Client.Generated/.
# Add that project to StreamElements.Client.slnx and reference it from
# StreamElements.Client.csproj once generated.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SPEC_FILE="$REPO_ROOT/streamelements-openapi.yaml"
GENERATED_DIR="$REPO_ROOT/src/StreamElements.Client.Generated"

FETCH=false
for arg in "$@"; do
  case "$arg" in
    --fetch) FETCH=true ;;
    *) echo "Unknown argument: $arg"; exit 1 ;;
  esac
done

if [ "$FETCH" = true ]; then
  echo "Fetching StreamElements OpenAPI spec..."
  curl -fsSL "https://raw.githubusercontent.com/StreamElements/api-docs/main/api.yaml" \
    -o "$SPEC_FILE"
  echo "Saved to $SPEC_FILE"
fi

if [ ! -f "$SPEC_FILE" ]; then
  echo "Error: $SPEC_FILE not found. Run with --fetch or place the spec manually."
  exit 1
fi

CSPROJ="$GENERATED_DIR/StreamElements.Client.Generated.csproj"
CSPROJ_BACKUP=""
if [ -f "$CSPROJ" ]; then
  CSPROJ_BACKUP=$(cat "$CSPROJ")
fi

mkdir -p "$GENERATED_DIR"

kiota generate \
  --language CSharp \
  --class-name StreamElementsApiClient \
  --namespace-name StreamElements.Client.Generated \
  --output "$GENERATED_DIR" \
  --openapi "$SPEC_FILE" \
  --clean-output \
  --exclude-backward-compatible

if [ -n "$CSPROJ_BACKUP" ]; then
  echo "$CSPROJ_BACKUP" > "$CSPROJ"
fi

echo "Done. Review $GENERATED_DIR before committing."
