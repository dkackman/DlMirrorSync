$version = "0.1.0"
$name = "DLMirrorSync"
$src = "DLMirrorSync"
$outputRoot = ".\publish"

Remove-Item $outputRoot -Recurse -Force

# win-x64
dotnet build ./$src/$name.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# single standalone file
dotnet publish ./$src/$name.csproj -c Release -r win-x64 --self-contained true /p:PublishReadyToRun=true /property:PublishSingleFile=True /p:PublishTrimmed=false /p:PublishDir="bin\Release\net7.0\win-x64\publish\win-x64\" --output $outputRoot/standalone/win-x64  /property:DebugType=None /property:DebugSymbols=False
Compress-Archive -CompressionLevel Optimal -Path $outputRoot/standalone/win-x64/* -DestinationPath $outputRoot/$name-$version-standalone-win-x64.zip

#single file that needs dotnet isntalled
dotnet publish ./$src/$name.csproj -c Release -r win-x64 --self-contained false /p:PublishReadyToRun=true /property:PublishSingleFile=True /p:PublishTrimmed=false /p:PublishDir="bin\Release\net7.0\win-x64\" --output $outputRoot/singlefile/win-x64  /property:DebugType=None /property:DebugSymbols=False
Compress-Archive -CompressionLevel Optimal -Path $outputRoot/singlefile/win-x64/* -DestinationPath $outputRoot/$name-$version-singlefile-win-x64.zip


function Publish-Project {
    param(
        [string]$src,
        [string]$name,
        [string]$version,
        [string]$outputRoot,
        [bool]$selfContained,
        [string]$publishType
    )

    dotnet publish ./$src/$name.csproj -c Release -r win-x64 --self-contained $selfContained /p:PublishReadyToRun=true /property:PublishSingleFile=True /p:PublishTrimmed=false /p:PublishDir="bin\Release\net7.0\win-x64\" --output $outputRoot/$publishType/win-x64  /property:DebugType=None /property:DebugSymbols=False
    Compress-Archive -CompressionLevel Optimal -Path $outputRoot/$publishType/win-x64/* -DestinationPath $outputRoot/$name-$version-$publishType-win-x64.zip
}