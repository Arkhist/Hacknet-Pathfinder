using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hacknet;
using Hacknet.Localization;
using Hacknet.PlatformAPI.Storage;
using Hacknet.Security;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class SaveLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), nameof(OS.loadSaveFile))]
        internal static bool SaveLoadReplacementPrefix(ref OS __instance)
        {
            __instance.FirstTimeStartup = false;
            
            Stream saveStream = __instance.ForceLoadOverrideStream ?? SaveFileManager.GetSaveReadStream(__instance.SaveGameUserName);
            if (saveStream == null)
            {
                return false;
            }

            var os = __instance;
            
            var executor = new EventExecutor(new StreamReader(saveStream).ReadToEnd(), false);
            executor.RegisterExecutor("HacknetSave", (exec, info) =>
            {
                MissionGenerator.generationCount = info.Attributes.GetInt("generatedMissionCount", 100);
                os.username = os.defaultUser.name = info.Attributes.GetString("Username");
                os.LanguageCreatedIn = info.Attributes.GetString("Language", "en-us");
                os.IsInDLCMode = info.Attributes.GetBool("DLCMode") && Settings.EnableDLC;
                os.DisableEmailIcon = info.Attributes.GetBool("DisableMailIcon") && Settings.EnableDLC;

                if (os.LanguageCreatedIn != Settings.ActiveLocale)
                {
                    LocaleActivator.ActivateLocale(os.LanguageCreatedIn, os.content);
                    Settings.ActiveLocale = os.LanguageCreatedIn;
                }
            });
            executor.RegisterExecutor("HacknetSave.DLC", (exec, info) =>
            {
                os.IsDLCSave = true;

                os.IsInDLCMode = info.Attributes.GetBool("Active");
                os.HasLoadedDLCContent = info.Attributes.GetBool("LoadedContent");

                if (os.HasLoadedDLCContent && !DLC1SessionUpgrader.HasDLC1Installed)
                {
                    MainMenu.AccumErrors = "LOAD ERROR: Save " + os.SaveGameUserName + " is configured for Labyrinths DLC, but it is not installed on this computer.\n\n\n";
                    os.ExitScreen();
                    os.IsExiting = true;
                }
            });
            executor.RegisterExecutor("HackentSave.DLC.Flags", 
                (exec, info) => os.PreDLCFaction = info.Attributes.GetString("OriginalFaction"));
            executor.RegisterExecutor("Hacknet.DLC.OriginalVisibleNodes", 
                (exec, info) => os.PreDLCVisibleNodesCache = info.Content,
                ParseOption.ParseInterior);
            // todo: conditional actions loader
            executor.RegisterExecutor("HacknetSave.Flags", (exec, info) =>
            {
                os.Flags.Flags.Clear();

                foreach (var flag in info.Content.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    os.Flags.Flags.Add(flag
                        .Replace("[%%COMMAREPLACED%%]", ",")
                        .Replace("décrypté", "decypher"));
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("HacknetSave.NetworkMap", (exec, info) =>
            {
                Enum.TryParse(info.Attributes.GetString("sort"), out os.netMap.SortingAlgorithm);
            });
            executor.RegisterExecutor("HacknetSave.NetworkMap", (exec, info) =>
            {
                foreach (var daemon in os.netMap.nodes.SelectMany(x => x.daemons))
                    daemon.loadInit();
                
                os.netMap.loadAssignGameNodes();
            }, ParseOption.FireOnEnd);
            executor.RegisterExecutor("HacknetSave.NetworkMap.network.computer", (exec, info) =>
            {
                os.netMap.nodes.Add(LoadComputer(info, os));
            });
            
            executor.Parse();
            
            return false;
        }

        private static Computer LoadComputer(ElementInfo info, OS os)
        {
            var name = info.Attributes.GetString("name");
            var ip = info.Attributes.GetString("ip");
            var type = info.Attributes.GetByte("type");
            var spec = info.Attributes.GetString("spec");

            var location = info.Children.GetElement("location");
            var xCord = location?.Attributes.GetFloat("x") ?? 0f;
            var yCord = location?.Attributes.GetFloat("y") ?? 0f;

            var security = info.Children.GetElement("security");
            var level = security.Attributes.GetInt("level");

            Firewall firewall = null;
            if (info.Children.TryGetElement("firewall", out var firewallInfo))
            {
                firewall = new Firewall(
                    firewallInfo.Attributes.GetInt("complexity"),
                    info.Attributes.GetString("solution", null),
                    info.Attributes.GetFloat("additionalDelay")
                );
            }

            var comp = new Computer(name, ip, new Vector2(xCord, yCord), level, type, os)
            {
                idName = info.Attributes.GetString("id"),
                attatchedDeviceIDs = info.Attributes.GetString("devices", null),
                icon = info.Attributes.GetString("icon", null),
                HasTracker = info.Attributes.GetBool("tracker"),
                firewall = firewall,
                traceTime = security.Attributes.GetFloat("traceTime"),
                portsNeededForCrack = security.Attributes.GetInt("portsToCrack"),
                adminIP = security.Attributes.GetString("adminIP")
            };

            var proxyTime = security.Attributes.GetFloat("proxyTime");
            if (proxyTime > 0)
            {
                comp.addProxy(proxyTime);
            }
            else
            {
                comp.hasProxy = false;
                comp.proxyActive = false;
            }

            Administrator admin = null;
            if (info.Children.TryGetElement("admin", out var adminInfo))
            {
                switch (adminInfo.Attributes.GetString("type"))
                {
                    case "fast":
                        admin = new FastBasicAdministrator();
                        break;
                    case "basic":
                        admin = new BasicAdministrator();
                        break;
                    case "progress":
                        admin = new FastProgressOnlyAdministrator();
                        break;
                }

                if (admin != null)
                {
                    admin.ResetsPassword = adminInfo.Attributes.GetBool("resetPass");
                    admin.IsSuper = adminInfo.Attributes.GetBool("isSuper");
                }
            }

            comp.admin = admin;

            foreach (var link in info.Children.GetElement("links").Content.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries))
            {
                comp.links.Add(int.Parse(link));
            }

            if (info.Children.TryGetElement("portsOpen", out var portsOpen))
            {
                if ((portsOpen.Content?.Length ?? 0) > 0)
                    ComputerLoader.loadPortsIntoComputer(portsOpen.Content, comp);
            }

            if (info.Children.TryGetElement("portRemap", out var remap))
                comp.PortRemapping = PortRemappingSerializer.Deserialize(remap.Content);

            if (info.Children.TryGetElement("users", out var users))
            {
                foreach (var user in users.Children.Where(x => x.Name == "user"))
                {
                    var username = user.Attributes.GetString("name");
                    var pass = user.Attributes.GetString("pass");

                    var userDetail = new UserDetail(
                        username,
                        pass,
                        user.Attributes.GetByte("type")
                    )
                    {
                        known = user.Attributes.GetBool("known")
                    };

                    if (username.ToLower() == "admin")
                        comp.adminPass = pass;

                    comp.users.Add(userDetail);
                }
            }

            if (info.Children.TryGetElement("Memory", out var memory))
                comp.Memory = ReplacementsCommon.LoadMemoryContents(memory);

            if (info.Children.TryGetElement("daemons", out var daemons))
            {
                foreach (var daemon in daemons.Children)
                {
                    // don't continue making this, switch it to a dictionary
                    switch (daemon.Name)
                    {
                        case "MailServer":
                            var server = new MailServer(comp, info.Attributes.GetString("name"), os);
                            if (daemon.Attributes.TryGetValue("color", out var color))
                            {
                                var colorSplit = color.Split(',').Select(int.Parse).ToArray();
                                server.setThemeColor(new Color(colorSplit[0], colorSplit[1], colorSplit[2]));
                            }
                            break;
                        case "MissionListingServer":
                            break;
                    }
                }
            }

            return comp;
        }
    }
}
