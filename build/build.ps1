Set-StrictMode -Version "Latest"
$ErrorActionPreference = "Stop"

$version = "1.0.0"

$root = $PSScriptRoot | Split-Path -Parent
$solution = Join-Path -Path $root -ChildPath "PingViz.sln"


function Get-NuGet {
    $dir = Join-Path -Path $Root -ChildPath "build/nuget"
    $exe = Join-Path -Path $dir -ChildPath "nuget.exe"

    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory | Out-Null
    }

    if (-not (Test-Path $exe)) {
        Write-Host "Downloading NuGet..."
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $exe
    }

    return $exe
}


function Get-MsBuild {
    $vswhere = [System.Environment]::ExpandEnvironmentVariables("%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe")

    $path = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath

    if ($path) {
        $exe = Join-Path -Path $path -ChildPath 'MSBuild\15.0\Bin\MSBuild.exe'

        if (Test-Path $exe) {
            return $exe
        }
    }

    throw "Could not find MSBuild."
}


function Write-Start {
    param (
        [string] $Name
    )

    Write-Host "------------------------------"
    Write-Host "$Name..."
    Write-Host ""
}


function Write-End {
    param (
        [string] $Name
    )

    Write-Host ""
    Write-Host "$Name successful."
}


function Invoke-Restore {
    Write-Start -Name "Restoring packages"

    $nuget = Get-NuGet -Root $root

    & $nuget restore $solution

    if ($LASTEXITCODE -ne 0) {
        throw "NuGet restore failed."
    }

    Write-End -Name "Package restore"
}


function Invoke-Build {
    Write-Start -Name "Building"

    $msbuild = Get-MsBuild

    & $msbuild $solution /t:Rebuild "/p:Configuration=Release;Version=$version.0" /verbosity:minimal

    if ($LASTEXITCODE -ne 0) {
        throw "Build failed."
    }

    Write-End -Name "Build"
}


function Invoke-Tests {
    Write-Start -Name "Testing"

    $xunit = Join-Path -Path $root -ChildPath "packages/xunit.runner.console.2.3.1/tools/net452/xunit.console.exe"
    $assembly = Join-Path -Path $root -ChildPath "tests/PingViz.UnitTests/bin/Release/PingViz.UnitTests.dll"

    & $xunit $assembly

    if ($LASTEXITCODE -ne 0) {
        throw "Unit tests failed."
    }

    Write-End -Name "Testing"
}


function Invoke-Package {
    Write-Start "Packaging"

    $dir = Join-Path -Path $root -ChildPath "dist"
    $zip= Join-Path -Path $dir -ChildPath "PingViz-$version.zip"
    $source= Join-Path -Path $root -ChildPath "source/PingViz/bin/Release"

    # Make sure the output directory exists.
    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory | Out-Null
    }

    # We can't overwrite an existing file,
    # so delete the existing file if it exists.
    if (Test-Path $zip) {
        Remove-Item $zip
    }

    # Delete any files that we don't want to put in the zip file,
    # because we can only add an entire directory tree.
    $files = Get-ChildItem -Path $source -Recurse -File -Exclude "*.exe", "*.dll", "*.config" | ForEach-Object {
        Remove-Item $_
    }

    Add-Type -assembly "System.IO.Compression.FileSystem"
    [System.IO.Compression.ZipFile]::CreateFromDirectory($source, $zip)

    Write-End "Packaging"
}


Invoke-Restore
Invoke-Build
Invoke-Tests
Invoke-Package
