#!/usr/bin/env pwsh
# Surfaces failing tests from a test lane directly in that lane's own step log,
# in a human-readable form, plus GitHub `::error::` annotations that point at the
# failing source line.
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
    $root = $env:GITHUB_WORKSPACE
    if ([string]::IsNullOrWhiteSpace($root)) { return $Path }
    $norm = $Path -replace '\\', '/'
    $root = $root -replace '\\', '/'
    if ($norm.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $norm.Substring($root.Length).TrimStart('/')
    }
    return $norm
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

$grandTotal = 0

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

    $index = 0
    foreach ($block in $blocks) {
        $index++
        $grandTotal++

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
        # Stack = from the first "at ..." onward.
        $body = $block | Select-Object -Skip 1
        $messageLines = [System.Collections.Generic.List[string]]::new()
        $stackLines = [System.Collections.Generic.List[string]]::new()
        $inStack = $false
        foreach ($l in $body) {
            if (-not $inStack -and $l -match '^\s*at\s') { $inStack = $true }
            if ($inStack) {
                if ($l.Trim()) { $stackLines.Add($l.Trim()) }
            }
            elseif ($l.Trim()) {
                $messageLines.Add($l.Trim())
            }
        }
        $message = ($messageLines -join ' ').Trim()
        # Drop the MTP/xUnit exception-wrapper prefix so the real exception type and
        # message lead.
        $message = $message -replace '^Xunit\.Runner\.InProc\.SystemConsole\.TestingPlatform\.XunitException:\s*', ''
        if (-not $message) { $message = '(no message — see stack trace / test-results artifact)' }

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
        Write-Host ("     Reason   {0}" -f $message)
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

        # PR-checks annotation, anchored at the failing source line when known.
        $escTitle = $fqn -replace '%', '%25' -replace "`r", '' -replace "`n", ' '
        $escMsg = $message -replace '%', '%25' -replace "`r", '' -replace "`n", ' '
        if ($sourceFile) {
            Write-Host ("::error file={0},line={1},title={2}::{3}" -f $sourceFile, $sourceLine, $escTitle, $escMsg)
        }
        else {
            Write-Host ("::error title={0}::{1}" -f $escTitle, $escMsg)
        }
    }
}

Write-Host ''
if ($grandTotal -gt 0) {
    Write-Host ("  Total: {0} failing test{1}." -f $grandTotal, ($(if ($grandTotal -eq 1) { '' } else { 's' })))
}
exit 0
