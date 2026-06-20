#!/usr/bin/env pwsh
# Writes a per-step timing table for THIS job to the GitHub job summary, so the
# run summary page shows where the build/test time went without opening the log.
#
# Resolves the current job from the jobs API (matched by the runner it executes
# on, like report-failed-tests.ps1), reads each completed step's started/completed
# timestamps, and renders a table. Meant to run as a late `if: always()` step, so
# every run gets a breakdown. Best-effort: any missing token/permission/match just
# skips the table, and it always exits 0.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

if ([string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) { exit 0 }
if (-not ($env:GH_API_TOKEN -and $env:GITHUB_API_URL -and $env:GITHUB_REPOSITORY -and $env:GITHUB_RUN_ID -and $env:RUNNER_NAME)) { exit 0 }

function Format-Duration {
    param([double]$Seconds)
    # Floor the minutes. PowerShell's [int] cast ROUNDS (banker's), so
    # [int](99/60) = 2, not 1 — which would print "2m 39s" for 1m39s.
    $s = [int][math]::Round($Seconds)
    if ($s -ge 60) { return '{0}m {1:00}s' -f [int][math]::Floor($s / 60), ($s % 60) }
    return '{0}s' -f $s
}

function Format-Cell {
    param([string]$Text)
    return ($Text -replace '\|', '\|')
}

try {
    $headers = @{
        Authorization          = "Bearer $($env:GH_API_TOKEN)"
        Accept                 = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
    }
    $attempt = if ($env:GITHUB_RUN_ATTEMPT) { $env:GITHUB_RUN_ATTEMPT } else { '1' }
    $uri = '{0}/repos/{1}/actions/runs/{2}/attempts/{3}/jobs?per_page=100' -f `
        $env:GITHUB_API_URL, $env:GITHUB_REPOSITORY, $env:GITHUB_RUN_ID, $attempt
    $jobs = (Invoke-RestMethod -Headers $headers -Uri $uri -Method Get).jobs

    $job = $jobs | Where-Object { $_.runner_name -eq $env:RUNNER_NAME -and $_.status -eq 'in_progress' } | Select-Object -First 1
    if (-not $job) {
        $job = $jobs | Where-Object { $_.runner_name -eq $env:RUNNER_NAME } | Select-Object -Last 1
    }
    if (-not $job -or -not $job.steps) { exit 0 }

    $rows = [System.Collections.Generic.List[object]]::new()
    $total = 0.0
    foreach ($step in $job.steps) {
        if (-not $step.started_at -or -not $step.completed_at) { continue }
        $start = [datetimeoffset]::Parse([string]$step.started_at)
        $end = [datetimeoffset]::Parse([string]$step.completed_at)
        $seconds = ($end - $start).TotalSeconds
        if ($seconds -lt 0) { continue }
        $total += $seconds
        $rows.Add([pscustomobject]@{ Name = [string]$step.name; Seconds = $seconds })
    }
    if ($rows.Count -eq 0) { exit 0 }

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine(('### ⏱️ Timing — {0}' -f $job.name))
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('| Step | Duration |')
    [void]$sb.AppendLine('| --- | ---: |')
    foreach ($row in $rows) {
        [void]$sb.AppendLine(('| {0} | {1} |' -f (Format-Cell $row.Name), (Format-Duration $row.Seconds)))
    }
    [void]$sb.AppendLine(('| **Total (timed steps)** | **{0}** |' -f (Format-Duration $total)))
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value $sb.ToString()
    # Also echo to the step log (the rendered job summary isn't retrievable via API).
    Write-Host $sb.ToString()
}
catch { }

exit 0
