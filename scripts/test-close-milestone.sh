#!/usr/bin/env bash
#
# Exercises the close_milestone job's logic from .github/workflows/release-packages.yml outside
# Actions, since nothing else here can: that job only runs when a release publishes, and getting
# it wrong is invisible (a milestone quietly left open) rather than loud.
#
#   bash scripts/test-close-milestone.sh
#
# The lookup below is a COPY of the job's script, not the job itself - the job keeps its logic
# inline so it needs no checkout and no `contents` permission. Change one, change the other.
#
# Covered: a milestone with work still open is left open; one with nothing open is closed; a tag
# missing the `v` finds the `v` spelling; a tag with no slash and an absent milestone are no-ops;
# an exact title wins over the other spelling when both exist; a failed API read warns instead of
# reporting "nothing to close"; a prerelease tag matches nothing.
set -u

FIXTURE='[
  {"number":1,"title":"core/v5.1.0","open_issues":4},
  {"number":2,"title":"core/v5.2.0","open_issues":3},
  {"number":6,"title":"stock/v1.0.0","open_issues":0}
]'

DUPES='[
  {"number":9,"title":"core/5.1.0","open_issues":7},
  {"number":1,"title":"core/v5.1.0","open_issues":0}
]'

run() {
  local TAG="$1" PAYLOAD="$2" READ_OK="$3"
  local MILESTONES ALT MILESTONE NUMBER OPEN TITLE

  if [ "$READ_OK" != "ok" ]; then
    echo "  -> ::warning:: could not read the milestone list, so $TAG's milestone was left open."
    return
  fi
  MILESTONES="$PAYLOAD"

  ALT="$TAG"
  case "$TAG" in
    */v*) ALT="${TAG%%/*}/${TAG#*/v}" ;;
    */*)  ALT="${TAG%%/*}/v${TAG#*/}" ;;
  esac

  find_milestone() {
    echo "$MILESTONES" | jq -r --arg t "$1" \
      '[.[] | select(.title == $t)][0] | select(. != null) | "\(.number) \(.open_issues) \(.title)"'
  }

  MILESTONE=$(find_milestone "$TAG")
  if [ -z "$MILESTONE" ] && [ "$ALT" != "$TAG" ]; then
    MILESTONE=$(find_milestone "$ALT")
  fi

  if [ -z "$MILESTONE" ]; then
    echo "  -> no open milestone titled '$TAG' (or '$ALT'); nothing to close."
    return
  fi

  NUMBER=$(echo "$MILESTONE" | cut -d' ' -f1)
  OPEN=$(echo "$MILESTONE" | cut -d' ' -f2)
  TITLE=$(echo "$MILESTONE" | cut -d' ' -f3)

  if [ "$OPEN" != "0" ]; then
    echo "  -> ::warning:: milestone $TITLE still has $OPEN open; left open."
    return
  fi
  echo "  -> PATCH milestones/$NUMBER state=closed  (closed $TITLE)"
}

echo "1. tag core/v5.1.0, 4 open:";        run "core/v5.1.0" "$FIXTURE" ok
echo "2. tag stock/v1.0.0, 0 open:";       run "stock/v1.0.0" "$FIXTURE" ok
echo "3. tag core/5.1.0 (no v) -> alt:";   run "core/5.1.0" "$FIXTURE" ok
echo "4. tag v5.1.0 (no slash):";          run "v5.1.0" "$FIXTURE" ok
echo "5. tag gig/v9.9.9 (absent):";        run "gig/v9.9.9" "$FIXTURE" ok
echo "6. both spellings, tag core/v5.1.0 (exact must win, 0 open):"; run "core/v5.1.0" "$DUPES" ok
echo "7. both spellings, tag core/5.1.0 (exact must win, 7 open):";  run "core/5.1.0" "$DUPES" ok
echo "8. API read fails:";                 run "core/v5.1.0" "$FIXTURE" fail
echo "9. prerelease tag core/v5.1.0-beta.1:"; run "core/v5.1.0-beta.1" "$FIXTURE" ok
