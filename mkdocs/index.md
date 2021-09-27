# Hacknet Pathfinder

Hacknet Pathfinder is a cross-platform modding system for Hacknet.

## Automatic Install

Run the Hacknet Pathfinder installer, downloadable from: [Github](https://github.com/Arkhist/Hacknet-Pathfinder/releases)

- For Windows, run `PathfinderInstaller.exe`
- For Linux, run `PathfinderInstaller.py`

## Manual Install

1. Download Hacknet Pathfinder's zipped files from [Github](https://github.com/Arkhist/Hacknet-Pathfinder/releases).
2. Extract the contents into Hacknet's folder.
3. Run `PathfinderPatcher.exe` (For Linux users, `mono PathfinderPatcher.exe`).
4. (For Linux) Copy `Hacknet.bin.x86(_64)` to `HacknetPathfinder.bin.x86(_64)` and make `StartPathfinder.sh` executable.
5. Run HacknetPathfinder.exe on Windows or StartPathfinder.sh on Linux to start Pathfinder.

## Running

Hacknet Pathfinder runs as normal Hacknet, there is no change.

## Installing global mods

Move the global mod contents into `Hacknet/BepInEx/plugins`.

## Troubleshooting

### [Windows] System.NotSupportedException on launch

More precisely:

```None
System.NotSupportedException: An attempt was made to load an assembly from a network location
which would have caused the assembly to be sandboxed in previous versions of the .NET Framework
```

Solution: unblock all .dll files provided by Hacknet Pathfinder. This can be done manually by rightclicking each .dll files and editing their properties, or running the following Powershell command:

`get-childitem "HACKNET-FOLDER-HERE" | unblock-file`