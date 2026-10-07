#Requires -Version 5.1
<#
.SYNOPSIS
    Starts the whole app locally through Docker Compose with one command.

.DESCRIPTION
    Everything runs in Docker: the host needs only Docker (no .NET SDK, Node or psql). Works in Windows
    PowerShell 5.1 and PowerShell 7. Steps, in order:
      1. Creates deploy/.env from .env.example with random passwords if it does not exist yet
         (an existing deploy/.env is never modified).
      2. Starts the database and waits until it is healthy.
      3. Applies EF Core migrations as the database owner in a one-shot `migrate` container
         (deploy/docker/migrate.Dockerfile).
      4. Optionally activates the plant calendar (-ActivateCalendar), only if it is not activated yet.
      5. Builds and starts backend + frontend and waits until both answer.

    Run from the repo root:  powershell -ExecutionPolicy Bypass -File scripts\start-app.ps1
    Stop the stack:          docker compose -f deploy/compose.yaml down

.PARAMETER ActivateCalendar
    Activates the plant calendar on this local database with today's date in the plant timezone, as the CI E2E
    fixture does. Activation is permanent for this database volume; skipped when it is already activated.

.PARAMETER SkipMigrations
    Skips step 3, for when the database schema is already up to date.

.PARAMETER Open
    Opens the app in the default browser once it is ready.
#>
[CmdletBinding()]
param(
    [switch]$ActivateCalendar,
    [switch]$SkipMigrations,
    [switch]$Open
)
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$deployDir = Join-Path $repoRoot 'deploy'
$composeFile = Join-Path $deployDir 'compose.yaml'
$envFile = Join-Path $deployDir '.env'
$envExample = Join-Path $deployDir '.env.example'

function Write-Step([string]$message) { Write-Host "==> $message" -ForegroundColor Cyan }

function Invoke-Native {
    param([Parameter(Mandatory)][string]$FailureMessage, [Parameter(Mandatory)][scriptblock]$Command)
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "$FailureMessage (exit code $LASTEXITCODE)" }
}

# True when the native command exits 0; all output discarded. Windows PowerShell 5.1 turns redirected native
# stderr into a terminating error under ErrorActionPreference=Stop, so relax it for the call.
function Test-Native([scriptblock]$Command) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { & $Command *> $null; $LASTEXITCODE -eq 0 } finally { $ErrorActionPreference = $previous }
}

function New-Secret([int]$bytes = 16) {
    $buffer = New-Object byte[] $bytes
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($buffer)
    ($buffer | ForEach-Object { $_.ToString('x2') }) -join ''
}

function Read-EnvFile([string]$path) {
    $values = @{}
    foreach ($line in [IO.File]::ReadAllLines($path)) {
        if ($line -match '^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=(.*)$') { $values[$Matches[1]] = $Matches[2].Trim() }
    }
    $values
}

function Get-EnvValue([hashtable]$values, [string]$name, [string]$default) {
    if ([string]::IsNullOrWhiteSpace($values[$name])) { $default } else { $values[$name] }
}

# --- Prerequisites -----------------------------------------------------------------------------------------------
Write-Step 'Checking Docker'
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'Docker is not installed or not on PATH.' }
if (-not (Test-Native { docker info })) { throw 'Docker is not running. Start Docker Desktop and try again.' }

# --- 1. deploy/.env ----------------------------------------------------------------------------------------------
$generatedAdminPassword = $null
if (-not (Test-Path -LiteralPath $envFile)) {
    Write-Step 'Creating deploy/.env with random local passwords'
    $generatedAdminPassword = "Admin-9$(New-Secret 8)!"   # upper, lower, digit and symbol: Identity's default rules
    $content = [IO.File]::ReadAllText($envExample)
    $content = $content -replace '(?m)^POSTGRES_PASSWORD=[^\r\n]*', "POSTGRES_PASSWORD=$(New-Secret)"
    $content = $content -replace '(?m)^PMAI_APP_DB_PASSWORD=[^\r\n]*', "PMAI_APP_DB_PASSWORD=$(New-Secret)"
    $content = $content -replace '(?m)^SEED_ADMIN_PASSWORD=[^\r\n]*', "SEED_ADMIN_PASSWORD=$generatedAdminPassword"
    [IO.File]::WriteAllText($envFile, $content, (New-Object Text.UTF8Encoding $false))

    if (Test-Native { docker volume inspect deploy_db-data }) {
        Write-Warning ('A database volume from an earlier setup already exists and keeps its old passwords. ' +
            'If the backend cannot log in, wipe it with: docker compose -f deploy/compose.yaml down -v')
    }
}

