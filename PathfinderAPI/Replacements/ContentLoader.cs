using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hacknet;
using Hacknet.Extensions;
using Hacknet.Security;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathfinder.Administrator;
using Pathfinder.Daemon;
using Pathfinder.Event;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class ContentLoader
    {
        public abstract class ComputerExecutor
        {
            public OS Os { get; private set; }
            private ComputerHolder _comp;
            public Computer Comp => _comp;

            public virtual void Init(OS os, ref ComputerHolder comp)
            {
                Os = os;
                _comp = comp;
            }

            public abstract void Execute(IExecutor exec, ElementInfo info);
        }

        public class ComputerHolder
        {
            internal Computer Comp;
            internal ComputerHolder() {}

            public static implicit operator Computer(ComputerHolder holder) => holder.Comp;
        }

        private struct ComputerExecutorHolder
        {
            public string Element;
            public Type ExecutorType;
            public ParseOption Options;
        }
        
        private static readonly List<ComputerExecutorHolder> CustomExecutors = new List<ComputerExecutorHolder>();

        public static void RegisterExecutor<T>(string element, ParseOption options = ParseOption.None) where T : ComputerExecutor, new()
        {
            CustomExecutors.Add(new ComputerExecutorHolder
            {
                Element = element,
                ExecutorType = typeof(T),
                Options = options
            });
        }

        public static void UnregisterExecutor<T>() where T : ComputerExecutor, new()
        {
            var tType = typeof(T);
            CustomExecutors.RemoveAll(x => x.ExecutorType == tType);
        }

        static ContentLoader()
        {
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnPluginUnload(Assembly pluginAsm)
        {
            var allTypes = AccessTools.GetTypesFromAssembly(pluginAsm);
            CustomExecutors.RemoveAll(x => allTypes.Contains(x.ExecutorType));
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.loadComputer))]
        private static bool LoadComputerPrefix(string filename, bool preventAddingToNetmap, bool preventInitDaemons, out object __result)
        {
            __result = LoadComputer(filename, preventAddingToNetmap, preventInitDaemons);
            return false;
        }

        public static Computer LoadComputer(string filename, bool preventAddingToNetmap = false, bool preventInitDaemons = false)
        {
            filename = LocalizedFileLoader.GetLocalizedFilepath(filename);

            Computer comp = null;
            Computer eos = null;
            var os = OS.currentInstance;

            var executor = new EventExecutor(filename, true);

            var holder = new ComputerHolder();
            
            foreach (var custom in CustomExecutors)
            {
                var customInstance = (ComputerExecutor) Activator.CreateInstance(custom.ExecutorType);
                customInstance.Init(os, ref holder);
                executor.RegisterExecutor(custom.Element, customInstance.Execute, custom.Options);
            }

            executor.RegisterExecutor("Computer", (exec, info) =>
            {
                var typeString = info.Attributes.GetString("type", "1");
                byte type;
                if (!byte.TryParse(typeString, out type))
                {
                    if (typeString.ToLowerInvariant() == "empty")
                        type = 4;
                    else
                        throw new FormatException("Invalid computer type");
                }

                comp = new Computer(
                    info.Attributes.GetString("name", "UNKNOWN").Filter(),
                    info.Attributes.GetString("ip", NetworkMap.generateRandomIP()).Filter(),
                    os.netMap.getRandomPosition(),
                    info.Attributes.GetInt("security"),
                    type,
                    os
                )
                {
                    idName = info.Attributes.GetString("id", "UNKNOWN").Filter(),
                    AllowsDefaultBootModule = info.Attributes.GetBool("allowsDefaultBootModule", true),
                    icon = info.Attributes.GetString("icon", null)
                };
                holder.Comp = comp;

                if (type == 4)
                {
                    var home = comp.files.root.searchForFolder("home");
                    if (home != null)
                    {
                        home.folders.Clear();
                        home.files.Clear();
                    }
                }
            });
            executor.RegisterExecutor("Computer.file", (exec, info) =>
            {
                var path = info.Attributes.GetString("path", "home");
                var name = info.Attributes.GetString("name", "Data").Filter();
                var contents = (info.Content ?? Computer.generateBinaryString(500)).Filter();
                var eduSafe = info.Attributes.GetBool("EduSafe", true);
                var eduOnly = info.Attributes.GetBool("EduSafeOnly");

                if ((!eduSafe && Settings.EducationSafeBuild) || (eduOnly && Settings.EducationSafeBuild))
                    return;

                var folder = comp.getFolderFromPath(path, true);
                var existing = folder.searchForFile(name);
                if (existing != null)
                {
                    existing.data = contents;
                }
                else
                {
                    folder.files.Add(new FileEntry(contents, name));
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.encryptedFile", (exec, info) =>
            {
                var path = info.Attributes.GetString("path", "home");
                var name = info.Attributes.GetString("name", "Data").Filter();
                var contents = (info.Content ?? Computer.generateBinaryString(500)).Filter();
                var header = info.Attributes.GetString("header", "ERROR");
                var sourceIP = info.Attributes.GetString("ip", "ERROR");
                var password = info.Attributes.GetString("pass");
                var extension = info.Attributes.GetString("extension", null);
                var isDouble = info.Attributes.GetBool("double");

                var encryptedText = FileEncrypter.EncryptString(
                    contents,
                    header,
                    sourceIP,
                    password,
                    extension
                );
                if (isDouble)
                {
                    encryptedText = FileEncrypter.EncryptString(
                        encryptedText,
                        header,
                        sourceIP,
                        password,
                        "_LAYER2.dec"
                    );
                }

                var folder = comp.getFolderFromPath(path, true);
                var existing = folder.searchForFile(name);
                if (existing != null)
                {
                    existing.data = encryptedText;
                }
                else
                {
                    folder.files.Add(new FileEntry(encryptedText, name));
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.memoryDumpFile", (exec, info) =>
            {
                var path = info.Attributes.GetString("path", "home");
                var name = info.Attributes.GetString("name", "Data").Filter();
                var contents = ReplacementsCommon.LoadMemoryContents(info).GetEncodedFileString();
                
                var folder = comp.getFolderFromPath(path, true);
                var existing = folder.searchForFile(name);
                if (existing != null)
                {
                    existing.data = contents;
                }
                else
                {
                    folder.files.Add(new FileEntry(contents, name));
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.customthemefile", (exec, info) =>
            {
                var path = info.Attributes.GetString("path", "home");
                var name = info.Attributes.GetString("name", "Data").Filter();
                var contents = ThemeManager.getThemeDataStringForCustomTheme(info.Attributes.GetOrThrow("themePath", "Invalid theme path for customthemefile"));
                if (contents == null)
                    throw new FormatException("Invalid theme path for customthemefile");
                
                var folder = comp.getFolderFromPath(path, true);
                var existing = folder.searchForFile(name);
                if (existing != null)
                {
                    existing.data = contents;
                }
                else
                {
                    folder.files.Add(new FileEntry(contents, name));
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.ports", (exec, info) => ComputerLoader.loadPortsIntoComputer(info.Content ?? "", comp), ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.positionNear", (exec, info) =>
            {
                var nearNodeId = info.Attributes.GetString("target");
                var position = info.Attributes.GetInt("position") + 1;
                var total = info.Attributes.GetInt("total", 3);
                var force = info.Attributes.GetBool("force");
                var extraDistance = Math.Max(-1f, Math.Min(1f, info.Attributes.GetFloat("extraDistance")));

                ComputerLoader.postAllLoadedActions = (System.Action) Delegate.Combine(
                    ComputerLoader.postAllLoadedActions,
                    (System.Action)(() =>
                    {
                        var nearNode = Programs.getComputer(os, nearNodeId);
                        if (nearNode != null)
                            comp.location = nearNode.location + Corporation.getNearbyNodeOffset(nearNode.location, position, total, os.netMap, extraDistance, force);
                    }));
            });
            executor.RegisterExecutor("Computer.proxy", (exec, info) =>
            {
                var time = info.Attributes.GetFloat("time", 1f);
                if (time > 0f)
                {
                    comp.addProxy(time);
                }
                else
                {
                    comp.hasProxy = false;
                    comp.proxyActive = false;
                }
            });
            executor.RegisterExecutor("Computer.portsForCrack", (exec, info) =>
            {
                var num = info.Attributes.GetInt("val", -1);
                if (num != -1)
                {
                    comp.portsNeededForCrack = num - 1;
                }
            });
            executor.RegisterExecutor("Computer.firewall", (exec, info) =>
            {
                var level = info.Attributes.GetInt("level", 1);
                if (level > 0)
                {
                    if (info.Attributes.TryGetValue("solution", out var solution))
                    {
                        comp.addFirewall(level, solution, info.Attributes.GetFloat("additionalTime"));
                    }
                    else
                    {
                        comp.addFirewall(level);
                    }
                }
                else
                {
                    comp.firewall = null;
                }
            });
            executor.RegisterExecutor("Computer.link", (exec, info) =>
            {
                var linked = Programs.getComputer(os, info.Attributes.GetString("target"));
                if (linked != null)
                    comp.links.Add(os.netMap.nodes.IndexOf(linked));
            });
            executor.RegisterExecutor("Computer.dlink", (exec, info) =>
            {
                var linked = info.Attributes.GetString("target");
                ComputerLoader.postAllLoadedActions = (System.Action)Delegate.Combine(
                    ComputerLoader.postAllLoadedActions,
                    (System.Action) (() =>
                        {
                            var linkedNode = Programs.getComputer(os, linked);
                            if (linkedNode != null)
                                comp.links.Add(os.netMap.nodes.IndexOf(linkedNode));
                        }));
            });
            executor.RegisterExecutor("Computer.trace", (exec, info) =>
            {
                comp.traceTime = info.Attributes.GetFloat("time", 1f);
            });
            executor.RegisterExecutor("Computer.adminPass", (exec, info) =>
            {
                comp.setAdminPassword(info.Attributes.GetString("pass", PortExploits.getRandomPassword()));
            });
            executor.RegisterExecutor("Computer.admin", (exec, info) => AdministratorManager.LoadAdministrator(info, comp, os));
            executor.RegisterExecutor("Computer.portRemap", (exec, info) =>
            {
                try
                {
                    comp.PortRemapping = PortRemappingSerializer.Deserialize(info.Content);
                }
                catch (Exception e)
                {
                    throw new FormatException("Invalid port remappings", e);
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.ExternalCounterpart", (exec, info) =>
            {
                comp.externalCounterpart = new ExternalCounterpart(info.Attributes.GetString("name"), ExternalCounterpart.getIPForServerName(info.Attributes.GetString("id")));
            });
            executor.RegisterExecutor("Computer.account", (exec, info) =>
            {
                var username = info.Attributes.GetString("username", "ERROR").Filter();
                var pass = info.Attributes.GetString("password", "ERROR").Filter();
                var typeString = info.Attributes.GetString("type").ToLower();
                byte type = 0;
                switch (typeString)
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
                        type = info.Attributes.GetByte("type", 0);
                        break;
                }

                var noneExists = true;
                for (int i = 0; i < comp.users.Count; i++)
                {
                    var user = comp.users[i];
                    if (user.name == username)
                    {
                        user.pass = pass;
                        user.type = type;
                        
                        comp.users[i] = user;

                        if (username == "admin")
                            comp.adminPass = pass;

                        noneExists = false;
                    }
                }

                if (noneExists)
                {
                    comp.users.Add(new UserDetail(username, pass, type));
                }
            });
            executor.RegisterExecutor("Computer.tracker", (exec, info) => comp.HasTracker = true);
            executor.RegisterExecutor("Computer.missionListingServer", (exec, info) =>
            {
                comp.daemons.Add(new MissionListingServer(
                    comp,
                    info.Attributes.GetString("name", "ERROR"),
                    info.Attributes.GetString("group", "ERROR"),
                    os,
                    info.Attributes.GetBool("public")
                )
                {
                    missionAssigner = info.Attributes.GetBool("assigner")
                });
            });
            executor.RegisterExecutor("Computer.variableMissionListingServer", (exec, info) =>
            {
                comp.daemons.Add(new MissionListingServer(
                    comp,
                    info.Attributes.GetString("name", null).Filter(),
                    info.Attributes.GetString("iconPath", null),
                    info.Attributes.GetString("articleFolderPath", null),
                    info.Attributes.GetColor("color", Color.IndianRed).Value,
                    os,
                    info.Attributes.GetBool("public"),
                    info.Attributes.GetBool("assigner")
                )
                {
                    listingTitle = info.Attributes.GetString("title", null)
                });
            });
            executor.RegisterExecutor("Computer.missionHubServer", (exec, info) =>
            {
                var missionPath = ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" + info.Attributes.GetString("missionFolderPath", null);
                missionPath = missionPath.Replace("\\", "/");
                if (!missionPath.EndsWith("/"))
                    missionPath += "/";
                
                comp.daemons.Add(new MissionHubServer(
                    comp,
                    info.Attributes.GetString("serviceName", null),
                    info.Attributes.GetString("groupName", null).Filter(),
                    os
                )
                {
                    MissionSourceFolderPath = missionPath,
                    themeColor = info.Attributes.GetColor("themeColor", Color.PaleTurquoise).Value,
                    themeColorBackground = info.Attributes.GetColor("backgroundColor", Color.PaleTurquoise).Value,
                    themeColorLine = info.Attributes.GetColor("lineColor", Color.PaleTurquoise).Value,
                    allowAbandon = info.Attributes.GetBool("allowAbandon", true)
                });
            });
            executor.RegisterExecutor("Computer.mailServer", (exec, info) =>
            {
                var ms = new MailServer(
                    comp,
                    info.Attributes.GetString("name", "Mail Server"),
                    os
                )
                {
                    shouldGenerateJunkEmails = info.Attributes.GetBool("generateJunk", true),
                };

                var color = info.Attributes.GetColor("color", null);
                if (color != null)
                    ms.setThemeColor(color.Value);

                foreach (var emailInfo in info.Children.Where(x => x.Name == "email"))
                {
                    var sender = emailInfo.Attributes.GetString("sender", "UNKNOWN").Filter();
                    var subject = emailInfo.Attributes.GetString("subject", "UNKNOWN").Filter();
                    var content = (emailInfo.Content ?? "UNKNOWN").Filter();
                    if (emailInfo.Attributes.TryGetValue("recipient", out var recp))
                    {
                        recp = recp.Filter();
                        var email = MailServer.generateEmail(subject, content, sender);
                        ms.setupComplete = (System.Action)Delegate.Combine(ms.setupComplete, (System.Action)(() =>
                        {
                            ms.addMail(email, recp);
                        }));
                    }
                }
                
                comp.daemons.Add(ms);
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.addEmailDaemon", (exec, info) =>
            {
                comp.daemons.Add(new AddEmailDaemon(comp, "Final Task", os));
            });
            executor.RegisterExecutor("Computer.deathRowDatabase", (exec, info) =>
            {
                comp.daemons.Add(new DeathRowDatabaseDaemon(comp, "Death Row Database", os));
            });
            executor.RegisterExecutor("Computer.academicDatabase", (exec, info) =>
            {
                comp.daemons.Add(new AcademicDatabaseDaemon(comp, "International Academic Database", os));
            });
            executor.RegisterExecutor("Computer.ispSystem", (exec, info) =>
            {
                comp.daemons.Add(new ISPDaemon(comp, os));
            });
            executor.RegisterExecutor("Computer.messageBoard", (exec, info) =>
            {
                var messageBoard = new MessageBoardDaemon(comp, os)
                {
                    name = info.Attributes.GetString("name", "Anonymous"),
                    BoardName = info.Attributes.GetString("name", "Anonymous")
                };

                foreach (var message in info.Children.Where(x => x.Name == "thread"))
                {
                    var path = message.Content ?? "UNKNOWN";
                    if (Settings.IsInExtensionMode)
                        path = ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" + path;
                    else if (!path.StartsWith("Content/Missions/"))
                        path = "Content/Missions/" + path;
                    
                    messageBoard.AddThread(Utils.readEntireFile(path));
                }
                
                comp.daemons.Add(messageBoard);
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.addAvconDemoEndDaemon", (exec, info) =>
            {
                comp.daemons.Add(new AvconDemoEndDaemon(comp, "Demo End", os));
            });
            executor.RegisterExecutor("addWebServer", (exec, info) =>
            {
                var webServer = new WebServerDaemon(
                    comp,
                    info.Attributes.GetString("name", "Web Server"),
                    os,
                    info.Attributes.GetString("url", null)
                );
                webServer.registerAsDefaultBootDaemon();
                comp.daemons.Add(webServer);
            });
            executor.RegisterExecutor("Computer.addOnlineWebServer", (exec, info) =>
            {
                var onlineWebServer = new OnlineWebServerDaemon(
                    comp,
                    info.Attributes.GetString("name", "Web Server"),
                    os
                );
                if (info.Attributes.TryGetValue("url", out var url))
                {
                    onlineWebServer.setURL(url);
                }
                onlineWebServer.registerAsDefaultBootDaemon();
                comp.daemons.Add(onlineWebServer);
            });
            executor.RegisterExecutor("Computer.uploadServerDaemon", (exec, info) =>
            {
                var uploadServer = new UploadServerDaemon(
                    comp,
                    info.Attributes.GetString("name", "File Upload Server"),
                    info.Attributes.GetColor("color", new Color(0, 94, 38)).Value,
                    os,
                    info.Attributes.GetString("folder", null),
                    info.Attributes.GetBool("needsAuth")
                )
                {
                    hasReturnViewButton = info.Attributes.GetBool("hasReturnViewButton")
                };
                
                uploadServer.registerAsDefaultBootDaemon();
                comp.daemons.Add(uploadServer);
            });
            executor.RegisterExecutor("Computer.MedicalDatabase", (exec, info) =>
            {
                comp.daemons.Add(new MedicalDatabaseDaemon(comp, os));
            });
            executor.RegisterExecutor("Computer.HeartMonitor", (exec, info) =>
            {
                comp.daemons.Add(new HeartMonitorDaemon(comp, os)
                {
                    PatientID = info.Attributes.GetString("patient", "UNKNOWN")
                });
            });
            executor.RegisterExecutor("Computer.PointClicker", (exec, info) =>
            {
                comp.daemons.Add(new PointClickerDaemon(comp, "Point Clicker!", os));
            });
            executor.RegisterExecutor("Computer.SongChangerDaemon", (exec, info) =>
            {
                comp.daemons.Add(new SongChangerDaemon(comp, os));
            });
            executor.RegisterExecutor("Computer.DHSDaemon", (exec, info) =>
            {
                if (!DLC1SessionUpgrader.HasDLC1Installed)
                    throw new NotSupportedException("Labyrinths DLC must be installed for DHSDaemon!");

                var dlcHub = new DLCHubServer(
                    comp,
                    "DHS",
                    info.Attributes.GetString("groupName", "UNKNOWN"),
                    os
                )
                {
                    AddsFactionPointForMissionCompleteion = info.Attributes.GetBool("addsFactionPointOnMissionComplete", true),
                    AutoClearMissionsOnSingleComplete = info.Attributes.GetBool("autoClearMissionsOnPlayerComplete", true),
                    AllowContractAbbandon = info.Attributes.GetBool("allowContractAbbandon"),
                    themeColor = info.Attributes.GetColor("themeColor", new Color(38, 201, 155)).Value
                };
                
                foreach (var userInfo in info.Children.Where(x => x.Name.ToLower() == "user" || x.Name.ToLower() == "agent"))
                {
                    if (userInfo.Attributes.TryGetValue("name", out var name))
                    {
                        dlcHub.AddAgent(
                            name.Filter(),
                            userInfo.Attributes.GetString("pass", "password").Filter(),
                            userInfo.Attributes.GetColor("color", Color.LightGreen).Value
                        );
                    }
                }
                
                comp.daemons.Add(dlcHub);
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.CustomConnectDisplayDaemon", (exec, info) =>
            {
                comp.daemons.Add(new CustomConnectDisplayDaemon(comp, os));
            });
            executor.RegisterExecutor("Computer.DatabaseDaemon", (exec, info) =>
            {
                if (!DLC1SessionUpgrader.HasDLC1Installed)
                    throw new NotSupportedException("Labyrinths DLC must be installed for DatabaseDaemon!");
                
                comp.daemons.Add(new DatabaseDaemon(
                    comp,
                    os,
                    info.Attributes.GetString("Name", "Database"),
                    DatabaseDaemon.GetDatabasePermissionsFromString(info.Attributes.GetString("Permissions")),
                    info.Attributes.GetString("DataType", null),
                    info.Attributes.GetString("Foldername", null),
                    info.Attributes.GetColor("Color")
                )
                {
                    adminResetEmailHostID = info.Attributes.GetString("AdminEmailHostID", null),
                    adminResetPassEmailAccount = info.Attributes.GetString("AdminEmailAccount"),
                    Dataset = info.Children.Count == 0 ? null : info.Children.Cast<object>().ToList()
                });
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.WhitelistAuthenticatorDaemon", (exec, info) =>
            {
                comp.daemons.Add(new WhitelistConnectionDaemon(comp, os)
                {
                    RemoteSourceIP = info.Attributes.GetString("Remote", null),
                    AuthenticatesItself = info.Attributes.GetBool("SelfAuthenticating", true)
                });
            });
            executor.RegisterExecutor("Computer.MarkovTextDaemon", (exec, info) =>
            {
                comp.daemons.Add(new MarkovTextDaemon(
                    comp,
                    os,
                    info.Attributes.GetString("Name", null),
                    info.Attributes.GetString("SourceFilesContentFolder", null)
                ));
            });
            executor.RegisterExecutor("Computer.IRCDaemon", (exec, info) =>
            {
                var irc = new IRCDaemon(comp, os, info.Attributes.GetString("name", "IRC Server"))
                {
                    ThemeColor = info.Attributes.GetColor("themeColor", new Color(184, 2, 141)).Value,
                    RequiresLogin = info.Attributes.GetBool("needsLogin", false)
                };

                foreach (var child in info.Children)
                {
                    switch (child.Name.ToLower())
                    {
                        case "user":
                        case "agent":
                            if (child.Attributes.TryGetValue("name", out var name))
                            {
                                irc.UserColors.Add(name.Filter(), child.Attributes.GetColor("color", Color.LightGreen).Value);
                            }
                            break;
                        case "post":
                            if (child.Content != null && child.Attributes.TryGetValue("user", out var user))
                            {
                                irc.StartingMessages.Add(new KeyValuePair<string, string>(user.Filter(), child.Content.Filter()));
                            }
                            break;
                    }
                }
                
                comp.daemons.Add(irc);
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.AircraftDaemon", (exec, info) =>
            {
                if (!DLC1SessionUpgrader.HasDLC1Installed)
                    throw new NotSupportedException("Labrinths DLC has to be installed for aircraft daemon!");
                
                comp.daemons.Add(new AircraftDaemon(
                    comp,
                    os,
                    info.Attributes.GetString("Name", null),
                    info.Attributes.GetVector("OriginX", "OriginY", Vector2.Zero).Value,
                    info.Attributes.GetVector("DestX", "DestY", Vector2.One * 0.5f).Value,
                    info.Attributes.GetFloat("Progress", 0.5f)
                ));
            });
            executor.RegisterExecutor("Computer.LogoCustomConnectDisplayDaemon", (exec, info) =>
            {
                comp.daemons.Add(new LogoCustomConnectDisplayDaemon(
                    comp,
                    os,
                    info.Attributes.GetString("logo", null),
                    info.Attributes.GetString("title", null),
                    info.Attributes.GetBool("overdrawLogo"),
                    info.Attributes.GetString("buttonAlignment", null)
                ));
            });
            executor.RegisterExecutor("Computer.LogoDaemon", (exec, info) =>
            {
                comp.daemons.Add(new LogoDaemon(
                    comp,
                    os,
                    comp.name,
                    info.Attributes.GetBool("ShowsTitle", true),
                    info.Attributes.GetString("LogoImagePath", null)
                )
                {
                    TextColor = info.Attributes.GetColor("TextColor", Color.White).Value,
                    BodyText = info.Content
                });
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.DLCCredits", (exec, info) =>
            {
                DLCCreditsDaemon credits;
                string title, text = null;
                if (info.Attributes.TryGetValue("Title", out title) || info.Attributes.TryGetValue("ButtonText", out text))
                {
                    credits = new DLCCreditsDaemon(comp, os, title.Filter(), text.Filter());
                }
                else
                {
                    credits = new DLCCreditsDaemon(comp, os);
                }
                
                credits.ConditionalActionsToLoadOnButtonPress = info.Attributes.GetString("ConditionalActionSetToRunOnButtonPressPath", null);
                comp.daemons.Add(credits);
            });
            executor.RegisterExecutor("Computer.CreditsDaemon", (exec, info) =>
            {
                DLCCreditsDaemon credits;
                string title, text = null;
                if (info.Attributes.TryGetValue("Title", out title) || info.Attributes.TryGetValue("ButtonText", out text))
                {
                    credits = new DLCCreditsDaemon(comp, os, title.Filter(), text.Filter());
                }
                else
                {
                    credits = new DLCCreditsDaemon(comp, os);
                }
                
                credits.ConditionalActionsToLoadOnButtonPress = info.Attributes.GetString("ConditionalActionSetToRunOnButtonPressPath", null);
            });
            executor.RegisterExecutor("Computer.FastActionHost", (exec, info) =>
            {
                comp.daemons.Add(new FastActionHost(comp, os, comp.name));
            });
            executor.RegisterExecutor("Computer.PorthackHeart", (exec, info) =>
            {
                comp.daemons.Add(new PorthackHeartDaemon(comp, os));
            });
            executor.RegisterExecutor("Computer.Memory", (exec, info) => comp.Memory = ReplacementsCommon.LoadMemoryContents(info), ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.eosDevice", (exec, info) =>
            {
                eos = new Computer(
                    info.Attributes.GetString("name", "Unregistered eOS Device"),
                    NetworkMap.generateRandomIP(),
                    os.netMap.getRandomPosition(),
                    0,
                    5,
                    os
                )
                {
                    idName = info.Attributes.GetString("id", comp.idName + "_eos"),
                    icon = info.Attributes.GetString("icon", "ePhone"),
                    location = comp.location + Corporation.getNearbyNodeOffset(comp.location, Utils.random.Next(12), 12, os.netMap),
                    portsNeededForCrack = 2
                };
                
                ComputerLoader.loadPortsIntoComputer("22,3659", eos);
                eos.setAdminPassword(info.Attributes.GetString("passOverride", "alpine"));
                
                EOSComp.GenerateEOSFilesystem(eos);
                if (info.Attributes.GetBool("empty", false))
                {
                    var apps = eos.files.root.searchForFolder("eos").searchForFolder("apps");
                    apps?.folders.Clear();
                    apps?.files.Clear();
                }
                
                os.netMap.nodes.Add(eos);

                if (comp.attatchedDeviceIDs != null)
                    comp.attatchedDeviceIDs += ",";
                comp.attatchedDeviceIDs += eos.idName;
            });
            executor.RegisterExecutor("Computer.eosDevice.note", (exec, info) =>
            {
                var noteFile = info.Attributes.GetString("filename", null);
                var content = info.Content.TrimStart().Filter();
                var notesFolder = eos.files.root.searchForFolder("eos").searchForFolder("notes");

                if (noteFile == null)
                {
                    var firstNewline = content.IndexOf('\n');
                    if (firstNewline == -1)
                        firstNewline = content.Length;
                    noteFile = content.Substring(0, firstNewline);
                    if (noteFile.Length > 50)
                        noteFile = noteFile.Substring(0, 47) + "...";
                }
                notesFolder.files.Add(new FileEntry(content, noteFile));
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("Computer.eosDevice.mail", (exec, info) =>
            {
                var username = info.Attributes.GetString("username", null);
                var password = info.Attributes.GetString("pass", null);
                var mailFolder = eos.files.root.searchForFolder("eos").searchForFolder("mail");

                mailFolder.files.Add(new FileEntry(
                    $"MAIL ACCOUNT : {username}\nAccount   :{username}\nPassword :{password}\nLast Sync :{DateTime.Now.ToString()}\n\n{Computer.generateBinaryString(512)}",
                    username + ".act"
                ));
            });
            executor.RegisterExecutor("Computer.eosDevice.file", (exec, info) =>
            {
                eos.getFolderFromPath(info.Attributes.GetString("path", "home")).files.Add(new FileEntry(
                    (info.Content ?? "").Filter().TrimStart(),
                    info.Attributes.GetString("name", null)
                ));
            }, ParseOption.ParseInterior);

            foreach (var customDaemon in DaemonManager.CustomDaemons)
            {
                executor.RegisterExecutor("Computer." + customDaemon.Name, (exec, info) =>
                {
                    DaemonManager.TryLoadCustomDaemon(info, comp, os);
                });
            }

            if (!executor.TryParse(out var ex))
            {
                throw new FormatException($"{filename}: {ex.Message}", ex);
            }

            if (comp == null)
                return null;

            if (!preventInitDaemons)
                comp.initDaemons();
            if (!preventAddingToNetmap)
            {
                os.netMap.nodes.Add(comp);
                eos?.links.Add(os.netMap.nodes.Count - 1);
            }

            return comp;
        }
    }
}
