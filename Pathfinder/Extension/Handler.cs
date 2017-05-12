using System;
using System.Collections.Generic;
using System.IO;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Util;

namespace Pathfinder.Extension
{
    public static class Handler
    {
        public static Info ActiveInfo { get; internal set; }

        internal static Dictionary<string, Info> idToInfo = new Dictionary<string, Info>();
        internal static Dictionary<string, Texture2D> idToLogo = new Dictionary<string, Texture2D>();
        internal static Dictionary<string, GUI.Button> idToButton = new Dictionary<string, GUI.Button>();

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
            if (idToInfo.ContainsKey(id))
                return null;

            extensionInfo.Id = id;
            idToInfo.Add(id, extensionInfo);
            Texture2D t = null;
            if (File.Exists(extensionInfo.LogoPath))
                using (var fs = File.OpenRead(extensionInfo.LogoPath))
                    t = Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, fs);
            idToLogo.Add(id, t);
            idToButton.Add(id, new GUI.Button(-1, -1, 450, 50, extensionInfo.Name, Color.White));
            return id;
        }

        internal static bool UnregisterExtension(string id)
        {
            id = Utility.GetId(id);
            if (!idToInfo.ContainsKey(id))
                return true;
            var info = idToInfo[id];
            info.Id = null;
            idToLogo.Remove(id);
            idToButton.Remove(id);
            return idToInfo.Remove(id);
        }
    }
}
