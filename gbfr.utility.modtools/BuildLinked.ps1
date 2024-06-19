# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/gbfr.utility.modtools/*" -Force -Recurse
dotnet publish "./gbfr.utility.modtools.csproj" -c Release -o "$env:RELOADEDIIMODS/gbfr.utility.modtools" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location