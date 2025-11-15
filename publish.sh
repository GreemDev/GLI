rm -rf buildOutput
mkdir buildOutput
dotnet publish -c Release -r linux-x64
mv src/Cli/bin/Release/net10.0/linux-x64/publish/GitLabCli buildOutput/GitLabCli-linux_x64
dotnet publish -c Release -r linux-arm64
mv src/Cli/bin/Release/net10.0/linux-arm64/publish/GitLabCli buildOutput/GitLabCli-linux_arm64
dotnet publish -c Release -r win-x64
mv src/Cli/bin/Release/net10.0/win-x64/publish/GitLabCli.exe buildOutput/GitLabCli-win_x64.exe
dotnet publish -c Release -r win-arm64
mv src/Cli/bin/Release/net10.0/win-arm64/publish/GitLabCli.exe buildOutput/GitLabCli-win_arm64.exe
dotnet publish -c Release -r osx-arm64
mv src/Cli/bin/Release/net10.0/osx-arm64/publish/GitLabCli buildOutput/GitLabCli-mac_arm64
dotnet publish -c Release -r osx-x64
mv src/Cli/bin/Release/net10.0/osx-x64/publish/GitLabCli buildOutput/GitLabCli-mac_x64

rm -rf src/Cli/bin/Release