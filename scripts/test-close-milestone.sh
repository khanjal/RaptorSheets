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
# Requires bash, jq and python with PyYAML, all of which this repo already depends on elsewhere.
set -u

script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
repo_root=$(cd "$script_dir/.." && pwd)
target="$repo_root/.github/scripts/close-milestone.sh"
workflow="$repo_root/.github/workflows/release-packages.yml"

if [[ ! -f "$target" ]]; then
  echo "FAIL: $target does not exist"
  exit 1
fi

work=$(mktemp -d)
cleanup() {
  rm -rf "$work"
  return 0
}
trap cleanup EXIT

mkdir -p "$work/bin"

# The script is only half the job: it reads TAG and GH_TOKEN out of the environment, it has to be
# cloned before anything can run it, and the job's own settings decide whether a failure is loud.
# Extraction used to prove the step and the cases agreed for free - whatever the step ran was what
# the cases ran - and running the file directly costs that, so the wiring is asserted instead.
#
# Asserted against the parsed YAML, not with greps over the file. Greps were tried and were not
# good enough: file-global, they are satisfied by `env:` or `shell:` sitting on the wrong step, and
# `grep -q 'uses: actions/checkout@v4'` passes on a commented-out line or on a checkout belonging
# to some other job. Each of those leaves the job broken - a missing script, an empty TAG - with
# continue-on-error absorbing it and the release going green. Structure is the only honest way to
# ask "does THIS step still do X".
cat > "$work/assert_wiring.py" <<'PY'
import sys
import yaml

workflow_path = sys.argv[1]
job = yaml.safe_load(open(workflow_path))["jobs"]["close_milestone"]
problems = []

# The job's own settings. `contents: read` is what lets the checkout clone at all; without it
# checkout 403s, the script is never there, and continue-on-error hides the whole thing.
if job.get("permissions") != {"issues": "write", "contents": "read"}:
    problems.append(
        "permissions are %r, expected issues: write (milestones) and contents: read (checkout)"
        % (job.get("permissions"),)
    )

if job.get("continue-on-error") is not True:
    problems.append("continue-on-error is %r, expected True - a milestone hiccup must not turn a "
                    "published release red" % (job.get("continue-on-error"),))

if job.get("if") != "github.event_name == 'release'":
    problems.append("the job's `if` is %r, expected it to run only for a release - a manual "
                    "dispatch has no tag to match" % (job.get("if"),))

if job.get("needs") != "build":
    problems.append("`needs` is %r, expected 'build' - nothing should close a milestone before the "
                    "package is published" % (job.get("needs"),))

steps = job.get("steps") or []
names = [step.get("name") for step in steps]

# The checkout, and its position. A checkout that runs AFTER the script would leave `bash` with a
# missing file: exit 127, absorbed, milestone left open.
checkout = next((i for i, s in enumerate(steps)
                 if (s.get("uses") or "").startswith("actions/checkout@")), None)
runner = next((i for i, s in enumerate(steps)
               if s.get("name") == "Close the release's milestone"), None)

if checkout is None:
    problems.append("no actions/checkout step: the script it runs is a file in the repo (steps: %r)"
                    % (names,))
elif runner is None:
    problems.append("no step named \"Close the release's milestone\" (steps: %r)" % (names,))
elif checkout > runner:
    problems.append("the checkout is step %d but the script runs at step %d - it has to be cloned "
                    "first" % (checkout + 1, runner + 1))

if checkout is not None:
    with_block = steps[checkout].get("with") or {}
    if with_block.get("persist-credentials") is not False:
        problems.append(
            "the checkout's persist-credentials is %r, expected False - nothing runs git after the "
            "clone and the next step already holds a token"
            % (with_block.get("persist-credentials"),)
        )

if runner is not None:
    step = steps[runner]

    expected_run = 'bash .github/scripts/close-milestone.sh "$TAG"'
    if (step.get("run") or "").strip() != expected_run:
        problems.append("the step's run is %r, expected %r"
                        % (step.get("run"), expected_run))

    # Inert for a one-line `bash <file>` wrapper, whose status is the inner shell's either way -
    # the script sets its own `-eo pipefail`, which is what actually governs the logic. Kept and
    # asserted so a future multi-line `run:` here starts strict instead of silently getting the
    # `bash -e` (no pipefail) that a step without this line would get.
    if step.get("shell") != "bash":
        problems.append("the step's shell is %r, expected 'bash'" % (step.get("shell"),))

    expected_env = {
        "GH_TOKEN": "${{ github.token }}",
        "TAG": "${{ github.event.release.tag_name }}",
    }
    actual_env = step.get("env") or {}
    for key, expected in expected_env.items():
        if actual_env.get(key) != expected:
            problems.append("the step's env.%s is %r, expected %r"
                            % (key, actual_env.get(key), expected))

