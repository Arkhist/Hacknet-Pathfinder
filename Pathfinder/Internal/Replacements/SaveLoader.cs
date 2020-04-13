using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Hacknet;
using Hacknet.Mission;
using Hacknet.Factions;
using Hacknet.Extensions;
using Hacknet.Security;
using Microsoft.Xna.Framework;
using Pathfinder.Game;
using Pathfinder.Game.Computer;
using Pathfinder.Game.MailServer;
using Pathfinder.ModManager;
using Pathfinder.Util;
using Pathfinder.Util.XML;
using Sax.Net;
using Pathfinder.Util.Types;

namespace Pathfinder.Internal.Replacements
{
    /* Found in OS.loadSaveFile */
    public static class SaveLoader
    {

        private class LabyrinthsNotInstalledException : Exception {}

        private static Dictionary<string, Dictionary<string, ReadExecution>> ActionInject = new Dictionary<string, Dictionary<string, ReadExecution>>();
        private static Dictionary<string, Dictionary<string, ReadExecution>> ComputerActionInject = new Dictionary<string, Dictionary<string, ReadExecution>>();

        public static event OpenFile OnOpenFile;
        public static event Read OnRead;
        public static event ReadDocument OnReadDocument;
        public static event ReadComment OnReadComment;
        public static event ReadElement OnReadElement;
        public static event ReadEndElement OnReadEndElement;
        public static event ReadText OnReadText;
        public static event ReadProcessingInstructions OnReadProcessingInstructions;
        public static event CloseFile OnCloseFile;

        public static void AddEventExecutor(string elementName, ReadExecution exec)
        {
            if (!ActionInject.ContainsKey(Manager.CurrentMod.GetCleanId()))
                ActionInject.Add(Manager.CurrentMod.GetCleanId(), new Dictionary<string, ReadExecution> { [elementName] = exec });
            else if (!ActionInject[Manager.CurrentMod.GetCleanId()].ContainsKey(elementName))
                ActionInject[Manager.CurrentMod.GetCleanId()].Add(elementName, exec);
        }

        public static void AddComputerExecutor(string elementName, ReadExecution exec)
        {
            if (!ComputerActionInject.ContainsKey(Manager.CurrentMod.GetCleanId()))
                ComputerActionInject.Add(Manager.CurrentMod.GetCleanId(), new Dictionary<string, ReadExecution> { [elementName] = exec });
            else if (!ComputerActionInject[Manager.CurrentMod.GetCleanId()].ContainsKey(elementName))
                ComputerActionInject[Manager.CurrentMod.GetCleanId()].Add(elementName, exec);
        }

        public static bool RemoveEventExecutor(string elementName)
        {
            var inject = ActionInject.FirstOrDefault(i => elementName.Split('.')[0] == i.Key);
            if (inject.Value != null)
                return inject.Value.Remove(elementName.Substring(elementName.IndexOf('.') + 1));
            return ActionInject[Manager.CurrentMod.GetCleanId()].Remove(elementName);
        }

        public static bool RemoveComputerExecutor(string elementName)
        {
            var inject = ComputerActionInject.FirstOrDefault(i => elementName.Split('.')[0] == i.Key);
            if (inject.Value != null)
                return inject.Value.Remove(elementName.Substring(elementName.IndexOf('.') + 1));
            return ComputerActionInject[Manager.CurrentMod.GetCleanId()].Remove(elementName);
        }

