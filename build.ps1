param(
    [string]$Version
)

$ErrorActionPreference = "Stop"

$startupProject = Join-Path $PSScriptRoot "Startup\Startup.csproj"
$nuspecPath = Join-Path $PSScriptRoot "spectralis.nuspec"
$publishDir = Join-Path $PSScriptRoot "publish"
$buildDir = Join-Path $PSScriptRoot "build"
$releaseDir = Join-Path $PSScriptRoot "releases"
$startupProjectXml = [xml](Get-Content $startupProject)

if (-not $Version) {
    $Version = [string]($startupProjectXml.Project.PropertyGroup.Version | Select-Object -First 1)
}

if (-not $Version) {
    throw "No version was provided and Startup.csproj does not define one."
}

$squirrelWindowsVersion = [string]($startupProjectXml.Project.PropertyGroup.SquirrelWindowsVersion | Select-Object -First 1)
if (-not $squirrelWindowsVersion) {
    throw "Startup.csproj does not define SquirrelWindowsVersion."
}

$squirrelExe = Join-Path $env:USERPROFILE ".nuget\packages\squirrel.windows\$squirrelWindowsVersion\tools\Squirrel.exe"
$packagePath = Join-Path $buildDir "Spectralis.$Version.nupkg"
$fullPackagePath = Join-Path $releaseDir "Spectralis-$Version-full.nupkg"
$publishExePath = Join-Path $publishDir "Spectralis.exe"

function Assert-LastExitCode([string]$CommandName) {
    if ($LASTEXITCODE -ne 0) {
        throw "$CommandName failed with exit code $LASTEXITCODE."
    }
}

function Remove-OptionalArtifact([string]$Path) {
    for ($attempt = 0; $attempt -lt 10; $attempt++) {
        if (-not (Test-Path $Path)) {
            return
        }

        Remove-Item $Path -Force -ErrorAction SilentlyContinue

        if (-not (Test-Path $Path)) {
            return
        }

        Start-Sleep -Milliseconds 500
    }
}

Remove-Item $publishDir, $releaseDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $buildDir | Out-Null

dotnet publish .\Startup\Startup.csproj -c Release -r win-x64 --self-contained true -o publish
Assert-LastExitCode "dotnet publish"

if (-not (Test-Path $publishExePath)) {
    throw "Expected publish output was not found: $publishExePath"
}

nuget pack $nuspecPath -Version $Version -OutputDirectory $buildDir -NoPackageAnalysis
Assert-LastExitCode "nuget pack"

if (-not (Test-Path $squirrelExe)) {
    throw "Squirrel.exe was not found at $squirrelExe"
}

$squirrelProcess = Start-Process `
    -FilePath $squirrelExe `
    -ArgumentList @("--releasify", $packagePath, "--releaseDir", $releaseDir) `
    -Wait `
    -PassThru `
    -NoNewWindow

if ($squirrelProcess.ExitCode -ne 0) {
    throw "Squirrel releasify failed with exit code $($squirrelProcess.ExitCode)."
}

foreach ($cleanupPath in @(
    (Join-Path $releaseDir "Setup.wixobj"),
    (Join-Path $releaseDir "Setup.wxs"),
    (Join-Path $releaseDir "Setup.msi"),
    (Join-Path $releaseDir "Spectralis.$Version.nupkg")
)) {
    Remove-OptionalArtifact $cleanupPath
}

foreach ($expectedPath in @(
    (Join-Path $releaseDir "Setup.exe"),
    (Join-Path $releaseDir "RELEASES"),
    $fullPackagePath
)) {
    if (-not (Test-Path $expectedPath)) {
        throw "Expected release artifact was not found: $expectedPath"
    }
}

$global:LASTEXITCODE = 0
return
