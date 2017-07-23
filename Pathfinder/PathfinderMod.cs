#pragma warning disable CS0618 // Type or member is obsolete

using System;
using Pathfinder.Util.File;

namespace Pathfinder
{
    [Obsolete("Use ModManager.IMod")]
    public interface IPathfinderMod : ModManager.IMod {}
    [Obsolete("Use ModManager.Mod")]
    public abstract class PathfinderMod : ModManager.Mod, IPathfinderMod {}
    [Obsolete("Use ModManager.Placeholder")]
    public class ModPlaceholder : ModManager.Placeholder, IPathfinderMod
    {
        public ModPlaceholder(string id) : base(id) {}
    }
}
