#!/usr/bin/env pwsh
# Surfaces failing tests from a test lane in two places: a human-readable block in
# that lane's own step log, and a Markdown list appended to the GitHub job summary
# ($GITHUB_STEP_SUMMARY) so the failures also show on the run's summary page.
#
# The xUnit-v3 / Microsoft-Testing-Platform per-project log
# (`<Project>_<tfm>_<arch>.log`, UTF-16) carries the test-level detail that the
# step's console output drops (the console only shows per-DLL `failed: N`). Each
# failure block in that log looks like:
#
#     failed <FullyQualifiedName>[(theory args)] (<duration>)
#         <assertion / exception message, may span lines>
#             at <stack frame>
#             at <... in /path/File.cs:line>
#
# Called from the test lane after `dotnet test`; the lane keeps and propagates the
# real test exit code, so this script is best-effort and always exits 0.
[CmdletBinding()]
param(
    # The relative results directory the lane passed to `--results-directory`
    # (e.g. "TestResults/unit"). MTP resolves that path per test project, so the
    # per-project logs land in several places that all share the
    # "/TestResults/unit/" path segment; we match on the segment rather than a
    # single directory, which also keeps the two lanes apart.
    [Parameter(Mandatory = $true)]
    [string]$ResultsDirectory
)

$ErrorActionPreference = 'Continue'

# The box-drawing separator and the cross mark are UTF-8; make sure the host emits
# UTF-8 regardless of the runner's default console encoding.
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

function Get-RepoRelativePath {
    param([string]$Path)
    $norm = $Path -replace '\\', '/'
    $root = $env:GITHUB_WORKSPACE
    if (-not [string]::IsNullOrWhiteSpace($root)) {
        $root = $root -replace '\\', '/'
        if ($norm.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $norm.Substring($root.Length).TrimStart('/')
        }
    }
    # Deterministic builds (ContinuousIntegrationBuild) map the repo root to "/_/",
    # so lib verticals report frames as /_/src/...; normalise that too.
    if ($norm -match '^/_/(.+)$') { return $Matches[1] }
    return $norm
}

