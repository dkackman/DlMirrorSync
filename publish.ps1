$version = "0.1.0"
$name = "DLMirrorSync"
$src = "DLMirrorSync"
$outputRoot = "./publish"
$framework = "net7.0"

Remove-Item $outputRoot -Recurse -Force
Remove-Item ./$src/bin/Release -Recurse -Force

function Publish-Project {
    param(
        [string]$runtime
    )
 
    # single standalone file
    dotnet publish ./$src/$name.csproj -c Release -r $runtime --framework $framework --self-contained true /p:PublishReadyToRun=true /property:PublishSingleFile=True /p:PublishTrimmed=false /property:IncludeNativeLibrariesForSelfExtract=True /p:PublishDir="bin\Release\$framework\$runtime\" --output $outputRoot/standalone/$runtime
    Compress-Archive -CompressionLevel Optimal -Path $outputRoot/standalone/$runtime/* -DestinationPath $outputRoot/$name-$version-standalone-$runtime.zip

    # single file that needs dotnet installed
    dotnet publish ./$src/$name.csproj -c Release -r $runtime --framework $framework --self-contained false /p:PublishReadyToRun=false /property:PublishSingleFile=True /p:PublishTrimmed=false /property:IncludeNativeLibrariesForSelfExtract=True /p:PublishDir="bin\Release\$framework\$runtime\" --output $outputRoot/singlefile/$runtime
    Compress-Archive -CompressionLevel Optimal -Path $outputRoot/singlefile/$runtime/* -DestinationPath $outputRoot/$name-$version-singlefile-$runtime.zip
}

Publish-Project("win-x64")
#Publish-Project("linux-x64")
#Publish-Project("osx.11.0-x64")
