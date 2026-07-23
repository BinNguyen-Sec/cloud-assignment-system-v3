$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

Write-Warning "This removes the local PostgreSQL volume and all local development data."
$confirmation = Read-Host "Type RESET to continue"

if ($confirmation -ne 'RESET') {
    Write-Host "Cancelled."
    exit 0
}

docker compose down -v
docker compose up -d database
Write-Host "Local PostgreSQL was reset." -ForegroundColor Green
