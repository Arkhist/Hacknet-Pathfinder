using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using Hacknet.Extensions;
using Hacknet.Gui;
using Hacknet.Screens;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using MonoMod.Cil;
using HN = global::Hacknet;

namespace BepInEx.Hacknet
{
    public class HacknetChainloader : BaseChainloader<HacknetPlugin>
    {
        public static HacknetChainloader Instance;

        internal ManualLogSource Log = Logger.CreateLogSource("HacknetChainloader");
        internal List<string> TemporaryPluginGUIDs = new List<string>();

        internal Harmony HarmonyInstance;
        private bool _firstLoadDone = false;
        
        public override void Initialize(string gameExePath = null)
        {
            base.Initialize(gameExePath);
            Instance = this;

            HarmonyInstance = new Harmony("BepInEx.Hacknet.Chainloader");

            HarmonyInstance.PatchAll(typeof(ExtensionPluginPatches));
        }

        protected override IList<PluginInfo> DiscoverPlugins()
        {
            if (!_firstLoadDone)
            {
                _firstLoadDone = true;
                return base.DiscoverPlugins();
            }

            var plugins = base.DiscoverPlugins();
            TemporaryPluginGUIDs = plugins.Select(plugin => plugin.Metadata.GUID).ToList();
            return plugins;
        }

        public override HacknetPlugin LoadPlugin(PluginInfo pluginInfo, Assembly pluginAssembly)
        {
            var type = pluginAssembly.GetType(pluginInfo.TypeName);

            var pluginInstance = (HacknetPlugin)Activator.CreateInstance(type);
            if (!pluginInstance.Load())
            {
                throw new Exception($"{pluginInfo.Metadata.GUID} returned false on it's load method");
            }

            return pluginInstance;
        }

        internal void UnloadTemps()
        {
            Log.LogMessage("Unloading extension plugins...");
            
            foreach (var temp in TemporaryPluginGUIDs)
            {
                if (!(this.Plugins[temp].Instance as HacknetPlugin)?.Unload() ?? true)
                {
                    Log.LogError($"{temp} failed to unload, this could cause problems without a game restart!");
                }
                this.Plugins.Remove(temp);
                
                Log.LogMessage($"Unloaded {temp}");
            }
            TemporaryPluginGUIDs.Clear();
            
            Log.LogMessage("Finished unloading extension plugins");
        }
    }

    [HarmonyPatch]
    internal static class ExtensionPluginPatches
    {
        // BepInEx.Paths's setters are private, this creates setters we can use
        private static readonly MethodInfo PluginPathSetter = AccessTools.PropertySetter(typeof(Paths), nameof(Paths.PluginPath));
        private static readonly MethodInfo ConfigPathSetter = AccessTools.PropertySetter(typeof(Paths), nameof(Paths.ConfigPath));

        private static bool FirstExtensionLoaded = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtensionsMenuScreen), nameof(ExtensionsMenuScreen.ActivateExtensionPage))]
        internal static bool LoadTempPluginsPrefix(ExtensionInfo info)
        {
            if (!FirstExtensionLoaded)
            {
                HacknetChainloader.Instance.HarmonyInstance.PatchAll(typeof(ChainloaderFix));
                FirstExtensionLoaded = true;
            }
            
            try
            {
                var newPluginPath = Path.Combine(info.GetFullFolderPath(), "Plugins");
                var newConfigPath = Path.Combine(newPluginPath, "Configs");

                if (Directory.Exists(newPluginPath) && Directory.GetFiles(newPluginPath).Length > 0)
                {
                    PluginPathSetter.Invoke(null, new object[] { newPluginPath });
                    ConfigPathSetter.Invoke(null, new object[] { newConfigPath });
                    HacknetChainloader.Instance.Execute();
                }

                return true;
            }
            catch (Exception ex)
            {
                HacknetChainloader.Instance.Log.LogError($"A fatal exception occured while loading extension plugins, aborting:\n{ex}");

                return false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HN.OS), nameof(HN.OS.quitGame))]
        internal static void UnloadOnOSQuitPostfix() => HacknetChainloader.Instance.UnloadTemps();
        
        // I would hook Hacknet.Screens.DrawExtensionInfoDetail instead, but for some reason that method is cursed, so I look here instead
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Button), nameof(Button.doButton), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(string), typeof(Color?) })]
        internal static void OnBackButtonPressPostfix(int myID, bool __result)
        {
            if (myID == 7900040 && __result)
                HacknetChainloader.Instance.UnloadTemps();
        }
    }

    [HarmonyPatch]
    internal static class ChainloaderFix
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(BaseChainloader<HacknetPlugin>), "Execute")]
        internal static void FixChainloaderForReload(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(Assembly), "LoadFile", new Type[] { typeof(string) }))
            );

            c.Remove();
            c.EmitDelegate<Func<string, Assembly>>(path =>
            {
                byte[] asmBytes;

                using (var asm = AssemblyDefinition.ReadAssembly(path))
                {
                    asm.Name.Name = asm.Name.Name + "-" + DateTime.Now.Ticks;

                    using (var ms = new MemoryStream())
                    {
                        asm.Write(ms);
                        asmBytes = ms.ToArray();
                    }
                }

                return Assembly.Load(asmBytes);
            });
        }
    }
}
