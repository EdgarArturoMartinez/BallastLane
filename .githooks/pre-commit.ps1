param()
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$daily = Join-Path $repoRoot "docs/DAILY_CONTEXT.md"
$mem = Join-Path $repoRoot "memories/repo/project-conventions.md"
$cop = Join-Path $repoRoot ".github/copilot-instructions.md"

$errors = @()
if (-not (Test-Path $daily)) { $errors += "$daily is missing" } else { if ((Get-Content $daily -Raw).Trim().Length -lt 10) { $errors += "$daily seems empty or too short" } }
if (-not (Test-Path $mem)) { $errors += "$mem is missing" }
if (-not (Test-Path $cop)) { $errors += "$cop is missing" }

if ($errors.Count -gt 0) {
    Write-Host "Pre-commit checks failed:" -ForegroundColor Red
    $errors | ForEach-Object { Write-Host " - $_" }
    Write-Host "Commit aborted. Update the files and try again." -ForegroundColor Yellow
    exit 1
}

exit 0
