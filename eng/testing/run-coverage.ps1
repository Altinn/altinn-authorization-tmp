#!/usr/bin/env pwsh
param(
    [int]$Threshold = 0,
    [string[]]$Projects,
    [string]$Configuration = 'Release',
    [switch]$NoBuild,
    [string]$ThresholdsFile,
    [string]$OwnedRoot = (Get-Location).Path
)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$srcRoot = Join-Path $repoRoot 'src'
$resultsDir = Join-Path $repoRoot 'TestResults'

$dotnetCov = Get-Command dotnet-coverage -ErrorAction SilentlyContinue
if (-not $dotnetCov) {
    Write-Host "Installing dotnet-coverage..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-coverage
}

if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }
New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null

if (-not $Projects) {
    $Projects = Get-ChildItem -Path $srcRoot -Filter '*.Tests.csproj' -Recurse |
        Select-Object -ExpandProperty FullName
}
Write-Host "Found $($Projects.Count) test project(s)" -ForegroundColor Cyan

if (-not $NoBuild) {
    foreach ($proj in $Projects) {
        dotnet build $proj --configuration $Configuration --verbosity minimal
        if ($LASTEXITCODE -ne 0) { exit 1 }
    }
}

$failed = @()
$coverageFiles = @()

foreach ($proj in $Projects) {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($proj)
    $projDir = [System.IO.Path]::GetDirectoryName($proj)
    Write-Host "`n=== $projName ===" -ForegroundColor Green

    $binDir = Join-Path $projDir "bin" $Configuration "net9.0"
    $dllPath = Join-Path $binDir "$projName.dll"
    $runtimeConfig = Join-Path $binDir "$projName.runtimeconfig.json"
    $outFile = Join-Path $resultsDir "$projName.cobertura.xml"

    # xUnit v3 test projects build as executables (OutputType=Exe) and run on
    # Microsoft Testing Platform. A runtimeconfig.json is always emitted for
    # Exe output; running the managed dll via `dotnet <dll>` works
    # cross-platform (Windows, Linux, macOS) under dotnet-coverage.
    $isV3Exe = (Test-Path $dllPath) -and (Test-Path $runtimeConfig)

    if ($isV3Exe) {
        Write-Host "  dotnet-coverage collect (xUnit v3 / MTP)" -ForegroundColor DarkGray
        # Note: running the managed dll directly invokes xUnit v3's native
        # in-process runner (not MTP). That runner exits 0 when every test is
        # [Skip]ped, so no --ignore-exit-code handling is required here
        # (unlike the `dotnet test -- --ignore-exit-code 8` path used by the
        # workflow's Build and Test step).
        dotnet-coverage collect --output $outFile --output-format cobertura -- dotnet $dllPath 2>&1 | Out-Default
    }
    else {
        Write-Host "  dotnet test (vstest fallback)" -ForegroundColor DarkGray
        $tmpDir = Join-Path $resultsDir $projName
        dotnet test $proj --no-build --configuration $Configuration --collect:"XPlat Code Coverage" --results-directory $tmpDir --logger "console;verbosity=minimal" 2>&1 | Out-Default
        $cobFile = Get-ChildItem -Path $tmpDir -Filter 'coverage.cobertura.xml' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($cobFile) { Copy-Item $cobFile.FullName $outFile }
    }

    if ($LASTEXITCODE -ne 0) { $failed += $projName }
    if (Test-Path $outFile) { $coverageFiles += $outFile }
}

if ($coverageFiles.Count -eq 0) {
    Write-Host "No coverage files generated." -ForegroundColor Yellow
    exit 1
}

Write-Host "`n=== Coverage Summary ===" -ForegroundColor Cyan
$belowThreshold = @()
$warningsBelow = @()

