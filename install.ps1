# run publish.ps1 first
# run this script as admin

Param(
    [Parameter(Mandatory=$false)]
    [string]$BinPath = ".\standalone\win-x64\DlMirrorSync.exe"
)

$serviceName = "Data Layer Mirror Sync Service"
$Description = "The Data Layer Mirror Sync Service watches for new chia data layer stores and subscribes to them."

sc.exe create $serviceName binpath=$BinPath start=auto
sc.exe description $serviceName $Description
