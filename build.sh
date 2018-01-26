#!/bin/env bash

case $OSTYPE in
    msys | cygwin | win32)
        MSBuild="C:\Program Files(x86)\MSBuild\14.0\Bin\msbuild.exe"
        SteamDir="C:\Program Files(x86)\Steam"
        ;;
    *)
        MSBuild="msbuild"
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

mkbundle -o ./PathfinderPatcher.arch64 ./PathfinderPatcher.exe --cross default --static
mkbundle -o ./PathfinderPatcher.ubuntu64 ./PathfinderPatcher.exe --cross mono-5.8.0-ubuntu-16.04-x64 --static
mkbundle -o ./PathfinderPatcher.ubuntu86 ./PathfinderPatcher.exe --cross mono-5.8.0-ubuntu-16.04-x86 --static
#mkbundle -o ./PathfinderPatcher.ubuntu.arm64 ./PathfinderPatcher.exe --cross mono-5.8.0-ubuntu-16.04-arm64 --static
mkbundle -o ./PathfinderPatcher.osx ./PathfinderPatcher.exe --cross mono-5.8.0-osx-10.7-x64.zip --static
#mkbundle -o ./PathfinderPatcher.deb.64 ./PathfinderPatcher.exe --cross mono-5.8.0-debian-9-x64 --static

zip -r - PathfinderPatcher.exe PathfinderPatcher.arch64 PathfinderPatcher.osx PathfinderPatcher.ubuntu86 PathfinderPatcher.ubuntu64 Pathfinder.dll Mono.Cecil.dll Mono.Cecil.Inject.dll Cecil_LICENSE.txt Cecil_Inject_LICENSE.txt ../README.md > ../releases/Pathfinder.Release.V_.zip