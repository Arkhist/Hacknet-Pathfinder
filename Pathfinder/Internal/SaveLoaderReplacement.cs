using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Hacknet;
using Hacknet.Security;
using Microsoft.Xna.Framework;
using Pathfinder.Game.Computer;
using Pathfinder.Game.Folder;
using Pathfinder.Util;
using Sax.Net;

namespace Pathfinder.Internal
{
    public static class SaveLoaderReplacement
    {
        public static Computer LoadComputer(XmlReader reader, OS os)
        {
            Computer computer = null;
            var processor = new SaxProcessor();
            processor.AddActionForTag("computer", (info) =>
            {
                var spec = info.Attributes.GetValue("spec");
                var security = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "security");
                var proxyTime = (security?.Attributes).GetFloat("proxyTime", -1f);
                var admin = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "admin");
                var portsOpen = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "portsOpen")?.Value;

                computer = new Computer(
                    info.Attributes.GetValue("name"),
                    info.Attributes.GetValue("ip"),
                    (info.Elements.FirstOrDefault(
                        (ininfo) => ininfo.Name == "location"
                    )?.Attributes).GetVector2("x", "y"),
                    (security?.Attributes).GetInt("level", 1),
                    info.Attributes.GetByte("type"),
                    os
                )
                {
                    firewall = LoadFirewall(info),
                    admin = Utility.GetAdminFromString(
                        (admin?.Attributes).GetValueOrDefault("type", "none"),
                        (admin?.Attributes).GetBool("resetPass"),
                        (admin?.Attributes).GetBool("isSuper")),
                    HasTracker = info.Attributes.GetBool("tracker"),
                    traceTime = (security?.Attributes).GetFloat("traceTime", -1f),
                    portsNeededForCrack = (security?.Attributes).GetInt("portsToCrack"),
                    adminIP = (security?.Attributes).GetValue("adminIP", "192.167.1.111"),
                    idName = info.Attributes.GetValue("id"),
                    icon = info.Attributes.GetValue("icon"),
                    attatchedDeviceIDs = info.Attributes.GetValue("devices")
                };
                if (proxyTime <= 0f)
                {
                    computer.hasProxy = false;
                    computer.proxyActive = false;
                }
                else computer.addProxy(proxyTime);

                computer.PortRemapping = PortRemappingSerializer.Deserialize(
                    info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "portRemap")?.Value);

                var userDetail = LoadUserDetail(info);
                computer.users.Clear();
                if (userDetail.HasValue)
                {
                    if (userDetail.Value.name.ToLower() == "admin") computer.adminPass = userDetail.Value.pass;
                    computer.users.Add(userDetail.Value);
                }

                computer.Memory = ContentLoaderReplacement.DeserializeMemoryContent(info);

