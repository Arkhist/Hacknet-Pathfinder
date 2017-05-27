using System;
using System.Collections.Generic;
using Pathfinder.Util;
using Pathfinder.Util.Attribute;

namespace Pathfinder.GUI.ModOptions
{
    static class Handler
    {
        public static Dictionary<string, AbstractOptions> ModOptions = new Dictionary<string, AbstractOptions>();

        public static void LoadFor(IPathfinderMod mod)
        {
            var optionType = mod.GetType().GetFirstAttribute<ModOptionsAttribute>()?.ModOptionsType;
            if (optionType == null) return;
            var options = (AbstractOptions)Activator.CreateInstance(optionType);
            ModOptions.Add(mod.GetCleanId(), options);
        }
    }
}
