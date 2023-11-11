$version = "0.1.0"
$name = "DLMirrorSync"
$src = "DLMirrorSync"
$outputRoot = "./publish"
$framework = "net7.0"

Remove-Item $outputRoot -Recurse -Force

function Publish-Project {
    param(
        [string]$runtime
    )
 
    dotnet build ./$src/$name.csproj -c Release -r $runtime --framework $framework --self-contained true /p:PublishSingleFile=true

    # single standalone file
    dotnet publish ./$src/$name.csproj -c Release -r $runtime --framework $framework --self-contained true /p:PublishReadyToRun=true /property:PublishSingleFile=True /p:PublishTrimmed=false /property:IncludeNativeLibrariesForSelfExtract=True /p:PublishDir="bin\Release\$framework\$runtime\" --output $outputRoot/standalone/$runtime  /property:DebugType=None /property:DebugSymbols=False
    Compress-Archive -CompressionLevel Optimal -Path $outputRoot/standalone/$runtime/* -DestinationPath $outputRoot/$name-$version-standalone-$runtime.zip

    #single file that needs dotnet installed
    dotnet publish ./$src/$name.csproj -c Release -r $runtime --framework $framework --self-contained false /p:PublishReadyToRun=true /property:PublishSingleFile=True /p:PublishTrimmed=false /p:PublishDir="bin\Release\$framework\$runtime\" --output $outputRoot/singlefile/$runtime  /property:DebugType=None /property:DebugSymbols=False
    Compress-Archive -CompressionLevel Optimal -Path $outputRoot/singlefile/$runtime/* -DestinationPath $outputRoot/$name-$version-singlefile-$runtime.zip
}

Publish-Project("win-x64")
Publish-Project("linux-x64")
Publish-Project("osx.11.0-x64")
