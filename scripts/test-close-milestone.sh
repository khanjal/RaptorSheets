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
# Two cosmetic differences are deliberate: this file is analysed by SonarCloud (the job's copy,
# being inside YAML, is not), so it uses lower_case locals and `[[` where the job uses the
# uppercase env-style names and `[` that read naturally in a workflow. The jq expression and the
# field parsing - the parts a test is actually pinning - are identical to the job's.
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
  local tag="$1" payload="$2" read_ok="$3"
  local milestones alt milestone number open title

  if [[ "$read_ok" != "ok" ]]; then
    echo "  -> ::warning:: could not read the milestone list, so $tag's milestone was left open."
    return 0
  fi
  milestones="$payload"

  alt="$tag"
  case "$tag" in
    */v*) alt="${tag%%/*}/${tag#*/v}" ;;
    */*)  alt="${tag%%/*}/v${tag#*/}" ;;
    *)    alt="$tag" ;;
  esac

  find_milestone() {
    local wanted="$1"
    echo "$milestones" | jq -r --arg t "$wanted" \
      '[.[] | select(.title == $t)][0] | select(. != null) | "\(.number) \(.open_issues) \(.title)"'
    return 0
  }

  milestone=$(find_milestone "$tag")
  if [[ -z "$milestone" ]] && [[ "$alt" != "$tag" ]]; then
    milestone=$(find_milestone "$alt")
  fi

  if [[ -z "$milestone" ]]; then
    echo "  -> no open milestone titled '$tag' (or '$alt'); nothing to close."
    return 0
  fi

  number=$(echo "$milestone" | cut -d' ' -f1)
  open=$(echo "$milestone" | cut -d' ' -f2)
  title=$(echo "$milestone" | cut -d' ' -f3)

  if [[ "$open" != "0" ]]; then
    echo "  -> ::warning:: milestone $title still has $open open; left open."
    return 0
  fi
  echo "  -> PATCH milestones/$number state=closed  (closed $title)"
  return 0
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
