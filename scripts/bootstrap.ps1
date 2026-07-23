$ErrorActionPreference = 'Stop'

function Assert-Command {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

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

Write-Host "Checking required tools..." -ForegroundColor Cyan

Assert-Command git
Assert-Command dotnet
Assert-Command node
Assert-Command npm
Assert-Command docker

Write-Host "Git:    $(git --version)"
Write-Host ".NET:   $(dotnet --version)"
Write-Host "Node:   $(node --version)"
Write-Host "npm:    $(npm --version)"
Write-Host "Docker: $(docker --version)"

if (-not (Test-Path .env)) {
    Copy-Item .env.example .env
    Write-Host `
        "Created local .env from .env.example." `
        -ForegroundColor Yellow
}

& .\scripts\setup-phase3-database.ps1

Push-Location .\frontend\cloud-assignment-web

try {
    Invoke-NativeCommand `
        -FilePath $NpmCommand `
        -ArgumentList @('install') `
        -StepName 'Installing frontend dependencies...'
}
finally {
    Pop-Location
}

Write-Host "Running Phase 3 verification..." `
    -ForegroundColor Cyan

& .\scripts\verify.ps1

Write-Host ""
Write-Host "Course Management bootstrap completed." `
    -ForegroundColor Green

Write-Host `
    "API:      dotnet run --project .\backend\src\CloudAssignment.Api"

Write-Host `
    "Frontend: cd .\frontend\cloud-assignment-web; npm run dev"
