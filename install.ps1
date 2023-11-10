Param(
    [Parameter(Mandatory=$false)]
    [string]$BinPath = ".\src\bin\Release\net7.0\win-x64\publish\win-x64\DLSync.exe"
)

$serviceName = "Data Layer Subscription Sync Service"
$Description = "The Data Layer Subscription Sync Service watches for new stores and subscribes to them."

sc.exe create $serviceName binpath=$BinPath start=auto
sc.exe description $serviceName $Description