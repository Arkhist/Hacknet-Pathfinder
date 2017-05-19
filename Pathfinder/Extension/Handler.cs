using System;
using System.Collections.Generic;
using System.IO;
using Hacknet;
using Hacknet.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;

namespace Pathfinder.Extension
{
    public static class Handler
    {
        public static Info ActiveInfo { get; internal set; }
        public static ExtensionInfo ActiveExtension
        {
            get { return ExtensionLoader.ActiveExtensionInfo; }
            set { ExtensionLoader.ActiveExtensionInfo = value; }
        }

        internal static Dictionary<string, Tuple<Info, Texture2D, GUI.Button>> ModExtensions =
                                                                         new Dictionary<string, Tuple<Info, Texture2D, GUI.Button>>();

        public static string RegisterExtension(string id, Info extensionInfo)
        {
            if (Pathfinder.CurrentMod == null)
                throw new InvalidOperationException("RegisterExtension can not be called outside of mod loading.\nMod Blame: "
                                                    + Utility.GetPreviousStackFrameIdentity());
            id = Utility.GetId(id, throwFindingPeriod: true);
            Logger.Verbose("Mod {0} attempting to register extension {1} with id {2}",
                           Utility.ActiveModId,
                           extensionInfo.GetType().FullName,
                           id);
            if (ModExtensions.ContainsKey(id))
                return null;

            extensionInfo.Id = id;
            Texture2D t = null;
            if (File.Exists(extensionInfo.LogoPath))
                using (var fs = File.OpenRead(extensionInfo.LogoPath))
                    t = Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, fs);
            ModExtensions.Add(id, new Tuple<Info, Texture2D, GUI.Button>(extensionInfo, t,
                                                                    new GUI.Button(-1, -1, 450, 50, extensionInfo.Name, Color.White)));
            return id;
        }

        internal static bool UnregisterExtension(string id)
        {
            id = Utility.GetId(id);
            if (!ModExtensions.ContainsKey(id))
                return true;
            var info = ModExtensions[id];
            info.Item1.Id = null;
            return ModExtensions.Remove(id);
        }
    }
}
