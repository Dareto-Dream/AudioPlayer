$version = "1.0.0"

# Clean
Remove-Item -Recurse -Force publish, releases -ErrorAction Ignore

# Publish app
dotnet publish -c Release -r win-x64 --self-contained true -o publish

# Pack NuGet
nuget pack Spectralis.nuspec -Version $version -OutputDirectory build

# Releasify
Squirrel.exe --releasify "build\Spectralis.$version.nupkg" --releaseDir releases