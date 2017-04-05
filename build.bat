REM set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v3.5
set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319

cd lib

call %msBuildDir%\msBuild.exe ../PathfinderPatcher/PathfinderPatcher.csproj /p:Configuration=Release

call PathfinderPatcher.exe -exeDir "C:\Program Files (x86)\Steam\steamapps\common\Hacknet" -spit

cd ../bin/Release

call %msBuildDir%\msbuild.exe ../../Pathfinder.csproj /p:Configuration=Release

cd ../../lib

call PathfinderPatcher.exe -pathfinderDir "..\bin\Release" -exeDir "C:\Program Files (x86)\Steam\steamapps\common\Hacknet"

set msBuildDir=