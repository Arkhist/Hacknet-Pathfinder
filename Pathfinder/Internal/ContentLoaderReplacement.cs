using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Hacknet;
using Hacknet.Security;
using Microsoft.Xna.Framework;
using Pathfinder.Util;

namespace Pathfinder.Internal
{
    public static class ContentLoaderReplacement
    {
        public static Computer loadComputer(string filename, OS os, bool preventAddingToNetmap = false, bool preventInitDaemons = false)
        {
            filename = LocalizedFileLoader.GetLocalizedFilepath(filename);
            Computer nearbyNodeOffset = null;
            Stream stream = null;
            XmlReader xmlReader = null;
            string themeData;
            if (!filename.EndsWith("ExampleComputer.xml")) stream = File.OpenRead(filename);
            else
            {
                themeData = File.ReadAllText(filename);
                int num = themeData.IndexOf("<!--START_LABYRINTHS_ONLY_CONTENT-->");
                string str12 = "<!--END_LABYRINTHS_ONLY_CONTENT-->";
                int num1 = themeData.IndexOf(str12);
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
                    ComputerLoader.filter(info.Attributes.GetValue("name") ?? "UNKNOWN"),
                    ComputerLoader.filter(info.Attributes.GetValue("ip") ?? Utility.GenerateRandomIP()),
                    ComputerLoader.os.netMap.getRandomPosition(),
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
                            var encodedFileStr = ComputerLoader.filter(elementInfo.Attributes.GetValue("name") ?? "Data");
                            var eduSafe = info.Attributes.GetValue("EduSafe")?.ToLower() != "false";
                            var eduSafeOnly = info.Attributes.GetValue("EduSafeOnly")?.ToLower() == "true";
                            themeData = elementInfo.Value;
                            if (string.IsNullOrEmpty(themeData)) themeData = Computer.generateBinaryString(500);
                            themeData = ComputerLoader.filter(themeData);
                            var folderFromPath = nearbyNodeOffset.getFolderFromPath(
                                elementInfo.Attributes.GetValue("path") ?? "home", true);
                            if (info.Attributes.GetValue("EduSafe")?.ToLower() != "false"
                                || !Settings.EducationSafeBuild
                                && Settings.EducationSafeBuild || !eduSafeOnly)
                            {
                                if (folderFromPath.searchForFile(encodedFileStr) == null)
                                    folderFromPath.files.Add(new FileEntry(themeData, encodedFileStr));
                                else
                                    folderFromPath.searchForFile(encodedFileStr).data = encodedFileStr;
                            }
                            break;
                        case "encryptedfile":
                            encodedFileStr = ComputerLoader.filter(elementInfo.Attributes.GetValue("name") ?? "Data");
                            var header = elementInfo.Attributes.GetValue("header") ?? "ERROR";
                            var ip = elementInfo.Attributes.GetValue("ip") ?? "ERROR";
                            var pass = elementInfo.Attributes.GetValue("pass") ?? "";
                            var extension = elementInfo.Attributes.GetValue("extension");
                            var doubleAttr = elementInfo.Attributes.GetValue("double")?.ToLower() == "true";
                            themeData = elementInfo.Value;
                            if (string.IsNullOrEmpty(themeData)) themeData = Computer.generateBinaryString(500);
                            themeData = ComputerLoader.filter(themeData);
                            if (elementInfo.Attributes.GetValue("double")?.ToLower() == "true")
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
                            encodedFileStr = elementInfo.Attributes.GetValue("name") ?? "Data";
                            var memoryContent = deserializeMemoryContent(elementInfo);
                            folderFromPath = nearbyNodeOffset.getFolderFromPath(
                                elementInfo.Attributes.GetValue("path") ?? "home", true);
                            if (folderFromPath.searchForFile(encodedFileStr) == null)
                                folderFromPath.files.Add(
                                    new FileEntry(memoryContent.GetEncodedFileString(), encodedFileStr)
                                );
                            else
                                folderFromPath.searchForFile(encodedFileStr).data = memoryContent.GetEncodedFileString();
                            break;
                        case "customthemefile":
                            encodedFileStr = ComputerLoader.filter(elementInfo.Attributes.GetValue("name") ?? "Data");
                            themeData = ThemeManager.getThemeDataStringForCustomTheme(elementInfo.Attributes.GetValue("themePath"));
                            themeData = ComputerLoader.filter(
                                string.IsNullOrEmpty(themeData)
                                ? "DEFINITION ERROR - Theme generated incorrectly. No Custom theme found at definition path"
                                : themeData);
                            folderFromPath = nearbyNodeOffset.getFolderFromPath(
                                elementInfo.Attributes.GetValue("path") ?? "home", true);
                            if (folderFromPath.searchForFile(encodedFileStr) == null)
                                folderFromPath.files.Add(new FileEntry(themeData, encodedFileStr));
                            else
                                folderFromPath.searchForFile(encodedFileStr).data = themeData;
                            break;
                        case "ports":
                            ComputerLoader.loadPortsIntoComputer(elementInfo.Value, nearbyNodeOffset);
                            break;
                        case "positionnear":
                            var target = elementInfo.Attributes.GetValue("target") ?? "";
                            var position = elementInfo.Attributes.Contains("position")
                                                      ? Convert.ToInt32(elementInfo.Attributes.GetValue("position"))
                                                      : 1;
                            var total = elementInfo.Attributes.Contains("total")
                                                      ? Convert.ToInt32(elementInfo.Attributes.GetValue("total"))
                                                      : 3;
                            var force = elementInfo.Attributes.GetValue("force")?.ToLower() == "true";
                            var extraDistance = elementInfo.Attributes.Contains("extraDistance")
                                                      ? Convert.ToSingle(elementInfo.Attributes.GetValue("extraDistance"))
                                                      : 0f;
                            extraDistance = Math.Max(-1f, Math.Min(1f, extraDistance));
                            ComputerLoader.postAllLoadedActions += () =>
                            {
                                var c = Programs.getComputer(ComputerLoader.os, target);
                                if (c != null)
                                    nearbyNodeOffset.location = c.location
                                        + Corporation.getNearbyNodeOffset(
                                            c.location,
                                            position,
                                            total,
                                            ComputerLoader.os.netMap,
                                            extraDistance,
                                            force);
                            };
                            break;
                        case "proxy":
                            var time = elementInfo.Attributes.Contains("time")
                                ? Convert.ToSingle(elementInfo.Attributes.GetValue("time")) : 1f;
                            if (time <= 0f)
                            {
                                nearbyNodeOffset.hasProxy = false;
                                nearbyNodeOffset.proxyActive = false;
                            }
                            else
                                nearbyNodeOffset.addProxy(Computer.BASE_PROXY_TICKS * time);
                            break;
                        case "portsforcrack":
                            var val = elementInfo.Attributes.Contains("val")
                                ? Convert.ToInt32(elementInfo.Attributes.GetValue("val")) : -1;
                            if (val != -1)
                                nearbyNodeOffset.portsNeededForCrack = val - 1;
                            break;
                        case "firewall":
                            var level = elementInfo.Attributes.Contains("level")
                                ? Convert.ToInt32(elementInfo.Attributes.GetValue("level")) : 1;
                            if (level <= 0) nearbyNodeOffset.firewall = null;
                            else
                            {
                                var solution = elementInfo.Attributes.GetValue("solution");
                                if (solution == null)
                                    nearbyNodeOffset.addFirewall(level);
                                else
                                    nearbyNodeOffset.addFirewall(
                                        level, solution, elementInfo.Attributes.Contains("additionalTime")
                                            ? Convert.ToSingle(elementInfo.Attributes.GetValue("additionalTime")) : 0f);
                            }
                            break;
                        case "link":
                            var linkedComp =
                                Programs.getComputer(ComputerLoader.os,
                                                     elementInfo.Attributes.GetValue("target") ?? "");
                            if (linkedComp != null)
                                nearbyNodeOffset.links.Add(ComputerLoader.os.netMap.nodes.IndexOf(linkedComp));
                            break;
                        case "dlink":
                            var offsetComp = nearbyNodeOffset;
                            ComputerLoader.postAllLoadedActions += () =>
                            {
                                linkedComp =
                                    Programs.getComputer(ComputerLoader.os,
                                                         elementInfo.Attributes.GetValue("target") ?? "");
                                if (linkedComp != null)
                                    offsetComp.links.Add(ComputerLoader.os.netMap.nodes.IndexOf(linkedComp));
                            };
                            break;
                        case "trace":
                            nearbyNodeOffset.traceTime = elementInfo.Attributes.Contains("time")
                                ? Convert.ToSingle(elementInfo.Attributes.GetValue("time")) : 1f;
                            break;
                        case "adminpass":
                            nearbyNodeOffset.setAdminPassword(
                                elementInfo.Attributes.Contains("pass")
                                ? ComputerLoader.filter(elementInfo.Attributes.GetValue("pass"))
                                : PortExploits.getRandomPassword());
                            break;
                        case "admin":
                            nearbyNodeOffset.admin = Utility.GetAdminFromString(
                                elementInfo.Attributes.GetValue("type") ?? "basic",
                                elementInfo.Attributes.GetValue("resetPassword")?.ToLower() != "false",
                                elementInfo.Attributes.GetValue("isSuper")?.ToLower() == "true");
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
                            password = ComputerLoader.filter(elementInfo.Attributes.GetValue("password") ?? "ERROR"),
                            username = ComputerLoader.filter(elementInfo.Attributes.GetValue("username") ?? "ERROR");
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
                                nearbyNodeOffset.users.Add(new UserDetail(username, password, type));
                            break;
                        case "tracker":
                            nearbyNodeOffset.HasTracker = true;
                            break;
                        case "missionlistingserver":
                            nearbyNodeOffset.daemons.Add(
                                new MissionListingServer(nearbyNodeOffset,
                                                         ComputerLoader.filter(
                                                             elementInfo.Attributes.GetValue("name") ?? "ERROR"),
                                                         ComputerLoader.filter(
                                                             elementInfo.Attributes.GetValue("group") ?? "ERROR"),
                                                         os,
                                                         elementInfo.Attributes.GetValue("public")?.ToLower() == "true",
                                                         elementInfo.Attributes.GetValue("assigner")?.ToLower() == "true"));
                            break;
                        case "variablemissionlistingserver":
                            var title = elementInfo.Attributes.Contains("title")
                                              ? ComputerLoader.filter(elementInfo.Attributes.GetValue("title"))
                                              : null;
                            var missionListingServer = 
                                new MissionListingServer(nearbyNodeOffset,
                                                         elementInfo.Attributes.Contains("name")
                                                            ? ComputerLoader.filter(
                                                                elementInfo.Attributes.GetValue("name")) 
                                                            : null,
                                                         elementInfo.Attributes.GetValue("iconPath"),
                                                         elementInfo.Attributes.GetValue("articleFolderPath"),
                                                         Utility.GetColorFromString(
                                                             elementInfo.Attributes.GetValue("color"),
                                                             Color.IndianRed),
                                                         os,
                                                         elementInfo.Attributes.GetValue("public")?.ToLower() == "true",
                                                         elementInfo.Attributes.GetValue("assigner")?.ToLower() == "true");
                            if (title != null) missionListingServer.listingTitle = title;
                            nearbyNodeOffset.daemons.Add(missionListingServer);
                            break;
                        case "missionhubserver":
                            break;
                        case "mailserver":
                            break;
                        case "addemaildaemon":
                            break;
                        case "deathrowdatabase":
                            break;
                        case "academicdatabase":
                            break;
                        case "ispsystem":
                            break;
                        case "messageboard":
                            break;
                        case "addavcondemoenddaemon":
                            break;
                        case "addwebserver":
                            break;
                        case "addonlinewebserver":
                            break;
                        case "uploadserverdaemon":
                            break;
                        case "medicaldatabase":
                            break;
                        case "heartmonitor":
                            break;
                        case "pointclicker":
                            break;
                        case "porthackheart":
                            break;
                        case "songchangerdaemon":
                            break;
                        case "dhsdaemon":
                            break;
                        case "customconnectdisplaydaemon":
                            break;
                        case "databasedaemon":
                            break;
                        case "whitelistauthenticatordaemon":
                            break;
                        case "markovtextdaemon":
                            break;
                        case "ircdaemon":
                            break;
                        case "aircraftdaemon":
                            break;
                        case "logocustomconnectdisplaydaemon":
                            break;
                        case "logodaemon":
                            break;
                        case "dlccredits":
                        case "creditsdaemon":
                            var buttonText = ComputerLoader.filter(elementInfo.Attributes.GetValue("ButtonText") ?? "");
                            title = ComputerLoader.filter(elementInfo.Attributes.GetValue("Title") ?? "");
                            var dLCCreditsDaemon =
                                string.IsNullOrEmpty(buttonText) && string.IsNullOrEmpty(title)
                                      ? new DLCCreditsDaemon(nearbyNodeOffset, ComputerLoader.os)
                                      : new DLCCreditsDaemon(nearbyNodeOffset, ComputerLoader.os, title, buttonText);

                            dLCCreditsDaemon.ConditionalActionsToLoadOnButtonPress =
                                elementInfo.Attributes.GetValue("ConditionalActionSetToRunOnButtonPressPath");
                            nearbyNodeOffset.daemons.Add(dLCCreditsDaemon);
                            break;
                        case "fastactionhost":
                            break;
                        case "eosdevice":
                            break;
                        case "memory":
                            break;
                    }
                }
                if (!preventInitDaemons) nearbyNodeOffset.initDaemons();
                if (!preventAddingToNetmap) ComputerLoader.os.netMap.nodes.Add(nearbyNodeOffset);
                result = nearbyNodeOffset;
            });
            processor.Process(stream);
            return result;
        }

        public static MemoryContents deserializeMemoryContent(SaxProcessor.ElementInfo info)
        {
            var memoryInfo = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "Memory");
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
                            memoryContent.CommandsRun.Add(ComputerLoader.filter(Folder.deFilter(
                                string.IsNullOrEmpty(cmdStr) ? " " : cmdStr)));
                    }
                    else memoryContent.CommandsRun.Add(ComputerLoader.filter(Folder.deFilter(commandString)));
                }

            var dataListInfo = memoryInfo.Elements.FirstOrDefault((ininfo) => ininfo.Name == "Data");
            if (dataListInfo != null)
                foreach (var blockInfo in dataListInfo.Elements)
                {
                    if (blockInfo.Name != "Block") continue;
                    memoryContent.DataBlocks.Add(ComputerLoader.filter(Folder.deFilter(blockInfo.Value)));
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
