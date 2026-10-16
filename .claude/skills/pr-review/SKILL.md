---
name: pr-review
description: How a pull request in this repository (khanjal/RaptorSheets, a .NET 8 Google Sheets library published to NuGet) gets reviewed before it merges — a model review before it opens and another of the finished PR before merge, never by the model that wrote the code and never in the session that wrote it (usually Sonnet, then Fable); then Copilot once, last, on the code that will actually merge; the milestone named for the release tag that every PR goes in, and that an issue gets once it is scheduled for a release; all three comment surfaces; the PR's own SonarCloud issues read from the API rather than the green tick; and the live-sheet suite that does not gate the PR and therefore has to be read on purpose. Use this whenever opening a PR here, whenever about to merge one, and whenever asked to check, triage or review the open PRs — including from a cloud session, which has no local memory of any of this.
---

# Reviewing a pull request before it merges

Every PR here is reviewed three times before it merges: by a model before it opens, by a model
again on the finished PR, and then by Copilot on the code as it will merge. Neither model review is
ever by the model that wrote the code — when Opus wrote it, the usual case here, that is Sonnet then
Fable; the table in step 1 has the rest — and neither runs in the session that wrote the code: each
is its own sub-agent with a model override, reading the diff cold.

Green CI replaces none of them, and in this repo that gap is unusually wide. The gating job
runs **unit tests only** — `FullyQualifiedName!~Integration`. The tests that talk to real Google
spreadsheets, the only ones that can catch a formula this library writes but Sheets rejects, run
in a separate workflow that gates nothing. A fully green PR here has not been proven against
Sheets at all.

## The checklist in every PR description

Every PR description carries the **Reviews** checklist from `.github/pull_request_template.md`:
who wrote the code, the first review, the final review, Copilot, findings handled, the milestone,
Sonar at zero. "Written by" is what step 1's table is read off, so a session picking up somebody
else's PR does not have to guess — if it is blank, read the commits' `Co-Authored-By` trailers. Tick
each box as it is done, with the model and the commit it reviewed, so anyone reading the PR can see
what is still owed. A PR is not ready to merge with a box unticked.

`gh pr create --body` does **not** apply the template — GitHub only uses it for PRs opened in
the web UI — so paste the checklist into the body yourself, and add it to any open PR that
lacks it. Update the ticks by editing the body:

```bash
gh api repos/khanjal/RaptorSheets/pulls/<n> --jq .body > body.md   # edit, then:
gh api repos/khanjal/RaptorSheets/pulls/<n> -X PATCH -F body=@body.md
```

This repo has a single remote, so `gh pr` and `gh issue` resolve it without `-R`. `gh api` does
not take `-R` at all — it fails with "unknown shorthand flag" — and does not need it, because
its paths name the repo.

## 1. Before the PR opens: a review by a different model

Always, whatever Copilot's quota. The model that wrote the code shares its own wrong assumptions, so
a review by the same model misses exactly the bugs that matter most. Run the review as a sub-agent
with a model override — the Agent tool's `model` parameter — not as `/code-review` in the session
that wrote the code.

- **`sonnet` (Sonnet 5), before the PR opens.** The quick first pass.
- **`fable` (Fable 5.1), on the finished PR — step 5.** It goes after everything the first review
  prompted is fixed, so it reviews the fixes too: a fix for one review's finding is new code nobody
  has reviewed yet.

**Neither review is ever by the model that wrote the code.** The pairing above assumes Opus
wrote it, the usual case. If it was written with a different model, swap in Opus for whichever
reviewer matches it:

| Written by | First review (step 1) | Final review (step 5) |
|---|---|---|
| Opus | Sonnet | Fable |
| Sonnet | Opus | Fable |
| Fable | Sonnet | Opus |
| Haiku, or anything else | Sonnet | Fable |