# Load per-assembly thresholds if provided
$assemblyThresholds = @{}
$assemblyWarnings = @{}
$globalFloor = $Threshold
if (-not $ThresholdsFile) {
    $defaultPath = Join-Path $PSScriptRoot 'coverage-thresholds.json'
    if (Test-Path $defaultPath) { $ThresholdsFile = $defaultPath }
}
if ($ThresholdsFile -and (Test-Path $ThresholdsFile)) {
    $cfg = Get-Content $ThresholdsFile -Raw | ConvertFrom-Json
    if ($cfg.globalThreshold -and $globalFloor -eq 0) { $globalFloor = $cfg.globalThreshold }
    if ($cfg.assemblies) {
        $cfg.assemblies.PSObject.Properties | ForEach-Object { $assemblyThresholds[$_.Name] = [int]$_.Value }
    }
    if ($cfg.warnings) {
        $cfg.warnings.PSObject.Properties | ForEach-Object { $assemblyWarnings[$_.Name] = [int]$_.Value }
    }
    Write-Host "  Loaded thresholds from $ThresholdsFile" -ForegroundColor DarkGray
}

$ownedRootFull = try { (Resolve-Path $OwnedRoot -ErrorAction Stop).Path } catch { $OwnedRoot }
$ownedRootPrefix = $ownedRootFull.TrimEnd('\', '/')
Write-Host "  Enforcing thresholds only for assemblies owned by: $ownedRootPrefix" -ForegroundColor DarkGray

function Test-IsOwnedByVertical {
    param($pkg, [string]$rootPrefix)
    if (-not $rootPrefix) { return $true }
    $classes = $pkg.classes.class
    if (-not $classes) { return $false }
    foreach ($cls in $classes) {
        $fn = $cls.filename
        if (-not $fn) { continue }
        if ($fn.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }
    return $false
}

foreach ($file in $coverageFiles) {
    [xml]$xml = Get-Content $file
    foreach ($pkg in $xml.coverage.packages.package) {
        $n = $pkg.name
        $lr = [math]::Round([double]$pkg.'line-rate' * 100, 2)
        $br = [math]::Round([double]$pkg.'branch-rate' * 100, 2)
        if ($n -match '^Altinn\.' -and $n -notmatch 'Tests|TestUtils|Mocks') {
            $owned = Test-IsOwnedByVertical -pkg $pkg -rootPrefix $ownedRootPrefix
            $marker = if ($owned) { '' } else { ' (ref)' }
            $c = if ($lr -lt 50) { 'Red' } elseif ($lr -lt 70) { 'Yellow' } else { 'Green' }
            Write-Host ("  {0,-50} {1,7}% line  {2,7}% branch{3}" -f $n, $lr, $br, $marker) -ForegroundColor $c
            if (-not $owned) { continue }
            $floor = if ($assemblyThresholds.ContainsKey($n)) { $assemblyThresholds[$n] } else { $globalFloor }
            if ($floor -gt 0 -and $lr -lt $floor) {
                $belowThreshold += "$n ($lr% < $floor%)"
            }
            if ($assemblyWarnings.ContainsKey($n)) {
                $warnFloor = $assemblyWarnings[$n]
                if ($warnFloor -gt 0 -and $lr -lt $warnFloor) {
                    $warningsBelow += "$n ($lr% < $warnFloor%)"
                }
            }
        }
    }
}

if ($warningsBelow.Count -gt 0) {
    Write-Host "`nWarning — below ratchet (non-fatal):" -ForegroundColor Yellow
    $warningsBelow | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}

if ($belowThreshold.Count -gt 0) {
    Write-Host "`nBelow threshold:" -ForegroundColor Red
    $belowThreshold | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}
elseif ($globalFloor -gt 0 -or $assemblyThresholds.Count -gt 0) {
    Write-Host "`nAll enforced assemblies meet their coverage thresholds." -ForegroundColor Green
}

$rg = Get-Command reportgenerator -ErrorAction SilentlyContinue
if ($rg) {
    $rd = Join-Path $resultsDir 'Report'
    reportgenerator "-reports:$($coverageFiles -join ';')" "-targetdir:$rd" "-reporttypes:Html;TextSummary"
    Write-Host "`nReport: $rd\index.html" -ForegroundColor Cyan
}
else {
    Write-Host "`nTip: dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
}

if ($failed.Count -gt 0) {
    Write-Host "`nTest failures: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}
Write-Host "`nDone." -ForegroundColor Green
