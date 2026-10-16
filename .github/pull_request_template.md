<!-- What changed, and why. -->


## Verification

<!-- Actual counts, not "tests pass". The gating workflow runs unit tests only. -->

- Unit suite: ___ passed, ___ failed, ___ skipped (`dotnet test --configuration Release --filter "FullyQualifiedName!~Integration"`)
- Live-sheet suite: not run / run on ___ — `integration-tests.yml` never runs on a PR. Run it by
  hand for a change to formula generation, sheet creation, self-heal or anything else only a real
  spreadsheet can falsify.

## Reviews

Neither model review may be by the model that wrote the code, and neither runs in the session that
wrote it — each is its own sub-agent with a model override; see `.claude/skills/pr-review`. Copilot
goes last, on the code that will actually merge. Tick each box when it is done, and fill in the
model and the commit it reviewed.

Written by: ___ (the model that wrote the code; it decides which models review it)

- [ ] **First review**, before the PR opened. Model: ___ · commit: ___ (and again at ___ if the code changed a lot after it)
- [ ] **Final review** of the finished PR, before merge. Model: ___ · commit: ___
- [ ] **Copilot**, requested once, last — after the final review's fixes are pushed and green. Commit: ___ · Result: reviewed / quota exhausted / never registered
- [ ] Every finding fixed with a test, or written down above with why not
- [ ] In a milestone: the release it ships in, named for the tag (`core/v5.1.0`, `gig/v5.1.0`, `stock/v1.0.0`)
- [ ] SonarCloud issues on this PR: 0 (read from the API, not the tick — and skipped entirely on a fork PR)
