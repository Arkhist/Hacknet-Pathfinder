using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hacknet;
using Hacknet.Extensions;
using Hacknet.Security;
using Microsoft.Xna.Framework;
using Pathfinder.Game;
using Pathfinder.Game.Computer;
using Pathfinder.Game.MailServer;
using Pathfinder.ModManager;
using Pathfinder.Util;
using Sax.Net;

namespace Pathfinder.Internal
{
    /* Found in ComputerLoader.loadComputer */
    public static class ContentLoaderReplacement
    {
        public delegate void Injection(SaxProcessor.ElementInfo info, string filename, bool preventNetmapAdd, bool preventDaemonInit);

        private static Dictionary<string, Dictionary<string, Injection>> ActionInject = new Dictionary<string, Dictionary<string, Injection>>();

        public static void AddActionInjection(string id, Injection inject)
        {
            if (!ActionInject.ContainsKey(Manager.CurrentMod.GetCleanId()))
                ActionInject.Add(Manager.CurrentMod.GetCleanId(), new Dictionary<string, Injection> { [id] = inject });
            else if (!ActionInject[Manager.CurrentMod.GetCleanId()].ContainsKey(id))
                ActionInject[Manager.CurrentMod.GetCleanId()].Add(id, inject);
        }

        public static bool RemoveActionInjection(string id)
        {
            var inject = ActionInject.FirstOrDefault(i => id.Split('.')[0] == i.Key);
            if (inject.Value != null)
                return inject.Value.Remove(id.Substring(id.IndexOf('.') + 1));
            return ActionInject[Manager.CurrentMod.GetCleanId()].Remove(id);
        }

