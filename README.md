# Pathfinder
An extensive modding API and loader for Hacknet that enables practically limitless programable extensions to the game

### Installation

1. Extract all of the binary files into the directory containing Hacknet.exe (goto game properties in Steam, goto the local tab, and click browse local files)
2. Run PathfinderPatcher.exe
3. Start HacknetPathfinder.exe and then close (or you can merely create a Mods folder in the game directory)
4. Drop any mod DLLs you plan to play with into the Mods directory
5. Start HacknetPathfinder.exe

### Creating Mods

1. Start a new .NET library project
2. Link it against the Pathfinder.dll (recommended you also at least link it against FNA.dll and HacknetPathfinder.exe)
    * You may need to go into the project options and set the project to target the x86 platform
3. Create a class that implments the Pathfinder.ModManager.IMod interface
4. And now you have a basic functioning mod, the rest is up to you

### Contributing to Pathfinder

1. Clone the project like so `git clone https://github.com/Arkhist/Hacknet-Pathfinder`
2. Copy FNA.dll, AlienFXManagedWrapper3.5.dll, and Steamworks.NET.dll into the lib directory
3. Open the solution file in a .NET 4.0 compatible IDE
4. Build the PathfinderPatcher and Pathfinder projects (if you're using an IDE other then Xamarin Studio on Windows, build.bat should prevent any problematic IDE annoyances)
5. And now you're ready to contribute to the development of the mod

## Links

[Discord](https://discord.gg/65SaxGg)

[Github](https://github.com/Arkhist/Hacknet-Pathfinder)

[Documentation](https://arkhist.github.io/Hacknet-Pathfinder/)

[Bug Reports](https://github.com/Arkhist/Hacknet-Pathfinder/issues)

## Warning
Use at your own risk
