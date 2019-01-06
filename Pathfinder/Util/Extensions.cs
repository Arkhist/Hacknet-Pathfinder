using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hacknet;
using Hacknet.Extensions;
using Hacknet.PlatformAPI.Storage;

namespace Pathfinder.Util
{
    public static class Extensions
    {
        public static T GetFirstAttribute<T>(this MethodInfo info, bool inherit = false) where T : System.Attribute
            => info.GetCustomAttributes(inherit).Length > 0
                ? info.GetCustomAttributes(typeof(T), inherit)[0] as T
                : null;

        public static T GetFirstAttribute<T>(this Type type, bool inherit = false) where T : System.Attribute
            => type.GetCustomAttributes(inherit).Length > 0
                ? type.GetCustomAttributes(typeof(T), inherit)[0] as T
                : null;

        public static T GetFirstAttribute<T>(this FieldInfo info, bool inherit = false) where T : System.Attribute
            => info.GetCustomAttributes(inherit).Length > 0
                ? info.GetCustomAttributes(typeof(T), inherit)[0] as T
                : null;

        private static Dictionary<string, Func<string>> funcReplace = new Dictionary<string, Func<string>>
        {
            {"PATH", () => MissionGenerationParser.Path},
            {"FILE", () => MissionGenerationParser.File},
            {"COMP", () => MissionGenerationParser.Comp},
            {"CLIENT", () => MissionGenerationParser.Client},
            {"TARGET", () => MissionGenerationParser.Target},
            {"OTHER", () => MissionGenerationParser.Other},
            {"LC_CLIENT", () => MissionGenerationParser.Client.Replace(' ', '\u005F').ToLower()},

            {"BINARY", () => Computer.generateBinaryString(2000)},
            {"BINARYSMALL", () => Computer.generateBinaryString(800)},
            {"PLAYERNAME", () =>  ComputerLoader.os.defaultUser.name},
            {"PLAYER_IP", () =>  ComputerLoader.os.thisComputer.ip},
            {"PLAYER_ACCOUNT_PASSWORD", () =>  SaveFileManager.LastLoggedInUser.Password},
            {"RANDOM_IP", NetworkMap.generateRandomIP},
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
        public static string HacknetConvert(this string input)
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
            return string.Join("", split.Where(t => t.Item2).Select(t => t.Item1))
                         .Replace("\t", "    ")
                         .Replace("&quot;", "'")
                         .Replace("\u00a0", "")
                         .Replace("[PRÉNOM]#[NOM]#[NUM_DOSSIER]#32#Rural#N/A#N/A#N/A#[DERNIERS_MOTS]",
                                  "[FIRST_NAME]#[LAST_NAME]#[RECORD_NUM]#32#Rural#N/A#N/A#N/A#[LAST_WORDS]")
                         .Replace("[PRÉNOM]", "[FIRST_NAME]");
        }

        public static void ThrowNoLabyrinths(this string input, bool inputOnly = false)
        {
            if (DLC1SessionUpgrader.HasDLC1Installed)
                throw new NotSupportedException("Labyrinths DLC not found.\n"
                                                + (inputOnly
                                                   ? input
                                                   : input + " requires Hacknet Labyrinths to be installed."));
        }

        public static bool ToBool(this string input, bool defaultVal = false)
            => defaultVal ? input?.ToLower() != "false" && input != "0" : input?.ToLower() == "true" || input == "1";

        public static List<Tuple<string, bool>> SerialSplit(this string text, char selector, string enders = "\\w", char esc = '\\')
        {
            var tokens = new List<Tuple<string, bool>>();
            byte move = 0; // 0 normal, 1 group capture, 2 escape
            var frag = new StringBuilder(100);
            var checkForWhitespace = enders == "\\w";

            for (var i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (move)
                {
                    case 0:
                        if (c == selector)
                        {
                            if (text.IndexOf(selector, i + 1) == -1
                                // if second capture selector exists
                                || (checkForWhitespace
                                    && text.IndexOfAny(new char[] { ' ', '\t', '\n' }, i) < text.IndexOf(selector, i + 1))
                                // if whitespace
                                || (!checkForWhitespace
                                    && text.IndexOfAny(enders.ToCharArray(), i) < text.IndexOf(selector, i + 1))
                                // if voider of capture
                                || text.IndexOf(esc, i) == text.IndexOf(selector, i + 1) - 1)
                                // if capture is escaped
                                frag.Append(c); // continue seeking
                            else
                            {
                                // execute group capture
                                if (frag.Length != 0) tokens.Add(new Tuple<string, bool>(frag.ToString(), false));
                                frag.Clear();
                                move = 1;
                            }
                        }
                        else if (c == esc) move = 2; // escape character
                        else frag.Append(c); // continue seeking
                        break;
                    case 1:
                        if (c == selector)
                        {
                            // end group capture
                            if (frag.Length != 0) tokens.Add(new Tuple<string, bool>(frag.ToString(), true));
                            frag.Clear();
                            move = 0;
                        }
                        else frag.Append(c); // continue capture
                        break;
                    case 2:
                        // end escape
                        frag.Append(c);
                        move = 0;
                        break;
                }
            }
            if (frag.Length != 0)
                tokens.Add(new Tuple<string, bool>(frag.ToString(), false));
            return tokens;
        }
    }
}
