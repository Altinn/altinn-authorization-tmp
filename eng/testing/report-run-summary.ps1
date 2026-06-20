#!/usr/bin/env pwsh
# Writes a compact per-vertical timing block to the run summary page: a header
# (wall-clock, runner-time, vertical count) and one aligned line per vertical
# (build / unit / integration / total / queue), slowest first.
#
# The per-vertical jobs run in parallel, so this aggregation job (needs: the
# matrix) is the only place that can see the run as a whole. Rendered in a code
# block so the columns line up and the lines don't collapse into one paragraph.
#
# Best-effort: any missing token/permission/data just skips the block, exit 0.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

if ([string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) { exit 0 }
if (-not ($env:GH_API_TOKEN -and $env:GITHUB_API_URL -and $env:GITHUB_REPOSITORY -and $env:GITHUB_RUN_ID)) { exit 0 }

function Format-Duration {
    param($Seconds)
    if ($null -eq $Seconds) { return '-' }
    # Floor the leading unit. PowerShell's [int] cast ROUNDS (banker's), so
    # [int](280/60) = 5, which would print "5m 40s" for 4m40s.
    $s = [int][math]::Round([double]$Seconds)
    if ($s -ge 3600) { return '{0}h {1:00}m' -f [int][math]::Floor($s / 3600), [int][math]::Floor(($s % 3600) / 60) }
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
        $started = [datetimeoffset]$job.started_at
        $completed = [datetimeoffset]$job.completed_at
        $total = ($completed - $started).TotalSeconds
        if ($total -lt 0) { continue }
        $queue = if ($job.created_at) { [math]::Max(0, ($started - [datetimeoffset]$job.created_at).TotalSeconds) } else { 0 }
        [pscustomobject]@{
            Vertical    = $m.Groups['v'].Value
            Build       = (Step-Seconds $job 'Build')
            Unit        = (Step-Seconds $job 'Test - unit lane*')
            Integration = (Step-Seconds $job 'Test - integration lane*')
            Total       = $total
            Queue       = $queue
            Completed   = $completed
        }
    }
    if (-not $verticals) { exit 0 }

    $sorted = $verticals | Sort-Object Total -Descending
    $runnerSeconds = ($verticals | Measure-Object -Property Total -Sum).Sum

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

    $nameWidth = ($verticals | ForEach-Object { $_.Vertical.Length } | Measure-Object -Maximum).Maximum

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine('```')
    [void]$sb.AppendLine(('⏱️ CI {0} wall-clock · {1} runner-time · {2} {3}' -f `
        (Format-Duration $wall), (Format-Duration $runnerSeconds), $verticals.Count, `
            $(if ($verticals.Count -eq 1) { 'vertical' } else { 'verticals' })))
    [void]$sb.AppendLine('')
    foreach ($v in $sorted) {
        [void]$sb.AppendLine(('{0}  build {1} · unit {2} · integration {3} · total {4} · queue {5}' -f `
            $v.Vertical.PadRight($nameWidth), `
            (Format-Duration $v.Build).PadLeft(7), `
            (Format-Duration $v.Unit).PadLeft(6), `
            (Format-Duration $v.Integration).PadLeft(7), `
            (Format-Duration $v.Total).PadLeft(7), `
            (Format-Duration $v.Queue).PadLeft(6)))
    }
    [void]$sb.AppendLine('```')
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value $sb.ToString()
    Write-Host $sb.ToString()
}
catch {
    Write-Host "report-run-summary: skipped ($($_.Exception.Message))"
}

exit 0
