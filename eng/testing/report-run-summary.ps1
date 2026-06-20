#!/usr/bin/env pwsh
# Writes a RUN-LEVEL timing overview to the GitHub job summary, so the run summary
# page shows where the whole CI run's time went, not just one vertical at a time.
#
# The per-vertical jobs run in parallel, so no single job can see the run as a
# whole. This runs in a final aggregation job (needs: the matrix) and queries the
# jobs API for every vertical's queue/exec timings, then reports:
#   - wall-clock latency (gated by the slowest vertical, the "long pole")
#   - total runner-time (sum of all vertical durations = the cost/load figure)
#   - max queue wait (time spent waiting for a runner)
#   - a per-vertical table (queue / build / unit / integration / total), sorted
#     by total so the dominant vertical is obvious.
#
# Best-effort: any missing token/permission/data just skips the section, exit 0.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

if ([string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) { exit 0 }
if (-not ($env:GH_API_TOKEN -and $env:GITHUB_API_URL -and $env:GITHUB_REPOSITORY -and $env:GITHUB_RUN_ID)) { exit 0 }

function Format-Duration {
    param([double]$Seconds)
    # Floor the leading unit. PowerShell's [int] cast ROUNDS (banker's), so
    # [int](280/60) = 5, not 4 — which would print "5m 40s" for 4m40s.
    $s = [int][math]::Round($Seconds)
    if ($s -ge 3600) { return '{0}h {1:00}m' -f [int][math]::Floor($s / 3600), [int][math]::Floor(($s % 3600) / 60) }
    if ($s -ge 60) { return '{0}m {1:00}s' -f [int][math]::Floor($s / 60), ($s % 60) }
    return '{0}s' -f $s
}

function Format-Cell {
    param([string]$Text)
    return ($Text -replace '\|', '\|')
}

function Step-Seconds {
    param($Job, [string]$Like)
    $step = $Job.steps | Where-Object { $_.name -like $Like -and $_.started_at -and $_.completed_at } | Select-Object -First 1
    if (-not $step) { return $null }
    $secs = ([datetimeoffset]::Parse([string]$step.completed_at) - [datetimeoffset]::Parse([string]$step.started_at)).TotalSeconds
    if ($secs -lt 0) { return $null }
    return $secs
}

function Cell-Duration {
    param($Seconds)
    if ($null -eq $Seconds) { return '—' }
    return (Format-Duration $Seconds)
}

try {
    $headers = @{
        Authorization          = "Bearer $($env:GH_API_TOKEN)"
        Accept                 = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
    }
    $attempt = if ($env:GITHUB_RUN_ATTEMPT) { $env:GITHUB_RUN_ATTEMPT } else { '1' }

    # Page through every job of this run attempt.
    $allJobs = [System.Collections.Generic.List[object]]::new()
    $page = 1
    while ($true) {
        $uri = '{0}/repos/{1}/actions/runs/{2}/attempts/{3}/jobs?per_page=100&page={4}' -f `
            $env:GITHUB_API_URL, $env:GITHUB_REPOSITORY, $env:GITHUB_RUN_ID, $attempt, $page
        $resp = Invoke-RestMethod -Headers $headers -Uri $uri -Method Get
        if (-not $resp.jobs -or $resp.jobs.Count -eq 0) { break }
        $resp.jobs | ForEach-Object { $allJobs.Add($_) }
        if ($allJobs.Count -ge $resp.total_count) { break }
        $page++
    }

    # The per-vertical matrix jobs are named "ci (<vertical>) / <job>". Everything
    # else (find-verticals, CodeQL, this aggregation job) is excluded.
    $rows = [System.Collections.Generic.List[object]]::new()
    foreach ($job in $allJobs) {
        $m = [regex]::Match([string]$job.name, '^ci \((?<v>.+?)\)')
        if (-not $m.Success) { continue }
        if (-not $job.started_at -or -not $job.completed_at) { continue }
        $created = if ($job.created_at) { [datetimeoffset]::Parse([string]$job.created_at) } else { $null }
        $started = [datetimeoffset]::Parse([string]$job.started_at)
        $completed = [datetimeoffset]::Parse([string]$job.completed_at)
        $queued = if ($created) { [math]::Max(0, ($started - $created).TotalSeconds) } else { 0 }
        $total = ($completed - $started).TotalSeconds
        if ($total -lt 0) { continue }
        $rows.Add([pscustomobject]@{
                Vertical    = $m.Groups['v'].Value
                Queue       = $queued
                Build       = (Step-Seconds $job 'Build')
                Unit        = (Step-Seconds $job 'Test - unit lane*')
                Integration = (Step-Seconds $job 'Test - integration lane*')
                Total       = $total
                Completed   = $completed
                Conclusion  = [string]$job.conclusion
            })
    }
    if ($rows.Count -eq 0) { exit 0 }

    $sorted = $rows | Sort-Object -Property Total -Descending
    $longPole = $sorted[0]
    $runnerSeconds = ($rows | Measure-Object -Property Total -Sum).Sum
    $maxQueue = ($rows | Measure-Object -Property Queue -Maximum).Maximum

    # Wall-clock = run start (or earliest job created) to the last vertical finishing.
    $runStart = $null
    try {
        $runUri = '{0}/repos/{1}/actions/runs/{2}' -f $env:GITHUB_API_URL, $env:GITHUB_REPOSITORY, $env:GITHUB_RUN_ID
        $run = Invoke-RestMethod -Headers $headers -Uri $runUri -Method Get
        if ($run.run_started_at) { $runStart = [datetimeoffset]::Parse([string]$run.run_started_at) }
    }
    catch { }
    $maxCompleted = ($rows | Measure-Object -Property Completed -Maximum).Maximum
    if (-not $runStart) {
        # Fall back to the earliest matrix-job creation if the run object is unavailable.
        $runStart = $maxCompleted
        foreach ($job in $allJobs) {
            if ($job.created_at) {
                $c = [datetimeoffset]::Parse([string]$job.created_at)
                if ($c -lt $runStart) { $runStart = $c }
            }
        }
    }
    $wall = ($maxCompleted - $runStart).TotalSeconds

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine('## ⏱️ CI run timing overview')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine(('- **Wall-clock:** {0}  ·  long pole: `{1}` at {2}' -f `
        (Format-Duration $wall), (Format-Cell $longPole.Vertical), (Format-Duration $longPole.Total)))
    [void]$sb.AppendLine(('- **Runner-time:** {0} across {1} {2}' -f `
        (Format-Duration $runnerSeconds), $rows.Count, $(if ($rows.Count -eq 1) { 'vertical' } else { 'verticals' })))
    [void]$sb.AppendLine(('- **Max queue wait:** {0}' -f (Format-Duration $maxQueue)))
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('| Vertical | Queue | Build | Unit | Integration | Total |')
    [void]$sb.AppendLine('| --- | ---: | ---: | ---: | ---: | ---: |')
    foreach ($r in $sorted) {
        $name = if ($r.Conclusion -and $r.Conclusion -ne 'success') { '{0} ⚠️' -f $r.Vertical } else { $r.Vertical }
        [void]$sb.AppendLine(('| {0} | {1} | {2} | {3} | {4} | {5} |' -f `
            (Format-Cell $name), (Format-Duration $r.Queue), (Cell-Duration $r.Build), `
            (Cell-Duration $r.Unit), (Cell-Duration $r.Integration), (Format-Duration $r.Total)))
    }
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value $sb.ToString()
    # Also echo to the step log (the rendered job summary isn't retrievable via API).
    Write-Host $sb.ToString()
}
catch {
    Write-Host "report-run-summary: skipped ($($_.Exception.Message))"
}

exit 0