Give it: the repo path; the diff range as `origin/main...<commit>` after a `git fetch`, never a
bare `main...`, which in a stale checkout pulls in commits already merged; that it is read-only
(no edits, commits or GitHub posts); what the change is meant to do; and to hunt for bugs with a
concrete failure scenario each, including in unchanged code whose assumptions the change broke.
Ask for at most 15 findings with file:line and a confidence, and for it to say plainly if it
finds nothing.

Four things in this codebase are worth naming in the brief, because they are where the bugs
have actually been:

- **Anything that computes a row index.** `RowId` means a different thing on read than on write
  (issue #134), and a change that gets this wrong duplicates a user's row rather than editing it.
- **Anything that writes a generated formula.** The unit suite cannot tell whether Sheets will
  accept it (issue #128). If the change touches formula generation, the review should say so, and
  step 3 below applies.
- **Anything that changes a header, column set or sheet layout**, because self-heal will then
  rewrite real user spreadsheets on the next `GetSheets` — including for callers who did not ask
  for it. `autoHeal` (#113, shipped in #139) is the opt-out; a change that makes self-heal do more
  should be judged as if it runs unannounced on someone's data, because it does.
- **Anything that touches shared test state.** The integration suites leak state between tests
  and fail by order (issue #130), so a new fixture or a changed `CleanSlateSheetFixture` path can
  turn a whole domain's suite red for reasons unrelated to the change.

Run it in the background and do the build and test run meanwhile. When it returns:

- check each finding against the code before acting on it — a reviewer can be right about a
  problem and wrong about its cause
- fix the real ones on the branch with a test
- write down the ones not fixed, with why, in the PR description as possibilities to check
- say in the PR description which model reviewed it and what became of each finding

If the code then changes well beyond fixes to those findings — a new approach, a new area — run
step 1 again on the new commits, and add that run to the checklist's first-review line.

## 2. When the PR opens: the milestone it ships in

**Every PR goes in a milestone**, set when the PR opens and not left for the merge. A PR with none
drops out of the milestone's list, which is how a release's contents get checked; one merged without
it can be added afterwards, but is easy to forget.

Milestones here are named for the **tag the work ships under** — `core/v5.1.0`, `gig/v5.1.0`,
`stock/v1.0.0` — because the packages are versioned and released independently, and
`release-packages.yml` reads the package out of the tag. The exact match is load-bearing: that
workflow closes the milestone whose title equals the tag.

| Milestone | What it holds |
|---|---|
| `core/v5.1.0` | Integration-suite rebuild (#135, #130), formulas proven against Sheets (#128), reapply finished (#120) |
| `core/v5.2.0` | `SheetRange` (#65), partial reads (#69), the ambient batch scope (#61) |
| `core/v6.0.0` | The sheet-shape investigations (#81, #85, #86), where a breaking change belongs |
| `gig/v5.1.0` | The Gig release riding `core/v5.1.0` |
| `gig/v5.2.0` | Gig analytics: the Tags rollup (#126) |
| `stock/v1.0.0` | Stock's first publish to NuGet |

Two rules make the per-package naming work:

- **A cross-cutting PR picks the package that ships it first**, usually Core, since GitHub allows a
  PR only one milestone. Name the other packages it goes out in in the description instead.
- **Work that ships in no package** — test suites, CI, tooling, docs — goes in the **next Core
  release**: the open `core/*` milestone with the lowest version, `core/v5.1.0` today. Several
  milestones are open at once, so "whichever one is open" picks nothing; Core is what every package
  rebuilds against, so its next release is the honest home. A milestone then reads as everything
  that happened in that cycle, not only what the package contains.

`gh pr create` takes `--milestone <title>`. Afterwards:

```bash
gh pr edit <n> --milestone "core/v5.1.0"
gh issue edit <n> --milestone "core/v5.1.0"
gh api repos/khanjal/RaptorSheets/milestones --jq '.[] | "\(.number) \(.title) open=\(.open_issues)"'
```

An issue gets the milestone of the release it is scheduled for. One nobody has committed to a
release is better left out than parked in a version that will not hold it — and when the next
version's work is being chosen, the milestone is the list to fill, not the label.

`release-packages.yml` closes a milestone when its release publishes, but **only when nothing in it
is still open** — `open_issues` counts open PRs too, so one unmerged PR or one unfinished issue
holds the milestone open and the run says so in a warning. Before tagging a release, move the
stragglers to the next milestone rather than leaving the one being released half-finished.

## 3. Tests: the gating job is unit-only

`dotnet.yml` runs `dotnet test --filter "FullyQualifiedName!~Integration"`. Locally, that is:

```bash
dotnet test --configuration Release --filter "FullyQualifiedName!~Integration"
```

Report the actual pass count, not "tests pass". Two things the green tick does not tell you:

- **A skipped test is not a failure.** The integration tests skip themselves when credentials or
  a spreadsheet id are missing, so a config regression shows up as ~100 extra skips and a green
  check. The workflow surfaces the count in the job summary as a warning; read it. It is
  deliberately not a hard failure yet.
- **Live-sheet coverage did not run on this PR.** `integration-tests.yml` fires on push to main,
  on a 06:00 UTC cron and manually — never on a PR — and `release-packages.yml` gates on
  `dotnet.yml`, not on it. If the change touches formula generation, sheet creation, self-heal or
  anything else that only a real spreadsheet can falsify, run it by hand before merging:

```bash
gh workflow run integration-tests.yml -r <branch>
```

It takes ~18 minutes serialised and is flaky by nature — Google's metadata is briefly stale after
a tab is regenerated — and its concurrency group means a second run queues rather than racing.
A failure there is worth reading before assuming it is flake.

## 4. Sonar: the PR's issues, not the tick

A green Sonar check can hide an open issue. Read the count directly:

```bash
curl -s "https://sonarcloud.io/api/issues/search?componentKeys=khanjal_RaptorSheets&pullRequest=<n>&resolved=false"
```

Check first that the analysis is of the branch head — it lags a push by a few minutes, and a
stale count reads as current. `project_analyses/search` ignores `pullRequest` and returns main's,
so use the pull-request list, whose `commit.sha` for the PR must equal its head:

```bash
curl -s "https://sonarcloud.io/api/project_pull_requests/list?project=khanjal_RaptorSheets" \
  | python -c "import sys,json;[print(p['key'],p['commit']['sha']) for p in json.load(sys.stdin)['pullRequests']]"
```

Zero is the bar before merging. The PR checks only score new code, so `main` can build up a
backlog nobody sees on a PR — query it without `pullRequest` when triaging.

The scan runs in the `code-quality` job, which `needs: test` and is skipped entirely for a PR
from a fork. On a fork PR there is no Sonar result at all, and the box should say that rather
than be ticked.

## 5. The final model review of the finished PR

When everything else is done — the first review's findings fixed, CI green, Sonar at zero, any
live-sheet run passed — run the final reviewer from step 1's table (`fable` when Opus wrote the
code) as a sub-agent on the whole PR as it now stands (`origin/main...` the branch head, fetched),
with the same brief as step 1. Tell it what the first review found and what became of each, so it
spends its time on what that missed rather than confirming what is already fixed.

Handle its findings the way step 1 says. A fix it prompts is small by then and does not need a
further final review unless it changes behaviour well beyond the finding; Sonar (step 4) does need
checking again after it. Say on the PR that it ran.

## 6. Last: request Copilot, once

Only once the final review's fixes are pushed and CI and Sonar are green on them — when the PR would
otherwise be mergeable — so Copilot sees the code that will actually merge. And only once per PR:
each review spends from a monthly allowance, and once that is gone a request gets nothing at all. Do
not request it at open, nor again after the fixes it prompts. Run the timeline query below before
requesting: a `Copilot` line already there, on a PR opened before this rule, means its one request
is spent.

**Copilot does review PRs here, when it is asked.** Verified 2026-10-15: #99, #104, #105 and #107
all got substantive reviews, and so did #109 and #133 later. The PRs with no reviewer are the ones
where nobody requested one — #120, #136, #139, #141 and #142 have an empty `review_requested`
timeline. A missing Copilot review here means a missing request, not a reviewer that was never
wired up. Do not write "Copilot: reviewed" on a checklist because the request returned success,
though — read the review body first (step 7).

Copilot still matters after two model reviews: both models come from the same vendor, and Copilot
does not. It is the one reviewer whose blind spots are not correlated with the author's.

Request the review with the GraphQL mutation, naming the reviewer bot by its node id:

```bash
gh api repos/khanjal/RaptorSheets/pulls/<n> --jq .node_id
gh api graphql -f query='mutation{requestReviews(input:{pullRequestId:"<node id>",botIds:["BOT_kgDOCnlnWA"],union:true}){pullRequest{number}}}'
```

`BOT_kgDOCnlnWA` is `copilot-pull-request-reviewer`. `suggestedActors` lists only
`copilot-swe-agent`, which is the coding agent: requesting it returns success and nothing ever
arrives. The mutation's return value proves nothing either way. Check the timeline:

```bash
gh api repos/khanjal/RaptorSheets/issues/<n>/timeline --jq '.[] | select(.event=="review_requested") | .requested_reviewer.login'
```

**That query prints `Copilot`, not `copilot-pull-request-reviewer`** —
`copilot-pull-request-reviewer[bot]` is the login on the review itself, in
`pulls/<n>/reviews`. Grepping the timeline for the bot's name finds nothing even on a PR where the
request landed, and requesting again on that false negative spends a second review from the
allowance, which is the exact double-spend this ordering exists to avoid. Empty output means no
request landed. If it will not register, ask the user to add Copilot from the GitHub UI.

## 7. Read what came back

The review lags the request, so read it at the moment of merging, not when CI goes green; treat it
as gone only an hour after the request. If the merge instruction comes in a later turn, read again
immediately before merging — an earlier read is stale. There are three surfaces, and reading two of
them is the usual miss:

```bash
gh api repos/khanjal/RaptorSheets/pulls/<n>/reviews    # review bodies
gh api repos/khanjal/RaptorSheets/pulls/<n>/comments   # inline comments
gh api repos/khanjal/RaptorSheets/issues/<n>/comments  # issue-level: SonarQube Cloud, humans
```

**A quota-exhausted review looks like a clean one.** Its state is `COMMENTED`, it has no inline
comments, and its whole body reads "Copilot was unable to review this pull request because the
user who requested the review has reached their quota limit." Read the body before calling a PR
reviewed.

Treat Copilot's findings as informed, not authoritative. Check each against the current code, and
handle the real ones the way step 1 says. Like the final review's, their fixes do not need another
model review unless they change behaviour well beyond the finding; Sonar (step 4) needs checking
again after them. That leaves a Copilot-prompted fix reviewed only by its test, Sonar and the
session's own check — the accepted price of Copilot going last, not a gap to close with another
model review.

## 8. When Copilot could not review: steps 1 and 5 are the review of record

If the quota is gone, or the request would not register at all, or nothing came back within the
hour, the two model reviews are what the PR merges on. Do not wait for the allowance to reset. Say
so on the PR, rather than calling it reviewed as if Copilot had done it. Check the timeline first,
though: an empty `review_requested` means the request never went out, and that is a step 6 to run,
not a quota to work around.

## 9. Merge

- Every box in the PR's Reviews checklist is ticked.
- Any review that came back has been read on all three surfaces (step 7) and each finding fixed
  or answered.
- The final review in step 5 has run on the latest commit, or on one the later commits only fixed
  its or Copilot's findings in.
- The PR is in the milestone of the release it ships in (step 2).
- All checks green on the latest commit, not an earlier one. A failure that is NuGet refusing a
  download is infrastructure: re-run the failed job.
- Squash merge, which is what the history uses.
- A change that alters public API surface needs a version bump before release; `release-packages.yml`
  gates on `dotnet.yml` having passed, so a merge with a red unit suite blocks the next publish.
- Never enable auto-merge unless the user asked.
