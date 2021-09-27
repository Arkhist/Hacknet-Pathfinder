# Contributing

## Compiling and installing

- Clone from source Hacknet Pathfinder [https://github.com/Arkhist/Hacknet-Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder)
- Compile PathfinderPatcher (`msbuild`[^1] or `xbuild` in root folder or specically against PathfinderPatcher.csproj)
- Move `PathfinderPatcher/bin/Debug/PathfinderPatcher.exe` to Hacknet's root directory
- Run `PathfinderPatcher.exe` in the root Hacknet directory to obtain `HacknetPathfinder.exe`
- Move `HacknetPathfinder.exe` and `FNA.dll` to the Pathfinder `libs` folder.
- Compile the rest of the project (`msbuild`[^1] or `xbuild` in root folder)
- Copy everything in `BepInEx.Hacknet/bin/Debug` into `Hacknet/BepInEx/core`.
- Copy `PathfinderAPI/bin/PathfinderAPI.dll` into `Hacknet/BepInEx/plugins`

[^1]: `msbuild` is broken on Arch unless you use mono-git from the AUR, see [this bug report](https://bugs.archlinux.org/task/71007).

## Adding features

Branch off the `master` branch, program your feature and send the dev team a Pull Request!
