using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;

namespace BepInEx.Hacknet
{
    public abstract class HacknetPlugin
    {
        protected HacknetPlugin()
        {
            var metadata = MetadataHelper.GetMetadata(this);

            HarmonyInstance = new Harmony("BepInEx.Plugin." + metadata.GUID);

            Log = Logger.CreateLogSource(metadata.Name);

            Config = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, metadata.GUID + ".cfg"), false, metadata);
        }

        public ManualLogSource Log { get; }

        public ConfigFile Config { get; }

        public Harmony HarmonyInstance { get; set; }

        public abstract bool Load();

        public virtual bool Unload()
        {
            HarmonyInstance.UnpatchSelf();
            return true;
        }
    }
}
