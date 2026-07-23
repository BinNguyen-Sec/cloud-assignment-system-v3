$ErrorActionPreference = 'Stop'

$BaseUrl = 'http://localhost:8080/api/v1'
$Session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

Write-Host "Logging in as Teacher..." -ForegroundColor Cyan
$LoginBody = @{
    email = 'teacher@arcana.local'
    password = 'Arcana@2026!'
} | ConvertTo-Json

$Login = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/auth/login" `
    -ContentType 'application/json' `
    -Body $LoginBody `
    -WebSession $Session

if (-not $Login.accessToken) {
    throw 'Login did not return an access token.'
}

$Headers = @{ Authorization = "Bearer $($Login.accessToken)" }

Write-Host "Reading current user..." -ForegroundColor Cyan
$Me = Invoke-RestMethod `
    -Method Get `
    -Uri "$BaseUrl/auth/me" `
    -Headers $Headers `
    -WebSession $Session

if ($Me.role -ne 'Teacher') {
    throw "Expected Teacher role but received '$($Me.role)'."
}

Write-Host "Checking Teacher overview policy..." -ForegroundColor Cyan
$Overview = Invoke-RestMethod `
    -Method Get `
    -Uri "$BaseUrl/teacher/overview" `
    -Headers $Headers `
    -WebSession $Session

Write-Host "Rotating refresh token..." -ForegroundColor Cyan
$Refresh = Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/auth/refresh" `
    -WebSession $Session

if (-not $Refresh.accessToken) {
    throw 'Refresh did not return a new access token.'
}

Write-Host "Logging out..." -ForegroundColor Cyan
Invoke-RestMethod `
    -Method Post `
    -Uri "$BaseUrl/auth/logout" `
    -WebSession $Session | Out-Null

Write-Host "Authentication smoke test passed." -ForegroundColor Green
Write-Host "User: $($Me.fullName) | Role: $($Me.role) | Phase: $($Overview.phase)"
