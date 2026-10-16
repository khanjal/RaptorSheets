#!/usr/bin/env bash
#
# Tests .github/scripts/close-milestone.sh - the script the close_milestone job in
# .github/workflows/release-packages.yml runs - against a fake `gh`. It runs that file directly, so
# there is no second copy of the logic to drift out of step with the one that ships. (It used to
# lift the script back out of the job's `run:` block, which worked, but left the file SonarCloud
# lints and the file a release runs still not being the same file: Sonar analyses files, so it
# cannot see inside YAML.)
#
#   bash scripts/test-close-milestone.sh        # prints ok/FAIL per case, exits 1 if any failed
#
# Why it exists: that job only runs when a release publishes, and getting it wrong is invisible - a
# milestone quietly left open - rather than loud. An earlier version of this file printed what the
# logic would do and left a human to eyeball it, which meant it could not fail; every case here
# asserts an expected exit status and an expected line of output.
#
# Requires bash and jq, both of which this repo already depends on elsewhere.
set -u

script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
repo_root=$(cd "$script_dir/.." && pwd)
target="$repo_root/.github/scripts/close-milestone.sh"
workflow="$repo_root/.github/workflows/release-packages.yml"

if [[ ! -f "$target" ]]; then
  echo "FAIL: $target does not exist"
  exit 1
fi

# The script is only half the job. Extraction used to prove the two agreed for free - whatever the
# step ran was what the cases ran - and running the file directly costs that, so the wiring is
# asserted here instead. The greps are anchored to whole lines, because a loose match is satisfied
# by the path sitting in a comment: `run: echo hi # bash .github/scripts/close-milestone.sh` would
# otherwise pass.
if ! grep -Eq '^[[:space:]]*run: bash [.]github/scripts/close-milestone[.]sh "[$]TAG"$' "$workflow"; then
  echo "FAIL: the close_milestone step no longer runs .github/scripts/close-milestone.sh"
  exit 1
fi

# TAG and GH_TOKEN come from the workflow, not from the script: the cases set TAG themselves, so
# nothing else here would notice `release.name` substituted for `release.tag_name`, and without the
# token every `gh` call fails and the job degrades to warning it could not read the list.
if ! grep -Eq 'TAG: [$][{][{] github[.]event[.]release[.]tag_name [}][}]$' "$workflow"; then
  echo "FAIL: the close_milestone step no longer passes github.event.release.tag_name as TAG"
  exit 1
fi

if ! grep -Eq 'GH_TOKEN: [$][{][{] github[.]token [}][}]$' "$workflow"; then
  echo "FAIL: the close_milestone step no longer passes github.token as GH_TOKEN"
  exit 1
fi

# The job must still clone, since the script it runs is a file in the repo. Losing the checkout is
# the quietest break of the lot: `bash <missing file>` exits 127, continue-on-error absorbs it, the
# release goes green and the milestone stays open. The slice runs to end-of-file because
# close_milestone is the last job; a job added after it would weaken this rather than break it.
if ! sed -n '/^  close_milestone:/,$p' "$workflow" | grep -q 'uses: actions/checkout@v4'; then
  echo "FAIL: the close_milestone job no longer checks out the repo, so the script will not exist"
  exit 1
fi

# `-eo pipefail` used to reach the logic from the step, which sets `shell: bash`. It no longer
# can - `bash <file>` starts a fresh shell with no options - so the script carries the flags and
# the step keeps the shell for its wrapper line. Both are asserted because neither is observable
# from a case: `-e` is covered behaviourally by the wrong-shape payload below, but every pipeline
# in the script is `echo | tool`, and `echo` into a pipe does not fail, so nothing can make
# pipefail matter. These two greps are all that stand behind it.
if ! grep -Eq '^[[:space:]]*shell: bash$' "$workflow"; then
  echo "FAIL: the close_milestone step no longer sets shell: bash"
  exit 1
fi

if ! grep -q 'set -eo pipefail' "$target"; then
  echo "FAIL: $target no longer sets -eo pipefail, which it cannot inherit from the step"
  exit 1
fi

work=$(mktemp -d)
cleanup() {
  rm -rf "$work"
  return 0
}
trap cleanup EXIT

mkdir -p "$work/bin"

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

  # Bare `bash <file>`, exactly as the step invokes it - deliberately no flags. The step's
  # `shell: bash` applies to its one-line wrapper, not to this shell, so the script is responsible
  # for its own `-eo pipefail`. Passing flags here would mask that: the script could lose `set -e`
  # and every case below would still pass.
  actual=$(PATH="$work/bin:$PATH" \
    TAG="$tag" \
    GITHUB_REPOSITORY="khanjal/RaptorSheets" \
    GH_TOKEN="not-a-real-token" \
    gh_log="$log" \
    fake_milestones="$payload" \
    fake_get_fails="$get_fails" \
    fake_patch_fails="$patch_fails" \
    bash "$target" 2>&1)
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

# The whole message, not just the prefix, because the "(or '...')" half is the only thing that pins
# alt_spelling's default arm. Inline, `ALT` was pre-assigned to `$TAG` and the `case` only
# overwrote it, so a missing default arm was harmless; as a function that returns the spelling,
# dropping it makes alt empty and this case report "(or '')".
assert_case "a tag with no slash matches nothing" \
  "v5.1.0" "$default_milestones" 0 \
  "No open milestone titled 'v5.1.0' (or 'v5.1.0'); nothing to close."

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
