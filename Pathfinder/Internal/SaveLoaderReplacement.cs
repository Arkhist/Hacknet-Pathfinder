using System;
using System.IO;
using System.Linq;
using System.Xml;
using Hacknet;
using Hacknet.Security;
using Microsoft.Xna.Framework;
using Pathfinder.Util;

namespace Pathfinder.Internal
{
    public static class SaveLoaderReplacement
    {
        public static Computer loadComputer(XmlReader reader, OS os)
        {
            Computer computer = null;
            var processor = new SaxProcessor();
            processor.AddActionForTag("computer", (info) =>
            {
                var name = info.Attributes.GetValue("name");
                var ip = info.Attributes.GetValue("ip");
                byte type;
                byte.TryParse(info.Attributes.GetValue("type"), out type);
                var spec = info.Attributes.GetValue("spec");
                var id = info.Attributes.GetValue("id");
                var devices = info.Attributes.GetValue("devices");
                var icon = info.Attributes.GetValue("icon");
                var tracker = info.Attributes.GetValue("tracker").ToLower() == "true";

                var location = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "location");
                float x;
                float y;
                float.TryParse(location?.Attributes.GetValue("x"), out x);
                float.TryParse(location?.Attributes.GetValue("y"), out y);


                var security = info.Elements.First((ininfo) => ininfo.Name == "security");
                int level;
                int.TryParse(security?.Attributes.GetValue("level"), out level);
                level = level == 0 ? 1 : level;
                float traceTime = -1f;
                float.TryParse(security?.Attributes.GetValue("traceTime"), out traceTime);
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                traceTime = traceTime == 0.0f ? -1f : traceTime;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                int portsToCrack;
                int.TryParse(security?.Attributes.GetValue("portsToCrack"), out portsToCrack);
                var adminIP = security?.Attributes.GetValue("adminIP") ?? "192.167.1.111";
                float proxyTime = -1f;
                float.TryParse(security?.Attributes.GetValue("proxyTime"), out proxyTime);
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                proxyTime = proxyTime == 0.0f ? -1f : proxyTime;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                var admin = info.Elements.First((ininfo) => ininfo.Name == "admin");

                var firewall = loadFirewall(info);
                var portsOpen = info.Elements.First((ininfo) => ininfo.Name == "portsOpen")?.Value;

                computer = new Computer(name, ip, new Vector2(x, y), level, type, os)
                {
                    firewall = firewall,
                    admin = Utility.GetAdminFromString(
                        admin?.Attributes.GetValue("type") ?? "none",
                        admin.Attributes.GetValue("resetPass").ToLower() == "true",
                        admin.Attributes.GetValue("isSuper").ToLower() == "true"),
                    HasTracker = tracker
                };
                if (proxyTime <= 0f)
                {
                    computer.hasProxy = false;
                    computer.proxyActive = false;
                }
                else computer.addProxy(proxyTime);

                computer.PortRemapping = PortRemappingSerializer.Deserialize(
                    info.Elements.First((ininfo) => ininfo.Name == "portRemap")?.Value);

                var userDetail = loadUserDetail(info);
                computer.users.Clear();
                if (userDetail.HasValue)
                {
                    if (userDetail.Value.name.ToLower() == "admin") computer.adminPass = userDetail.Value.pass;
                    computer.users.Add(userDetail.Value);
                }

                computer.Memory = ContentLoaderReplacement.deserializeMemoryContent(info);

                foreach (var daemonInfo in info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "daemons").Elements)
                {
                    switch (daemonInfo.Name)
                    {
                        case "MailServer":
                            var mailServer = new MailServer(computer, daemonInfo.Attributes.GetValue("name"), os);
                            computer.daemons.Add(mailServer);
                            var mailServerColor = daemonInfo.Attributes.GetValue("color");
                            if (mailServerColor != null)
                                mailServer.setThemeColor(Utils.convertStringToColor(mailServerColor));
                            break;
                        case "MissionListingServer":
                            string listingName = daemonInfo.Attributes.GetValue("name"),
                            listingGroup = daemonInfo.Attributes.GetValue("group"),
                            listingTitle = daemonInfo.Attributes.GetValue("title"),
                            listingIcon = daemonInfo.Attributes.GetValue("icon"),
                            listingColor = daemonInfo.Attributes.GetValue("color"),
                            listingArticles = daemonInfo.Attributes.GetValue("articles");
                            bool listingAssign = daemonInfo.Attributes.GetValue("assign").ToLower().Equals("true"),
                            listingPublic = daemonInfo.Attributes.GetValue("public").ToLower().Equals("true");
                            var missionListingServer =
                                listingIcon == null || listingColor == null
                                 ? new MissionListingServer(computer, listingName, listingGroup,
                                                            os, listingPublic, listingAssign)
                                 : new MissionListingServer(computer, listingName, listingIcon, listingArticles,
                                                            Utils.convertStringToColor(listingColor), os,
                                                            listingPublic, listingAssign);
                            if (listingTitle != null)
                                missionListingServer.listingTitle = listingTitle;
                            computer.daemons.Add(missionListingServer);
                            break;
                        case "AddEmailServer":
                            var addEmailDaemon = new AddEmailDaemon(computer,
                                                                    daemonInfo.Attributes.GetValue("name"), os);
                            computer.daemons.Add(addEmailDaemon);
                            break;
                        case "MessageBoard":
                            var messageBoardDaemon = new MessageBoardDaemon(computer, os)
                            {
                                name = daemonInfo.Attributes.GetValue("name")
                            };
                            var boardName = daemonInfo.Attributes.GetValue("boardName");
                            if (boardName != null)
                                messageBoardDaemon.BoardName = boardName;
                            computer.daemons.Add(messageBoardDaemon);
                            break;
                        case "WebServer":
                            computer.daemons.Add(new WebServerDaemon(computer, daemonInfo.Attributes.GetValue("name"),
                                                                     os,
                                                                     daemonInfo.Attributes.GetValue("url")));
                            break;
                        case "OnlineWebServer":
                            var onlineWebServerDaemon =
                                new OnlineWebServerDaemon(computer, daemonInfo.Attributes.GetValue("name"), os);
                            onlineWebServerDaemon.setURL(daemonInfo.Attributes.GetValue("url"));
                            computer.daemons.Add(onlineWebServerDaemon);
                            break;
                        case "AcademicDatabse":
                            computer.daemons.Add(new AcademicDatabaseDaemon(computer,
                                                                            daemonInfo.Attributes.GetValue("name"),
                                                                            os));
                            break;
                        case "MissionHubServer":
                            computer.daemons.Add(new MissionHubServer(computer, "unknown", "unknown", os));
                            break;
                        case "DeathRowDatabase":
                            computer.daemons.Add(new DeathRowDatabaseDaemon(computer, "Death Row Database", os));
                            break;
                        case "MedicalDatabase":
                            computer.daemons.Add(new MedicalDatabaseDaemon(computer, os));
                            break;
                        case "HeartMonitor":
                            computer.daemons.Add(new HeartMonitorDaemon(computer, os)
                            {
                                PatientID = daemonInfo.Attributes.GetValue("patient") ?? "UNKNOWN"
                            });
                            break;
                        case "PointClicker":
                            computer.daemons.Add(new PointClickerDaemon(computer, "Point Clicker!", os));
                            break;
                        case "ispSystem":
                            computer.daemons.Add(new ISPDaemon(computer, os));
                            break;
                        case "porthackheart":
                            computer.daemons.Add(new PorthackHeartDaemon(computer, os));
                            break;
                        case "SongChangerDaemon":
                            computer.daemons.Add(new SongChangerDaemon(computer, os));
                            break;
                        case "UploadServerDaemon":
                            computer.daemons.Add(
                                new UploadServerDaemon(computer,
                                                       daemonInfo.Attributes.GetValue("name") ?? "",
                                                       Utility.GetColorFromString(
                                                           daemonInfo.Attributes.GetValue("color")
                                                          ),
                                                       os,
                                                       daemonInfo.Attributes.GetValue("foldername") ?? "",
                                                       daemonInfo.Attributes.GetValue("needsAuh").ToLower() == "true")
                                {
                                    hasReturnViewButton = daemonInfo.Attributes.GetValue("hasReturnViewButton").ToLower() == "true"
                                });
                            break;
                        case "DHSDaemon":
                            computer.daemons.Add(new DLCHubServer(computer, "unknown", "unknown", os));
                            break;
                        case "CustomConnectDisplayDaemon":
                            computer.daemons.Add(new CustomConnectDisplayDaemon(computer, os));
                            break;
                        case "DatabaseDaemon":
                            var databaseAdminEmail = daemonInfo.Attributes.GetValue("AdminEmailAccount");
                            var databaseDaemon =
                                new DatabaseDaemon(computer,
                                                   os,
                                                   daemonInfo.Attributes.GetValue("Name"),
                                                   daemonInfo.Attributes.GetValue("Permissions"),
                                                   daemonInfo.Attributes.GetValue("DataType"),
                                                   daemonInfo.Attributes.GetValue("Foldername"),
                                                   Utility.GetColorFromString(daemonInfo.Attributes.GetValue("Color")));
                            if (!string.IsNullOrWhiteSpace(databaseAdminEmail))
                            {
                                databaseDaemon.adminResetEmailHostID = daemonInfo.Attributes.GetValue("AdminEmailHostID");
                                databaseDaemon.adminResetPassEmailAccount = databaseAdminEmail;
                            }
                            computer.daemons.Add(databaseDaemon);
                            break;
                        case "WhitelistAuthenticatorDaemon":
                            computer.daemons.Add(new WhitelistConnectionDaemon(computer, os)
                            {
                                AuthenticatesItself =
                                    daemonInfo.Attributes.GetValue("SelfAuthenticating").ToLower() != "false"
                            });
                            break;
                        case "IRCDaemon":
                            computer.daemons.Add(new IRCDaemon(computer, os, "LOAD ERROR"));
                            break;
                        case "MarkovTextDaemon":
                            computer.daemons.Add(
                                new MarkovTextDaemon(computer, os, daemonInfo.Attributes.GetValue("Name"),
                                                     daemonInfo.Attributes.GetValue("SourceFilesContentFolder")));
                            break;
                        case "AircraftDaemon":
                            var origin = Vector2.Zero;
                            if (daemonInfo.Attributes.Contains("OriginX"))
                                origin.X = Convert.ToSingle(daemonInfo.Attributes.GetValue("OriginX").Replace(",", "."));
                            if (daemonInfo.Attributes.Contains("OriginY"))
                                origin.Y = Convert.ToSingle(daemonInfo.Attributes.GetValue("OriginY").Replace(",", "."));
                            var dest = Vector2.One * 0.5f;
                            if (daemonInfo.Attributes.Contains("DestX"))
                                dest.Y = Convert.ToSingle(daemonInfo.Attributes.GetValue("DestX").Replace(",", "."));
                            if (daemonInfo.Attributes.Contains("DestY"))
                                dest.Y = Convert.ToSingle(daemonInfo.Attributes.GetValue("DestY").Replace(",", "."));
                            var progress = 0.5f;
                            if (daemonInfo.Attributes.Contains("Progress"))
                                progress = Convert.ToSingle(daemonInfo.Attributes.GetValue("Progress").Replace(",", "."));
                            computer.daemons.Add(
                                new AircraftDaemon(computer, os,
                                                   daemonInfo.Attributes.GetValue("Name") ?? "Pacific Charter Flight",
                                                   origin, dest, progress));
                            break;
                        case "LogoCustomConnectDisplayDaemon":
                            computer.daemons.Add(
                                new LogoCustomConnectDisplayDaemon(
                                    computer, os,
                                    daemonInfo.Attributes.GetValue("logo"),
                                    daemonInfo.Attributes.GetValue("title"),
                                    daemonInfo.Attributes.GetValue("overdrawLogo").ToLower() == "true",
                                    daemonInfo.Attributes.GetValue("buttonAlignment")));
                            break;
                        case "LogoDaemon":
                            computer.daemons.Add(
                                new LogoDaemon(computer, os, name,
                                               daemonInfo.Attributes.GetValue("ShowsTitle") != "false",
                                               daemonInfo.Attributes.GetValue("LogoImagePath"))
                                {
                                    TextColor = Utility.GetColorFromString(daemonInfo.Attributes.GetValue("Color"))
                                }
                            );
                            break;
                        case "DLCCredits":
                            string dlcCreditsButton = daemonInfo.Attributes.GetValue("Button"),
                            dlcCreditsTitle = daemonInfo.Attributes.GetValue("Title");
                            var dLCCreditsDaemon =
                                ((dlcCreditsTitle == null && dlcCreditsButton == null)
                                 ? new DLCCreditsDaemon(computer, os)
                                 : new DLCCreditsDaemon(computer, os, dlcCreditsTitle, dlcCreditsButton));
                            if (daemonInfo.Attributes.Contains("Action"))
                                dLCCreditsDaemon.ConditionalActionsToLoadOnButtonPress =
                                                    daemonInfo.Attributes.GetValue("Action");
                            computer.daemons.Add(dLCCreditsDaemon);
                            break;
                        case "FastActionHost":
                            computer.daemons.Add(new FastActionHost(computer, os, name));
                            break;
                    }
                }

                computer.files = loadFilesystem(info);
                computer.traceTime = traceTime;
                computer.portsNeededForCrack = portsToCrack;
                computer.adminIP = adminIP;
                computer.idName = id;
                computer.icon = icon;
                computer.attatchedDeviceIDs = devices;

                var links = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "links");
                if (links != null)
                    foreach (var c in links.Value.Split(new char[0]))
                        if (c != "") computer.links.Add(Convert.ToInt32(c));

