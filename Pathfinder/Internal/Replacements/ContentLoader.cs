using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hacknet;
using Hacknet.Extensions;
using Hacknet.Security;
using Microsoft.Xna.Framework;
using Pathfinder.Game;
using Pathfinder.Game.Computer;
using Pathfinder.Game.MailServer;
using Pathfinder.ModManager;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Internal.Replacements
{
    /* Found in ComputerLoader.loadComputer */
    public static class ContentLoader
    {
        private static readonly ModTaggedDict<string, ReadExecution> ComputerLoaders = new ModTaggedDict<string, ReadExecution>();

        public static event OpenFile OnOpenFile;
        public static event Read OnRead;
        public static event ReadDocument OnReadDocument;
        public static event ReadComment OnReadComment;
        public static event ReadElement OnReadElement;
        public static event ReadEndElement OnReadEndElement;
        public static event ReadText OnReadText;
        public static event ReadProcessingInstructions OnReadProcessingInstructions;
        public static event CloseFile OnCloseFile;

        public static bool PreventDaemonInit { get; internal set; }
        public static bool PreventNetmapAdd { get; internal set; }


        public static void AddComputerLoader(string name, ReadExecution exec, bool overrideable = false)
        { if (overrideable) ComputerLoaders.AddOverrideable(name, exec); else ComputerLoaders.Add(name, exec); }

        public static bool RemoveComputerLoader(string name)
            => ComputerLoaders.Remove(name);


        public static Computer LoadComputer(string filename, OS os, bool preventAddingToNetmap = false, bool preventInitDaemons = false)
        {
            PreventNetmapAdd = preventAddingToNetmap;
            PreventDaemonInit = preventInitDaemons;

            filename = LocalizedFileLoader.GetLocalizedFilepath(filename);
            Computer result = null;
            string themeData;

            var executor = new EventExecutor(filename, ignoreCase: true);
            var readContent = true;

            if (filename.Contains("ExampleComputer.xml") && !Util.Extensions.CheckLabyrinths())
            {
                executor.OnRead += exec =>
                {
                    var val = Regex.Replace(exec.Reader.Value.Trim().ToLower().Replace('_', ' '), @"s/\s{2,}/ /g", "");
                    if (readContent && val == "end labyrinths only content")
                        readContent = true;
                    else if (!readContent && val == "start labyrinths only content")
                        readContent = false;
                    return readContent;
                };
            }

            executor.AddExecutor("Computer", (exec, info) =>
            {
                var compType = info.Attributes.GetValue("type");
                result = new Computer(
                    info.Attributes.GetValue("name", true) ?? "UNKNOWN",
                    info.Attributes.GetValue("ip", true) ?? Utility.GenerateRandomIP(),
                    os.netMap.getRandomPosition(),
                    Convert.ToInt32(info.Attributes.GetValue("security")),
                    compType?.ToLower() == "empty" ? (byte)4 : Convert.ToByte(compType),
                    os)
                {
                    idName = info.Attributes.GetValue("id") ?? "UNKNOWN",
                    AllowsDefaultBootModule = info.Attributes.GetValue("allowsDefaultBootModule")?.ToLower() != "false",
                    icon = info.Attributes.GetValue("icon")
                };
                if (result.type != 4) return;
                var folder = result.files.root.searchForFolder("home");
                folder?.files.Clear();
                folder?.folders.Clear();
            });

            executor.AddExecutor("Computer.File", (exec, info) =>
            {
                var encodedFileStr = info.Attributes.GetValueOrDefault("name", "Data", true).HacknetFilter();
                themeData = info.Value;
                if (string.IsNullOrEmpty(themeData)) themeData = Utility.GenerateBinString();
                themeData = themeData.HacknetFilter();
                var folderFromPath = result.getFolderFromPath(
                    info.Attributes.GetValueOrDefault("path", "home"), true);
                if (!info.Attributes.GetBool("EduSafe", true) &&
                    (Settings.EducationSafeBuild || !Settings.EducationSafeBuild) &&
                    info.Attributes.GetBool("EduSafeOnly")) return;
                var file = folderFromPath.searchForFile(encodedFileStr);
                if (file == null)
                    folderFromPath.files.Add(new FileEntry(themeData, encodedFileStr));
                else
                    file.data = encodedFileStr;
            }, true);

            executor.AddExecutor("Computer.EncryptedFile", (exec, info) =>
            {
                var encodedFileStr = info.Attributes.GetValueOrDefault("name", "Data", true);
                var header = info.Attributes.GetValueOrDefault("header", "ERROR");
                var ip = info.Attributes.GetValueOrDefault("ip", "ERROR");
                var pass = info.Attributes.GetValueOrDefault("pass", "");
                var extension = info.Attributes.GetValue("extension");
                var doubleAttr = info.Attributes.GetBool("double");
                themeData = info.Value;
                if (string.IsNullOrEmpty(themeData)) themeData = Utility.GenerateBinString();
                themeData = themeData.HacknetFilter();
                if (doubleAttr)
                    themeData = FileEncrypter.EncryptString(themeData, header, ip, pass, extension);
                themeData = FileEncrypter.EncryptString(themeData, header, ip, pass,
                                                        (doubleAttr ? "_LAYER2.dec" : extension));
                var folderFromPath = result.getFolderFromPath(
                    info.Attributes.GetValue("path") ?? "home", true);
                var file = folderFromPath.searchForFile(encodedFileStr);
                if (file == null)
                    folderFromPath.files.Add(new FileEntry(themeData, encodedFileStr));
                else
                    file.data = themeData;
            }, true);


            executor.AddExecutor("Computer.MemoryDumpFile", (exec, info) =>
            {
                var memoryInfo = info.Children.FirstOrDefault(e => e.Name == "Memory");
                if(memoryInfo == null) {
                    /* TODO: error reporting */
                }
                var memoryContents = ReplacementsCommon.LoadMemoryContents(memoryInfo);
                var fileName = info.Attributes.GetValueOrDefault("name", "Data");
                var folderFromPath = result.getFolderFromPath(info.Attributes.GetValueOrDefault("path", "home"), true);
                var file = folderFromPath.searchForFile(fileName);
                if(file != null)
                    file.data = memoryContents.GetEncodedFileString();
                else
                    folderFromPath.files.Add(new FileEntry(memoryContents.GetEncodedFileString(), fileName));
            }, true);

            executor.AddExecutor("Computer.Memory", (exec, info) =>
            {
               result.Memory = ReplacementsCommon.LoadMemoryContents(info);
            }, true);

            executor.AddExecutor("Computer.CustomThemeFile", (exec, info) =>
            {
                var encodedFileStr = info.Attributes.GetValueOrDefault("name", "Data", true);
                themeData = ThemeManager.getThemeDataStringForCustomTheme(info.Attributes.GetValue("themePath"));
                themeData = string.IsNullOrEmpty(themeData)
                    ? "DEFINITION ERROR - Theme generated incorrectly. No Custom theme found at definition path"
                    : themeData.HacknetFilter();
                var folderFromPath = result.getFolderFromPath(
                    info.Attributes.GetValueOrDefault("path", "home"), true);
                var file = folderFromPath.searchForFile(encodedFileStr);
                if (file == null)
                    folderFromPath.files.Add(new FileEntry(themeData, encodedFileStr));
                else
                    file.data = themeData;
            });

            executor.AddExecutor("Computer.Ports", (exec, info) =>
                ComputerLoader.loadPortsIntoComputer(info.Value, result), true);

            executor.AddExecutor("Computer.PositionNear", (exec, info) =>
            {
                var target = info.Attributes.GetValueOrDefault("target", "");
                var position = info.Attributes.GetInt("position", 1);
                var total = info.Attributes.GetInt("total", 3);
                var force = info.Attributes.GetBool("force");
                var extraDistance = MathHelper.Clamp(info.Attributes.GetFloat("extraDistance"), -1, 1);
                ComputerLoader.postAllLoadedActions += () =>
                {
                    var c = Programs.getComputer(os, target);
                    if (c != null)
                    {
                        result.location = c.location
                                          + Corporation.getNearbyNodeOffset(
                                              c.location,
                                              position,
                                              total,
                                              os.netMap,
                                              extraDistance,
                                              force);
                    }
                };
            });

            executor.AddExecutor("Computer.Proxy",(exec, info) =>
            {
                var time = info.Attributes.GetFloat("time", 1);
                if (time <= 0f)
                {
                    result.hasProxy = false;
                    result.proxyActive = false;
                }
            });

            executor.AddExecutor("Computer.PortsForCrack", (exec, info) =>
            {
                var val = info.Attributes.GetInt("val", -1);
                if (val != -1) result.portsNeededForCrack = val - 1;
            });

            executor.AddExecutor("Computer.Firewall", (exec, info) =>
            {
                var level = info.Attributes.GetInt("level", 1);
                if (level <= 0) result.firewall = null;
                else
                {
                    var solution = info.Attributes.GetValue("solution");
                    if (solution == null) result.addFirewall(level);
                    else
                    {
                        result.addFirewall(
                            level, solution, info.Attributes.GetFloat("additionalTime"));
                    }
                }
            });

            executor.AddExecutor("Computer.Link", (exec, info) =>
            {
                var linkedComp =
                    Programs.getComputer(os, info.Attributes.GetValueOrDefault("target", ""));
                if (linkedComp != null)
                    result.links.Add(os.netMap.nodes.IndexOf(linkedComp));
            });

            executor.AddExecutor("Computer.Dlink", (exec, info) =>
            {
                /* captures for lambda */
                var offsetComp = result;
                var linkTo = info.Attributes.GetValueOrDefault("target", "");
                ComputerLoader.postAllLoadedActions += () =>
                {
                    var linkedComp =
                        Programs.getComputer(os, linkTo);
                    if (linkedComp != null)
                        offsetComp.links.Add(os.netMap.nodes.IndexOf(linkedComp));
                };
            });

            executor.AddExecutor("Computer.Trace", (exec, info) =>
                result.traceTime = info.Attributes.GetFloat("time", 1));

            executor.AddExecutor("Computer.AdminPass", (exec, info) =>
                result.setAdminPassword(
                    info.Attributes.GetValue("pass", true) ?? PortExploits.getRandomPassword()));

            executor.AddExecutor("Computer.Admin", (exec, info) =>
                result.admin = Utility.GetAdminFromString(
                    info.Attributes.GetValueOrDefault("type", "basic"),
                    info.Attributes.GetBool("resetPassword", true),
                    info.Attributes.GetBool("isSuper")
                ));

            executor.AddExecutor("Computer.PortRemap", (exec, info) =>
            {
                if (!string.IsNullOrWhiteSpace(info.Value))
                    result.PortRemapping = PortRemappingSerializer.Deserialize(info.Value);
            }, true);

            executor.AddExecutor("Computer.ExternalCounterpart", (exec, info)
                => result.externalCounterpart = new ExternalCounterpart(info.Attributes.GetValue("name"),
                    ExternalCounterpart.getIPForServerName(info.Attributes.GetValue("id"))));

            executor.AddExecutor("Computer.Account", (exec, info) =>
            {
                byte type = 0;
                string typeStr = info.Attributes.GetValueOrDefault("type", "admin").ToLower(),
                password = info.Attributes.GetValueOrDefault("password", "ERROR", true),
                username = info.Attributes.GetValueOrDefault("username", "ERROR", true);
                switch (typeStr)
                {
                    case "admin": type = 0; break;
                    case "all": type = 1; break;
                    case "mail": type = 2; break;
                    case "missionlist": type = 3; break;
                    default:
                        if(char.IsDigit(typeStr[0]))
                            byte.TryParse(typeStr, out type);
                        break;
                }
                var addUser = true;
                for (int i = 0; i < result.users.Count; i++)
                {
                    var userDetail = result.users[i];
                    if (userDetail.name == username)
                    {
                        userDetail.pass = password;
                        userDetail.type = type;
                        result.users[i] = userDetail;
                        if (username == "admin") result.adminPass = password;
                        addUser = false;
                    }
                }
                if (addUser) result.AddUserDetail(username, password, type);
            });

            executor.AddExecutor("Computer.Tracker", (exec, info) => result.HasTracker = true);

            executor.AddExecutor("Computer.MissionListingServer", (exec, info) =>
            {
                result.AddDaemon<MissionListingServer>(
                    info.Attributes.GetValueOrDefault("name", "ERROR", true),
                    info.Attributes.GetValueOrDefault("group", "ERROR", true),
                    os,
                    info.Attributes.GetBool("public"),
                    info.Attributes.GetBool("assigner"));
            });

            executor.AddExecutor("Computer.VariableMissionListingServer", (exec, info) =>
            {
                var title = info.Attributes.GetValue("title", true);
                var missionListingServer = result.AddDaemon<MissionListingServer>(
                    info.Attributes.GetValue("name", true),
                    info.Attributes.GetValue("iconPath"),
                    info.Attributes.GetValue("articleFolderPath"),
                    Utility.GetColorFromString(info.Attributes.GetValue("color"), Color.IndianRed),
                    os,
                    info.Attributes.GetBool("public"),
                    info.Attributes.GetBool("assigner")
                );
                if (title != null) missionListingServer.listingTitle = title;
            });

            executor.AddExecutor("Computer.MissionHubServer", (exec, info) =>
            {
                var missionPath = info.Attributes.GetValueOrDefault("missionFolderPath", "").Replace('\\', '/');
                if (!missionPath.EndsWith("/", StringComparison.InvariantCulture))
                    missionPath += "/";
                var hubServer = result.AddDaemon<MissionHubServer>(
                    info.Attributes.GetValue("serviceName"),
                    info.Attributes.GetValueOrDefault("groupName", "", true),
                    os
                );
                hubServer.MissionSourceFolderPath =
                    (Settings.IsInExtensionMode
                        ? ExtensionLoader.ActiveExtensionInfo.FolderPath + "/"
                        : "Content/Missions/") + missionPath;
                hubServer.themeColor = info.Attributes.GetColor("themeColor", Color.PaleTurquoise);
                hubServer.themeColorBackground = info.Attributes.GetColor("backgroundColor", Color.PaleTurquoise);
                hubServer.themeColorLine = info.Attributes.GetColor("line Color", Color.PaleTurquoise);
                hubServer.allowAbandon = info.Attributes.GetBool("allowAbandon", true);
            });

            executor.AddExecutor("Computer.MailServer", (exec, info) =>
            {
                var mailServer = result.AddDaemon<MailServer>(
                    info.Attributes.GetValueOrDefault("name", "Mail Server"),
                    os);
                mailServer.shouldGenerateJunkEmails = info.Attributes.GetBool("generateJunk", true);
                var color = info.Attributes.GetColor("color", true);
                if (color.HasValue) mailServer.setThemeColor(color.Value);
                foreach (var emailInfo in info.Children.Where((i) => i.Name.ToLower() == "email"))
                {
                    mailServer.AddEmailToServer(
                        emailInfo.Attributes.GetValue("sender"),
                        emailInfo.Attributes.GetValue("recipient"),
                        emailInfo.Attributes.GetValue("subject"),
                        emailInfo.Value
                    );
                }
            }, true);

            executor.AddExecutor("Computer.AddEmailDaemon", (exec, info) 
                => result.AddDaemon<AddEmailDaemon>("Final Task", os));

            executor.AddExecutor("Computer.DeathRowDatabase", (exec, info)
                => result.AddDaemon<DeathRowDatabaseDaemon>("Death Row Database", os));

            executor.AddExecutor("Computer.AcademicDatabase", (exec, info)
                => result.AddDaemon<AcademicDatabaseDaemon>("International Academic Database", os));

            executor.AddExecutor("Computer.IspSystem", (exec, info) 
                => result.AddDaemon<ISPDaemon>(os));

            executor.AddExecutor("Computer.MessageBoard", (exec, info) =>
            {
                var messageBoardDaemon = result.AddDaemon<MessageBoardDaemon>(os);
                messageBoardDaemon.name = info.Attributes.GetValueOrDefault("name", "Anonymous");
                messageBoardDaemon.BoardName = messageBoardDaemon.name;
                const string content = "Content/Missions/";
                foreach (var threadInfo in info.Children.Where((cinfo) => cinfo.Name.ToLower() == "thread")) {
                    var threadLoc = threadInfo.Value ?? "UNKNOWN";
                    if (threadLoc.StartsWith(content, StringComparison.InvariantCulture))
                        threadLoc = threadLoc.Substring(content.Length);
                    messageBoardDaemon.AddThread(Utils.readEntireFile(
                        (Settings.IsInExtensionMode ?
                            ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" :
                            content
                        ) + threadLoc
                    ));
                }
            }, true);

            executor.AddExecutor("Computer.AddAvconDemoEndDaemon", (exec, info) 
                => result.AddDaemon<AvconDemoEndDaemon>("Demo End", os));

            executor.AddExecutor("Computer.AddWebServer", (exec, info) =>
                result.AddDaemon<WebServerDaemon>(
                    info.Attributes.GetValueOrDefault("name", "Web Server"),
                    os,
                    info.Attributes.GetValue("url")
                ).registerAsDefaultBootDaemon());

            executor.AddExecutor("Computer.AddOnlineWebServer", (exec, info) =>
            {
                var webOnlineServerDaemon = result.AddDaemon<OnlineWebServerDaemon>(
                    info.Attributes.GetValueOrDefault("name", "Web Server"),
                    os);
                webOnlineServerDaemon.setURL(
                    info.Attributes.GetValueOrDefault("url", webOnlineServerDaemon.webURL));
                webOnlineServerDaemon.registerAsDefaultBootDaemon();
            });


            executor.AddExecutor("Computer.UploadServerDaemon", (exec, info) =>
            {
                var uploadServerDaemon = result.AddDaemon<UploadServerDaemon>(
                                    info.Attributes.GetValueOrDefault("name", "File Upload Server"),
                                    info.Attributes.GetColor("color", new Color(0, 94, 38)),
                                    os,
                                    info.Attributes.GetValue("folder"),
                                    info.Attributes.GetBool("needsAuth")
                            );
                uploadServerDaemon.hasReturnViewButton = info.Attributes.GetBool("hasReturnViewButton");
                uploadServerDaemon.registerAsDefaultBootDaemon();
            });

            executor.AddExecutor("Computer.MedicalDatabase", (exec, info)
                => result.AddDaemon<MedicalDatabaseDaemon>(os));

            executor.AddExecutor("Computer.HeartMonitor", (exec, info)
                => result.AddDaemon<HeartMonitorDaemon>(os)
                    .PatientID = info.Attributes.GetValueOrDefault("patient", "UNKNOWN"));

            executor.AddExecutor("Computer.PointClicker", (exec, info)
                => result.AddDaemon<PointClickerDaemon>("Point Clicker!", os));

            executor.AddExecutor("Computer.PorthackHeart", (exec, info)
                => result.AddDaemon<PorthackHeartDaemon>(os));

            executor.AddExecutor("Computer.SongChangerDaemon", (exec, info)
                => result.AddDaemon<SongChangerDaemon>(os));

            executor.AddExecutor("Computer.CustomConnectDisplayDaemon", (exec, info)
                => result.AddDaemon<CustomConnectDisplayDaemon>(os));

            executor.AddExecutor("Computer.DatabaseDaemon", (exec, info) =>
            {
                info.Name.ThrowNoLabyrinths();
                var emailAccount = info.Attributes.GetValue("AdminEmailAccount");

                var databaseColor = info.Attributes.GetColor("Color", true);
                if (!databaseColor.HasValue)
                    databaseColor = os.highlightColor;

                var databaseDaemon = result.AddDaemon<DatabaseDaemon>(
                    os,
                    info.Attributes.GetValueOrDefault("Name", "Database"),
                    DatabaseDaemon.GetDatabasePermissionsFromString(
                        info.Attributes.GetValueOrDefault("Permissions", "")
                    ),
                    info.Attributes.GetValue("DataType"),
                    info.Attributes.GetValueOrDefault("Foldername", "Database"),
                    databaseColor);
                if (!string.IsNullOrWhiteSpace(emailAccount))
                {
                    databaseDaemon.adminResetEmailHostID = info.Attributes.GetValue("AdminEmailHostID");
                    databaseDaemon.adminResetPassEmailAccount = emailAccount;
                }
                if (info.Children.Count > 0)
                {
                    var dataset = databaseDaemon.GetDataset();
                    foreach (var e in info.Children)
                        if (e.Name == databaseDaemon.DataTypeIdentifier)
                            dataset.Add(new DatabaseDaemonHandler.DataInfo(e));
                }
            }, true);



            executor.AddExecutor("Computer.WhitelistAuthenticatorDaemon", (exec, info)
                => result.AddDaemon(
                    new WhitelistConnectionDaemon(result, os)
                    {
                        RemoteSourceIP = info.Attributes.GetValue("Remote"),
                        AuthenticatesItself = info.Attributes.GetBool("SelfAuthenticating", true)
                    }));


            executor.AddExecutor("Computer.MarkovTextDaemon", (exec, info)
                => result.AddDaemon<MarkovTextDaemon>(
                    os,
                    info.Attributes.GetValue("Name"),
                    info.Attributes.GetValue("SourceFilesContentFolder")
            ));

            executor.AddExecutor("Computer.IrcDaemon", (exec, info) =>
            {
                var rCDaemon = result.AddDaemon<IRCDaemon>(
                    os,
                    info.Attributes.GetValueOrDefault("Remote", "IRC Server")
                );
                rCDaemon.ThemeColor = info.Attributes.GetColor("themeColor", new Color(184, 2, 141));
                rCDaemon.RequiresLogin = info.Attributes.GetBool("needsLogin");
                foreach (var cinfo in info.Children)
                {
                    switch (cinfo.Name.ToLower())
                    {
                        case "user":
                        case "agent":
                            var name = cinfo.Attributes.GetValue("name", true);
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                rCDaemon.UserColors.Add(name,
                                    cinfo.Attributes.GetColor("color", Color.LightGreen));
                            }

                            break;
                        case "post":
                            var user = cinfo.Attributes.GetValue("user", true);
                            if (!string.IsNullOrWhiteSpace(user))
                            {
                                rCDaemon.StartingMessages.Add(
                                    new KeyValuePair<string, string>(user, cinfo.Value?.HacknetFilter()));
                            }

                            break;
                    }
                }
            }, true);

            executor.AddExecutor("Computer.AircraftDaemon", (exec, info) =>
            {
                info.Name.ThrowNoLabyrinths();
                result.AddDaemon<AircraftDaemon>(
                    os,
                    info.Attributes.GetValue("Name"),
                    info.Attributes.GetVector2(defaultVal: Vector2.Zero),
                    info.Attributes.GetVector2("Dest", defaultVal: Vector2.One * 0.5f),
                    info.Attributes.GetFloat("Progress", 0.5f)
                );
            });

            executor.AddExecutor("Computer.LogoCustomConnectDisplayDaemon", (exec, info)
                => result.AddDaemon<LogoCustomConnectDisplayDaemon>(
                    os,
                    info.Attributes.GetValue("logo"),
                    info.Attributes.GetValue("title", true),
                    info.Attributes.GetBool("overdrawLogo"),
                    info.Attributes.GetValue("buttonAlignment")
            ));

            executor.AddExecutor("Computer.LogoDaemon", (exec, info)
                => result.AddDaemon(
                    new LogoDaemon(
                        result,
                        os,
                        result.name,
                        info.Attributes.GetBool("ShowsTitle", true),
                        info.Attributes.GetValue("LogoImagePath"))
                    {
                        TextColor = info.Attributes.GetColor("TextColor", Color.White),
                        BodyText = info.Value
                    }
            ), true);

            executor.AddExecutor("Computer.DHSDaemon", (exec, info) =>
            {
                info.Name.ThrowNoLabyrinths();

                var groupName = info.Attributes.GetValueOrDefault("groupName", "UNKNOWN");
                var addsFactionPoint = info.Attributes.GetBool("addsFactionPointOnMissionComplete", true);
                var autoClearMissions = info.Attributes.GetBool("autoClearMissionsOnPlayerComplete", true);
                var allowContractAbbandon = info.Attributes.GetBool("allowContractAbbandon");
                var themeColor = info.Attributes.GetColor("themeColor", new Color(38, 201, 155));

                var dlcHubServer = result.AddDaemon<DLCHubServer>("DHS", groupName, os);
                dlcHubServer.AddsFactionPointForMissionCompleteion = addsFactionPoint;
                dlcHubServer.AutoClearMissionsOnSingleComplete = autoClearMissions;
                dlcHubServer.AllowContractAbbandon = allowContractAbbandon;
                dlcHubServer.themeColor = themeColor;

                foreach (var cinfo in info.Children.Where(v =>
                    v.Name.ToLower() == "user" || v.Name.ToLower() == "agent")
                )
                {
                    var name = cinfo.Attributes.GetValue("name", true);
                    var password = cinfo.Attributes.GetValueOrDefault("pass", "password");
                    var color = cinfo.Attributes.GetColor("color", Color.LightGreen);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        dlcHubServer.AddAgent(name, password, color);
                    }
                }
            }, true);

            void creditFunc(IExecutor exec, ElementInfo info)
            {
                var inputArr = new List<object>
                {
                    os,
                    info.Attributes.GetValue("Title", true),
                    info.Attributes.GetValue("ButtonText", true)
                }.Where((i) => i != null).ToArray();
                result.AddDaemon<DLCCreditsDaemon>(inputArr.ToArray())
                    .ConditionalActionsToLoadOnButtonPress =
                        info.Attributes.GetValue("ConditionalActionSetToRunOnButtonPressPath");
            }

            executor.AddExecutor("Computer.DLCCredits", creditFunc);
            executor.AddExecutor("Computer.CreditsDaemon", creditFunc);

            executor.AddExecutor("Computer.FastActionHost", (exec, info)
                => result.AddDaemon<FastActionHost>(os, result.name));

            executor.AddExecutor("Computer.eosDevice", (exec, info)
                => AddEosComputer(info, result, os), true);

            HandlerListener.DaemonLoadListener(result, executor);

            foreach (var exec in ComputerLoaders)
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

            executor.OnCloseFile += reader =>
            {
                if (!preventInitDaemons) result.initDaemons();
                if (!preventAddingToNetmap) os.netMap.nodes.Add(result);
            };
            
            executor.Parse();
            return result;
        }

        public static void AddEosComputer(ElementInfo info, Computer attached, OS os)
        {
            var empty = info.Attributes.GetBool("empty");
            var computer = new Computer(
                info.Attributes.GetValueOrDefault("name", "Unregistered eOS Device", true),
                Utility.GenerateRandomIP(),
                os.netMap.getRandomPosition(),
                0,
                5,
                os
            )
            {
                idName = info.Attributes.GetValueOrDefault("id", attached.idName + "_eos"),
                icon = info.Attributes.GetValueOrDefault("icon", "ePhone"),
                location = attached.location + Corporation.getNearbyNodeOffset(attached.location, Utility.Random.Next(12), 12, os.netMap),
                portsNeededForCrack = 2
            };
            computer.setAdminPassword(info.Attributes.GetValueOrDefault("passOverride", "alpine"));
            ComputerLoader.loadPortsIntoComputer("22,3659", computer);
            EOSComp.GenerateEOSFilesystem(computer);
            var eos = computer.files.root.searchForFolder("eos");
            var notes = eos.searchForFolder("notes");
            var mail = eos.searchForFolder("mail");

            foreach (var cinfo in info.Children)
            {
                switch (cinfo.Name)
                {
                    case "Note":
                        var val = cinfo.Value.TrimStart().HacknetFilter();
                        var filename = cinfo.Attributes.GetValue("filename", true);
                        if (filename == null)
                        {
                            var length = val.IndexOf('\n');
                            if (length == -1) length = val.Length;
                            filename = val.Substring(0, length);
                            if (filename.Length > 50) filename = filename.Substring(0, 47) + "...";
                            filename = filename.Replace(" ", "_").Replace(":", "").ToLower().Trim() + ".txt";
                        }
                        notes.files.Add(new FileEntry(val, filename));
                        break;
                    case "Mail":
                        var username = cinfo.Attributes.GetValue("username", true);
                        mail.files.Add(new FileEntry(
                            "MAIL ACCOUNT : " + username + "\nAccount   :" + username + "\nPassword :"
                            + cinfo.Attributes.GetValue("pass", true) + "\nLast Sync :" + DateTime.Now + "\n\n"
                            + Utility.GenerateBinString(512),
                            username + ".act"
                        ));
                        break;
                    case "File":
                        computer.getFolderFromPath(
                            cinfo.Attributes.GetValueOrDefault("path", "home"),
                            true
                        ).files.Add(
                            new FileEntry(
                                cinfo.Value?.HacknetFilter().TrimStart(),
                                cinfo.Attributes.GetValue("name"))
                        );
                        break;
                }
            }
            if (empty)
            {
                var folder3 = eos.searchForFolder("apps");
                if (folder3 != null)
                {
                    folder3.files.Clear();
                    folder3.folders.Clear();
                }
            }
            os.netMap.nodes.Add(computer);
            ComputerLoader.postAllLoadedActions += () => computer.links.Add(os.netMap.nodes.IndexOf(attached));
            if (attached.attatchedDeviceIDs != null)
                attached.attatchedDeviceIDs += ",";
            attached.attatchedDeviceIDs += computer.idName;
        }

        public static ReadExecution GenerateCommandListHandler(MemoryContents memory)
            => (exec, info) =>
            {
                foreach (var commandInfo in info.Children)
                {
                    if (commandInfo.Name != "Command") continue;
                    var commandString = commandInfo.Value;
                    if (commandString.Contains("\n"))
                    {
                        var cmdArr = commandString.Split(Utils.robustNewlineDelim, StringSplitOptions.None);
                        foreach (var cmdStr in cmdArr)
                        {
                            memory.CommandsRun.Add(Folder
                                .deFilter(string.IsNullOrEmpty(cmdStr) ? " " : cmdStr)
                                .HacknetFilter());
                        }
                    }
                    else memory.CommandsRun.Add(Folder.deFilter(commandString).HacknetFilter());
                }
            };
    }
}