        public static Computer LoadComputer(string filename, OS os, bool preventAddingToNetmap = false, bool preventInitDaemons = false)
        {
            filename = LocalizedFileLoader.GetLocalizedFilepath(filename);
            Computer nearbyNodeOffset = null;
            Stream stream = null;
            string themeData;
            if (!filename.EndsWith("ExampleComputer.xml")) stream = File.OpenRead(filename);
            else
            {
                themeData = File.ReadAllText(filename);
                var num = themeData.IndexOf("<!--START_LABYRINTHS_ONLY_CONTENT-->");
                string str12 = "<!--END_LABYRINTHS_ONLY_CONTENT-->";
                var num1 = themeData.IndexOf(str12);
                if (num >= 0 && num1 >= 0)
                    themeData = string.Concat(themeData.Substring(0, num), themeData.Substring(num1 + str12.Length));
                stream = Utils.GenerateStreamFromString(themeData);
            }

            Computer result = null;
            var processor = new SaxProcessor();
            processor.AddActionForTag("Computer", (info) =>
            {
                var compType = info.Attributes.GetValue("type");
                nearbyNodeOffset = new Computer(
                        info.Attributes.GetValue("name")?.HacknetFilter() ?? "UNKNOWN",
                        info.Attributes.GetValue("ip")?.HacknetFilter() ?? Utility.GenerateRandomIP(),
                        os.netMap.getRandomPosition(),
                        Convert.ToInt32(info.Attributes.GetValue("security")),
                        compType?.ToLower() == "empty" ? (byte)4 : Convert.ToByte(compType),
                        os)
                {
                    idName = info.Attributes.GetValue("id") ?? "UNKNOWN",
                    AllowsDefaultBootModule = info.Attributes.GetValue("allowsDefaultBootModule")?.ToLower() != "false",
                    icon = info.Attributes.GetValue("icon")
                };
                if (nearbyNodeOffset.type == 4)
                {
                    var folder = nearbyNodeOffset.files.root.searchForFolder("home");
                    folder?.files.Clear();
                    folder?.folders.Clear();
                }
                foreach (var elementInfo in info.Elements)
                {
                    switch (elementInfo.Name.ToLower())
                    {
                        case "file":
                            var encodedFileStr = elementInfo.Attributes.GetValueOrDefault("name", "Data", true);
                            themeData = elementInfo.Value;
                            if (string.IsNullOrEmpty(themeData)) themeData = Utility.GenerateBinString();
                            themeData = themeData.HacknetFilter();
                            var folderFromPath = nearbyNodeOffset.getFolderFromPath(
                                elementInfo.Attributes.GetValueOrDefault("path", "home"), true);
                            if (info.Attributes.GetBool("EduSafe", true)
                                || !Settings.EducationSafeBuild
                                && Settings.EducationSafeBuild || !info.Attributes.GetBool("EduSafeOnly"))
                            {
                                if (folderFromPath.searchForFile(encodedFileStr) == null)
                                    folderFromPath.files.Add(new FileEntry(themeData, encodedFileStr));
                                else
                                    folderFromPath.searchForFile(encodedFileStr).data = encodedFileStr;
                            }
                            break;
                        case "encryptedfile":
                            encodedFileStr = elementInfo.Attributes.GetValueOrDefault("name", "Data", true);
                            var header = elementInfo.Attributes.GetValueOrDefault("header", "ERROR");
                            var ip = elementInfo.Attributes.GetValueOrDefault("ip", "ERROR");
                            var pass = elementInfo.Attributes.GetValueOrDefault("pass", "");
                            var extension = elementInfo.Attributes.GetValue("extension");
                            var doubleAttr = elementInfo.Attributes.GetBool("double");
                            themeData = elementInfo.Value;
                            if (string.IsNullOrEmpty(themeData)) themeData = Utility.GenerateBinString();
                            themeData = themeData.HacknetFilter();
                            if (doubleAttr)
                                themeData = FileEncrypter.EncryptString(themeData, header, ip, pass, extension);
                            themeData = FileEncrypter.EncryptString(themeData, header, ip, pass,
                                                                    (doubleAttr ? "_LAYER2.dec" : extension));
                            folderFromPath = nearbyNodeOffset.getFolderFromPath(
                                elementInfo.Attributes.GetValue("path") ?? "home", true);
                            if (folderFromPath.searchForFile(encodedFileStr) == null)
                                folderFromPath.files.Add(new FileEntry(themeData, encodedFileStr));
                            else
                                folderFromPath.searchForFile(encodedFileStr).data = themeData;
                            break;
                        case "memorydumpfile":
                            encodedFileStr = elementInfo.Attributes.GetValueOrDefault("name", "Data");
                            var memoryContent = DeserializeMemoryContent(elementInfo);
                            folderFromPath = nearbyNodeOffset.getFolderFromPath(
                                elementInfo.Attributes.GetValueOrDefault("path", "home"), true);
                            if (folderFromPath.searchForFile(encodedFileStr) == null)
                                folderFromPath.files.Add(
                                    new FileEntry(memoryContent.GetEncodedFileString(), encodedFileStr)
                                );
                            else
                                folderFromPath.searchForFile(encodedFileStr).data = memoryContent.GetEncodedFileString();
                            break;
                        case "customthemefile":
                            encodedFileStr = elementInfo.Attributes.GetValueOrDefault("name", "Data", true);
                            themeData = ThemeManager.getThemeDataStringForCustomTheme(elementInfo.Attributes.GetValue("themePath"));
                            themeData = string.IsNullOrEmpty(themeData)
                                ? "DEFINITION ERROR - Theme generated incorrectly. No Custom theme found at definition path"
                                : themeData.HacknetFilter();
                            folderFromPath = nearbyNodeOffset.getFolderFromPath(
                                elementInfo.Attributes.GetValueOrDefault("path", "home"), true);
                            if (folderFromPath.searchForFile(encodedFileStr) == null)
                                folderFromPath.files.Add(new FileEntry(themeData, encodedFileStr));
                            else
                                folderFromPath.searchForFile(encodedFileStr).data = themeData;
                            break;
                        case "ports":
                            ComputerLoader.loadPortsIntoComputer(elementInfo.Value, nearbyNodeOffset);
                            break;
                        case "positionnear":
                            var target = elementInfo.Attributes.GetValueOrDefault("target", "");
                            var position = elementInfo.Attributes.GetInt("position", 1);
                            var total = elementInfo.Attributes.GetInt("total", 3);
                            var force = elementInfo.Attributes.GetBool("force");
                            var extraDistance =
                                Math.Max(-1f, Math.Min(1f, elementInfo.Attributes.GetFloat("extraDistance")));
                            ComputerLoader.postAllLoadedActions += () =>
                            {
                                var c = Programs.getComputer(os, target);
                                if (c != null)
                                    nearbyNodeOffset.location = c.location
                                        + Corporation.getNearbyNodeOffset(
                                            c.location,
                                            position,
                                            total,
                                            os.netMap,
                                            extraDistance,
                                            force);
                            };
                            break;
                        case "proxy":
                            var time = elementInfo.Attributes.GetFloat("time", 1);
                            if (time <= 0f)
                            {
                                nearbyNodeOffset.hasProxy = false;
                                nearbyNodeOffset.proxyActive = false;
                            }
                            else
                                nearbyNodeOffset.addProxy(Computer.BASE_PROXY_TICKS * time);
                            break;
                        case "portsforcrack":
                            var val = elementInfo.Attributes.GetInt("val", -1);
                            if (val != -1)
                                nearbyNodeOffset.portsNeededForCrack = val - 1;
                            break;
                        case "firewall":
                            var level = elementInfo.Attributes.GetInt("level", 1);
                            if (level <= 0) nearbyNodeOffset.firewall = null;
                            else
                            {
                                var solution = elementInfo.Attributes.GetValue("solution");
                                if (solution == null)
                                    nearbyNodeOffset.addFirewall(level);
                                else
                                    nearbyNodeOffset.addFirewall(
                                        level, solution, elementInfo.Attributes.GetFloat("additionalTime"));
                            }
                            break;
                        case "link":
                            var linkedComp =
                                Programs.getComputer(os, elementInfo.Attributes.GetValueOrDefault("target", ""));
                            if (linkedComp != null)
                                nearbyNodeOffset.links.Add(os.netMap.nodes.IndexOf(linkedComp));
                            break;
                        case "dlink":
                            var offsetComp = nearbyNodeOffset;
                            ComputerLoader.postAllLoadedActions += () =>
                            {
                                linkedComp =
                                    Programs.getComputer(os, elementInfo.Attributes.GetValueOrDefault("target", ""));
                                if (linkedComp != null)
                                    offsetComp.links.Add(os.netMap.nodes.IndexOf(linkedComp));
                            };
                            break;
                        case "trace":
                            nearbyNodeOffset.traceTime = elementInfo.Attributes.GetFloat("time", 1);
                            break;
                        case "adminpass":
                            nearbyNodeOffset.setAdminPassword(
                                elementInfo.Attributes.GetValue("pass", true) ?? PortExploits.getRandomPassword());
                            break;
                        case "admin":
                            nearbyNodeOffset.admin = Utility.GetAdminFromString(
                                elementInfo.Attributes.GetValueOrDefault("type", "basic"),
                                elementInfo.Attributes.GetBool("resetPassword", true),
                                elementInfo.Attributes.GetBool("isSuper")
                            );
                            break;
                        case "portremap":
                            if (elementInfo.Value != null)
                                nearbyNodeOffset.PortRemapping = PortRemappingSerializer.Deserialize(elementInfo.Value);
                            break;
                        case "externalcounterpart":
                            nearbyNodeOffset.externalCounterpart =
                                                new ExternalCounterpart(elementInfo.Attributes.GetValue("name"),
                                                                        ExternalCounterpart.getIPForServerName(
                                                                            elementInfo.Attributes.GetValue("id")));
                            break;
                        case "account":
                            byte type = 3;
                            string typeStr = elementInfo.Attributes.GetValue("type").ToLower(),
                            password = elementInfo.Attributes.GetValueOrDefault("password", "ERROR", true),
                            username = elementInfo.Attributes.GetValueOrDefault("username", "ERROR", true);
                            switch (typeStr)
                            {
                                case "admin":
                                    type = 0;
                                    break;
                                case "all":
                                    type = 1;
                                    break;
                                case "mail":
                                    type = 2;
                                    break;
                                case "missionlist":
                                    type = 3;
                                    break;
                                default:
                                    type = Convert.ToByte(typeStr);
                                    break;
                            }
                            var addUser = true;
                            for (int i = 0; i < nearbyNodeOffset.users.Count; i++)
                            {
                                var userDetail = nearbyNodeOffset.users[i];
                                if (userDetail.name == username)
                                {
                                    userDetail.pass = password;
                                    userDetail.type = type;
                                    nearbyNodeOffset.users[i] = userDetail;
                                    if (username == "admin")
                                        nearbyNodeOffset.adminPass = password;
                                    addUser = false;
                                }
                            }
                            if (addUser)
                                nearbyNodeOffset.AddUserDetail(username, password, type);
                            break;
                        case "tracker":
                            nearbyNodeOffset.HasTracker = true;
                            break;
                        case "missionlistingserver":
                            nearbyNodeOffset.AddDaemon<MissionListingServer>(
                                elementInfo.Attributes.GetValueOrDefault("name", "ERROR", true),
                                elementInfo.Attributes.GetValueOrDefault("group", "ERROR", true),
                                os,
                                elementInfo.Attributes.GetBool("public"),
                                elementInfo.Attributes.GetBool("assigner"));
                            break;
                        case "variablemissionlistingserver":
                            var title = elementInfo.Attributes.GetValue("title", true);
                            var missionListingServer = nearbyNodeOffset.AddDaemon<MissionListingServer>(
                                elementInfo.Attributes.GetValue("name", true),
                                elementInfo.Attributes.GetValue("iconPath"),
                                elementInfo.Attributes.GetValue("articleFolderPath"),
                                Utility.GetColorFromString(elementInfo.Attributes.GetValue("color"), Color.IndianRed),
                                os,
                                elementInfo.Attributes.GetBool("public"),
                                elementInfo.Attributes.GetBool("assigner")
                            );
                            if (title != null) missionListingServer.listingTitle = title;
                            break;
                        case "missionhubserver":
                            var missionPath = elementInfo.Attributes.GetValueOrDefault("missionFolderPath", "").Replace('\\', '/');
                            if (!missionPath.EndsWith("/"))
                                missionPath += "/";
                            var hubServer = nearbyNodeOffset.AddDaemon<MissionHubServer>(
                                elementInfo.Attributes.GetValue("serviceName"),
                                elementInfo.Attributes.GetValueOrDefault("groupName", "", true),
                                os
                            );
                            hubServer.MissionSourceFolderPath =
                                "Content/Missions/" + (Settings.IsInExtensionMode
                                                       ? ExtensionLoader.ActiveExtensionInfo.FolderPath + "/"
                                                       : "") + missionPath;
                            hubServer.themeColor = elementInfo.Attributes.GetColor("themeColor", Color.PaleTurquoise);
                            hubServer.themeColorBackground = elementInfo.Attributes.GetColor("backgroundColor", Color.PaleTurquoise);
                            hubServer.themeColorLine = elementInfo.Attributes.GetColor("line Color", Color.PaleTurquoise);
                            hubServer.allowAbandon = elementInfo.Attributes.GetBool("allowAbandon", true);
                            break;
                        case "mailserver":
                            var mailServer = nearbyNodeOffset.AddDaemon<MailServer>(
                                elementInfo.Attributes.GetValueOrDefault("name", "Mail Server"),
                                os
                            );
                            mailServer.shouldGenerateJunkEmails = elementInfo.Attributes.GetBool("generateJunk", true);
                            var color = elementInfo.Attributes.GetColor("color", true);
                            if (color.HasValue)
                                mailServer.setThemeColor(color.Value);
                            foreach (var ininfo in elementInfo.Elements.Where((iinfo) => iinfo.Name.ToLower() == "email"))
                                mailServer.AddEmailToServer(
                                    ininfo.Attributes.GetValue("sender"),
                                    ininfo.Attributes.GetValue("recipient"),
                                    ininfo.Attributes.GetValue("subject"),
                                    ininfo.Value
                                );
                            break;
                        case "addemaildaemon":
                            nearbyNodeOffset.AddDaemon<AddEmailDaemon>("Final Task", os);
                            break;
                        case "deathrowdatabase":
                            nearbyNodeOffset.AddDaemon<DeathRowDatabaseDaemon>("Death Row Database", os);
                            break;
                        case "academicdatabase":
                            nearbyNodeOffset.AddDaemon<AcademicDatabaseDaemon>("International Academic Database", os);
                            break;
                        case "ispsystem":
                            nearbyNodeOffset.AddDaemon<ISPDaemon>(os);
                            break;
                        case "messageboard":
                            var messageBoardDaemon = nearbyNodeOffset.AddDaemon<MessageBoardDaemon>(os);
                            messageBoardDaemon.name = elementInfo.Attributes.GetValueOrDefault("name", "Anonymous");
                            messageBoardDaemon.BoardName = messageBoardDaemon.name;
                            var threadInfo =
                                elementInfo.Elements.FirstOrDefault((ininfo) => ininfo.Name.ToLower() == "thread");
                            var threadLoc = threadInfo?.Value ?? "UNKNOWN";
                            const string content = "Content/Missions/";
                            if (threadLoc?.StartsWith(content) == true)
                                threadLoc = threadLoc.Substring(content.Length);
                            if (threadLoc != null)
                                messageBoardDaemon.AddThread(Utils.readEntireFile(
                                    content + (Settings.IsInExtensionMode
                                               ? ExtensionLoader.ActiveExtensionInfo.FolderPath + "/"
                                               : "") + threadLoc));
                            break;
                        case "addavcondemoenddaemon":
                            nearbyNodeOffset.AddDaemon<AvconDemoEndDaemon>("Demo End", os);
                            break;
                        case "addwebserver":
                            nearbyNodeOffset.AddDaemon<WebServerDaemon>(
                                elementInfo.Attributes.GetValueOrDefault("name", "Web Server"),
                                os,
                                elementInfo.Attributes.GetValue("url")
                            ).registerAsDefaultBootDaemon();
                            break;
                        case "addonlinewebserver":
                            var webOnlineServerDaemon = nearbyNodeOffset.AddDaemon<OnlineWebServerDaemon>(
                                elementInfo.Attributes.GetValueOrDefault("name", "Web Server"),
                                os);
                            webOnlineServerDaemon.setURL(
                                elementInfo.Attributes.GetValueOrDefault("url", webOnlineServerDaemon.webURL));
                            webOnlineServerDaemon.registerAsDefaultBootDaemon();
                            break;
                        case "uploadserverdaemon":
                            var uploadServerDaemon = nearbyNodeOffset.AddDaemon<UploadServerDaemon>(
                                    elementInfo.Attributes.GetValueOrDefault("name", "File Upload Server"),
                                    elementInfo.Attributes.GetColor("color", new Color(0, 94, 38)),
                                    os,
                                    elementInfo.Attributes.GetValue("folder"),
                                    elementInfo.Attributes.GetBool("needsAuth")
                            );
                            uploadServerDaemon.hasReturnViewButton = elementInfo.Attributes.GetBool("hasReturnViewButton");
                            uploadServerDaemon.registerAsDefaultBootDaemon();
                            break;
                        case "medicaldatabase":
                            nearbyNodeOffset.AddDaemon<MedicalDatabaseDaemon>(os);
                            break;
                        case "heartmonitor":
                            nearbyNodeOffset.AddDaemon<HeartMonitorDaemon>(os)
                                            .PatientID = elementInfo.Attributes.GetValueOrDefault("patient", "UNKNOWN");
                            break;
                        case "pointclicker":
                            nearbyNodeOffset.AddDaemon<PointClickerDaemon>("Point Clicker!", os);
                            break;
                        case "porthackheart":
                            nearbyNodeOffset.AddDaemon<PorthackHeartDaemon>(os);
                            break;
                        case "songchangerdaemon":
                            nearbyNodeOffset.AddDaemon<SongChangerDaemon>(os);
                            break;
                        case "dhsdaemon":
                            break;
                        case "customconnectdisplaydaemon":
                            nearbyNodeOffset.AddDaemon<CustomConnectDisplayDaemon>(os);
                            break;
                        case "databasedaemon":
                            elementInfo.Name.ThrowNoLabyrinths();

                            var emailAccount = elementInfo.Attributes.GetValue("AdminEmailAccount");
                            var databaseColor = elementInfo.Attributes.GetColor("Color", true);
                            if (!databaseColor.HasValue)
                                databaseColor = os.highlightColor;

                            var databaseDaemon = nearbyNodeOffset.AddDaemon<DatabaseDaemon>(
                                os,
                                elementInfo.Attributes.GetValueOrDefault("Name", "Database"),
                                DatabaseDaemon.GetDatabasePermissionsFromString(
                                    elementInfo.Attributes.GetValueOrDefault("Permissions", "")
                                ),
                                elementInfo.Attributes.GetValue("DataType"),
                                elementInfo.Attributes.GetValueOrDefault("Foldername", "Database"),
                                databaseColor);

                            if (!string.IsNullOrWhiteSpace(emailAccount))
                            {
                                databaseDaemon.adminResetEmailHostID = elementInfo.Attributes.GetValue("AdminEmailHostID");
                                databaseDaemon.adminResetPassEmailAccount = emailAccount;
                            }
                            if (elementInfo.Elements.Count > 0)
                            {
                                var dataset = databaseDaemon.GetDataset();
                                foreach (var e in elementInfo.Elements)
                                    if (e.Name == databaseDaemon.DataTypeIdentifier)
                                        dataset.Add(new DatabaseDaemonHandler.DataInfo(e));
                            }
                            break;
                        case "whitelistauthenticatordaemon":
                            var whitelistDaemon = nearbyNodeOffset.AddDaemon<WhitelistConnectionDaemon>(os);
                            whitelistDaemon.RemoteSourceIP = elementInfo.Attributes.GetValue("Remote");
                            whitelistDaemon.AuthenticatesItself =
                                elementInfo.Attributes.GetBool("SelfAuthenticating", true);
                            break;
                        case "markovtextdaemon":
                            nearbyNodeOffset.AddDaemon<MarkovTextDaemon>(
                                os,
                                elementInfo.Attributes.GetValue("Name"),
                                elementInfo.Attributes.GetValue("SourceFilesContentFolder")
                            );
                            break;
                        case "ircdaemon":
                            var rCDaemon = nearbyNodeOffset.AddDaemon<IRCDaemon>(
                                os,
                                elementInfo.Attributes.GetValueOrDefault("Remote", "IRC Server")
                            );
                            rCDaemon.ThemeColor = elementInfo.Attributes.GetColor("themeColor", new Color(184, 2, 141));
                            rCDaemon.RequiresLogin = elementInfo.Attributes.GetBool("needsLogin");
                            foreach (var ininfo in elementInfo.Elements)
                            {
                                switch (ininfo.Name.ToLower())
                                {
                                    case "user":
                                    case "agent":
                                        var name = ininfo.Attributes.GetValue("name", true);
                                        if (!string.IsNullOrWhiteSpace(name))
                                            rCDaemon.UserColors.Add(name,
                                                                    ininfo.Attributes.GetColor("color", Color.LightGreen));
                                        break;
                                    case "post":
                                        var user = ininfo.Attributes.GetValue("user", true);
                                        if (!string.IsNullOrWhiteSpace(user))
                                            rCDaemon.StartingMessages.Add(
                                                new KeyValuePair<string, string>(user, ininfo.Value?.HacknetFilter()));
                                        break;
                                }
                            }
                            break;
                        case "aircraftdaemon":
                            elementInfo.Name.ThrowNoLabyrinths();
                            nearbyNodeOffset.AddDaemon<AircraftDaemon>(
                                os,
                                elementInfo.Attributes.GetValue("Name"),
                                elementInfo.Attributes.GetVector2(defaultVal: Vector2.Zero),
                                elementInfo.Attributes.GetVector2("Dest", defaultVal: Vector2.One * 0.5f),
                                elementInfo.Attributes.GetFloat("Progress", 0.5f)
                            );
                            break;
                        case "logocustomconnectdisplaydaemon":
                            nearbyNodeOffset.AddDaemon<LogoCustomConnectDisplayDaemon>(
                                os,
                                elementInfo.Attributes.GetValue("logo"),
                                elementInfo.Attributes.GetValue("title", true),
                                elementInfo.Attributes.GetBool("overdrawLogo"),
                                elementInfo.Attributes.GetValue("buttonAlignment")
                            );
                            break;
                        case "logodaemon":
                            var logoDaemon = nearbyNodeOffset.AddDaemon<LogoDaemon>(
                                os,
                                nearbyNodeOffset.name,
                                elementInfo.Attributes.GetBool("ShowsTitle", true),
                                elementInfo.Attributes.GetValue("LogoImagePath")
                            );
                            logoDaemon.TextColor = elementInfo.Attributes.GetColor("TextColor", Color.White);
                            logoDaemon.BodyText = elementInfo.Value;
                            break;
                        case "dlccredits":
                        case "creditsdaemon":
                            var input = new List<object>
                            {
                                os,
                                elementInfo.Attributes.GetValue("Title", true),
                                elementInfo.Attributes.GetValue("ButtonText", true)
                            };
                            input.Where((i) => i != null);
                            nearbyNodeOffset.AddDaemon<DLCCreditsDaemon>(input.ToArray())
                                            .ConditionalActionsToLoadOnButtonPress =
                                                elementInfo.Attributes.GetValue("ConditionalActionSetToRunOnButtonPressPath");
                            break;
                        case "fastactionhost":
                            nearbyNodeOffset.AddDaemon<FastActionHost>(os);
                            break;
                        case "eosdevice":
                            AddEosComputer(elementInfo, nearbyNodeOffset, os);
                            break;
                        case "memory":
                            nearbyNodeOffset.Memory = DeserializeMemoryContent(elementInfo);
                            break;
                    }
                }

                HandlerListener.DaemonLoadListener(nearbyNodeOffset, info);

                foreach (var dict in ActionInject)
                    foreach (var inject in dict.Value)
                        inject.Value(info, filename, preventAddingToNetmap, preventInitDaemons);
                if (!preventInitDaemons) nearbyNodeOffset.initDaemons();
                if (!preventAddingToNetmap) os.netMap.nodes.Add(nearbyNodeOffset);
                result = nearbyNodeOffset;
            });
            processor.Process(stream);
            return result;
        }

