using System;
using System.Reflection;
using BepInEx.Bootstrap;

namespace BepInEx.Hacknet
{
    public class HacknetChainloader : BaseChainloader<HacknetPlugin>
    {
        public override HacknetPlugin LoadPlugin(PluginInfo pluginInfo, Assembly pluginAssembly)
        {
            var type = pluginAssembly.GetType(pluginInfo.TypeName);

            var pluginInstance = (HacknetPlugin)Activator.CreateInstance(type);
            pluginInstance.Load();

            return pluginInstance;
        }
    }
}