        public static void LoadSaveFile(Stream stream, OS os)
        {
            var reader = new StreamReader(stream);
            var saveFileText = reader.ReadToEnd();

            var executor = new EventExecutor(saveFileText, isPath: false);
            var subExecutor = new ParsedTreeExecutor();

            executor.AddExecutor("HacknetSave", (exec,info) =>
            {
                /* OS.loadTitleSaveData */
                MissionGenerator.generationCount = info.Attributes.GetInt("generatedMissionCount", 100);
                os.defaultUser.name = os.username = info.Attributes.GetValue("Username");
                os.LanguageCreatedIn = info.Attributes.GetValueOrDefault("Language", "en-us");
                os.IsInDLCMode = info.Attributes.GetBool("DLCMode") && Util.Extensions.CheckLabyrinths();
                os.DisableEmailIcon = info.Attributes.GetBool("DisableMailIcon") && Util.Extensions.CheckLabyrinths();
            });

            /* OS.LoadExtraTitleSaveData / OS.ReadDLCSaveData */
            executor.AddExecutor("HacknetSave.DLC", (exec, info) =>
            {
                os.IsInDLCMode = info.Attributes.GetBool("Active");
                var hasLoadedDLC = os.HasLoadedDLCContent = info.Attributes.GetBool("LoadedContent");

                if(hasLoadedDLC && !Util.Extensions.CheckLabyrinths())
                    throw new LabyrinthsNotInstalledException();
            });

            executor.AddExecutor("HacknetSave.DLC.Flags", (exec, info) =>
                os.PreDLCFaction = info.Attributes.GetValue("OriginalFaction"));

            executor.AddExecutor("HacknetSave.DLC.OriginalVisibleNodes", (exec, info) =>
                os.PreDLCVisibleNodesCache = info.Value, true);

            executor.AddExecutor("HacknetSave.DLC.ConditionalActions", (exec, info) =>
            {
                /* TODO: Hook up ConditionalActions parser */
            }, true);

            executor.AddExecutor("HacknetSave.Flags", (exec,info) =>
            {
                /* ProgressionFlags.Load */
                var flags = os.Flags;

                flags.Flags.Clear();
                if (info.Value == null) return;
                foreach(var flag in info.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trueFlag = flag.Replace("[%%COMMAREPLACED%%]", ",");
                    // ReSharper disable StringLiteralTypo
                    if(trueFlag == "décrypté")
                        trueFlag = "decypher";
                    // ReSharper restore StringLiteralTypo
                    flags.AddFlag(trueFlag);
                }
            }, true);

            /* NetworkMap.load */

            executor.AddExecutor("HacknetSave.NetworkMap", (exec,info) =>
            {
                var sortAlgo = info.Attributes.GetValue("sort");
                if(!Enum.TryParse(sortAlgo, out os.netMap.SortingAlgorithm))
                {
                    /* TODO: Error reporting here */
                }

                subExecutor.Execute(info);

                foreach (var d in os.netMap.nodes.SelectMany(node => node.daemons))
                {
                    d.loadInit();
                }
                os.netMap.loadAssignGameNodes();
            }, true);

            executor.AddExecutor("HacknetSave.NetworkMap.visible", (exec, info) =>
            {
                var visibleNodes = info.Value;
                foreach(var node in visibleNodes.Split())
                {
                    os.netMap.visibleNodes.Add(Convert.ToInt32(node));
                }
            }, true);


            executor.AddExecutor("HacknetSave.NetworkMap.network.computer", (exec, info) =>
            {
                var computer = SaveLoader.LoadComputer(info, os);
                os.netMap.nodes.Add(computer);
            }, true);


            executor.AddExecutor("HacknetSave.mission", (exec, info) =>
            {
                os.currentMission = SaveLoader.LoadMission(info);
            }, true);

            executor.AddExecutor("HacknetSave.AllFactions", (exec, info) =>
            {
                /* AllFactions.loadFromSave */
                var loadedFactions = new AllFactions();
                string curFac = loadedFactions.currentFaction = info.Attributes.GetValue("current");
                foreach(var child in info.Children) {
                    Faction loaded = LoadFaction(child);
                    loadedFactions.factions.Add(loaded.idName, loaded);
                }

                os.allFactions = loadedFactions;
                if(curFac != "")
                    os.currentFaction = loadedFactions.factions[curFac];
            }, true);

            executor.AddExecutor("HacknetSave.other", (exec, info) =>
            {
                /* OS.loadOtherSaveData */
                MusicManager.playSongImmediatley(info.Attributes.GetValue("music"));
                os.homeNodeID = info.Attributes.GetValueOrDefault("homeNode", "entropy00");
                os.homeAssetServerID = info.Attributes.GetValueOrDefault("homeAssetsNode", "entropy01");
            }, true);

            foreach (var exec in SaveDataLoaders)
                executor.AddExecutor(exec.Key, exec.Value);

            executor.OnOpenFile += OnOpenFile;
            executor.OnRead += OnRead;
            executor.OnReadElement += OnReadElement;
            executor.OnReadEndElement += OnReadEndElement;
            executor.OnReadDocument += OnReadDocument;
            executor.OnReadComment += OnReadComment;
            executor.OnReadText += OnReadText;
            executor.OnReadProcessingInstructions += OnReadProcessingInstructions;
            executor.OnCloseFile += OnCloseFile;

            executor.OnCloseFile += _ =>
            {
                OS.WillLoadSave = false;
                os.FirstTimeStartup = false;
            };

            /* hook up child parser */
            subExecutor.delegateData = executor.delegateData;

            executor.Parse();
        }

