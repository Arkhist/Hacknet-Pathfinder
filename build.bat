REM set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v3.5
set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
set steamDir=%1
cd lib

call %msBuildDir%\msBuild.exe ../PathfinderPatcher/PathfinderPatcher.csproj /p:Configuration=Release

call PathfinderPatcher.exe -exeDir "%steamDir%\steamapps\common\Hacknet" -spit

call %msBuildDir%\msbuild.exe ../Pathfinder.csproj /p:Configuration=Release

call PathfinderPatcher.exe -pathfinderDir "..\bin\Release" -exeDir "%steamDir%\steamapps\common\Hacknet"

set msBuildDir=