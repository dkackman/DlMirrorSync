# run publish.ps1 first
# run this script as admin

Param(
    [Parameter(Mandatory=$false)]
    [string]$BinPath = ".\publish\standalone\win-x64\DlMirrorSync.exe"
)

# Check if BinPath is absolute
if (-not [System.IO.Path]::IsPathRooted($BinPath)) {
    # If BinPath is not absolute, make it absolute
    $BinPath = Join-Path -Path $PSScriptRoot -ChildPath $BinPath
}
$BinPath = Resolve-Path $BinPath

$serviceName = "Data Layer Mirror Sync Service"
$Description = "The Data Layer Mirror Sync Service watches for new chia data layer stores and subscribes to them."

sc.exe create $serviceName start=auto binpath="$BinPath $env:USERPROFILE\.chia\mainnet\config\config.yaml"
sc.exe description $serviceName $Description
