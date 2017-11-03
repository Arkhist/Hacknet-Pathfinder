using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hacknet;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.ModManager;
using Pathfinder.Util;

namespace Pathfinder
{
    public static class Pathfinder
    {
        /*private static readonly Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly Version Version = new Version(AssemblyVersion.Major,AssemblyVersion.Minor,1);
        private static Version latestVersion;*/

        class InvalidIdException : ArgumentException { public InvalidIdException(string msg) : base(msg) {} }

        public static string ModFolderPath => Manager.ModFolderPath;
        public static string DepFolderPath => Manager.DepFolderPath;
        public static IMod CurrentMod => Manager.CurrentMod;

        /// <summary>
        /// Gets the loaded mod identifiers.
        /// </summary>
        /// <value>The loaded mod identifiers.</value>
        public static List<string> LoadedModIdentifiers => Manager.LoadedModIds;

        /// <summary>
        /// Gets the unloaded mod identifiers.
        /// </summary>
        /// <value>The  mod identifiers.</value>
        public static List<string> UnloadedModIdentifiers => Manager.UnloadedModIds.ToList();

        static Pathfinder()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Manager.LoadedMods.Add("Hacknet", new ModPlaceholder("Hacknet"));
            Manager.LoadedMods.Add("Pathfinder", new ModPlaceholder("Pathfinder"));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        internal static void Initialize()
        {
            Logger.Verbose("Registering Pathfinder listeners");

            /*var verStr = Updater.GetString("https://api.github.com/repos/Arkhist/Hacknet-Pathfinder/releases/latest",
                                           "tag_name"); // Does not work, IDK
            if (Version.TryParse(verStr.Select(c => Char.IsDigit(c) || c == '.' ? c : (char)0).ToString(), out latestVersion)
               && latestVersion > Version)
                EventManager.RegisterListener<DrawMainMenuEvent>(DrawNewReleaseGraphic);*/

            EventManager.RegisterListener<CommandSentEvent>(Internal.ExecutionOverride.OverrideCommands);
            EventManager.RegisterListener<ExecutablePortExecuteEvent>(Internal.ExecutionOverride.OverridePortHack);

            EventManager.RegisterListener<CommandSentEvent>(Internal.HandlerListener.CommandListener);

            EventManager.RegisterListener<DrawMainMenuEvent>(Internal.GUI.ModList.DrawModList);
            EventManager.RegisterListener<DrawMainMenuButtonsEvent>(Internal.GUI.ModList.DrawModListButton);

            EventManager.RegisterListener<DrawExtensionMenuEvent>(Internal.GUI.ModExtensionsUI.ExtensionMenuListener);
            EventManager.RegisterListener<DrawExtensionMenuListEvent>(Internal.GUI.ModExtensionsUI.ExtensionListMenuListener);
            EventManager.RegisterListener<OSLoadContentEvent>(Internal.GUI.ModExtensionsUI.LoadContentForModExtensionListener);
            EventManager.RegisterListener<OSUnloadContentEvent>(Internal.GUI.ModExtensionsUI.UnloadContentForModExtensionListener);

            EventManager.RegisterListener<OptionsMenuLoadContentEvent>(Internal.HandlerListener.OptionsMenuLoadContentListener);
            EventManager.RegisterListener<OptionsMenuDrawEvent>(Internal.HandlerListener.OptionsMenuDrawListener);
            EventManager.RegisterListener<OptionsMenuApplyEvent>(Internal.HandlerListener.OptionsMenuApplyListener);
            EventManager.RegisterListener<OptionsMenuUpdateEvent>(Internal.HandlerListener.OptionsMenuUpdateListener);

            EventManager.RegisterListener<ExecutableExecuteEvent>(Internal.HandlerListener.ExecutableListener);

            EventManager.RegisterListener<OSSaveWriteEvent>(ManageSaveXml);

            EventManager.RegisterListener<GameLoadContentEvent>(ExeInfoManager.LoadExecutableStruct);

            EventManager.RegisterListener<GameUnloadEvent>(Manager.UnloadMods);

            Logger.Verbose("Loading mods");
            Manager.LoadMods();
        }

        /// <summary>
        /// Determines whether a mod is loaded
        /// </summary>
        /// <returns><c>true</c>, if mod is loaded, <c>false</c> otherwise.</returns>
        /// <param name="id">Mod Identifier.</param>
        public static bool IsModLoaded(string id) => Manager.LoadedMods.ContainsKey(id);

        /// <summary>
        /// Determines whether a mod identifier is valid
        /// </summary>
        /// <returns><c>true</c>, if mod identifier is valid, <c>false</c> otherwise.</returns>
        /// <param name="id">The Mod Identifier.</param>
        /// <param name="shouldThrowReason">If set to <c>true</c> then this method will throw.</param>
        public static bool IsModIdentifierValid(string id, bool shouldThrowReason = false)
        {
            if (String.IsNullOrEmpty(id))
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' is null or empty");
                return false;
            }
            if (IsModLoaded(id))
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' has already been loaded");
                return false;
            }
            if (id.Contains('.'))
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' contains a period");
                return false;
            }
            if (Char.IsDigit(id[0]))
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' starts with a digit");
                return false;
            }
            if (id.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || id.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                if (shouldThrowReason)
                    throw new InvalidIdException("Mod Idenfitier '" + id + "' contains invalid path characters");
                return false;
            }
            return true;
        }

        internal static void ManageSaveXml(OSSaveWriteEvent e)
        {
            var i = e.SaveString.IndexOf("</HacknetSave>", StringComparison.Ordinal);
            string modListStr = "";
            foreach (var pair in Manager.OperationalMods)
                modListStr += "\t<Mod assembly='" + Path.GetFileName(pair.Value.GetType().Assembly.Location) + "'>"
                                                        + pair.Key + "</Mod>\n";
            e.SaveString = e.SaveString.Insert(i, "\n<PathfinderMods>\n" + modListStr + "</PathfinderMods>\n");
        }

        internal static void DrawNewReleaseGraphic(DrawMainMenuEvent e)
        {
            if(e.GameTime.ElapsedGameTime.Seconds % 3 != 0)
                TextItem.doFontLabel(new Vector2(300, 100), "New Release Up", GuiData.font, Color.White);
        }
    }
}
