param([string]$ApiBaseUrl = 'http://localhost:8080/api/v1')

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$fixture = Join-Path $RepoRoot 'scripts\fixtures\student-import-smoke.xlsx'

$session = Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/auth/login" -ContentType 'application/json' -Body (@{
    email = 'teacher@arcana.local'
    password = 'Arcana@2026!'
} | ConvertTo-Json)
$headers = @{ Authorization = "Bearer $($session.accessToken)" }
$code = "SMOKE-$([Guid]::NewGuid().ToString('N').Substring(0, 10).ToUpperInvariant())"
$course = Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/courses" -Headers $headers -ContentType 'application/json' -Body (@{
    code = $code
    name = 'Phase 3 Smoke Course'
    description = 'Automated Course Management smoke test.'
    semester = 'Semester 1'
    academicYear = '2026-2027'
    themeKey = 'astral'
} | ConvertTo-Json)

$list = Invoke-RestMethod -Method Get -Uri "$ApiBaseUrl/courses?q=$code&page=1&pageSize=20&archived=false" -Headers $headers
if (-not ($list.items | Where-Object { $_.id -eq $course.id })) { throw 'Created course was not returned by search.' }

$enrolled = Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/courses/$($course.id)/students" -Headers $headers -ContentType 'application/json' -Body (@{ email = 'student2@arcana.local' } | ConvertTo-Json)
if ($enrolled.email -ne 'student2@arcana.local') { throw 'Manual enrollment failed.' }

$previewJson = & curl.exe --silent --show-error --fail-with-body -X POST `
    -H "Authorization: Bearer $($session.accessToken)" `
    -F "file=@$fixture;type=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" `
    "$ApiBaseUrl/courses/$($course.id)/students/import-preview"
if ($LASTEXITCODE -ne 0) { throw 'Excel preview request failed.' }
$preview = $previewJson | ConvertFrom-Json
if ($preview.validRows -ne 1 -or $preview.invalidRows -ne 1) { throw 'Excel preview counts are incorrect.' }

$result = Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/courses/$($course.id)/students/imports/$($preview.batchId)/confirm" -Headers $headers
if ($result.importedRows -ne 1) { throw 'Excel confirmation did not import the expected student.' }

Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/courses/$($course.id)/archive" -Headers $headers | Out-Null
Write-Host 'Phase 3 smoke test passed.' -ForegroundColor Green
Write-Host "Course: $code | Manual: student2 | Excel: student3 | Invalid rows: 1"
