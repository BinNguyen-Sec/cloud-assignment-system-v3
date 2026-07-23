$ErrorActionPreference = 'Stop'

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter()][string[]]$ArgumentList = @(),
        [Parameter(Mandatory = $true)][string]$StepName
    )

    Write-Host $StepName -ForegroundColor Cyan
    & $FilePath @ArgumentList
    if ($LASTEXITCODE -ne 0) {
        throw "$StepName failed with exit code $LASTEXITCODE."
    }
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot
$Solution = '.\backend\CloudAssignment.sln'
$InfrastructureProject = '.\backend\src\CloudAssignment.Infrastructure'
$ApiProject = '.\backend\src\CloudAssignment.Api'
$MigrationsPath = '.\backend\src\CloudAssignment.Infrastructure\Persistence\Migrations'

Invoke-NativeCommand -FilePath 'docker' -ArgumentList @('compose', 'up', '-d', 'database') -StepName 'Starting PostgreSQL database...'

$databaseHealthy = $false
for ($attempt = 1; $attempt -le 30; $attempt++) {
    $healthStatus = & docker inspect --format '{{.State.Health.Status}}' cloud-assignment-v3-db 2>$null
    if ($LASTEXITCODE -eq 0 -and $healthStatus -eq 'healthy') {
        $databaseHealthy = $true
        break
    }
    Start-Sleep -Seconds 2
}
if (-not $databaseHealthy) { throw 'PostgreSQL did not become healthy.' }

Invoke-NativeCommand -FilePath 'dotnet' -ArgumentList @('tool', 'restore') -StepName 'Restoring local .NET tools...'
Invoke-NativeCommand -FilePath 'dotnet' -ArgumentList @('restore', $Solution, '--force-evaluate') -StepName 'Restoring backend dependencies...'
Invoke-NativeCommand -FilePath 'dotnet' -ArgumentList @('build', $Solution, '--configuration', 'Release') -StepName 'Building Phase 3 backend...'

$migrationExists = (Test-Path $MigrationsPath) -and $null -ne (
    Get-ChildItem -Path $MigrationsPath -Filter '*_CourseManagement.cs' -ErrorAction SilentlyContinue |
    Select-Object -First 1
)

if (-not $migrationExists) {
    Invoke-NativeCommand -FilePath 'dotnet' -ArgumentList @(
        'ef', 'migrations', 'add', 'CourseManagement',
        '--project', $InfrastructureProject,
        '--startup-project', $ApiProject,
        '--context', 'ApplicationDbContext',
        '--output-dir', 'Persistence\Migrations'
    ) -StepName 'Generating CourseManagement migration...'
} else {
    Write-Host 'CourseManagement migration already exists.' -ForegroundColor Yellow
}

Invoke-NativeCommand -FilePath 'dotnet' -ArgumentList @(
    'ef', 'database', 'update',
    '--project', $InfrastructureProject,
    '--startup-project', $ApiProject,
    '--context', 'ApplicationDbContext'
) -StepName 'Applying Course Management schema...'

Invoke-NativeCommand -FilePath 'dotnet' -ArgumentList @(
    'ef', 'migrations', 'list',
    '--project', $InfrastructureProject,
    '--startup-project', $ApiProject,
    '--context', 'ApplicationDbContext'
) -StepName 'Verifying applied migrations...'

Write-Host ''
Write-Host 'Phase 3 database is genuinely ready.' -ForegroundColor Green
