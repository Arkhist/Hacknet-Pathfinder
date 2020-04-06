using System;
using System.Collections.Generic;
using System.Linq;
using Hacknet;
using Hacknet.Extensions;
using Hacknet.PlatformAPI.Storage;
using Pathfinder.ModManager;
using Pathfinder.Util;

namespace Pathfinder.Game
{
    public static class TextLoader
    {
        private static Dictionary<string, Func<string>> funcReplace = new Dictionary<string, Func<string>>
        {
            {"PATH", () => MissionGenerationParser.Path},
            {"FILE", () => MissionGenerationParser.File},
            {"COMP", () => MissionGenerationParser.Comp},
            {"CLIENT", () => MissionGenerationParser.Client},
            {"TARGET", () => MissionGenerationParser.Target},
            {"OTHER", () => MissionGenerationParser.Other},
            {"LC_CLIENT", () => MissionGenerationParser.Client.Replace(' ', '\u005F').ToLower()},

            {"BINARY", () => Utility.GenerateBinString(2000, Utils.random)},
            {"BINARYSMALL", () => Utility.GenerateBinString(800, Utils.random)},
            {"PLAYERNAME", () =>  ComputerLoader.os.defaultUser.name},
            {"PLAYER_IP", () =>  ComputerLoader.os.thisComputer.ip},
            {"PLAYER_ACCOUNT_PASSWORD", () =>  SaveFileManager.LastLoggedInUser.Password},
            {"RANDOM_IP", Hacknet.NetworkMap.generateRandomIP},
            {"SSH_CRACK", () =>  PortExploits.crackExeData[22]},
            {"FTP_CRACK", () =>  PortExploits.crackExeData[21]},
            {"WEB_CRACK", () =>  PortExploits.crackExeData[80]},
            {"DECYPHER_PROGRAM", () =>  PortExploits.crackExeData[9]},
            {"DECHEAD_PROGRAM", () =>  PortExploits.crackExeData[10]},
            {"CLOCK_PROGRAM", () =>  PortExploits.crackExeData[11]},
            {"MEDICAL_PROGRAM", () =>  PortExploits.crackExeData[104]},
            {"SMTP_CRACK", () =>  PortExploits.crackExeData[25]},
            {"SQL_CRACK", () =>  PortExploits.crackExeData[1433]},
            {"SECURITYTRACER_PROGRAM", () =>  PortExploits.crackExeData[4]},
            {"HACKNET_EXE", () =>  PortExploits.crackExeData[15]},
            {"HEXCLOCK_EXE", () =>  PortExploits.crackExeData[16]},
            {"SEQUENCER_EXE", () =>  PortExploits.crackExeData[17]},
            {"THEMECHANGER_EXE", () =>  PortExploits.crackExeData[14]},
            {"EOS_SCANNER_EXE", () =>  PortExploits.crackExeData[13]},
            {"TRACEKILL_EXE", () =>  PortExploits.crackExeData[12]},
            {"GREEN_THEME", () =>  ThemeManager.getThemeDataString(OSTheme.HackerGreen)},
            {"WHITE_THEME", () =>  ThemeManager.getThemeDataString(OSTheme.HacknetWhite)},
            {"YELLOW_THEME", () =>  ThemeManager.getThemeDataString(OSTheme.HacknetYellow)},
            {"TEAL_THEME", () =>  ThemeManager.getThemeDataString(OSTheme.HacknetTeal)},
            {"BASE_THEME", () =>  ThemeManager.getThemeDataString(OSTheme.HacknetBlue)},
            {"PURPLE_THEME", () =>  ThemeManager.getThemeDataString(OSTheme.HacknetPurple)},
            {"MINT_THEME", () =>  ThemeManager.getThemeDataString(OSTheme.HacknetMint)},
            {"PACEMAKER_FW_WORKING", () =>  PortExploits.ValidPacemakerFirmware},
            {"PACEMAKER_FW_DANGER", () =>  PortExploits.DangerousPacemakerFirmware},
            {"RTSP_EXE", () =>  PortExploits.crackExeData[554]},
            {"EXT_SEQUENCER_EXE", () =>  PortExploits.crackExeData[40]},
            {"SHELL_OPENER_EXE", () =>  PortExploits.crackExeData[41]},
            {"FTP_FAST_EXE", () =>  PortExploits.crackExeData[211]},
            {"EXTENSION_FOLDER_PATH", () => (ExtensionLoader.ActiveExtensionInfo != null
                                             ? ExtensionLoader.ActiveExtensionInfo.GetFullFolderPath().Replace("/Content", "/ Content")
                                             : "ERROR GETTING PATH") },
            {"PLAYERLOCATION", () =>  "UNKNOWN"},

            {"TORRENT_EXE", () => DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[6881] : null},
            {"SSL_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[443] : null},
            {"KAGUYA_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[31] : null},
            {"SIGNAL_SCRAMBLER_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[32] : null},
            {"MEM_FORENSICS_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[33] : null},
            {"MEM_DUMP_GENERATOR", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[34] : null},
            {"PACIFIC_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[192] : null},
            {"NETMAP_ORGANIZER_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[35] : null},
            {"SHELL_CONTROLLER_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[36] : null},
            {"NOTES_DUMPER_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[37] : null},
            {"CLOCK_V2_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[38] : null},
            {"DLC_MUSIC_EXE", () =>  DLC1SessionUpgrader.HasDLC1Installed ? PortExploits.crackExeData[39] : null},
            {"GIBSON_IP", () =>  DLC1SessionUpgrader.HasDLC1Installed ? ComputerLoader.os.GibsonIP : null}
        };

        public static void AddFunc(string search, Func<string> func) =>
            funcReplace.Add(Manager.CurrentMod.GetCleanId() + "." + search, func);

        public static bool RemoveFunc(string id)
        {
            if (funcReplace.ContainsKey(id))
                return funcReplace.Remove(id);
            return funcReplace.Remove(Manager.CurrentMod.GetCleanId() + "." + id);
        }

        public static string HacknetFilter(this string input)
        {
            var split = input.SerialSplit('#');
            for (var i = 0; i < split.Count; i++)
            {
                if (split[i].Item2)
                {
                    if (funcReplace.ContainsKey(split[i].Item1))
                        split[i] = new Tuple<string, bool>(funcReplace[split[i].Item1]() ?? "", split[i].Item2);
                    else split[i] = new Tuple<string, bool>("#" + split[i].Item1 + "#", split[i].Item2);
                }
            }
            return string.Join("", split/*.Where(t => t.Item2)*/.Select(t => t.Item1))
                         .Replace("\t", "    ")
                         .Replace("&quot;", "'")
                         .Replace("\u00a0", "")
                         .Replace("[PRÉNOM]#[NOM]#[NUM_DOSSIER]#32#Rural#N/A#N/A#N/A#[DERNIERS_MOTS]",
                                  "[FIRST_NAME]#[LAST_NAME]#[RECORD_NUM]#32#Rural#N/A#N/A#N/A#[LAST_WORDS]")
                         .Replace("[PRÉNOM]", "[FIRST_NAME]");
        }

        public static string XmlFilter(this string input) => Hacknet.Folder.Filter(input);
        public static string XmlDefilter(this string input) => Hacknet.Folder.deFilter(input);
    }
}
