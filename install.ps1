Param(
    [Parameter(Mandatory=$false)]
    [string]$BinPath = ".\src\bin\Release\net7.0\win-x64\publish\win-x64\DLMirrorSync.exe"
)

$serviceName = "Data Layer Mirror Sync Service"
$Description = "The Data Layer Mirror Sync Service watches for new chia data layer stores and subscribes to them."

sc.exe create $serviceName binpath=$BinPath start=auto
sc.exe description $serviceName $Description