$envValues = Read-EnvFile $envFile
foreach ($required in 'POSTGRES_PASSWORD', 'PMAI_APP_DB_PASSWORD', 'SEED_ADMIN_PASSWORD') {
    if ([string]::IsNullOrWhiteSpace($envValues[$required])) { throw "$required is empty in deploy/.env; fill it in first." }
}
$backendPort = Get-EnvValue $envValues 'BACKEND_PORT' '8081'
$frontendPort = Get-EnvValue $envValues 'FRONTEND_PORT' '3000'
$plantTimeZone = Get-EnvValue $envValues 'PLANT_TIMEZONE' 'Asia/Tokyo'

# --- 2. Database -------------------------------------------------------------------------------------------------
Write-Step 'Starting the database'
Invoke-Native 'The database did not start' { docker compose -f $composeFile up -d --build --wait db }

# --- 3. Migrations (owner login in a one-shot container; the app itself runs as pmai_app, DEC-016) ----------------
if ($SkipMigrations) {
    Write-Step 'Skipping migrations (-SkipMigrations)'
} else {
    Write-Step 'Applying migrations as the database owner (in Docker)'
    Invoke-Native 'Migrations failed' { docker compose -f $composeFile run --rm --build migrate }
}

# --- 4. Optional plant calendar activation -----------------------------------------------------------------------
# psql runs inside the db container with the owner credentials from the container's own environment; $1 is the
# plant timezone, passed positionally. PostgreSQL also computes "today" in that timezone.
$psqlAsOwner = 'export PGPASSWORD=$POSTGRES_PASSWORD; exec psql -X -q -t -A --no-password -U $POSTGRES_USER -d $POSTGRES_DB -v ON_ERROR_STOP=1 -v time_zone=$1'
if ($ActivateCalendar) {
    $probe = "SELECT (SELECT count(*) FROM public.plant_calendar_state) || ' ' || to_char(now() AT TIME ZONE :'time_zone', 'YYYY-MM-DD');" |
        docker compose -f $composeFile exec -T db sh -eu -c "$psqlAsOwner -f -" sh $plantTimeZone
    if ($LASTEXITCODE -ne 0) { throw 'Could not read the plant calendar state (are migrations applied?).' }
    $activatedCount, $activationDate = "$probe".Trim() -split ' '
    if ($activatedCount -ne '0') {
        Write-Step 'Plant calendar is already activated; leaving it unchanged'
    } else {
        Write-Step "Activating the plant calendar from $activationDate ($plantTimeZone)"
        [IO.File]::ReadAllText((Join-Path $repoRoot 'scripts/calendar/activate.sql')) |
            docker compose -f $composeFile exec -T db sh -eu -c "$psqlAsOwner -v activation_date=`$2 -f -" sh $plantTimeZone $activationDate
        if ($LASTEXITCODE -ne 0) { throw 'Calendar activation was rejected; inspect the current state before retrying.' }
    }
}

# --- 5. Backend + frontend ---------------------------------------------------------------------------------------
Write-Step 'Building and starting backend and frontend'
Invoke-Native 'The stack did not start' { docker compose -f $composeFile up -d --build }

Write-Step 'Waiting for the app to answer'
$backendUrl = "http://localhost:$backendPort/health"
$frontendUrl = "http://localhost:$frontendPort/"
$deadline = (Get-Date).AddMinutes(3)
$ready = $false
while ((Get-Date) -lt $deadline) {
    try {
        $null = Invoke-WebRequest -Uri $backendUrl -UseBasicParsing -TimeoutSec 5
        $null = Invoke-WebRequest -Uri $frontendUrl -UseBasicParsing -TimeoutSec 5
        $ready = $true
        break
    } catch { Start-Sleep -Seconds 2 }
}
if (-not $ready) {
    docker compose -f $composeFile logs --no-color --tail=80 backend
    throw 'The app did not become ready within 3 minutes; backend logs are above.'
}

Write-Host ''
Write-Host "App is running: $frontendUrl" -ForegroundColor Green
if ($generatedAdminPassword) {
    Write-Host "Sign in as 'admin' with password: $generatedAdminPassword (stored in deploy/.env)"
} else {
    Write-Host "Sign in as 'admin' with SEED_ADMIN_PASSWORD from deploy/.env"
}
Write-Host 'Stop it with: docker compose -f deploy/compose.yaml down'
if ($Open) { Start-Process $frontendUrl }
