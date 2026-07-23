$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

Write-Host "Building backend..." -ForegroundColor Cyan
dotnet build .\backend\CloudAssignment.sln --configuration Release

Write-Host "Testing backend..." -ForegroundColor Cyan
dotnet test .\backend\CloudAssignment.sln --configuration Release --no-build

Write-Host "Checking frontend..." -ForegroundColor Cyan
Push-Location .\frontend\cloud-assignment-web
try {
    npm run typecheck
    npm run test
    npm run build
}
finally {
    Pop-Location
}

Write-Host "All foundation quality gates passed." -ForegroundColor Green