        /* ActiveMission.load */
        private static ActiveMission LoadMission(ElementInfo info) {
            string nextMission = info.Attributes.GetValue("next");
            if(nextMission == "NULL_MISSION") {
                return null;
            }
            string goalsFile = info.Attributes.GetValue("goals");

            string genTarget = info.Attributes.GetValue("genTarget");
            if(!string.IsNullOrWhiteSpace(genTarget)) {
                MissionGenerationParser.Comp = genTarget;
                MissionGenerationParser.File = info.Attributes.GetValue("genFile");
                MissionGenerationParser.Path = info.Attributes.GetValue("genPath");
                MissionGenerationParser.Target = info.Attributes.GetValue("genTargetName");
                MissionGenerationParser.Other = info.Attributes.GetValue("genOther");
            }

            if(!Settings.IsInExtensionMode && !goalsFile.StartsWith("Content", StringComparison.Ordinal))
                goalsFile = "Content/" + goalsFile;

            ActiveMission initMission;
            try
            {
                initMission = (ActiveMission) ComputerLoader.readMission(goalsFile);
            }
            catch(Exception ex)
            {
                /* TODO: Error reporting */
                initMission = new ActiveMission(
                    new List<MisisonGoal>(),
                    "NONE",
                    new MailServer.EMailData("Unknown", "Unknown", "Unknown", new List<string>())
                );
            }

            string sender = "ERRORBOT";
            string subject = "ERROR";
            string body = "ERROR :: MAIL LOAD FAILED";

            var mailData = info.Children.FirstOrDefault(e => e.Name == "email");

            if(mailData != null) {
                sender = Folder.deFilter(mailData.Attributes.GetValue("sender"));
                subject = Folder.deFilter(mailData.Attributes.GetValue("subject"));
                body = Folder.deFilter(mailData.Value);
            }

            var loadedMission = new ActiveMission(
                initMission.goals,
                nextMission,
                new MailServer.EMailData(sender, body, subject, initMission.email.attachments)
            )
            {
                /* "activeCheck" check originally botched... twice. */
                activeCheck = info.Attributes.GetBool("activeCheck") || initMission.activeCheck,
                reloadGoalsSourceFile = goalsFile,
                requiredRank = info.Attributes.GetInt("reqRank", 0)
            };

            var endFuncData = info.Children.FirstOrDefault(e => e.Name == "endFunc");

            if(endFuncData == null)
            {
                /* TODO: Error reporting */
            }


            loadedMission.endFunctionName = endFuncData.Attributes.GetValue("name");
            loadedMission.endFunctionValue = endFuncData.Attributes.GetInt("val");

            var postingData = info.Children.FirstOrDefault(e => e.Name == "posting");

            if(postingData == null)
            {
                /* TODO: Error reporting */
            }

            loadedMission.postingTitle = Folder.deFilter(postingData.Attributes.GetValue("title"));
            loadedMission.postingBody = Folder.deFilter(postingData.Value);

            return loadedMission;
        }

