# Pathfinder
An extensive modding API and loader for Hacknet that enables practically limitless programable extensions to the game.
Patches bugs from the original game.

### Installation

1. Extract all of the binary files into the directory containing Hacknet.exe (right click Hacknet, click Properties, Local Files, then Browse Local Files, or hover over Manage and click Browse local files)
2. Run PathfinderPatcher.exe
3.
    1. Start HacknetPathfinder.exe (Windows)
    2. Start HacknetPathfinder.bin.* (Unix)
    3. Create a Mods directory
4. (If you are installing mods) Drop any mod DLLs you plan to play with into the Mods directory
5.
    1. Start HacknetPathfinder.exe (Windows)
    2. Start HacknetPathfinder.bin.* (Unix)

### Creating Mods

1. Start a new .NET library project
2. Link it against the Pathfinder.dll (recommended you also at least link it against FNA.dll and HacknetPathfinder.exe)
    * You may need to go into the project options and set the project to target the x86 platform
3. Create a class that implements the Pathfinder.ModManager.IMod interface
4. And now you have a basic functioning mod, the rest is up to you

### Contributing to Pathfinder

1. Clone the project with `git clone https://github.com/Arkhist/Hacknet-Pathfinder`
2. Run `build.ps1` (Windows) or `build.sh` (Unix)
3. Open the solution file in a .NET 4.0 compatible IDE

## Bugs patched
DoesNotLoadFlags extension conditional not being parsed.

## FAQ

Q. How do I fix `Impossible to load mod <hacknet-steam-dir>\Mods\<mod>.dll : An attempt...`

A. Right click the mod dll and go into properties. Then click on the "unblock" checkbox. It should work perfectly now.

Q. How do I fix `Permission Denied` on Linux

A. Open the properties of the specific file, go to permissions, and `Allow the file to run as a program`

## Links

[Discord](https://discord.gg/65SaxGg)

[Github](https://github.com/Arkhist/Hacknet-Pathfinder)

[Documentation](https://arkhist.github.io/Hacknet-Pathfinder/)

[Bug Reports](https://github.com/Arkhist/Hacknet-Pathfinder/issues)

## Warning
Use at your own risk, the API is not stable, things may change between versions.
