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
step = next(step for step in steps if step.get("name") == wanted)

# The script is only half the job: it reads TAG and GH_TOKEN out of the environment, and the
# workflow is what puts them there. Nothing else here would notice `release.name` substituted for
# `release.tag_name` - the tests set TAG themselves - so the wiring is asserted rather than
# assumed.
# The step sets `shell: bash`, which Actions runs as `bash --noprofile --norc -eo pipefail {0}`.
# The cases invoke the script with `-eo pipefail` to match, so if the step ever loses this the
# harness would be running under stricter rules than the job - a silent divergence of exactly the
# kind these assertions exist to prevent.
if step.get("shell") != "bash":
    sys.exit(
        "the step's shell is %r, expected 'bash' (the harness runs with -eo pipefail to match)"
        % (step.get("shell"),)
    )

expected_env = {
    "GH_TOKEN": "${{ github.token }}",
    "TAG": "${{ github.event.release.tag_name }}",
}
actual_env = step.get("env") or {}
for key, expected in expected_env.items():
    if actual_env.get(key) != expected:
        sys.exit(
            "the step's env.%s is %r, expected %r" % (key, actual_env.get(key), expected)
        )

with open(out_path, "w", newline="\n") as handle:
    handle.write(step["run"])
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

# Record what the script actually asked for. Without this the fake is blind to its own arguments,
# and a PATCH aimed at the wrong milestone, or carrying state=open, or a GET that stopped asking
# for open milestones, all look identical from outside - each one passed every case before.
printf '%s\n' "$*" >> "${gh_log:?gh_log must be set}"

for arg in "$@"; do
  if [[ "$arg" == "PATCH" ]]; then
    if [[ "${fake_patch_fails:-0}" == "1" ]]; then
      echo "fake gh: the PATCH was refused" >&2
      exit 1
    fi
    # The real call prints the updated milestone, so printing one here makes a script that forgets
    # to discard it leak JSON into the job log where a case can see it.
    printf '{"number":0,"state":"closed","title":"fake"}\n'
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

# The sample tags, named rather than repeated: every case and every expectation below derives
# from these, so the fixtures and the strings they are asserted against cannot drift apart, and
# changing the sample version is a one-line edit.
tag_core="core/v5.1.0"
tag_core_no_v="core/5.1.0"
tag_core_next="core/v5.2.0"
tag_stock="stock/v1.0.0"

milestone_json() {
  local number="$1" title="$2" open="$3"
  printf '{"number":%s,"title":"%s","open_issues":%s}' "$number" "$title" "$open"
  return 0
}

default_milestones="[
  $(milestone_json 1 "$tag_core" 4),
  $(milestone_json 2 "$tag_core_next" 3),
  $(milestone_json 6 "$tag_stock" 0)
]"

both_spellings="[
  $(milestone_json 9 "$tag_core_no_v" 7),
  $(milestone_json 1 "$tag_core" 0)
]"

v_spelling_only="[
  $(milestone_json 1 "$tag_core" 0)
]"

# Refuse to run unless the fake is the `gh` the script will find. Without this the test can reach
# the real API: on 2026-10-16 a sibling harness put its fake on PATH as a Windows path, the
# drive-letter colon split it into two directories, the real `gh` won, and the script closed a live
# milestone. Nothing here is allowed to be one PATH mistake away from writing to GitHub.
resolved_gh=$(PATH="$work/bin:$PATH" bash -c 'command -v gh' || true)
if [[ "$resolved_gh" != "$work/bin/gh" ]]; then
  echo "FAIL: the fake gh is not what the script would run (found '${resolved_gh:-nothing}',"
  echo "      expected '$work/bin/gh'); refusing to run rather than risk the real API."
  exit 1
fi

failures=0
cases=0