                if (portsOpen.Length > 0) ComputerLoader.loadPortsIntoComputer(portsOpen, computer);

                if (spec == "mail") os.netMap.mailServer = computer;
                else if (spec == "player") os.thisComputer = computer;
            });
            processor.Process(reader.ToStream());
            return computer;
        }

        public static Firewall loadFirewall(SaxProcessor.ElementInfo info)
        {
            var firewallInfo = info.Elements.First((ininfo) => ininfo.Name == "firewall");
            return new Firewall(
                Convert.ToInt32(firewallInfo.Attributes.GetValue("complexity")),
                firewallInfo.Attributes.GetValue("solution"),
                Convert.ToSingle(firewallInfo.Attributes.GetValue("additionalDelay")));
        }

        public static UserDetail? loadUserDetail(SaxProcessor.ElementInfo info)
        {
            var userInfo = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "users");
            if (userInfo == null) return null;
            return new UserDetail(userInfo.Attributes.GetValue("name"),
                                  userInfo.Attributes.GetValue("pass"),
                                  Convert.ToByte(userInfo.Attributes.GetValue("type")))
            {
                known = userInfo.Attributes.GetValue("known").ToLower() == "true"
            };
        }



        public static FileSystem loadFilesystem(SaxProcessor.ElementInfo info)
        {
            return new FileSystem(true)
            {
                root = loadFolder(info.Elements.First((ininfo) => ininfo.Name == "filesystem"))
            };
        }

        public static Folder loadFolder(SaxProcessor.ElementInfo info)
        {
            if (info.Elements.Count == 0) return null;
            var outFolderInfo = info.Elements.FirstOrDefault((ininfo) => ininfo.Name == "folder");
            var outFolder = new Folder(Folder.deFilter(outFolderInfo.Attributes.GetValue("name")));
            foreach (var element in outFolderInfo.Elements)
            {
                if (element.Name == "folder")
                    foreach (var inElement in element.Elements)
                        outFolder.folders.Add(loadFolder(inElement));
                if (element.Name == "file"
                    && (element.Attributes.GetValue("EduSafe").ToLower() != "false" || !Settings.EducationSafeBuild))
                    outFolder.files.Add(
                        new FileEntry(
                            Folder.deFilter(element.Attributes.GetValue("name")),
                            Folder.deFilter(element.Value)));

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
            var settings = new XmlWriterSettings();
            settings.CheckCharacters = false; // don't get hung up on technically invalid XML characters
            settings.CloseOutput = false; // leave the stream open
            using (XmlWriter writer = XmlWriter.Create(s, settings))
            {
                writer.WriteNode(reader, true);
            }
        }
    }
}
