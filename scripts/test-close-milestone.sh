#!/usr/bin/env bash
#
# Tests the close_milestone job in .github/workflows/release-packages.yml by running the job's OWN
# script against a fake `gh`. The script is extracted from the workflow every run, found by step
# name, so there is no second copy of the logic to drift out of step with the one that ships.
#
#   bash scripts/test-close-milestone.sh        # prints ok/FAIL per case, exits 1 if any failed
#
# Why it exists: that job only runs when a release publishes, and getting it wrong is invisible - a
# milestone quietly left open - rather than loud. An earlier version of this file printed what the
# logic would do and left a human to eyeball it, which meant it could not fail; every case here
# asserts an expected exit status and an expected line of output.
#
# Requires bash, jq and python with PyYAML, all of which this repo already depends on elsewhere.
set -u

script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
repo_root=$(cd "$script_dir/.." && pwd)
workflow="$repo_root/.github/workflows/release-packages.yml"

work=$(mktemp -d)
cleanup() {
  rm -rf "$work"
  return 0
}
trap cleanup EXIT

mkdir -p "$work/bin"

# Pull the step's `run:` block straight out of the workflow. Found by step name rather than by
# index, so reordering the job's steps cannot leave this quietly testing the wrong one.
cat > "$work/extract.py" <<'PY'
import sys
import yaml

workflow_path, out_path = sys.argv[1], sys.argv[2]
steps = yaml.safe_load(open(workflow_path))["jobs"]["close_milestone"]["steps"]
wanted = "Close the release's milestone"
run = next(step["run"] for step in steps if step.get("name") == wanted)
with open(out_path, "w", newline="\n") as handle:
    handle.write(run)
PY

if ! python "$work/extract.py" "$workflow" "$work/close-milestone.sh"; then
  echo "FAIL: could not extract the close_milestone step out of $workflow"
  exit 1
fi

# Stands in for the real `gh api`: a GET prints $fake_milestones, a PATCH prints nothing, and
# either can be made to fail. Nothing here reaches the network or needs a token.
cat > "$work/bin/gh" <<'GH'
#!/usr/bin/env bash
set -u
for arg in "$@"; do
  if [[ "$arg" == "PATCH" ]]; then
    if [[ "${fake_patch_fails:-0}" == "1" ]]; then
      echo "fake gh: the PATCH was refused" >&2
      exit 1
    fi
    exit 0
  fi
done
if [[ "${fake_get_fails:-0}" == "1" ]]; then
  echo "fake gh: the GET was refused" >&2
  exit 1
fi
printf '%s' "${fake_milestones:-[]}"
exit 0
GH
chmod +x "$work/bin/gh"

default_milestones='[
  {"number":1,"title":"core/v5.1.0","open_issues":4},
  {"number":2,"title":"core/v5.2.0","open_issues":3},
  {"number":6,"title":"stock/v1.0.0","open_issues":0}
]'

both_spellings='[
  {"number":9,"title":"core/5.1.0","open_issues":7},
  {"number":1,"title":"core/v5.1.0","open_issues":0}
]'

v_spelling_only='[
  {"number":1,"title":"core/v5.1.0","open_issues":0}
]'

failures=0
cases=0

# assert_case <name> <tag> <payload> <expected rc> <expected text> [get_fails] [patch_fails]
assert_case() {
  local name="$1" tag="$2" payload="$3" expect_rc="$4" expect_text="$5"
  local get_fails="${6:-0}" patch_fails="${7:-0}"
  local actual rc

  cases=$((cases + 1))

  # `bash -e` is how Actions invokes a `run:` block when the step sets no `shell:`, so the script
  # is exercised under the same abort-on-error semantics it has in the job.
  actual=$(PATH="$work/bin:$PATH" \
    TAG="$tag" \
    GITHUB_REPOSITORY="khanjal/RaptorSheets" \
    GH_TOKEN="not-a-real-token" \
    fake_milestones="$payload" \
    fake_get_fails="$get_fails" \
    fake_patch_fails="$patch_fails" \
    bash -e "$work/close-milestone.sh" 2>&1)
  rc=$?

  if [[ "$rc" != "$expect_rc" ]]; then
    echo "FAIL  $name"
    echo "      expected exit $expect_rc, got $rc"
    echo "      output: $actual"
    failures=$((failures + 1))
    return 0
  fi

  if [[ "$actual" != *"$expect_text"* ]]; then
    echo "FAIL  $name"
    echo "      expected the output to contain: $expect_text"
    echo "      actual output:                  $actual"
    failures=$((failures + 1))
    return 0
  fi

  echo "ok    $name"
  return 0
}

assert_case "work still open leaves the milestone open" \
  "core/v5.1.0" "$default_milestones" 0 \
  "::warning::Milestone core/v5.1.0 still has 4 open"

assert_case "nothing open closes the milestone" \
  "stock/v1.0.0" "$default_milestones" 0 \
  "Closed milestone stock/v1.0.0"

assert_case "a tag without the v finds the v spelling" \
  "core/5.1.0" "$v_spelling_only" 0 \
  "Closed milestone core/v5.1.0"

assert_case "a tag with no slash matches nothing" \
  "v5.1.0" "$default_milestones" 0 \
  "No open milestone titled 'v5.1.0'"

assert_case "an absent milestone is a no-op" \
  "gig/v9.9.9" "$default_milestones" 0 \
  "No open milestone titled 'gig/v9.9.9'"

assert_case "both spellings present: the exact title is the one closed" \
  "core/v5.1.0" "$both_spellings" 0 \
  "Closed milestone core/v5.1.0"

assert_case "both spellings present: the exact title is the one read for open work" \
  "core/5.1.0" "$both_spellings" 0 \
  "still has 7 open"

assert_case "a prerelease tag matches nothing" \
  "core/v5.1.0-beta.1" "$default_milestones" 0 \
  "No open milestone titled 'core/v5.1.0-beta.1'"

assert_case "a failed read warns instead of reporting nothing to close" \
  "core/v5.1.0" "$default_milestones" 0 \
  "::warning::Could not read the milestone list" 1 0

assert_case "a failed close names the milestone instead of dying silently" \
  "stock/v1.0.0" "$default_milestones" 1 \
  "::error::Could not close milestone stock/v1.0.0" 0 1

assert_case "a body that is not an array is reported" \
  "core/v5.1.0" '{"message":"Not Found"}' 1 \
  "::error::The milestone list came back in an unexpected shape"

echo
if [[ "$failures" != "0" ]]; then
  echo "$failures of $cases cases FAILED"
  exit 1
fi
echo "all $cases cases passed"
exit 0
