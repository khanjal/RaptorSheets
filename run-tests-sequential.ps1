<#
.SYNOPSIS
    Runs each test project's tests one at a time instead of `dotnet test` at the repo root.

.DESCRIPTION
    Several test projects (currently Stock and Gig, and locally Job when credentials are
    configured) run integration tests against a live Google Sheets spreadsheet using the same
    service-account credentials. Their fixtures delete/recreate/reseed sheets once per test
    assembly. `dotnet test` at the repo root runs each test project's host process concurrently,
    so those fixtures can hit Google's Sheets API rate limits at the same time and fail with
    null/empty-result errors that have nothing to do with the code under test.

    Running each project sequentially avoids that contention. CI mitigates the same issue with a
    retry loop instead (see .github/workflows/dotnet.yml) since solution-wide parallel test runs
    are much faster there when it doesn't flake.

.EXAMPLE
    ./run-tests-sequential.ps1
#>

$ErrorActionPreference = 'Stop'

$testProjects = Get-ChildItem -Path $PSScriptRoot -Filter '*.Tests.csproj' -Recurse |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|\.claude)\\' } |
    Sort-Object FullName

$results = @()

foreach ($project in $testProjects) {
    Write-Host "`n=== Testing $($project.Name) ===" -ForegroundColor Cyan

    & dotnet test $project.FullName --nologo
    $results += [pscustomobject]@{
        Project = $project.Name
        Passed  = ($LASTEXITCODE -eq 0)
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
$results | ForEach-Object {
    $color = if ($_.Passed) { 'Green' } else { 'Red' }
    Write-Host "$($_.Project): $(if ($_.Passed) { 'PASSED' } else { 'FAILED' })" -ForegroundColor $color
}

if ($results | Where-Object { -not $_.Passed }) {
    exit 1
}