                foreach (var daemonInfo in info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "daemons").Elements)
                {
                    switch (daemonInfo.Name)
                    {
                        case "MailServer":
                            var mailServer = computer.AddDaemon<MailServer>(daemonInfo.Attributes.GetValue("name"), os);
                            var mailServerColor = daemonInfo.Attributes.GetColor("color", true);
                            if (mailServerColor != null)
                                mailServer.setThemeColor(mailServerColor.Value);
                            break;
                        case "MissionListingServer":
                            var input = new List<object>
                            {
                                daemonInfo.Attributes.GetValue("name")
                            };
                            var listingIcon = daemonInfo.Attributes.GetValue("icon");
                            var listingColor = daemonInfo.Attributes.GetColor("color", true);
                            if (listingIcon == null || listingColor == null)
                                input.Add(daemonInfo.Attributes.GetValue("group"));
                            else
                                input.AddRange(new object[]
                                {
                                    listingIcon,
                                    daemonInfo.Attributes.GetValue("articles"),
                                    listingColor
                                });
                            input.AddRange(new object[]
                            {
                                os,
                                daemonInfo.Attributes.GetBool("public"),
                                daemonInfo.Attributes.GetBool("assign")
                            });
                            var missionListingServer = computer.AddDaemon<MissionListingServer>(input.ToArray());

                            var listingTitle = daemonInfo.Attributes.GetValue("title");
                            if (listingTitle != null)
                                missionListingServer.listingTitle = listingTitle;
                            break;
                        case "AddEmailServer":
                            computer.AddDaemon<AddEmailDaemon>(daemonInfo.Attributes.GetValue("name"), os);
                            break;
                        case "MessageBoard":
                            var messageBoardDaemon = computer.AddDaemon<MessageBoardDaemon>(os);
                            messageBoardDaemon.name = daemonInfo.Attributes.GetValue("name");
                            var boardName = daemonInfo.Attributes.GetValue("boardName");
                            if (boardName != null)
                                messageBoardDaemon.BoardName = boardName;
                            computer.daemons.Add(messageBoardDaemon);
                            break;
                        case "WebServer":
                            computer.AddDaemon<WebServerDaemon>(daemonInfo.Attributes.GetValue("name"),
                                                                os,
                                                                daemonInfo.Attributes.GetValue("url"));
                            break;
                        case "OnlineWebServer":
                            var onlineWebServerDaemon =
                                computer.AddDaemon<OnlineWebServerDaemon>(daemonInfo.Attributes.GetValue("name"), os);
                            onlineWebServerDaemon.setURL(daemonInfo.Attributes.GetValue("url"));
                            break;
                        case "AcademicDatabse":
                            computer.AddDaemon<AcademicDatabaseDaemon>(daemonInfo.Attributes.GetValue("name"), os);
                            break;
                        case "MissionHubServer":
                            computer.AddDaemon<MissionHubServer>("unknown", "unknown", os);
                            break;
                        case "DeathRowDatabase":
                            computer.AddDaemon<DeathRowDatabaseDaemon>("Death Row Database", os);
                            break;
                        case "MedicalDatabase":
                            computer.AddDaemon<MedicalDatabaseDaemon>(os);
                            break;
                        case "HeartMonitor":
                            computer.AddDaemon<HeartMonitorDaemon>(os)
                                    .PatientID = daemonInfo.Attributes.GetValue("patient") ?? "UNKNOWN";
                            break;
                        case "PointClicker":
                            computer.AddDaemon<PointClickerDaemon>("Point Clicker!", os);
                            break;
                        case "ispSystem":
                            computer.AddDaemon<ISPDaemon>(os);
                            break;
                        case "porthackheart":
                            computer.AddDaemon<PorthackHeartDaemon>(os);
                            break;
                        case "SongChangerDaemon":
                            computer.AddDaemon<SongChangerDaemon>(os);
                            break;
                        case "UploadServerDaemon":
                            computer.AddDaemon<UploadServerDaemon>(
                                daemonInfo.Attributes.GetValueOrDefault("name", ""),
                                daemonInfo.Attributes.GetColor("color"),
                                os,
                                daemonInfo.Attributes.GetValueOrDefault("foldername", ""),
                                daemonInfo.Attributes.GetBool("needsAuh"))
                                    .hasReturnViewButton = daemonInfo.Attributes.GetBool("hasReturnViewButton");
                            break;
                        case "DHSDaemon":
                            computer.AddDaemon<DLCHubServer>("unknown", "unknown", os);
                            break;
                        case "CustomConnectDisplayDaemon":
                            computer.AddDaemon<CustomConnectDisplayDaemon>(os);
                            break;
                        case "DatabaseDaemon":
                            var databaseAdminEmail = daemonInfo.Attributes.GetValue("AdminEmailAccount");
                            var databaseDaemon =
                                computer.AddDaemon<DatabaseDaemon>(
                                    os,
                                    daemonInfo.Attributes.GetValue("Name"),
                                    daemonInfo.Attributes.GetValue("Permissions"),
                                    daemonInfo.Attributes.GetValue("DataType"),
                                    daemonInfo.Attributes.GetValue("Foldername"),
                                    daemonInfo.Attributes.GetColor("Color"));
                            if (!string.IsNullOrWhiteSpace(databaseAdminEmail))
                            {
                                databaseDaemon.adminResetEmailHostID = daemonInfo.Attributes.GetValue("AdminEmailHostID");
                                databaseDaemon.adminResetPassEmailAccount = databaseAdminEmail;
                            }
                            break;
                        case "WhitelistAuthenticatorDaemon":
                            computer.AddDaemon<WhitelistConnectionDaemon>(os).AuthenticatesItself =
                                    daemonInfo.Attributes.GetBool("SelfAuthenticating", true);
                            break;
                        case "IRCDaemon":
                            computer.AddDaemon<IRCDaemon>(os, "LOAD ERROR");
                            break;
                        case "MarkovTextDaemon":
                            computer.AddDaemon<MarkovTextDaemon>(
                                os,
                                daemonInfo.Attributes.GetValue("Name"),
                                daemonInfo.Attributes.GetValue("SourceFilesContentFolder")
                            );
                            break;
                        case "AircraftDaemon":
                            computer.AddDaemon<AircraftDaemon>(
                                os,
                                daemonInfo.Attributes.GetValueOrDefault("Name", "Pacific Charter Flight"),
                                daemonInfo.Attributes.GetVector2("Origin"),
                                daemonInfo.Attributes.GetVector2("Dest", defaultVal: Vector2.One * 0.5f),
                                daemonInfo.Attributes.GetFloat("Progress", 0.5f)
                            );
                            break;
                        case "LogoCustomConnectDisplayDaemon":
                            computer.AddDaemon<LogoCustomConnectDisplayDaemon>(
                                os,
                                daemonInfo.Attributes.GetValue("logo"),
                                daemonInfo.Attributes.GetValue("title"),
                                daemonInfo.Attributes.GetBool("overdrawLogo"),
                                daemonInfo.Attributes.GetValue("buttonAlignment")
                            );
                            break;
                        case "LogoDaemon":
                            computer.AddDaemon<LogoDaemon>(
                                os,
                                computer.name,
                                daemonInfo.Attributes.GetBool("ShowsTitle", true),
                                daemonInfo.Attributes.GetValue("LogoImagePath")
                            ).TextColor = daemonInfo.Attributes.GetColor("Color");
                            break;
                        case "DLCCredits":
                            input = new List<object>
                            {
                                os,
                                daemonInfo.Attributes.GetValue("Title"),
                                daemonInfo.Attributes.GetValue("Button")
                            };
                            input.Where((i) => i != null);
                            computer.AddDaemon<DLCCreditsDaemon>(input.ToArray())
                                            .ConditionalActionsToLoadOnButtonPress =
                                        daemonInfo.Attributes.GetValue("ConditionalActionSetToRunOnButtonPressPath");
                            break;
                        case "FastActionHost":
                            computer.AddDaemon<FastActionHost>(os, computer.name);
                            break;
                    }
                }

                computer.files = LoadFilesystem(info);

                var links = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "links");
                if (links != null)
                    foreach (var c in links.Value.Split())
                        if (c.Trim() != "") computer.links.Add(Convert.ToInt32(c));

                if (portsOpen.Length > 0) ComputerLoader.loadPortsIntoComputer(portsOpen, computer);

                if (spec == "mail") os.netMap.mailServer = computer;
                else if (spec == "player") os.thisComputer = computer;
            });
            processor.Process(reader.ToStream());
            return computer;
        }

        public static Firewall LoadFirewall(SaxProcessor.ElementInfo info)
        {
            var firewallInfo = info.Elements.First((ininfo) => ininfo.Name == "firewall");
            return new Firewall(
                firewallInfo.Attributes.GetInt("complexity"),
                firewallInfo.Attributes.GetValue("solution"),
                firewallInfo.Attributes.GetFloat("additionalDelay")
            );
        }

        public static UserDetail? LoadUserDetail(SaxProcessor.ElementInfo info)
        {
            var userInfo = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "users");
            if (userInfo == null) return null;
            return new UserDetail(userInfo.Attributes.GetValue("name"),
                                  userInfo.Attributes.GetValue("pass"),
                                  userInfo.Attributes.GetByte("type")
                                 )
            {
                known = userInfo.Attributes.GetBool("known")
            };
        }



        public static FileSystem LoadFilesystem(SaxProcessor.ElementInfo info)
            => new FileSystem(true)
            {
                root = LoadFolder(info.Elements.First((ininfo) => ininfo.Name == "filesystem"))
            };

        public static Folder LoadFolder(SaxProcessor.ElementInfo info)
        {
            if (info.Elements.Count == 0) return null;
            var outFolderInfo = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "folder");
            var outFolder = new Folder(Folder.deFilter(outFolderInfo.Attributes.GetValue("name")));
            foreach (var element in outFolderInfo.Elements)
            {
                if (element.Name == "folder")
                    foreach (var inElement in element.Elements)
                        outFolder.folders.Add(LoadFolder(inElement));
                if (element.Name == "file"
                    && (element.Attributes.GetBool("EduSafe", true) || !Settings.EducationSafeBuild))
                    outFolder.AddFile(
                        Folder.deFilter(element.Attributes.GetValue("name")),
                        Folder.deFilter(element.Value)
                    );
            }
            return outFolder;
        }

        public static MemoryStream ToStream(this XmlReader reader)
        {
            var ms = new MemoryStream();
            reader.CopyTo(ms);
            return ms;
        }

        public static FileStream ToStream(this XmlReader reader, string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Create);
            reader.CopyTo(fs);
            return fs;
        }

        public static void CopyTo(this XmlReader reader, Stream s)
        {
            using (var writer = XmlWriter.Create(s, new XmlWriterSettings
            {
                CheckCharacters = false, // don't get hung ually invalid XML characters
                CloseOutput = false // leave the stream open
            }))
            {
                writer.WriteNode(reader, true);
            }
        }
    }
}
