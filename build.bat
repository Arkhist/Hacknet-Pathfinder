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

zipjs.bat zipItem -source ./PathfinderPatcher.exe -source ./Pathfinder.dll -source ./Mono.Cecil.dll -source ./Mono.Cecil.Inject.dll -source ./Cecil_LICENSE.txt -source ./Cecil_Inject_LICENSE.txt -source ../README.md -destination ../releases/Pathfinder.Release.V_.zip -keep yes -force yes

cd ..