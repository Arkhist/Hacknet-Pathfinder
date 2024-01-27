using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Hacknet.Extensions;
using HN = global::Hacknet;

namespace BepInEx.Hacknet;

public abstract class HacknetPlugin
{
    protected HacknetPlugin()
    {
        var metadata = MetadataHelper.GetMetadata(this);

        HarmonyInstance = new Harmony("BepInEx.Plugin." + metadata.GUID);

        Log = Logger.CreateLogSource(metadata.Name);

        InstalledGlobally = !HN.Settings.IsInExtensionMode;

        Config = new ConfigFile(Path.Combine(Paths.ConfigPath, metadata.GUID + ".cfg"), false, metadata);

        if (!InstalledGlobally)
            UserConfig = new ConfigFile(
                Path.Combine("BepInEx/config/", ExtensionLoader.ActiveExtensionInfo.GetFoldersafeName(), metadata.GUID + ".cfg"),
                false,
                metadata
            );
    }

    public ManualLogSource Log { get; }

    public bool InstalledGlobally { get; }

    /// <summary>
    /// If this plugin is installed in an extension, holds a <see cref="ConfigFile"/> to be edited by the extension developer.
    /// <br/>
    /// Otherwise, if its installed globally, holds a <see cref="ConfigFile"/> to be edited by the user.
    /// <br/><br/>
    /// For the extension player's <see cref="ConfigFile"/>, see <see cref="UserConfig"/>.  
    /// </summary>
    public ConfigFile Config { get; }
    /// <summary>
    /// If this plugin is installed in an extension, holds a <see cref="ConfigFile"/> to be edited by the extension player.
    /// <br/>
    /// Otherwise, if its installed globally, holds the value <c>null</c>.
    /// <br/><br/>
    /// For the extension developer's <see cref="ConfigFile"/>, see <see cref="Config"/>.  
    /// </summary>
    public ConfigFile UserConfig { get; }

    public Harmony HarmonyInstance { get; set; }

    public abstract bool Load();

    /// <summary>
    /// Runs after all plugins have executed their Load method
    /// </summary>
    public virtual void PostLoad() {}

    public virtual bool Unload()
    {
        HarmonyInstance.UnpatchSelf();
        return true;
    }
}