        /* Computer.load */
        private static Computer LoadComputer(ElementInfo root, OS os)
        {
            /* Read top-level */

            var compName = root.Attributes.GetValue("name");
            var compIP = root.Attributes.GetValue("ip");
            var compType = root.Attributes.GetByte("type");
            var compSpec = root.Attributes.GetValue("spec");
            var compID = root.Attributes.GetValue("id");
            var compDevices = root.Attributes.GetValueOrDefault("devices", null);
            var compIcon = root.Attributes.GetValueOrDefault("icon", null);
            var compTracker = root.Attributes.GetBool("tracker");

            /* negative zeros chosen because they're unlikely to show up by accident */
            var compX = -0f;
            var compY = -0f;

            Computer result = null;
            var executor = new ParsedTreeExecutor(true);

            executor.AddExecutor("location", (exec, info) =>
            {
                compX = info.Attributes.GetFloat("x");
                compY = info.Attributes.GetFloat("y");
            });

            executor.AddExecutor("security", (exec, info) =>
            {
                var secLevel = info.Attributes.GetInt("level");

                /* HACK: This is THE EARLIEST we can construct the Computer instance */
                /* and other things rely on it being there, so we better get it done fast. */
                result = new Computer(compName, compIP, new Vector2(compX, compY), secLevel, compType, os)
                {
                    /* apply whatever else we've parsed so far */
                    idName = compID,
                    attatchedDeviceIDs = compDevices,
                    icon = compIcon,
                    HasTracker = compTracker,
                    firewall = null, /* may be overwritten later */

                    /* now apply *our* attributes */
                    traceTime = info.Attributes.GetFloat("traceTime"),
                    portsNeededForCrack = info.Attributes.GetInt("portsToCrack"),
                    adminIP = info.Attributes.GetValue("adminIP")
                };

                var proxyTime = info.Attributes.GetFloat("proxyTime");
                if(proxyTime > 0.0)
                    result.addProxy(proxyTime);
                else {
                    result.hasProxy = false;
                    result.proxyActive = false;
                }
            });

            executor.AddExecutor("admin", (exec, info) =>
            {
                Administrator admin = null;
                switch(info.Attributes.GetValue("type")) {
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
                if(admin != null) {
                    admin.ResetsPassword = info.Attributes.GetBool("resetPass");
                    admin.IsSuper = info.Attributes.GetBool("isSuper");
                }

                result.admin = admin;
            });

            executor.AddExecutor("links", (exec, info) =>
            {
                foreach(string idxStr in info.Value.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries)) {
                    result.links.Add(Convert.ToInt32(idxStr));
                }
            }, true);

            executor.AddExecutor("firewall", (exec, info) =>
                result.firewall = new Firewall(
                    info.Attributes.GetInt("complexity", 0),
                    info.Attributes.GetValueOrDefault("solution", null),
                    info.Attributes.GetFloat("additionalDelay", 0f)
                ));

            executor.AddExecutor("portsOpen", (exec, info) =>
            {
                if(info.Value.Length > 0)
                    ComputerLoader.loadPortsIntoComputer(info.Value, result);
            }, true);

            executor.AddExecutor("portRemap", (exec, info) =>
                result.PortRemapping = PortRemappingSerializer.Deserialize(info.Value), true);

            executor.AddExecutor("users", (exec, info) =>
            {
                foreach(var userData in info.Children.Where(e => e.Name == "user"))
                {
                    string userName = userData.Attributes.GetValue("name");
                    string userPass = userData.Attributes.GetValue("pass");
                    var user = new UserDetail(
                        userName,
                        userPass,
                        userData.Attributes.GetByte("type")
                    )
                    {
                        known = userData.Attributes.GetBool("known")
                    };

                    if (userName.ToLower() == "admin")
                        result.adminPass = userPass;

                    result.users.Add(user);
                }
            }, true);

            executor.AddExecutor("Memory", (exec, info) =>
                result.Memory = ReplacementsCommon.LoadMemoryContents(info), true);

            #region Daemons

            executor.AddExecutor("daemons.MailServer", (exec, info) =>
            {
                var server = result.AddDaemon<MailServer>(info.Attributes.GetValue("name"), os);
                if(info.Attributes.TryGetValue("color", out var colorStr))
                {
                    var server = result.AddDaemon<MailServer>(info.Attributes.GetValue("name"), os);
                    if (info.Attributes.TryGetValue("color", out var colorStr))
                    {
                        var color = Utility.GetColorFromString(colorStr, true, null);
                        if (color.HasValue)
                            server.setThemeColor(color.Value);
                    }
                });

                executor.AddExecutor("daemons.MissionListingServer", (exec, info) =>
                {
                    var serviceName = info.Attributes.GetValue("name");
                    var group = info.Attributes.GetValue("group");
                    var isPublic = info.Attributes.GetBool("public");
                    var isAssign = info.Attributes.GetBool("assign");
                    var title = info.Attributes.GetValueOrDefault("title", null);
                    var iconPath = info.Attributes.GetValueOrDefault("icon", null);
                    var input = info.Attributes.GetValueOrDefault("color", null);
                    var articleFolderPath = info.Attributes.GetValueOrDefault("articles", null);

                    MissionListingServer server;
                    if (iconPath == null || input == null)
                        server = result.AddDaemon<MissionListingServer>(serviceName, group, os, isPublic, isAssign);
                    else
                        server = result.AddDaemon<MissionListingServer>(
                            serviceName,
                            iconPath,
                            articleFolderPath,
                            Utils.convertStringToColor(input),
                            os,
                            isPublic,
                            isAssign
                        );
                    if (title != null)
                        server.listingTitle = title;
                });

                executor.AddExecutor("daemons.AddEmailServer", (exec, info) =>
                    result.AddDaemon<AddEmailDaemon>(info.Attributes.GetValue("name"), os));

                executor.AddExecutor("daemons.MessageBoard", (exec, info) =>
                {
                    var daemon = result.AddDaemon<MessageBoardDaemon>(os);
                    daemon.name = info.Attributes.GetValue("name");
                    if (info.Attributes.TryGetValue("boardName", out var boardName))
                    {
                        daemon.BoardName = boardName;
                    }
                });

                executor.AddExecutor("daemons.WebServer", (exec, info) =>
                    result.AddDaemon<WebServerDaemon>(info.Attributes.GetValue("name"), os, info.Attributes.GetValue("url")));

                executor.AddExecutor("daemons.OnlineWebServer", (exec, info) =>
                {
                    var daemon = result.AddDaemon<OnlineWebServerDaemon>(info.Attributes.GetValue("name"), os);
                    daemon.setURL(info.Attributes.GetValue("url"));
                });

                /* not my typo */
                executor.AddExecutor("daemons.AcademicDatabse", (exec, info) =>
                    result.AddDaemon<AcademicDatabaseDaemon>(info.Attributes.GetValue("name"), os));

                executor.AddExecutor("daemons.MissionHubServer", (exec, info) =>
                    result.AddDaemon<MissionHubServer>("unknown", "unknown", os));

                executor.AddExecutor("daemons.DeathRowDatabase", (exec, info) =>
                    result.AddDaemon<DeathRowDatabaseDaemon>("Death Row Database", os));

                executor.AddExecutor("daemons.MedicalDatabase", (exec, info) =>
                    result.AddDaemon<MedicalDatabaseDaemon>(os));

                executor.AddExecutor("daemons.HeartMonitor", (exec, info) =>
                {
                    var daemon = result.AddDaemon<HeartMonitorDaemon>(os);
                    daemon.PatientID = info.Attributes.GetValueOrDefault("patient", "UNKNOWN");
                });

                executor.AddExecutor("daemons.PointClicker", (exec, info) =>
                    result.AddDaemon<PointClickerDaemon>("Point Clicker!", os));

                executor.AddExecutor("daemons.ispSystem", (exec, info) =>
                    result.AddDaemon<ISPDaemon>(os));

                executor.AddExecutor("daemons.porthackheart", (exec, info) =>
                    result.AddDaemon<PorthackHeartDaemon>(os));

                executor.AddExecutor("daemons.SongChangerDaemon", (exec, info) =>
                    result.AddDaemon<SongChangerDaemon>(os));

                executor.AddExecutor("daemons.UploadServerDaemon", (exec, info) =>
                {
                    var daemon = result.AddDaemon<UploadServerDaemon>(
                        info.Attributes.GetValueOrDefault("name", ""),
                        Utility.GetColorFromString(info.Attributes.GetValue("color"), Color.White),
                        os,
                        info.Attributes.GetValueOrDefault("foldername", ""),
                        info.Attributes.GetBool("needsAuh")
                    );

                    daemon.hasReturnViewButton = info.Attributes.GetBool("hasReturnViewButton");
                });

                executor.AddExecutor("daemons.DHSDaemon", (exec, info) =>
                    result.AddDaemon<DLCHubServer>("unknown", "unknown", os));

                executor.AddExecutor("daemons.CustomConnectDisplayDaemon", (exec, info) =>
                    result.AddDaemon<CustomConnectDisplayDaemon>(os));

                executor.AddExecutor("daemons.DatabaseDaemon", (exec, info) =>
                {
                    var daemon = result.AddDaemon<DatabaseDaemon>(
                        os,
                        info.Attributes.GetValueOrDefault("Name", null),
                        info.Attributes.GetValueOrDefault("Permissions", null),
                        info.Attributes.GetValueOrDefault("DataType", null),
                        info.Attributes.GetValueOrDefault("Foldername", null),
                        Utility.GetColorFromString(info.Attributes.GetValueOrDefault("Color", null), true, null)
                    );

                    var AdminEmailAccount = info.Attributes.GetValue("AdminEmailAccount");

                var adminEmailAccount = info.Attributes.GetValue("AdminEmailAccount");

                if (string.IsNullOrWhiteSpace(adminEmailAccount)) return;
                daemon.adminResetPassEmailAccount = adminEmailAccount;
                daemon.adminResetEmailHostID = info.Attributes.GetValue("AdminEmailHostID");
            });

            executor.AddExecutor("daemons.WhiteListAuthenticatorDaemon", (exec, info) =>
            {
                var daemon = result.AddDaemon<WhitelistConnectionDaemon>(os);
                daemon.AuthenticatesItself = info.Attributes.GetBool("SelfAuthenticating");
            });

            executor.AddExecutor("daemons.IRCDaemon", (exec, info) =>
                result.AddDaemon<IRCDaemon>(os, "LOAD ERROR"));

                executor.AddExecutor("daemons.IRCDaemon", (exec, info) =>
                    result.AddDaemon<IRCDaemon>(os, "LOAD ERROR"));


                executor.AddExecutor("daemons.MarkovTextDaemon", (exec, info) =>
                    result.AddDaemon<MarkovTextDaemon>(
                        os,
                        info.Attributes.GetValue("Name"),
                        info.Attributes.GetValue("SourceFilesContentFolder")
                    )
                );

                executor.AddExecutor("daemons.AircraftDaemon", (exec, info) =>
                {
                    var srcVec = Vec2.Zero;
                    var dstVec = Vec2.One * 0.5f;
                    if (info.Attributes.ContainsKey("OriginX"))
                        srcVec.X = info.Attributes.GetFloat("OriginX");
                    if (info.Attributes.ContainsKey("OriginY"))
                        srcVec.Y = info.Attributes.GetFloat("OriginY");

                    if (info.Attributes.ContainsKey("DestX"))
                        dstVec.X = info.Attributes.GetFloat("DestX");
                    if (info.Attributes.ContainsKey("DestY"))
                        dstVec.Y = info.Attributes.GetFloat("DestY");

                    result.AddDaemon<AircraftDaemon>(
                        os,
                        info.Attributes.GetValueOrDefault("Name", "Pacific Charter Flight"),
                        srcVec,
                        dstVec,
                        info.Attributes.GetFloat("Progress", .5f)
                    );
                });

                executor.AddExecutor("daemons.LogoCustomConnectDisplayDaemon", (exec, info) =>
                    result.AddDaemon<LogoCustomConnectDisplayDaemon>(
                        os,
                        info.Attributes.GetValueOrDefault("logo", null),
                        info.Attributes.GetValueOrDefault("title", null),
                        info.Attributes.GetBool("overdrawLogo"),
                        info.Attributes.GetValueOrDefault("buttonAlignment", null)
                    )
                );

                executor.AddExecutor("daemons.LogoDaemon", (exec, info) =>
                {
                    var daemon = result.AddDaemon<LogoDaemon>(
                        os,
                        result.name,
                        info.Attributes.GetBool("ShowsTitle", true),
                        info.Attributes.GetValueOrDefault("LogoImagePath", null)
                    );

                    daemon.TextColor = Utility.GetColorFromString(info.Attributes.GetValue("TextColor"), Color.White);
                });

                executor.AddExecutor("daemons.DLCCredits", (exec, info) =>
                {
                    var overrideButtonText = info.Attributes.GetValueOrDefault("Button", null);
                    var overrideTitle = info.Attributes.GetValueOrDefault("Title", null);
                    DLCCreditsDaemon daemon;
                    if (overrideButtonText == null && overrideTitle == null)
                        daemon = result.AddDaemon<DLCCreditsDaemon>(os);
                    else
                        daemon = result.AddDaemon<DLCCreditsDaemon>(os, overrideTitle, overrideButtonText);
                    if (info.Attributes.TryGetValue("Action", out var buttonAction))
                        daemon.ConditionalActionsToLoadOnButtonPress = buttonAction;
                });

                executor.AddExecutor("daemon.FastActionHost", (exec, info) =>
                    result.AddDaemon<FastActionHost>(os, result.name));

            #endregion

            executor.AddExecutor("filesystem", (exec, info) => {
                var rootInfo = info.Children.FirstOrDefault(e => e.Name == "folder");
                if(rootInfo == null)
                {
                    /* TODO: Error reporting */
                }

                result.files = new FileSystem(true)
                {
                    root = LoadFolder(rootInfo)
                };
            }, true);

            foreach (var exec in ComputerDataLoaders)
                executor.AddExecutor(exec.Key, exec.Value);

            executor.Execute(root);

            if(compSpec == "mail")
                os.netMap.mailServer = result;
            if(compSpec == "player")
                os.thisComputer = result;
            return result;
        }

        /* Faction.loadFromSave */
        private static Faction LoadFaction(ElementInfo root)
        {
            Faction result;
            string factionName = root.Attributes.GetValueOrDefault("name", "UNKNOWN");
            int neededValue = root.Attributes.GetInt("neededVal", 100);

            switch(root.Name) {
                case "HubFaction":
                    result = new HubFaction(factionName, neededValue);
                    break;
                case "EntropyFaction":
                    result = new EntropyFaction(factionName, neededValue);
                    break;
                case "CustomFaction":
                    result = new CustomFaction(factionName, 100);
                    foreach(var child in root.Children.Where(e => e.Name == "Action")) {
                        var action = new CustomFactionAction();
                        action.ValueRequiredForTrigger = child.Attributes.GetInt("ValueRequired", 10);
                        action.FlagsRequiredForTrigger = child.Attributes.GetValueOrDefault("Flags", null);
                        /* TODO: Hook up Conditional Action parser, specifically the "actions" part. */
                    }
                    break;
                default:
                    result = new Faction(factionName, neededValue);
                    break;
            }

            result.playerValue = root.Attributes.GetInt("playerVal", 0);
            result.idName = root.Attributes.GetValueOrDefault("id", "");
            result.playerHasPassedValue = root.Attributes.GetBool("playerHasPassed");

            return result;
        }

        /* Folder.load */
        private static Folder LoadFolder(ElementInfo data)
        {
            var folderName = Folder.deFilter(data.Attributes.GetValue("name"));
            var result = new Folder(folderName);
            foreach(var child in data.Children) {
                switch(child.Name) {
                    case "folder":
                        result.folders.Add(LoadFolder(child));
                        break;
                    case "file":
                        bool isEduSafe = child.Attributes.GetBool("EduSafe", true);
                        if(isEduSafe || !Settings.EducationSafeBuild) {
                            result.files.Add(new FileEntry(
                                Folder.deFilter(child.Value),
                                Folder.deFilter(child.Attributes.GetValue("name"))
                             ));
                        }
                        break;
                }
            }
            return result;
        }
    }
}