function Format-SummaryText {
    # Neutralise HTML angle brackets / ampersands so type names like List<int> and
    # <null> render literally in the Markdown summary.
    param([string]$Text)
    return ($Text -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;')
}

function Get-CommitSha {
    # The sha to link blobs against. On a pull_request run GITHUB_SHA is the
    # ephemeral merge commit, whose blob URLs 404 in the web UI; use the PR head
    # sha from the event payload instead. Falls back to GITHUB_SHA (push to main).
    if ($env:GITHUB_EVENT_PATH -and (Test-Path -LiteralPath $env:GITHUB_EVENT_PATH)) {
        try {
            $sha = (Get-Content -LiteralPath $env:GITHUB_EVENT_PATH -Raw | ConvertFrom-Json).pull_request.head.sha
            if ($sha) { return $sha }
        }
        catch { }
    }
    return $env:GITHUB_SHA
}

function Format-Source {
    # "<file>:<line>" (filename only; the project line gives the rest), linked to
    # the exact line on the head commit when the run context is known, otherwise
    # the same text as inline code.
    param([string]$File, [string]$Line)
    if (-not $File) { return '' }
    $text = '{0}:{1}' -f (($File -split '/')[-1]), $Line
    if ($env:GITHUB_SERVER_URL -and $env:GITHUB_REPOSITORY -and $script:CommitSha) {
        return '[{0}]({1}/{2}/blob/{3}/{4}#L{5})' -f $text, $env:GITHUB_SERVER_URL, $env:GITHUB_REPOSITORY, $script:CommitSha, $File, $Line
    }
    return '`{0}`' -f $text
}

$segment = '/' + (($ResultsDirectory -replace '\\', '/').Trim('/')) + '/'

$logs = Get-ChildItem -Path . -Recurse -Filter '*_x64.log' -ErrorAction SilentlyContinue |
        Where-Object {
            $_.Name -match '_net\d' -and
            (($_.FullName -replace '\\', '/') -like "*$segment*")
        }

if (-not $logs) {
    Write-Host "No '$ResultsDirectory' test logs found to scan for failing tests."
    exit 0
}

# Lane name (e.g. "unit" / "integration") for the job-summary heading.
$lane = (($ResultsDirectory -replace '\\', '/').TrimEnd('/') -split '/')[-1]

# Commit the job-summary source links point at (resolved once).
$script:CommitSha = Get-CommitSha

# Markdown accumulated for the GitHub job summary, written once at the end.
$summary = [System.Text.StringBuilder]::new()
$anyFailures = $false

foreach ($log in $logs) {
    # PS7 Get-Content auto-detects the UTF-16 BOM these logs carry.
    $lines = Get-Content -LiteralPath $log.FullName
    if (-not $lines) { continue }

    # Collect each failure as the "failed ..." header line plus every line up to
    # the next failed header or the run summary.
    $blocks = @()
    $current = $null
    foreach ($raw in $lines) {
        $line = $raw -replace "`r", ''
        if ($line -match '^\s*failed\s+\S') {
            if ($current) { $blocks += , $current }
            $current = [System.Collections.Generic.List[string]]::new()
            $current.Add($line)
            continue
        }
        if (-not $current) { continue }
        if ($line -match '^\s*(passed|skipped)\s+\S' -or
            $line -match '^\s*Test run summary' -or
            $line -match '^\s*In process file artifacts' -or
            $line -match '^\s*total:\s') {
            $blocks += , $current
            $current = $null
            continue
        }
        $current.Add($line)
    }
    if ($current) { $blocks += , $current }
    if ($blocks.Count -eq 0) { continue }

    $project = $log.BaseName -replace '_net\d.*$', ''

    Write-Host ''
    Write-Host ('─' * 78)
    Write-Host ("  ❌  {0} failed test{1} in {2}" -f $blocks.Count, ($(if ($blocks.Count -eq 1) { '' } else { 's' })), $project)
    Write-Host ('─' * 78)

    $anyFailures = $true
    [void]$summary.AppendLine(('**{0}** ({1} failed)' -f $project, $blocks.Count))
    [void]$summary.AppendLine('')

    $index = 0
    foreach ($block in $blocks) {
        $index++

        # Header: "failed <FQN>[(args)] (<duration>)" — duration is the last (...).
        $header = $block[0].Trim() -replace '^failed\s+', ''
        $duration = ''
        $m = [regex]::Match($header, '^(?<name>.+)\s+\((?<dur>[^)]*)\)\s*$')
        if ($m.Success) {
            $fqn = $m.Groups['name'].Value.Trim()
            $duration = $m.Groups['dur'].Value.Trim()
        }
        else {
            $fqn = $header
        }

        # Short, recognisable headline: Class.Method(args), full FQN kept for the
        # annotation title.
        $namePart = $fqn
        $argPart = ''
        $paren = $fqn.IndexOf('(')
        if ($paren -ge 0) {
            $namePart = $fqn.Substring(0, $paren)
            $argPart = $fqn.Substring($paren)
        }
        $segments = $namePart.Split('.')
        $short = if ($segments.Length -ge 2) { ($segments[-2..-1] -join '.') } else { $namePart }
        $short = "$short$argPart"

        # Message = block lines before the first stack frame ("at ...").
        # Stack = the "at ..." frames from there on.
        $body = $block | Select-Object -Skip 1
        $messageLines = [System.Collections.Generic.List[string]]::new()
        $stackLines = [System.Collections.Generic.List[string]]::new()
        $inStack = $false
        foreach ($l in $body) {
            $t = $l.Trim()
            if (-not $t) { continue }
            # Under parallel execution MTP interleaves per-test progress lines like
            # "[+10/x2/?0] Asm.dll - Other.Test (3s)"; they are not part of this
            # failure, so drop them wherever they land.
            if ($t -match '^\[[+\-x?\d/]+\]') { continue }
            if (-not $inStack -and $l -match '^\s*at\s') { $inStack = $true }
            if ($inStack) {
                # Only genuine "at ..." frames belong in the stack.
                if ($l -match '^\s*at\s') { $stackLines.Add($t) }
            }
            else {
                $messageLines.Add($t)
            }
        }
        if ($messageLines.Count -eq 0) {
            $messageLines.Add('(no message — see the stack trace below)')
        }
        # Drop the MTP/xUnit exception-wrapper prefix from the first line so the real
        # exception type and message lead.
        $messageLines[0] = $messageLines[0] -replace '^Xunit\.Runner\.InProc\.SystemConsole\.TestingPlatform\.XunitException:\s*', ''

        # Source location: first user stack frame carrying " in <file>:line".
        $sourceFile = ''
        $sourceLine = ''
        foreach ($s in $stackLines) {
            $sm = [regex]::Match($s, ' in (?<file>.+):(?:line )?(?<line>\d+)\s*$')
            if ($sm.Success) {
                $sourceFile = Get-RepoRelativePath $sm.Groups['file'].Value
                $sourceLine = $sm.Groups['line'].Value
                break
            }
        }

        Write-Host ''
        Write-Host ("  {0}) {1}" -f $index, $short)
        # Reason may span several lines; align continuations under the first one.
        Write-Host ("     Reason   {0}" -f $messageLines[0])
        for ($i = 1; $i -lt $messageLines.Count; $i++) {
            Write-Host ("              {0}" -f $messageLines[$i])
        }
        if ($sourceFile) {
            Write-Host ("     Source   {0}:{1}" -f $sourceFile, $sourceLine)
        }
        if ($duration) {
            Write-Host ("     Time     {0}" -f $duration)
        }
        if ($stackLines.Count -gt 0) {
            Write-Host "::group::     Stack trace"
            foreach ($s in $stackLines) { Write-Host "       $s" }
            Write-Host '::endgroup::'
        }

        # One list item in the job summary: `Class.Method`: reason (source link).
        $reasonText = (($messageLines | ForEach-Object { Format-SummaryText $_ }) -join ' ')
        $src = Format-Source $sourceFile $sourceLine
        $item = '- `{0}`: {1}' -f $short, $reasonText
        if ($src) { $item = '{0} ({1})' -f $item, $src }
        [void]$summary.AppendLine($item)
    }

    [void]$summary.AppendLine('')
}

if ($anyFailures -and -not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
    $heading = "### ❌ Failed tests ({0} lane)`n`n" -f $lane
    Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value ($heading + $summary.ToString())
}

exit 0
