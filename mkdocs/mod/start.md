# Getting started with Pathfinder Modding

Use [this template](https://github.com/Windows10CE/HacknetPluginTemplate) or follow the steps below.

- Start a new .NET library project with .NET Framework 4.0
- Link it against the `HacknetPathfinder.exe` (`Hacknet.exe` if you installed Pathfinder), `PathfinderAPI.dll`, `FNA.dll`, `BepInEx.Core.dll`, and `BepInEx.Hacknet.dll`. You may need more than this, but that's the reccomended set to begin on a simple mod.
- You may need to go into the project options and set the project to target the `x86` platform
- Create a class that inherits from `BepInEx.Hacknet.HacknetPlugin`, and add the `BepInEx.BepInPlugin` attribute to it with a name, guid, and version.

```CSharp
using BepInEx;
using BepInEx.Hacknet;

namespace HacknetPluginTemplate
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class HacknetPluginTemplate : HacknetPlugin
    {
        public const string ModGUID = "com.Windows10CE.Template";
        public const string ModName = "MOD NAME HERE";
        public const string ModVer = "1.0.0";

        public override bool Load()
        {
            return true;
        }
    }
}
```

And now you have a basic functioning mod, the rest is up to you!

All of the "Register" calls should be placed in the `Load()` function of your mod.

Install the mod by placing it in `Hacknet/BepInEx/plugins` or a folder called `Plugins` in your extension's folder if you want to make it extension-specific.