if problems:
    sys.exit("close_milestone: " + ("; ".join(problems)))
PY

if ! python "$work/assert_wiring.py" "$workflow"; then
  echo "FAIL: the close_milestone job's wiring no longer matches what these cases assume"
  exit 1
fi

# The script's own flags, which it cannot inherit: the step's `shell: bash` governs the wrapper
# line, and `bash <file>` then starts a fresh shell with no options at all. `-e` is covered
# behaviourally by the wrong-shape payload below. pipefail is not: real jq always drains stdin, so
# no payload can make `echo | jq` fail at the echo. (A stubbed jq that exits without reading would,
# on a payload past the pipe buffer - which is how this was checked rather than assumed.) So this
# line is all that stands behind pipefail, and it is here for the pipeline someone adds next.
if ! grep -Eq '^[[:space:]]*set -eo pipefail$' "$target"; then
  echo "FAIL: $target no longer sets -eo pipefail, which it cannot inherit from the step"
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

# Deliberately not khanjal/RaptorSheets. The PATH guard below is what keeps this offline, but the
# cases name real milestone numbers, so a placeholder means even a `gh` that somehow escaped the
# guard could not reach anything of ours. Nothing in the script treats one repo differently from
# another, so the cases lose nothing by it.
fake_repo="example-org/example-repo"

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

no_v_spelling_only="[
  $(milestone_json 9 "$tag_core_no_v" 0)
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
    GITHUB_REPOSITORY="$fake_repo" \
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

  # Every case: the lookup must GET the open milestones of the repo it was told about, a page at a
  # time big enough to hold them. The whole path, not just the query string - a repo hardcoded into
  # the URL would otherwise pass, and `state=open` dropped would read closed milestones as
  # candidates.
  if ! grep -qF -- "api repos/$fake_repo/milestones?state=open&per_page=100" "$log"; then
    echo "FAIL  $name"
    echo "      the script did not GET api repos/$fake_repo/milestones?state=open&per_page=100"
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
  "-X PATCH repos/$fake_repo/milestones/6 -f state=closed" \
  '{"number":0'

assert_case "a tag without the v finds the v spelling" \
  "$tag_core_no_v" "$v_spelling_only" 0 \
  "Closed milestone $tag_core"

# The mirror of the case above, and the only thing that pins alt_spelling's `*/v*)` arm: every
# other v-tag case finds its exact title first, so that arm could be replaced with `echo "$tag"`
# and nothing would notice. A tag of core/v5.1.0 against a milestone titled core/5.1.0 would then
# be left open for good.
assert_case "a tag with the v finds the no-v spelling" \
  "$tag_core" "$no_v_spelling_only" 0 \
  "Closed milestone $tag_core_no_v"

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

# The job passes the tag as an argument - `bash .github/scripts/close-milestone.sh "$TAG"` - while
# every case above hands it over through the environment. Both work, because the script reads
# `${1:-${TAG:-}}`, but that leaves the path the job actually takes unexercised: the script could
# stop reading $1 and every case would still pass while a release quietly closed nothing. Run with
# TAG unset so only the argument can be the source.
cases=$((cases + 1))
: > "$work/gh.log"
arg_output=$(PATH="$work/bin:$PATH" \
  GITHUB_REPOSITORY="$fake_repo" \
  GH_TOKEN="not-a-real-token" \
  gh_log="$work/gh.log" \
  fake_milestones="$default_milestones" \
  bash "$target" "$tag_stock" 2>&1)

if [[ "$arg_output" == *"Closed milestone $tag_stock"* ]]; then
  echo "ok    the tag can come from \$1, which is how the job passes it"
else
  echo "FAIL  the tag was passed as \$1, the way the job passes it, and was not used"
  echo "      output: $arg_output"
  failures=$((failures + 1))
fi

echo
if [[ "$failures" != "0" ]]; then
  echo "$failures of $cases cases FAILED"
  exit 1
fi
echo "all $cases cases passed"
exit 0