# assert_case <name> <tag> <payload> <expected rc, or "nonzero"> <expected text> \
#             [get_fails] [patch_fails] [expected gh call] [text that must NOT appear]
assert_case() {
  local name="$1" tag="$2" payload="$3" expect_rc="$4" expect_text="$5"
  local get_fails="${6:-0}" patch_fails="${7:-0}"
  local expect_call="${8:-}" reject_text="${9:-}"
  local actual rc log="$work/gh.log"

  cases=$((cases + 1))
  : > "$log"

  # `-eo pipefail` is what Actions gives a step that sets `shell: bash`, which this one does (and
  # extract.py asserts). Not the bare `bash -e` of a step with no `shell:`: that omits pipefail,
  # and a harness without it would pass a pipeline whose earlier command fails while the job
  # aborted on it.
  actual=$(PATH="$work/bin:$PATH" \
    TAG="$tag" \
    GITHUB_REPOSITORY="khanjal/RaptorSheets" \
    GH_TOKEN="not-a-real-token" \
    gh_log="$log" \
    fake_milestones="$payload" \
    fake_get_fails="$get_fails" \
    fake_patch_fails="$patch_fails" \
    bash -eo pipefail "$work/close-milestone.sh" 2>&1)
  rc=$?

  if [[ "$expect_rc" == "nonzero" ]]; then
    if [[ "$rc" == "0" ]]; then
      echo "FAIL  $name"
      echo "      expected a non-zero exit, got 0"
      echo "      output: $actual"
      failures=$((failures + 1))
      return 0
    fi
  elif [[ "$rc" != "$expect_rc" ]]; then
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

  if [[ -n "$reject_text" ]] && [[ "$actual" == *"$reject_text"* ]]; then
    echo "FAIL  $name"
    echo "      the output should not have contained: $reject_text"
    echo "      actual output:                        $actual"
    failures=$((failures + 1))
    return 0
  fi

  # Every case: the lookup must ask the API for the open milestones, a page at a time big enough
  # to hold them. A GET that stops saying state=open would read closed milestones as candidates.
  if ! grep -qF -- 'milestones?state=open&per_page=100' "$log"; then
    echo "FAIL  $name"
    echo "      the script did not GET milestones?state=open&per_page=100"
    echo "      calls made: $(tr '\n' '|' < "$log")"
    failures=$((failures + 1))
    return 0
  fi

  if [[ -n "$expect_call" ]] && ! grep -qF -- "$expect_call" "$log"; then
    echo "FAIL  $name"
    echo "      expected the script to call: $expect_call"
    echo "      calls made:                  $(tr '\n' '|' < "$log")"
    failures=$((failures + 1))
    return 0
  fi

  echo "ok    $name"
  return 0
}

assert_case "work still open leaves the milestone open" \
  "$tag_core" "$default_milestones" 0 \
  "::warning::Milestone $tag_core still has 4 open"

# The only case that reaches the write, so it is the one that pins what the write says: milestone 6
# is the stock/v1.0.0 fixture, and `state=closed` is the whole point. It also pins that the script
# discards the PATCH's response body, which the fake prints.
assert_case "nothing open closes the milestone, by number and with state=closed" \
  "$tag_stock" "$default_milestones" 0 \
  "Closed milestone $tag_stock" 0 0 \
  "-X PATCH repos/khanjal/RaptorSheets/milestones/6 -f state=closed" \
  '{"number":0'

assert_case "a tag without the v finds the v spelling" \
  "$tag_core_no_v" "$v_spelling_only" 0 \
  "Closed milestone $tag_core"

assert_case "a tag with no slash matches nothing" \
  "v5.1.0" "$default_milestones" 0 \
  "No open milestone titled 'v5.1.0'"

assert_case "an absent milestone is a no-op" \
  "gig/v9.9.9" "$default_milestones" 0 \
  "No open milestone titled 'gig/v9.9.9'"

assert_case "both spellings present: the exact title is the one closed" \
  "$tag_core" "$both_spellings" 0 \
  "Closed milestone $tag_core"

assert_case "both spellings present: the exact title is the one read for open work" \
  "$tag_core_no_v" "$both_spellings" 0 \
  "still has 7 open"

assert_case "a prerelease tag matches nothing" \
  "$tag_core-beta.1" "$default_milestones" 0 \
  "No open milestone titled '$tag_core-beta.1'"

assert_case "a failed read warns instead of reporting nothing to close" \
  "$tag_core" "$default_milestones" 0 \
  "::warning::Could not read the milestone list" 1 0

assert_case "a failed close names the milestone instead of dying silently" \
  "$tag_stock" "$default_milestones" 1 \
  "::error::Could not close milestone $tag_stock" 0 1

assert_case "a body that is not an array is reported" \
  "$tag_core" '{"message":"Not Found"}' 1 \
  "::error::The milestone list came back in an unexpected shape"

# An array of the wrong shape clears the type check above and then breaks jq inside the lookup.
# What matters is that the failure is not swallowed: the exit must be non-zero and the run must
# not claim there was nothing to close. Pinned as a property rather than as jq's exit code, which
# is jq's business - but it is what stops a future `return 0` in find_milestone from hiding this.
assert_case "an array of the wrong shape fails instead of reporting nothing to close" \
  "$tag_core" '[7]' nonzero \
  "" 0 0 "" \
  "nothing to close"

echo
if [[ "$failures" != "0" ]]; then
  echo "$failures of $cases cases FAILED"
  exit 1
fi
echo "all $cases cases passed"
exit 0
