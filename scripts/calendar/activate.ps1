#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ActivationDate,
    [Parameter(Mandatory)][string]$TimeZone,
    [string]$PsqlPath = 'psql'
)
$ErrorActionPreference = 'Stop'
$calendarDate = [datetime]::MinValue
if ($ActivationDate -cnotmatch '^[0-9]{4}-[0-9]{2}-[0-9]{2}$' -or
    -not [datetime]::TryParseExact($ActivationDate, 'yyyy-MM-dd', [cultureinfo]::InvariantCulture,
        [System.Globalization.DateTimeStyles]::None, [ref]$calendarDate)) {
    throw 'ActivationDate must be an explicit Gregorian yyyy-MM-dd date in years 0001 through 9999.'
}
if ([string]::IsNullOrWhiteSpace($TimeZone) -or $TimeZone -cne $TimeZone.Trim() -or $TimeZone.Length -gt 100) {
    throw 'TimeZone must be the exact configured nonblank plant timezone.'
}
try { $null = [TimeZoneInfo]::FindSystemTimeZoneById($TimeZone) }
catch { throw 'TimeZone is not available in this runtime.' }
foreach ($calendarVariable in @('PGHOST', 'PGPORT', 'PGDATABASE', 'PGUSER')) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($calendarVariable))) {
        throw "$calendarVariable must explicitly identify the approved owner database target."
    }
}
if ($env:PGUSER -eq 'pmai_app') { throw 'Calendar activation requires the database owner.' }
$calendarScript = Join-Path $PSScriptRoot 'activate.sql'
& $PsqlPath -X --no-password --set=ON_ERROR_STOP=1 "--set=activation_date=$ActivationDate" "--set=time_zone=$TimeZone" --file=$calendarScript
if ($LASTEXITCODE -ne 0) { throw 'Calendar activation was rejected; inspect current state. Never reset or replay an uncertain cutover.' }
