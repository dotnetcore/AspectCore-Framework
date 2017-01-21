@echo off

if not exist packages (
    md packages
)

for /R "packages" %%s in (*) do (
    del %%s
)

dotnet test test/AspectCore.Abstractions.Test --configuration Release
dotnet test test/AspectCore.Abstractions.Resolution.Test --configuration Release

dotnet pack src/AspectCore.Abstractions --configuration Release --output packages
dotnet pack src/AspectCore.Abstractions.Resolution --configuration Release --output packages

set /p key=input key:
set source=https://www.myget.org/F/aspectcore/api/v2/package

for /R "packages" %%s in (*symbols.nupkg) do ( 
    call nuget push %%s -s %source%  %key%
)

pause
