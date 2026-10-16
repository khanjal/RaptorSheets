#!/usr/bin/env bash
#
# Closes the milestone a published release's tag names, when nothing in it is still open.
#
#   bash .github/scripts/close-milestone.sh core/v5.1.0
#
# Run by the close_milestone job in .github/workflows/release-packages.yml, which passes the
# release tag as $1 and supplies GH_TOKEN and GITHUB_REPOSITORY from the environment; the tag can
# also come from $TAG. Exercised by scripts/test-close-milestone.sh against a fake `gh`.
#
# This logic used to live inline in the job's `run:` block, with the test lifting it back out of
# the YAML to run it. One file instead: the job runs it, the test runs it, and SonarCloud - which
# analyses files, and so could never see a script embedded in YAML - lints it.
#
# A milestone here is titled for the release tag (core/v5.1.0), so the tag finds it by exact
# title. Closing is deliberately conditional: a milestone with open work is a release whose notes
# are not finished, so the run warns and leaves it rather than closing over the top of it.

# Give a release tag the other `v` spelling: core/v5.1.0 <-> core/5.1.0. detect_package accepts
# both, so a tag that differs from its milestone only by the v should still find it.
alt_spelling() {
  local tag="$1"

  case "$tag" in
    */v*) echo "${tag%%/*}/${tag#*/v}" ;;
    */*)  echo "${tag%%/*}/v${tag#*/}" ;;
    *)    echo "$tag" ;;
  esac

  return 0
}

# Print "<number> <open_issues> <title>" for the open milestone with exactly this title, or
# nothing. jq takes the first match itself rather than `| head -1`, which can leave jq writing
# into a closed pipe when more than one matches.
#
# `return $?` rather than `return 0`: jq's status has to reach the caller. The caller reads this
# through a command substitution, which does not inherit `-e`, so a swallowed failure here would
# leave the lookup empty and the run would report "nothing to close" having closed nothing - the
# same silent green the checks in close_milestone exist to prevent.
find_milestone() {
  local milestones="$1" wanted="$2"

  echo "$milestones" | jq -r --arg t "$wanted" \
    '[.[] | select(.title == $t)][0] | select(. != null) | "\(.number) \(.open_issues) \(.title)"'

  return $?
}

close_milestone() {
  local tag="$1"
  local milestones alt milestone number open title

  # The read is checked on its own rather than piped straight into jq. Piped, the exit status
  # belongs to the last command in the pipeline, so a 403, a 5xx or a rate limit would leave the
  # variable empty and the script would report "no milestone found" and go green having closed
  # nothing - a broken closer that stays invisible until someone notices a milestone is still
  # open months later.
  if ! milestones=$(gh api "repos/$GITHUB_REPOSITORY/milestones?state=open&per_page=100"); then
    echo "::warning::Could not read the milestone list, so $tag's milestone was left open."
    return 0
  fi

  # A body that is not an array would otherwise kill the script at the first jq call with nothing
  # but a non-zero status: `-e` aborts on the failed command substitution, and the job's only
  # trace is a red tick with no message. Say what happened instead.
  if ! echo "$milestones" | jq -e 'type == "array"' >/dev/null 2>&1; then
    echo "::error::The milestone list came back in an unexpected shape, so $tag's milestone was left open."
    return 1
  fi

  alt=$(alt_spelling "$tag")

  # Exact title first, the other spelling only if that found nothing: if both spellings ever exist
  # as separate milestones, the one the tag actually names is the one to close.
  milestone=$(find_milestone "$milestones" "$tag")
  if [[ -z "$milestone" ]] && [[ "$alt" != "$tag" ]]; then
    milestone=$(find_milestone "$milestones" "$alt")
  fi

  if [[ -z "$milestone" ]]; then
    echo "No open milestone titled '$tag' (or '$alt'); nothing to close."
    return 0
  fi

  number=$(echo "$milestone" | cut -d' ' -f1)
  open=$(echo "$milestone" | cut -d' ' -f2)
  title=$(echo "$milestone" | cut -d' ' -f3)

  if [[ "$open" != "0" ]]; then
    echo "::warning::Milestone $title still has $open open issue(s) or PR(s), so it was left open."
    return 0
  fi

  # The write gets the same treatment as the read above. Left bare, a transient 5xx on the PATCH
  # killed the script through `-e` with no output at all: the release stayed green
  # (continue-on-error), the milestone stayed open, and the only trace was a red job with nothing
  # in it naming which milestone or why.
  if ! gh api -X PATCH "repos/$GITHUB_REPOSITORY/milestones/$number" -f state=closed >/dev/null; then
    echo "::error::Could not close milestone $title (number $number), so it was left open."
    return 1
  fi

  echo "Closed milestone $title (number $number)."
  return 0
}

# Only when run as a script, so sourcing this file defines the functions and runs nothing.
#
# The flags are set here because they cannot be inherited. The step's `shell: bash` gives Actions'
# `bash --noprofile --norc -eo pipefail` to the one-line `bash <file>` wrapper, and `bash <file>`
# then starts a fresh shell with no options at all: `SHELLOPTS` is not exported, and the wrapper's
# own `-e` and pipefail are inert on a single command whose status is this script's anyway. So
# these two flags govern the logic, and nothing upstream does.
#
# `-e` is load-bearing: find_milestone passes jq's status up, and `-e` is what turns that into an
# abort rather than an empty lookup reported as "nothing to close". pipefail is not, today - real
# jq always drains stdin, so no payload can make `echo | jq` fail at the echo. It is here because
# the step had it, and so the pipeline someone adds next cannot reopen the hole that the step's
# `shell: bash` was added to close.
if [[ "${BASH_SOURCE[0]}" == "$0" ]]; then
  set -eo pipefail
  close_milestone "${1:-${TAG:-}}"
  exit $?
fi
