# Contributing

## Compiling and installing

- Clone from source Hacknet Pathfinder [https://github.com/Arkhist/Hacknet-Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder)
- Compile PathfinderPatcher (`msbuild` or `xbuild` in root folder)
- Move `PathfinderPatcher/bin/Debug/PathfinderPatcher.exe` to Hacknet's root directory
- Run `PathfinderPatcher.exe` in the root Hacknet directory to obtain `HacknetPathfinder.exe`
- Move `HacknetPathfinder.exe` and `FNA.dll` to the Pathfinder `libs` folder.
- Compile the rest of the project (`msbuild` or `xbuild` in root folder)
- Copy everything in `BepInEx.Hacknet/bin/Debug` goes into `Hacknet/BepInEx/core`.
- Copy `PathfinderAPI/bin/PathfinderAPI.dll` goes into `Hacknet/BepInEx/plugins`

## Adding features

Branch off the `master` branch, program your feature and send the dev team a Pull Request!
