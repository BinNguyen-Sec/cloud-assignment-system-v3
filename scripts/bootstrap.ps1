$ErrorActionPreference = 'Stop'

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

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
    Write-Host "Created local .env from .env.example." -ForegroundColor Yellow
}

Write-Host "Starting PostgreSQL 16..." -ForegroundColor Cyan
docker compose up -d database

Write-Host "Restoring backend dependencies..." -ForegroundColor Cyan
dotnet restore .\backend\CloudAssignment.sln

Write-Host "Installing frontend dependencies..." -ForegroundColor Cyan
Push-Location .\frontend\cloud-assignment-web
try {
    npm install
}
finally {
    Pop-Location
}

Write-Host "Running foundation verification..." -ForegroundColor Cyan
& .\scripts\verify.ps1

Write-Host ""
Write-Host "Foundation bootstrap completed." -ForegroundColor Green
Write-Host "API:      dotnet run --project .\backend\src\CloudAssignment.Api"
Write-Host "Frontend: cd .\frontend\cloud-assignment-web; npm run dev"
