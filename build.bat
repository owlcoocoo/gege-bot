@echo off

dotnet publish src\GegeBot -p:PublishProfile=src\GegeBot\Properties\PublishProfiles\win-x64.pubxml
dotnet publish src\GegeBot -p:PublishProfile=src\GegeBot\Properties\PublishProfiles\osx-x64.pubxml
dotnet publish src\GegeBot -p:PublishProfile=src\GegeBot\Properties\PublishProfiles\linux-x64.pubxml

pause