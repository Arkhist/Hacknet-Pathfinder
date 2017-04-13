@echo off

set msBuildDir="%programfiles(x86)%\MSBuild\14.0\Bin"
set steamDir=%1
IF [%steamDir%]==[] set "steamDir=%programfiles(x86)%\Steam"
cd lib

call %msBuildDir%\msBuild.exe ../PathfinderPatcher/PathfinderPatcher.csproj /p:Configuration=Release

call PathfinderPatcher.exe -exeDir "%steamDir%\steamapps\common\Hacknet" -spit

call %msBuildDir%\msbuild.exe ../Pathfinder.csproj /p:Configuration=Release

call PathfinderPatcher.exe -pathfinderDir "..\bin\Release" -exeDir "%steamDir%\steamapps\common\Hacknet"

set msBuildDir=

cd ..