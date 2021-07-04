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
using Pathfinder.Event;
using Pathfinder.Event.Loading.Save;
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

            Stream saveStream = __instance.ForceLoadOverrideStream ??
                                SaveFileManager.GetSaveReadStream(__instance.SaveGameUserName);
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
                    MainMenu.AccumErrors = "LOAD ERROR: Save " + os.SaveGameUserName +
                                           " is configured for Labyrinths DLC, but it is not installed on this computer.\n\n\n";
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

                foreach (var flag in info.Content.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    os.Flags.Flags.Add(flag
                        .Replace("[%%COMMAREPLACED%%]", ",")
                        .Replace("décrypté", "decypher"));
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("HacknetSave.NetworkMap",
                (exec, info) => { Enum.TryParse(info.Attributes.GetString("sort"), out os.netMap.SortingAlgorithm); });
            executor.RegisterExecutor("HacknetSave.NetworkMap", (exec, info) =>
            {
                foreach (var daemon in os.netMap.nodes.SelectMany(x => x.daemons))
                    daemon.loadInit();

                os.netMap.loadAssignGameNodes();
            }, ParseOption.FireOnEnd);
            executor.RegisterExecutor("HacknetSave.NetworkMap.network.computer",
                (exec, info) => { os.netMap.nodes.Add(LoadComputer(info, os)); },
                ParseOption.ParseInterior
            );

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

            var security = info.Children.GetElement("security");
            var level = security.Attributes.GetInt("level");

            Firewall firewall = null;
            if (info.Children.TryGetElement("firewall", out var firewallInfo))
            {
                firewall = new Firewall(
                    firewallInfo.Attributes.GetInt("complexity"),
                    firewallInfo.Attributes.GetString("solution", null),
                    firewallInfo.Attributes.GetFloat("additionalDelay")
                );
            }

            var comp = new Computer(name, ip, info.Attributes.GetVector("x", "y", Vector2.Zero).Value, level, type, os)
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

            foreach (var link in info.Children.GetElement("links").Content?
                .Split((char[]) null, StringSplitOptions.RemoveEmptyEntries) ?? new string[0])
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

            #region Daemons
            
            if (info.Children.TryGetElement("daemons", out var daemons))
            {
                foreach (var daemon in daemons.Children)
                {
                    if (EventManager<SaveComponentLoadEvent>.InvokeAll(new SaveComponentLoadEvent(comp, daemon, os, ComponentType.Daemon)).Cancelled)
                        continue;
                    switch (daemon.Name)
                    {
                        case "MailServer":
                            var mailserver = new MailServer(comp, info.Attributes.GetString("name"), os);
                            if (daemon.Attributes.TryGetValue("color", out var mail_color))
                            {
                                mailserver.setThemeColor(Utils.convertStringToColor(mail_color));
                            }

                            comp.daemons.Add(mailserver);
                            break;
                        case "MissionListingServer":
                            var listing_name = daemon.Attributes.GetString("name");
                            var listing_group = daemon.Attributes.GetString("group");
                            var listing_isPublic = daemon.Attributes.GetBool("public");
                            var listing_isAssigner = daemon.Attributes.GetBool("assign");
                            var listing_articles = daemon.Attributes.GetString("articles");

                            MissionListingServer listingserver;
                            if (daemon.Attributes.TryGetValue("icon", out var listing_icon) &&
                                daemon.Attributes.TryGetValue("color", out var listing_color))
                            {
                                listingserver = new MissionListingServer(comp, listing_name, listing_icon,
                                    listing_articles,
                                    Utils.convertStringToColor(listing_color), os, listing_isPublic,
                                    listing_isAssigner);
                            }
                            else
                            {
                                listingserver = new MissionListingServer(comp, name, listing_group, os,
                                    listing_isPublic, listing_isAssigner);
                            }

                            comp.daemons.Add(listingserver);
                            break;
                        case "AddEmailServer":
                            comp.daemons.Add(new AddEmailDaemon(comp, daemon.Attributes.GetString("name"), os));
                            break;
                        case "MessageBoard":
                            var messageboard = new MessageBoardDaemon(comp, os);
                            messageboard.name = daemon.Attributes.GetString("name");
                            messageboard.BoardName =
                                daemon.Attributes.GetString("boardName", messageboard.BoardName);
                            comp.daemons.Add(messageboard);
                            break;
                        case "WebServer":
                            comp.daemons.Add(new WebServerDaemon(
                                comp,
                                daemon.Attributes.GetString("name"),
                                os,
                                daemon.Attributes.GetString("url")
                            ));
                            break;
                        case "OnlineWebServer":
                            var onlinewebserver =
                                new OnlineWebServerDaemon(comp, daemon.Attributes.GetString("name"), os);
                            onlinewebserver.setURL(daemon.Attributes.GetString("url"));
                            comp.daemons.Add(onlinewebserver);
                            break;
                        case "AcademicDatabse":
                            comp.daemons.Add(new AcademicDatabaseDaemon(
                                comp,
                                daemon.Attributes.GetString("name"),
                                os
                            ));
                            break;
                        case "MissionHubServer":
                            comp.daemons.Add(new MissionHubServer(comp, "unknown", "unknown", os));
                            break;
                        case "DeathRowDatabase":
                            comp.daemons.Add(new DeathRowDatabaseDaemon(comp, "Death Row Database", os));
                            break;
                        case "MedicalDatabase":
                            comp.daemons.Add(new MedicalDatabaseDaemon(comp, os));
                            break;
                        case "HeartMonitor":
                            var heartmon = new HeartMonitorDaemon(comp, os);
                            heartmon.PatientID = daemon.Attributes.GetString("patient", "UNKNOWN");
                            comp.daemons.Add(heartmon);
                            break;
                        case "PointClicker":
                            comp.daemons.Add(new PointClickerDaemon(comp, "Point Clicker!", os));
                            break;
                        case "ispSystem":
                            comp.daemons.Add(new ISPDaemon(comp, os));
                            break;
                        case "porthackheart":
                            comp.daemons.Add(new PorthackHeartDaemon(comp, os));
                            break;
                        case "SongChangerDaemon":
                            comp.daemons.Add(new SongChangerDaemon(comp, os));
                            break;
                        case "UploadServerDaemon":
                            var uploadserver = new UploadServerDaemon(
                                comp,
                                daemon.Attributes.GetString("name"),
                                daemon.Attributes.GetColor("color", Color.White).Value,
                                os,
                                daemon.Attributes.GetString("foldername"),
                                daemon.Attributes.GetBool("needsAuh")
                            );
                            uploadserver.hasReturnViewButton = daemon.Attributes.GetBool("hasReturnViewButton");
                            comp.daemons.Add(uploadserver);
                            break;
                        case "DHSDaemon":
                            comp.daemons.Add(new DLCHubServer(comp, "unknown", "unknown", os));
                            break;
                        case "CustomConnectDisplayDaemon":
                            comp.daemons.Add(new CustomConnectDisplayDaemon(comp, os));
                            break;
                        case "DatabaseDaemon":
                            var databaseserver = new DatabaseDaemon(
                                comp,
                                os,
                                daemon.Attributes.GetString("Name", null),
                                daemon.Attributes.GetString("Permissions"),
                                daemon.Attributes.GetString("DataType"),
                                daemon.Attributes.GetString("Foldername"),
                                daemon.Attributes.GetColor("Color")
                            );
                            databaseserver.adminResetEmailHostID = daemon.Attributes.GetString("AdminEmailHostID", null);
                            databaseserver.adminResetPassEmailAccount = daemon.Attributes.GetString("AdminEmailAccount", null);
                            comp.daemons.Add(databaseserver);
                            break;
                        case "WhitelistAuthenticatorDaemon":
                            var whitelistserver = new WhitelistConnectionDaemon(comp, os);
                            whitelistserver.AuthenticatesItself = daemon.Attributes.GetBool("SelfAuthenticating", true);
                            comp.daemons.Add(whitelistserver);
                            break;
                        case "IRCDaemon":
                            comp.daemons.Add(new IRCDaemon(comp, os, "LOAD ERROR"));
                            break;
                        case "MarkovTextDaemon":
                            comp.daemons.Add(new MarkovTextDaemon(
                                comp,
                                os,
                                daemon.Attributes.GetString("Name", null),
                                daemon.Attributes.GetString("SourceFilesContentFolder", null)
                            ));
                            break;
                        case "AircraftDaemon":
                            comp.daemons.Add(new AircraftDaemon(
                                comp,
                                os,
                                daemon.Attributes.GetString("Name", "Pacific Charter Flight"),
                                daemon.Attributes.GetVector("OriginX", "OriginY", Vector2.Zero).Value,
                                daemon.Attributes.GetVector("DestX", "DestY", Vector2.One * 0.5f).Value,
                                daemon.Attributes.GetFloat("Progress")
                            ));
                            break;
                        case "LogoCustomConnectDisplayDaemon":
                            comp.daemons.Add(new LogoCustomConnectDisplayDaemon(
                                comp,
                                os,
                                daemon.Attributes.GetString("logo", null),
                                daemon.Attributes.GetString("title", null),
                                daemon.Attributes.GetBool("overdrawLogo"),
                                daemon.Attributes.GetString("buttonAlignment", null)
                            ));
                            break;
                        case "LogoDaemon":
                            comp.daemons.Add(new LogoDaemon(
                                comp,
                                os,
                                name,
                                daemon.Attributes.GetBool("ShowsTitle", true),
                                daemon.Attributes.GetString("LogoImagePath")
                            ));
                            break;
                        case "DLCCredits":
                            DLCCreditsDaemon dlcdaemon = null;
                            string credits_title, credits_button = null;
                            if (daemon.Attributes.TryGetValue("Title", out credits_title) ||
                                daemon.Attributes.TryGetValue("Button", out credits_button))
                                dlcdaemon = new DLCCreditsDaemon(comp, os, credits_title, credits_button);
                            else
                                dlcdaemon = new DLCCreditsDaemon(comp, os);
                            dlcdaemon.ConditionalActionsToLoadOnButtonPress = daemon.Attributes.GetString("Action", null);
                            comp.daemons.Add(dlcdaemon);
                            break;
                        case "FastActionHost":
                            comp.daemons.Add(new FastActionHost(comp, os, name));
                            break;
                    }
                }
            }
            
            #endregion

            return comp;
        }
    }
}