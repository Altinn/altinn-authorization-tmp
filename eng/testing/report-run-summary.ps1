#!/usr/bin/env pwsh
# Writes ONE line to the run summary page: total wall-clock and the slowest
# vertical (the long pole that gates it), with that vertical's build/integration
# split. The per-vertical jobs run in parallel, so this aggregation job (needs:
# the matrix) is the only place that can see the run as a whole. Anything more
# detailed is a click away in the run's own UI.
#
# Best-effort: any missing token/permission/data just skips the line, exit 0.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

if ([string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) { exit 0 }
if (-not ($env:GH_API_TOKEN -and $env:GITHUB_API_URL -and $env:GITHUB_REPOSITORY -and $env:GITHUB_RUN_ID)) { exit 0 }

function Format-Duration {
    param([double]$Seconds)
    # Floor the leading unit. PowerShell's [int] cast ROUNDS (banker's), so
    # [int](280/60) = 5, which would print "5m 40s" for 4m40s.
    $s = [int][math]::Round($Seconds)
    if ($s -ge 60) { return '{0}m {1:00}s' -f [int][math]::Floor($s / 60), ($s % 60) }
    return '{0}s' -f $s
}

function Step-Seconds {
    param($Job, [string]$Like)
    $step = $Job.steps | Where-Object { $_.name -like $Like -and $_.started_at -and $_.completed_at } | Select-Object -First 1
    if (-not $step) { return $null }
    $secs = ([datetimeoffset]$step.completed_at - [datetimeoffset]$step.started_at).TotalSeconds
    if ($secs -lt 0) { return $null }
    return $secs
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

    # Per-vertical matrix jobs are named "ci (<vertical>) / <job>".
    $verticals = foreach ($job in $jobs) {
        $m = [regex]::Match([string]$job.name, '^ci \((?<v>.+?)\)')
        if (-not $m.Success -or -not $job.started_at -or -not $job.completed_at) { continue }
        $total = ([datetimeoffset]$job.completed_at - [datetimeoffset]$job.started_at).TotalSeconds
        if ($total -lt 0) { continue }
        [pscustomobject]@{
            Vertical  = $m.Groups['v'].Value
            Total     = $total
            Completed = [datetimeoffset]$job.completed_at
            Job       = $job
        }
    }
    if (-not $verticals) { exit 0 }

    $longPole = $verticals | Sort-Object Total -Descending | Select-Object -First 1

    # Wall-clock = run start to the last vertical finishing.
    $runStart = $null
    try {
        $run = Invoke-RestMethod -Headers $headers -Uri ('{0}/repos/{1}/actions/runs/{2}' -f `
                $env:GITHUB_API_URL, $env:GITHUB_REPOSITORY, $env:GITHUB_RUN_ID)
        if ($run.run_started_at) { $runStart = [datetimeoffset]$run.run_started_at }
    }
    catch { }
    $maxCompleted = ($verticals | Measure-Object -Property Completed -Maximum).Maximum
    if (-not $runStart) { $runStart = $maxCompleted }
    $wall = ($maxCompleted - $runStart).TotalSeconds

    # The two phases worth optimizing inside the long pole.
    $split = @()
    $build = Step-Seconds $longPole.Job 'Build'
    $integration = Step-Seconds $longPole.Job 'Test - integration lane*'
    if ($null -ne $build) { $split += 'build {0}' -f (Format-Duration $build) }
    if ($null -ne $integration) { $split += 'integration {0}' -f (Format-Duration $integration) }
    $splitText = if ($split.Count) { ' (' + ($split -join ', ') + ')' } else { '' }

    $line = '⏱️ CI {0} · slowest: {1} {2}{3}' -f `
        (Format-Duration $wall), $longPole.Vertical, (Format-Duration $longPole.Total), $splitText
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value $line
    Write-Host $line
}
catch {
    Write-Host "report-run-summary: skipped ($($_.Exception.Message))"
}

exit 0
