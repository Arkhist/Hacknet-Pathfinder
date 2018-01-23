#!/bin/env bash

case $OSTYPE in
    msys | cygwin | win32)
        MSBuild="C:\Program Files(x86)\MSBuild\14.0\Bin\msbuild.exe"
        SteamDir="C:\Program Files(x86)\Steam"
        ;;
    *)
        MSBuild="xbuild"
        SteamDir="$HOME/.local/share/Steam"
esac

if(( $# == 1 )); then
    SteamDir="$1"
elif(( $# == 2 )); then
    MSBuildDir="$2"
fi

cd ./lib
$MSBuild ../PathfinderPatcher/PathfinderPatcher.csproj /p:Configuration=Release

case $OSTYPE in
    msys | cygwin | win32)
        PathfinderPatcher.exe -exeDir "$SteamDir\steamapps\common\Hacknet" -spit
    ;;
    *)
        mono PathfinderPatcher.exe -exeDir "$SteamDir/steamapps/common/Hacknet" -spit
esac

$MSBuild ../Pathfinder.csproj /p:Configuration=Release

mkbundle -o ./PathfinderPatcher ./PathfinderPatcher.exe --cross default

zip -r - PathfinderPatcher.exe PathfinderPatcher Pathfinder.dll Mono.Cecil.dll Mono.Cecil.Inject.dll Cecil_LICENSE.txt Cecil_Inject_LICENSE.txt ../README.md > ../releases/Pathfinder.Release.V_.zip