        public static void AddEosComputer(SaxProcessor.ElementInfo info, Computer attached, OS os)
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

            foreach (var ininfo in info.Elements)
            {
                switch (ininfo.Name.ToLower())
                {
                    case "note":
                        var val = ininfo.Value.TrimStart().HacknetFilter();
                        var filename = ininfo.Attributes.GetValue("filename", true);
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
                    case "mail":
                        var username = ininfo.Attributes.GetValue("username", true);
                        mail.files.Add(new FileEntry(
                            "MAIL ACCOUNT : " + username + "\nAccount   :" + username + "\nPassword :"
                            + ininfo.Attributes.GetValue("pass", true) + "\nLast Sync :" + DateTime.Now + "\n\n"
                            + Utility.GenerateBinString(512),
                            username + ".act"
                        ));
                        break;
                    case "file":
                        computer.getFolderFromPath(
                            ininfo.Attributes.GetValueOrDefault("path", "home"),
                            true
                        ).files.Add(
                            new FileEntry(
                                ininfo.Value?.HacknetFilter().TrimStart(),
                                ininfo.Attributes.GetValue("name"))
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

        public static MemoryContents DeserializeMemoryContent(SaxProcessor.ElementInfo memoryInfo)
        {
            if (memoryInfo == null) throw new FormatException("Unexpected end of file looking for start of Memory tag");
            var memoryContent = new MemoryContents();

            var commandListInfo = memoryInfo.Elements.FirstOrDefault((ininfo) => ininfo.Name == "Commands");
            if (commandListInfo != null)
                foreach (var commandInfo in commandListInfo.Elements)
                {
                    if (commandInfo.Name != "Command") continue;
                    var commandString = commandInfo.Value;
                    if (commandString.Contains("\n"))
                    {
                        var cmdArr = commandString.Split(Utils.robustNewlineDelim, StringSplitOptions.None);
                        foreach (var cmdStr in cmdArr)
                            memoryContent.CommandsRun.Add(Folder
                                                          .deFilter(string.IsNullOrEmpty(cmdStr) ? " " : cmdStr)
                                                          .HacknetFilter());
                    }
                    else memoryContent.CommandsRun.Add(Folder.deFilter(commandString).HacknetFilter());
                }

            var dataListInfo = memoryInfo.Elements.FirstOrDefault((ininfo) => ininfo.Name == "Data");
            if (dataListInfo != null)
                foreach (var blockInfo in dataListInfo.Elements)
                {
                    if (blockInfo.Name != "Block") continue;
                    memoryContent.DataBlocks.Add(Folder.deFilter(blockInfo.Value).HacknetFilter());
                }

            var fileFragListInfo = memoryInfo.Elements.FirstOrDefault((ininfo) => ininfo.Name == "FileFragments");
            if (fileFragListInfo != null)
                foreach (var fileInfo in fileFragListInfo.Elements)
                {
                    if (fileInfo.Name != "File") continue;
                    memoryContent.FileFragments.Add(
                    new KeyValuePair<string, string>(
                        Folder.deFilter(fileInfo.Attributes.GetValue("name") ?? "UNKNOWN"),
                        Folder.deFilter(fileInfo.Value)));
                }

            var imageListInfo = memoryInfo.Elements.FirstOrDefault((ininfo) => ininfo.Name == "Images");
            if (imageListInfo != null)
                foreach (var imageInfo in imageListInfo.Elements)
                {
                    if (imageInfo.Name != "Block") continue;
                    memoryContent.Images.Add(Folder.deFilter(imageInfo.Value));
                }

            return memoryContent;
        }
    }
}
