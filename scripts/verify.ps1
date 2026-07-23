$ErrorActionPreference = 'Stop'

function Invoke-NativeCommand {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter()]
        [string[]]$ArgumentList = @(),

        [Parameter(Mandatory = $true)]
        [string]$StepName
    )

    Write-Host $StepName -ForegroundColor Cyan

    & $FilePath @ArgumentList

    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        throw "$StepName failed with exit code $exitCode."
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

$NpmCommand = if ($env:OS -eq 'Windows_NT') { 'npm.cmd' } else { 'npm' }

Invoke-NativeCommand `
    -FilePath 'dotnet' `
    -ArgumentList @(
        'list',
        '.\backend\CloudAssignment.sln',
        'package',
        '--vulnerable',
        '--include-transitive'
    ) `
    -StepName 'Auditing backend packages...'

Invoke-NativeCommand `
    -FilePath 'dotnet' `
    -ArgumentList @(
        'build',
        '.\backend\CloudAssignment.sln',
        '--configuration',
        'Release'
    ) `
    -StepName 'Building backend...'

Invoke-NativeCommand `
    -FilePath 'dotnet' `
    -ArgumentList @(
        'test',
        '.\backend\CloudAssignment.sln',
        '--configuration',
        'Release',
        '--no-build'
    ) `
    -StepName 'Testing backend...'

Push-Location .\frontend\cloud-assignment-web

try {
    Invoke-NativeCommand `
        -FilePath $NpmCommand `
        -ArgumentList @('run', 'typecheck') `
        -StepName 'Type-checking frontend...'

    Invoke-NativeCommand `
        -FilePath $NpmCommand `
        -ArgumentList @('run', 'test') `
        -StepName 'Testing frontend...'

    Invoke-NativeCommand `
        -FilePath $NpmCommand `
        -ArgumentList @('run', 'build') `
        -StepName 'Building frontend...'
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "All Phase 3 quality gates passed." `
    -ForegroundColor Green
