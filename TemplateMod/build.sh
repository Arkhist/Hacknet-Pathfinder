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
$MSBuild ./TemplateMod.csproj /p:Configuration=Release