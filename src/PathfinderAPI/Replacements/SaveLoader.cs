using System.Reflection;
using Hacknet;
using Hacknet.Factions;
using Hacknet.Localization;
using Hacknet.PlatformAPI.Storage;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.Event.Loading;
using Pathfinder.Port;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements;

[HarmonyPatch]
public static class SaveLoader
{
    public abstract class SaveExecutor
    {
        public OS Os { get; private set; }

        public virtual void Init(OS os)
        {
            Os = os;
        }

        public abstract void Execute(EventExecutor exec, ElementInfo info);
    }

    private struct SaveExecutorHolder
    {
        public string Element;
        public Type ExecutorType;
        public ParseOption Options;
    }
        
    private static readonly List<SaveExecutorHolder> CustomExecutors = [];

    public static void RegisterExecutor<T>(string element, ParseOption options = ParseOption.None) where T : SaveExecutor, new() => RegisterExecutor(typeof(T), element, options);
    public static void RegisterExecutor(Type executorType, string element, ParseOption options = ParseOption.None)
    {
        executorType.ThrowNotInherit<SaveExecutor>(nameof(executorType));
        if(!executorType.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0))
            throw new ArgumentException("Type of executor registered must have a default constructor", nameof(executorType));

        CustomExecutors.Add(new SaveExecutorHolder
        {
            Element = element,
            ExecutorType = executorType,
            Options = options
        });
    }

    public static void UnregisterExecutor<T>() where T : SaveExecutor, new()
    {
        var tType = typeof(T);
        CustomExecutors.RemoveAll(x => x.ExecutorType == tType);
    }

    private static void OnPluginUnload(Assembly pluginAsm)
    {
        CustomExecutors.RemoveAll(x => x.ExecutorType.Assembly == pluginAsm);
    }

    private static EventExecutor executor = new EventExecutor();
    private static OS os = null;

    static SaveLoader()
    {
        EventManager.onPluginUnload += OnPluginUnload;
            
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
        executor.RegisterExecutor("HacknetSave.DLC.Flags",
            (exec, info) => os.PreDLCFaction = info.Attributes.GetString("OriginalFaction"));
        executor.RegisterExecutor("HacknetSave.DLC.OriginalVisibleNodes",
            (exec, info) => os.PreDLCVisibleNodesCache = info.Content ?? "",
            ParseOption.ParseInterior);
        executor.RegisterExecutor("HacknetSave.DLC.ConditionalActions",
            (exec, info) => os.ConditionalActions = ActionsLoader.LoadActionSets(info), 
            ParseOption.ParseInterior);
        executor.RegisterExecutor("HacknetSave.Flags", (exec, info) =>
        {
            os.Flags.Flags.Clear();

            foreach (var flag in info.Content?.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0])
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
        }, ParseOption.FireOnEnd);
        executor.RegisterExecutor("HacknetSave.NetworkMap.network.computer", (exec, info) =>
            {
                var comp = LoadComputer(info, os);
                if (comp != null)
                    os.netMap.nodes.Add(comp);
            },
            ParseOption.ParseInterior);
        executor.RegisterExecutor("HacknetSave.NetworkMap.visible", (exec, info) =>
        {
            foreach (var node in info.Content.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries))
            {
                os.netMap.visibleNodes.Add(int.Parse(node));
            }
        }, ParseOption.ParseInterior);
        executor.RegisterExecutor("HacknetSave.mission", (exec, info) => os.currentMission = LoadMission(info));
        executor.RegisterExecutor("HacknetSave.AllFactions", (exec, info) =>
        {
            os.allFactions = new AllFactions
            {
                currentFaction = info.Attributes.GetString("current", null)
            };

            foreach (var factionInfo in info.Children)
            {
                var faction = ReplacementsCommon.LoadFaction(factionInfo);
                os.allFactions.factions.Add(faction.idName, faction);
            }

            if (os.allFactions.currentFaction != null)
                os.allFactions.setCurrentFaction(os.allFactions.currentFaction, os);
        }, ParseOption.ParseInterior);
        executor.RegisterExecutor("HacknetSave.other", (exec, info) =>
        {
            MusicManager.playSongImmediatley(info.Attributes.GetString("music", null));
            os.homeNodeID = info.Attributes.GetString("homeNode", os.homeNodeID);
            os.homeAssetServerID = info.Attributes.GetString("homeAssetsNode", os.homeAssetServerID);
        });
    }
        
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

        os = __instance;

        executor.SetText(new StreamReader(saveStream).ReadToEnd(), false);

        foreach (var custom in CustomExecutors)
        {
            var customInstance = (SaveExecutor)Activator.CreateInstance(custom.ExecutorType);
            customInstance.Init(os);
            executor.RegisterTempExecutor(custom.Element, customInstance.Execute, custom.Options);
        }

        executor.Parse();

        return false;
    }

    public static Computer LoadComputer(ElementInfo info, OS os)
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

        ReplacementsCommon.isPathfinderComputer = true;
        var comp = new Computer(name, ip, location.Attributes.GetVector("x", "y", Vector2.Zero).Value, level, type, os)
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
        ReplacementsCommon.isPathfinderComputer = false;

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

        if (info.Children.TryGetElement("admin", out var adminInfo))
        {
            Administrator.AdministratorManager.LoadAdministrator(adminInfo, comp, os);
        }

        foreach (var link in info.Children.GetElement("links").Content?
                     .Split((char[]) null, StringSplitOptions.RemoveEmptyEntries) ?? new string[0])
        {
            comp.links.Add(int.Parse(link));
        }

        if (info.Children.TryGetElement("portsOpen", out var portsOpen))
            PortManager.LoadPortsFromStringVanilla(comp, portsOpen.Content);

        if (info.Children.TryGetElement("portRemap", out var remap))
            PortManager.LoadPortRemapsFromStringVanilla(comp, remap.Content);

        if (info.Children.TryGetElement("ports", out var ports))
        {
            if (ports.Content != null)
                foreach (var port in ports.Content.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = port.Split(':');
                    var record = PortManager.GetPortRecordFromProtocol(parts[0]);
                    var displayName = parts[3].Replace('_', ' ');
                    var portNum = int.Parse(parts[2]);
                    if(record == null)
                    {
                        PortManager.RegisterPort(record = new PortRecord(
                            parts[0],
                            displayName,
                            portNum,
                            int.Parse(parts[1])
                        ));
                    }

                    comp.AddPort(record.CreateState(
                        comp,
                        displayName,
                        portNum
                    ));
                }

            foreach (var port in ports.Children)
            {
                var record = PortManager.GetPortRecordFromProtocol(port.Name);
                var displayName = port.Children.GetElement("Display").Content;
                var portNum = int.Parse(port.Children.GetElement("Number").Content);
                if(record == null)
                {
                    PortManager.RegisterPort(record = new PortRecord(
                        port.Name,
                        displayName,
                        portNum,
                        int.Parse(port.Children.GetElement("Original").Content)
                    ));
                }
                comp.AddPort(record.CreateState(
                    comp,
                    displayName,
                    portNum
                ));
            }
        }

        comp.users.Clear();
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
                if (Daemon.DaemonManager.TryLoadCustomDaemon(daemon, comp, os))
                    continue;
                    
                switch (daemon.Name)
                {
                    case "MailServer":
                        var mailserver = new MailServer(comp, daemon.Attributes.GetString("name"), os);
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
                        )
                        {
                            TextColor = daemon.Attributes.GetColor("TextColor", Color.White).Value
                        });
                        break;
                    case "DLCCredits":
                        DLCCreditsDaemon dlcdaemon = null;
                        if (daemon.Attributes.TryGetValue("Title", out var credits_title) |
                            daemon.Attributes.TryGetValue("Button", out var credits_button))
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
            
        comp.files = new FileSystem(true)
        {
            root = LoadFolder(info.Children.GetElement("filesystem").Children.GetElement("folder"))
        };

        switch (spec)
        {
            case "player":
                os.thisComputer = comp;
                break;
            case "mail":
                os.netMap.mailServer = comp;
                break;
            case "academic":
                os.netMap.academicDatabase = comp;
                break;
        }

        if (EventManager<SaveComputerLoadedEvent>.InvokeAll(new SaveComputerLoadedEvent(os, comp, info)).Cancelled)
        {
            return null;
        }
            
        ComputerLookup.Add(comp);
        return comp;
    }

    public static Folder LoadFolder(ElementInfo info)
    {
        var result = new Folder(info.Attributes.GetString("name"));
        foreach (var child in info.Children)
        {
            switch (child.Name)
            {
                case "folder":
                    result.folders.Add(LoadFolder(child));
                    break;
                case "file":
                    if (child.Attributes.GetBool("EduSafe", true) || !Settings.EducationSafeBuild)
                    {
                        result.files.Add(new FileEntry(child.Content, child.Attributes.GetString("name")));
                    }
                    break;
            }
        }

        return result;
    }

    public static ActiveMission LoadMission(ElementInfo root)
    {
        var next = root.Attributes.GetString("next", null);
        if (next == "NULL_MISSION")
            return null;

        if (root.Attributes.TryGetValue("genTarget", out MissionGenerationParser.Comp))
        {
            MissionGenerationParser.File = root.Attributes.GetString("genFile", null);
            MissionGenerationParser.Path = root.Attributes.GetString("genPath", null);
            MissionGenerationParser.Target = root.Attributes.GetString("genTargetName", null);
            MissionGenerationParser.Other = root.Attributes.GetString("genOther", null);
        }
            
        var goalsFile = root.Attributes.GetOrThrow("goals", "Invalid goals file for active mission", PFStringExtensions.ContentFileExists).ContentFilePath();
        return MissionLoader.LoadContentMission(goalsFile);
    }
}
