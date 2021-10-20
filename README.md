# Pathfinder

An extensive modding API and loader for Hacknet that enables practically limitless programable extensions to the game.

## Installation

There are several options available to choose to install Pathfinder, the installer .exe, the installer .py, or the manually with the .zip.

### Installer

If you're on Windows, it's recommended that you use the installer .exe from [here](https://github.com/Arkhist/Hacknet-Pathfinder/releases). Just run the installer and it should automatically find your Hacknet folder, then just hit install. Launching Hacknet from Steam will launch Pathfinder (on Windows)!

If you decide to use the .py installer (or you're just on Linux and have to use it) keep in mind it requires python3 and tk to be installed before you run it.

If you're on Linux, once the installer is complete, make sure to +x StartPathfinder.sh yourself.

To uninstall, just reopen the installer and click uninstall. This will clear out all of the changes the installer made, and will also delete all of your mods with it.

### Manually (Windows)

Get the latest ZIP from the releases page [here](https://github.com/Arkhist/Hacknet-Pathfinder/releases) and extract it to your Hacknet folder.

Run PathfinderPatcher.exe and it will create HacknetPathfinder.exe, if you want this to be launched when you launch from Steam rename Hacknet.exe and replace it with HacknetPathfinder.exe.

To uninstall, just delete HacknetPathfinder.exe (or whatever you renamed it to) and move back the original Hacknet.exe if you renamed it. If you also want to remove all your mods and configs, delete the BepInEx directory.

### Manually (Linux)

Get the latest ZIP from the releases page [here](https://github.com/Arkhist/Hacknet-Pathfinder/releases) and extract it to your Hacknet folder.

Run PathfinderPatcher.exe and it will create HacknetPathfinder.exe.

Copy `Hacknet.bin.x86_64` to `HacknetPathfinder.bin.x86_64`

Make StartPathfinder.sh executable and run it.

## Troubleshooting

### The game crashes before it even loads! (Windows only)

If the game crashes before loading, or you see an error in the console that says `An attempt was made to load an assembly from a network location which would have caused the assembly to be sandboxed in previous versions of the .NET Framework.` on Windows, head into the BepInEx/core and BepInEx/plugins folders, open the properties for each DLL, and hit the unblock checkbox at the bottom.

As an alternative, run this command in PowerShell in the Hacknet directory `Get-ChildItem -Recurse "./BepInEx" | Unblock-File`

## Creating Mods

Use [this template](https://github.com/Windows10CE/HacknetPluginTemplate) or follow the steps below.

1. Start a new .NET library project with .NET Framework 4.0
2. Link it against the HacknetPathfinder.exe, PathfinderAPI.dll, FNA.dll, BepInEx.Core.dll, and BepInEx.Hacknet.dll. You may need more than this, but that's the reccomended set to begin on a simple mod.
    * You may need to go into the project options and set the project to target the x86 platform
3. Create a class that inherits from BepInEx.Hacknet.HacknetPlugin, and add the BepInEx.BepInPlugin attribute to it with a name, guid, and version.
4. And now you have a basic functioning mod, the rest is up to you.

Install the mod by placing it in Hacknet/BepInEx/plugins or a folder called Plugins in your extension's folder if you want to make it extension-specific.

## Contributing to Pathfinder

1. Clone the project with `git clone https://github.com/Arkhist/Hacknet-Pathfinder` or preferably fork it and clone that repo so you can pull request
2. Compile PathfinderAPI or BepInEx.Hacknet

### Testing contiburitons

Just run `msbuild /target:<ProjectName>:RunGame` and it automatically prepares and executes the game for that project. It also cleans up after itself and preserves your regular game files. Applicable projects are BepInEx.Hacknet, PathfinderAPI, and PathfinderUpdater

* Do note that if any project name contains `%`, `$`, `@`, `;`, `.`, `(`, `)`, or `'` it must be replaced with `_` (eg: `BepInEx.Hacknet` becomes `BepInEx_Hacknet`)

* Additional plugins or config modifications will be found in `Hacknet-Pathfinder/.debug` which is treated as `Hacknet/BepInEx` when RunGame is executed.

## Links

[Discord](https://discord.gg/65SaxGg)

[Github](https://github.com/Arkhist/Hacknet-Pathfinder)

[Bug Reports](https://github.com/Arkhist/Hacknet-Pathfinder/issues)
