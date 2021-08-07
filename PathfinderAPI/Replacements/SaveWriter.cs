using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Hacknet;
using Hacknet.Factions;
using Hacknet.PlatformAPI.Storage;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathfinder.Action;
using Pathfinder.Administrator;
using Pathfinder.Daemon;
using Pathfinder.Event;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class SaveWriter
    {
        internal static XElement GetHacknetSaveElement(OS os)
        {
            var result = new XElement("HacknetSave");
            result.SetAttributeValue("generatedMissionCount", MissionGenerator.generationCount);
            result.SetAttributeValue("Username", os.username);
            result.SetAttributeValue("Language", os.LanguageCreatedIn);
            result.SetAttributeValue("DLCMode", os.IsInDLCMode);
            result.SetAttributeValue("DisableEmailIcon", os.DisableEmailIcon);
            return result;
        }

        internal static XElement GetActionSaveElement(SerializableAction action)
        {
            if (action is PathfinderAction pfAction)
            {
                return pfAction.GetSaveElement();
            }
            
            var actionType = action.GetType();
            var saveTag = actionType.Name;
            if (saveTag.StartsWith("Hacknet."))
            {
                saveTag = saveTag.Substring("Hacknet.".Length);
            }
            if (saveTag.StartsWith("SA"))
            {
                saveTag = saveTag.Substring("SA".Length);
            }
            var result = new XElement(saveTag);
            string contentValue = null;
            var fields = actionType.GetFields();
            foreach (var field in fields)
            {
                if (Utils.FieldContainsAttributeOfType(field, typeof(XMLContentAttribute)))
                {
                    if (contentValue != null)
                    {
                        throw new InvalidOperationException("More than one field in object " + action.ToString() + " is a content serializable type");
                    }
                    contentValue = string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { field.GetValue(action) });
                }
                else
                {
                    var fieldVal = string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { field.GetValue(action) });
                    result.SetAttributeValue(field.Name, fieldVal);
                }
            }

            if (contentValue != null)
                result.Value = contentValue;

            return result;
        }

        internal static XElement GetConditionSaveElement(SerializableCondition cond)
        {
            if (cond is PathfinderCondition pfCond)
            {
                return pfCond.GetSaveElement();
            }
            
            var condType = cond.GetType();
            var saveTag = condType.Name;
            if (saveTag.StartsWith("Hacknet."))
            {
                saveTag = saveTag.Substring("Hacknet.".Length);
            }
            if (saveTag.StartsWith("SC"))
            {
                saveTag = saveTag.Substring("SC".Length);
            }
            var result = new XElement(saveTag);
            var fields = condType.GetFields();
            foreach (var field in fields)
            {
                var fieldVal = string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { field.GetValue(cond) });
                result.SetAttributeValue(field.Name, fieldVal);
            }

            return result;
        }

        internal static XElement GetConditionalActionSetSaveElement(SerializableConditionalActionSet set)
        {
            var result = GetConditionSaveElement(set.Condition);
            for (var i = 0; i < set.Actions.Count; i++)
                result.Add(GetActionSaveElement(set.Actions[i]));
            return result;
        }

        internal static XElement GetConditionalActionsSaveElement(RunnableConditionalActions actions)
        {
            var result = new XElement("ConditionalActions");
            foreach (var action in actions.Actions)
            {
                result.Add(GetConditionalActionSetSaveElement(action));
            }
            return result;
        }

        internal static XElement GetDLCSaveElement(OS os)
        {
            var result = new XElement("DLC");
            result.SetAttributeValue("Active", os.IsInDLCMode.ToString());
            result.SetAttributeValue("LoadedContent", os.HasLoadedDLCContent.ToString());
            var DLCFlags = new XElement("Flags");
            DLCFlags.SetAttributeValue("OriginalFaction", os.PreDLCFaction);
            result.Add(DLCFlags);
            var OriginalNodes = new XElement("OriginalVisibleNodes");
            OriginalNodes.SetValue(os.PreDLCVisibleNodesCache);
            result.Add(OriginalNodes);
            result.Add(GetConditionalActionsSaveElement(os.ConditionalActions));
            return result;
        }

        internal static XElement GetFlagsSaveElement(ProgressionFlags flags)
        {
            var result = new XElement("Flags");
            var flagsBuild = new StringBuilder();
            foreach (var flag in flags.Flags)
            {
                flagsBuild.Append(flag.Replace(",", "[%%COMMAREPLACED%%]"));
                flagsBuild.Append(",");
            }
            if (flagsBuild.Length > 0)
			{
				flagsBuild.Length--;
			}
            result.SetValue(flagsBuild.ToString());
            return result;
        }

        internal static XElement GetNetmapVisibleNodesSaveElement(NetworkMap nmap)
        {
            var result = new XElement("visible");
            var visible = new StringBuilder();
            for (var i = 0; i < nmap.visibleNodes.Count; i++)
            {
                visible.Append(nmap.visibleNodes[i] + ((i != nmap.visibleNodes.Count - 1) ? " " : ""));
            }
            result.SetValue(visible.ToString());
            return result;
        }

        internal static XElement GetFirewallSaveElement(Firewall firewall)
        {
            var result = new XElement("firewall");
            result.SetAttributeValue("complexity", firewall.complexity);
            result.SetAttributeValue("solution", firewall.solution);
            result.SetAttributeValue("additionalDelay", firewall.additionalDelay.ToString(CultureInfo.InvariantCulture));
            return result;
        }

        internal static XElement GetPortRemappingSaveElement(Dictionary<int, int> input)
        {
            if (input == null || input.Count == 0)
                return null;
            
            var result = new XElement("portRemap");
            var remaps = new StringBuilder();
            foreach (var item in input)
                remaps.Append(item.Key.ToString() + "=" + item.Value.ToString() + ",");
            remaps.Length--;
            result.SetValue(remaps.ToString());
            return result;
        }

        internal static XElement GetUserDetailSaveElement(UserDetail user)
        {
            var result = new XElement("user");
            result.SetAttributeValue("name", user.name);
            result.SetAttributeValue("pass", user.pass);
            result.SetAttributeValue("type", user.type);
            result.SetAttributeValue("known", user.known);
            return result;
        }

        internal static XElement GetMemoryContentsSaveElement(MemoryContents contents)
        {
            var result = new XElement("Memory");
            if (contents.DataBlocks != null && contents.DataBlocks.Count > 0)
            {
                var dataTag = new XElement("Data");
                foreach (var dataBlock in contents.DataBlocks)
                {
                    var blockTag = new XElement("Block");
                    blockTag.SetValue(dataBlock);
                    dataTag.Add(blockTag);
                }
                result.Add(dataTag);
            }
            if (contents.CommandsRun != null && contents.CommandsRun.Count > 0)
            {
                var dataTag = new XElement("Commands");
                foreach (var command in contents.CommandsRun)
                {
                    var blockTag = new XElement("Command");
                    blockTag.SetValue(command);
                    dataTag.Add(blockTag);
                }
                result.Add(dataTag);
            }
            if (contents.FileFragments != null && contents.FileFragments.Count > 0)
            {
                var dataTag = new XElement("FileFragments");
                foreach (var fileFrag in contents.FileFragments)
                {
                    var blockTag = new XElement("File");
                    blockTag.SetAttributeValue("name", fileFrag.Key);
                    blockTag.SetValue(fileFrag.Value);
                    dataTag.Add(blockTag);
                }
                result.Add(dataTag);
            }
            if (contents.Images != null && contents.Images.Count > 0)
            {
                var dataTag = new XElement("Images");
                foreach (var image in contents.Images)
                {
                    var blockTag = new XElement("Image");
                    blockTag.SetValue(image);
                    dataTag.Add(blockTag);
                }
                result.Add(dataTag);
            }
            return result;
        }

        internal static XElement GetFolderSaveElement(Folder folder)
        {
            var result = new XElement("folder");
            result.SetAttributeValue("name", folder.name);

            foreach (var internalFolder in folder.folders)
                result.Add(GetFolderSaveElement(internalFolder));

            foreach (var file in folder.files)
            {
                var fileTag = new XElement("file");
                fileTag.SetAttributeValue("name", file.name);
                fileTag.SetValue(file.data);
                result.Add(fileTag);
            }

            return result;
        }

        internal static XElement GetFilesystemSaveElement(FileSystem fs)
        {
            var result = new XElement("filesystem");
            result.Add(GetFolderSaveElement(fs.root));
            return result;
        }

        internal static XElement GetDaemonSaveElement(object daemon)
        {
            XElement CreateDaemonElement(object daemonObj, string name, string[] fields, string[] attribNames)
            {
                var type = daemonObj.GetType();
                if (fields.Length != attribNames.Length)
                    throw new ArgumentException("fields and attribNames arrays must be the same length");
                
                var daemonElement = new XElement(name);
                for (int i = 0; i < fields.Length; i++)
                {
                    var content = AccessTools.Field(type, fields[i]);
                    string contentString = null;
                    if (content.FieldType == typeof(Color))
                        contentString = Utils.convertColorToParseableString((Color) content.GetValue(daemonObj));
                    else
                        contentString = content.GetValue(daemonObj)?.ToString() ?? "";
                    daemonElement.SetAttributeValue(attribNames[i], contentString);
                }

                return daemonElement;
            }

            XElement result = null;

            if (daemon is BaseDaemon pfDaemon)
            {
                return pfDaemon.GetSaveElement();
            }
            
            switch (daemon)
            {
                case MailServer _:
                    result = CreateDaemonElement(daemon, "MailServer", 
                        new[] {"name", "themeColor"}, 
                        new[] {"name", "color"}
                    );
                    break;
                case MissionListingServer listServer:
                    if (listServer.HasCustomColor)
                        result = CreateDaemonElement(daemon, "MissionListingServer",
                            new[]
                            {
                                "name", "groupName", "isPublic", "missionAssigner", "listingTitle", "IconReloadPath",
                                "themeColor", "ArticleFolderPath"
                            },
                            new[] {"name", "group", "public", "assign", "title", "icon", "color", "articles"}
                        );
                    else
                        result = CreateDaemonElement(daemon, "MissionListingServer",
                            new[] {"name", "groupName", "isPublic", "missionAssigner", "listingTitle"},
                            new[] {"name", "group", "public", "assign", "title"}
                        );
                    break;
                case AddEmailDaemon _:
                    result = CreateDaemonElement(daemon, "AddEmailServer",
                        new[] {"name",},
                        new[] {"name"}
                    );
                    break;
                case MessageBoardDaemon _:
                    result = CreateDaemonElement(daemon, "MessageBoard",
                        new[] {"name", "BoardName"},
                        new[] {"name", "boardName"}
                    );
                    break;
                case OnlineWebServerDaemon _:
                    result = CreateDaemonElement(daemon, "OnlineWebServer",
                        new[] {"name", "webURL"},
                        new[] {"name", "url"}
                    );
                    break;
                case WebServerDaemon _:
                    result = CreateDaemonElement(daemon, "WebServer",
                        new[] {"name", "saveURL"},
                        new[] {"name", "url"}
                    );
                    break;
                case AcademicDatabaseDaemon _:
                    result = CreateDaemonElement(daemon, "AcademicDatabse",
                        new[] {"name"},
                        new[] {"name"}
                    );
                    break;
                case MissionHubServer _:
                    result = CreateDaemonElement(daemon, "MissionHubServer", new string[0], new string[0]);
                    break;
                case DeathRowDatabaseDaemon _:
                    result = CreateDaemonElement(daemon, "DeathRowDatabase", new string[0], new string[0]);
                    break;
                case MedicalDatabaseDaemon _:
                    result = CreateDaemonElement(daemon, "MedicalDatabase", new string[0], new string[0]);
                    break;
                case HeartMonitorDaemon _:
                    result = CreateDaemonElement(daemon, "HeartMonitor", new[] {"PatientID"}, new[] {"patient"});
                    break;
                case PointClickerDaemon _:
                    result = CreateDaemonElement(daemon, "PointClicker", new string[0], new string[0]);
                    break;
                case ISPDaemon _:
                    result = CreateDaemonElement(daemon, "ispSystem", new string[0], new string[0]);
                    break;
                case PorthackHeartDaemon _:
                    result = CreateDaemonElement(daemon, "porthackheart", new string[0], new string[0]);
                    break;
                case SongChangerDaemon _:
                    result = CreateDaemonElement(daemon, "SongChangerDaemon", new string[0], new string[0]);
                    break;
                case UploadServerDaemon _:
                    result = CreateDaemonElement(daemon, "UploadServerDaemon",
                        new[] {"name", "Foldername", "themeColor", "needsAuthentication", "hasReturnViewButton"},
                        new[] {"name", "foldername", "color", "needsAuth", "hasReturnViewButton"}
                    );
                    break;
                case DLCHubServer _:
                    result = CreateDaemonElement(daemon, "DHSDaemon", new string[0], new string[0]);
                    break;
                case DatabaseDaemon database:
                    result = CreateDaemonElement(daemon, "DatabaseDaemon",
                        new[] {"name", "Permissions", "DataTypeIdentifier", "Foldername"},
                        new[] {"Name", "Permissions", "DataType", "Foldername"}
                    );
                    if (database.HadThemeColorApplied)
                    {
                        result.SetAttributeValue("Color", Utils.convertColorToParseableString(database.ThemeColor));
                    }
                    if (database.adminResetPassEmailAccount.HasContent())
                    {
                        result.SetAttributeValue("AdminEmailAccount", database.adminResetPassEmailAccount);
                        result.SetAttributeValue("AdminEmailHostID", database.adminResetEmailHostID);
                    }

                    break;
                case WhitelistConnectionDaemon _:
                    result = CreateDaemonElement(daemon, "WhitelistAuthenticatorDaemon", 
                        new[] {"AuthenticatesItself"},
                        new[] {"SelfAuthenticating"}
                    );
                    break;
                case IRCDaemon _:
                    result = CreateDaemonElement(daemon, "IRCDaemon", new string[0], new string[0]);
                    break;
                case MarkovTextDaemon _:
                    result = CreateDaemonElement(daemon, "MarkovTextDaemon",
                        new[] {"name", "corpusFolderPath"},
                        new[] {"Name", "SourceFilesContentFolder"}
                    );
                    break;
                case AircraftDaemon aircraft:
                    result = CreateDaemonElement(daemon, "AircraftDaemon",
                        new[] {"name", "FlightProgress"},
                        new[] {"Name", "Progress"}
                    );
                    result.SetAttributeValue("OriginX", aircraft.mapOrigin.X);
                    result.SetAttributeValue("OriginY", aircraft.mapOrigin.Y);
                    result.SetAttributeValue("DestX", aircraft.mapDest.X);
                    result.SetAttributeValue("DestY", aircraft.mapDest.Y);
                    break;
                case LogoCustomConnectDisplayDaemon _:
                    result = CreateDaemonElement(daemon, "LogoCustomConnectDisplayDaemon",
                        new[] {"logoImageName", "titleImageName", "LogoShouldClipOverdraw", "buttonAlignmentName"},
                        new[] {"logo", "title", "overdrawLogo", "buttonAlignment"}
                    );
                    break;
                case CustomConnectDisplayDaemon _:
                    result = CreateDaemonElement(daemon, "CustomConnectDisplayDaemon", new string[0], new string[0]);
                    break;
                case LogoDaemon _:
                    result = CreateDaemonElement(daemon, "LogoDaemon",
                        new[] {"LogoImagePath", "showsTitle", "TextColor", "name"},
                        new[] {"LogoImagePath", "ShowsTitle", "TextColor", "Name"}
                    );
                    break;
                case DLCCreditsDaemon credits:
                    result = CreateDaemonElement(daemon, "DLCCredits", new string[0], new string[0]);
                    if (credits.OverrideTitle != null)
                    {
                        result.SetAttributeValue("Title", credits.OverrideTitle);
                    }
                    if (credits.OverrideButtonText != null)
                    {
                        result.SetAttributeValue("Button", credits.OverrideButtonText);
                    }
                    if (credits.ConditionalActionsToLoadOnButtonPress != null)
                    {
                        result.SetAttributeValue("Action", credits.ConditionalActionsToLoadOnButtonPress);
                    }

                    break;
                case FastActionHost fah:
                    result = CreateDaemonElement(daemon, "FastActionHost", new string[0], new string[0]);
                    fah.folder.files = fah.DelayedActions.GetAllFilesForActions();
                    break;
            }

            return result;
        }

        internal static XElement GetNodeSaveElement(Computer node)
        {
            var result = new XElement("computer");
            result.SetAttributeValue("name", node.name);
            result.SetAttributeValue("ip", node.ip);
            result.SetAttributeValue("type", node.type);
            var spec = "none";
            if (node.os.netMap.mailServer.Equals(node))
            {
                spec = "mail";
            }
            if (node.os.thisComputer.Equals(node))
            {
                spec = "player";
            }
            result.SetAttributeValue("spec", spec);
            result.SetAttributeValue("id", node.idName);
            if (node.icon != null)
            {
                result.SetAttributeValue("icon", node.icon);
            }
            if (node.attatchedDeviceIDs != null)
            {
                result.SetAttributeValue("devices", node.attatchedDeviceIDs);
            }
            if (node.HasTracker)
            {
                result.SetAttributeValue("tracker", "true");
            }

            var locationTag = new XElement("location");
            locationTag.SetAttributeValue("x", node.location.X.ToString(CultureInfo.InvariantCulture));
            locationTag.SetAttributeValue("y", node.location.Y.ToString(CultureInfo.InvariantCulture));
            result.Add(locationTag);

            var securityTag = new XElement("security");
            securityTag.SetAttributeValue("level", node.securityLevel);
            securityTag.SetAttributeValue("traceTime", node.traceTime.ToString(CultureInfo.InvariantCulture));
            if (node.startingOverloadTicks > 0f)
            {
                securityTag.SetAttributeValue("proxyTime", (node.hasProxy ? node.startingOverloadTicks.ToString(CultureInfo.InvariantCulture) : "-1"));
            }
            securityTag.SetAttributeValue("portsToCrack", node.portsNeededForCrack);
            securityTag.SetAttributeValue("adminIP", node.adminIP);
            result.Add(securityTag);

            var adminTag = new XElement("admin");
            string adminType;
            switch (node.admin)
            {
                case null:
                    adminType = "none";
                    break;
                case BaseAdministrator pfAdmin:
                    adminTag = XMLStorageAttribute.WriteToElement(pfAdmin);
                    adminType = node.admin.GetType().Name;
                    break;
                case FastBasicAdministrator _:
                    adminType = "fast";
                    break;
                case FastProgressOnlyAdministrator _:
                    adminType = "progress";
                    break;
                case BasicAdministrator _:
                    adminType = "basic";
                    break;
                default:
                    adminType = node.admin.GetType().Name;
                    break;
            }

            adminTag.SetAttributeValue("type", adminType);
            adminTag.SetAttributeValue("resetPass", node.admin?.ResetsPassword ?? false);
            adminTag.SetAttributeValue("isSuper", node.admin?.IsSuper ?? false);
            result.Add(adminTag);


            var linksTag = new XElement("links");
            var links = new StringBuilder();
            foreach (var link in node.links)
                links.Append(" " + link);

            linksTag.SetValue(links.ToString());
            result.Add(linksTag);

            if (node.firewall != null)
                result.Add(GetFirewallSaveElement(node.firewall));
            
            var portsOpenTag = new XElement("portsOpen");
            var ports = new StringBuilder();
            for (var i = 0; i < node.portsOpen.Count; i++)
                ports.Append(" " + node.ports[i]);
            portsOpenTag.SetValue(ports);
            result.Add(portsOpenTag);

            var portRemaps = GetPortRemappingSaveElement(node.PortRemapping);
            if (portRemaps != null)
                result.Add(portRemaps);

            var usersTag = new XElement("users");
            foreach (var detail in node.users)
                usersTag.Add(GetUserDetailSaveElement(detail));

            result.Add(usersTag);

            if (node.Memory != null)
                result.Add(GetMemoryContentsSaveElement(node.Memory));

            
            var daemonsTag = new XElement("daemons");
            
            daemonsTag.Add(node.daemons.Select(GetDaemonSaveElement).ToArray());
            
            result.Add(daemonsTag);

            result.Add(GetFilesystemSaveElement(node.files));

            return result;
        }

        internal static XElement GetNetmapNodesSaveElement(NetworkMap nmap)
        {
            var result = new XElement("network");
            foreach (var node in nmap.nodes)
            {
                result.Add(GetNodeSaveElement(node));
            }
            return result;
        }

        internal static XElement GetNetmapSaveElement(NetworkMap nmap)
        {
            var result = new XElement("NetworkMap");
            result.SetAttributeValue("sort", nmap.SortingAlgorithm);
            result.Add(GetNetmapVisibleNodesSaveElement(nmap));
            result.Add(GetNetmapNodesSaveElement(nmap));
            return result;
        }

        internal static XElement GetMissionSaveElement(ActiveMission mission)
        {
            var result = new XElement("mission");
            if (mission == null) {
                result.SetAttributeValue("next", "NULL_MISSION");
                result.SetAttributeValue("goals", "none");
                result.SetAttributeValue("activeCheck", "none");
                return result;
            }
            result.SetAttributeValue("next", mission.nextMission);
            result.SetAttributeValue("goals", mission.reloadGoalsSourceFile);
            result.SetAttributeValue("reqRank", mission.requiredRank);

            if (mission.wasAutoGenerated)
            {
                result.SetAttributeValue("genTarget", mission.genTarget);
                result.SetAttributeValue("genFile", mission.genFile);
                result.SetAttributeValue("genPath", mission.genPath);
                result.SetAttributeValue("genTargetName", mission.genTargetName);
                result.SetAttributeValue("genOther", mission.genOther);
            }

            result.SetAttributeValue("activeCheck", mission.activeCheck);

            var email = new XElement("email");
            email.SetAttributeValue("sender", mission.email.sender);
            email.SetAttributeValue("subject", mission.email.subject);
            email.SetValue(mission.email.body);
            result.Add(email);

            var endFunctionVal = new XElement("endFunc");
            endFunctionVal.SetAttributeValue("val", mission.endFunctionValue);
            endFunctionVal.SetAttributeValue("name", mission.endFunctionName);
            result.Add(endFunctionVal);

            var postingTag = new XElement("posting");
            postingTag.SetAttributeValue("title", mission.postingTitle);
            postingTag.SetValue(mission.postingBody);
            result.Add(postingTag);
            return result;
        }

        internal static XElement GetFactionSaveElement(Faction faction)
        {
            var tagName = "Faction";
            switch (faction)
            {
                case EntropyFaction _:
                    tagName = "EntropyFaction";
                    break;
                case HubFaction _:
                    tagName = "HubFaction";
                    break;
                case CustomFaction _:
                    tagName = "CustomFaction";
                    break;
            }
            var result = new XElement(tagName);
            result.SetAttributeValue("name", faction.name);
            result.SetAttributeValue("id", faction.idName);
            result.SetAttributeValue("neededVal", faction.neededValue);
            result.SetAttributeValue("playerVal", faction.playerValue);
            result.SetAttributeValue("playerHasPassed", faction.playerHasPassedValue);

            if (faction is CustomFaction customFaction)
            {
                foreach (var action in customFaction.CustomActions)
                {
                    var actionElement = new XElement("Action");
                    actionElement.SetAttributeValue("ValueRequired", action.ValueRequiredForTrigger);
                    if (action.FlagsRequiredForTrigger != null)
                    {
                        actionElement.SetAttributeValue("Flags", action.FlagsRequiredForTrigger);
                    }
                    
                    actionElement.Add(action.TriggerActions.Select(GetActionSaveElement).ToArray());
                    
                    result.Add(actionElement);
                }
            }
            
            return result;
        }

        internal static XElement GetAllFactionsSaveElement(AllFactions factions)
        {
            var result = new XElement("AllFactions");
            result.SetAttributeValue("current", factions.currentFaction);
            foreach (var faction in factions.factions)
                result.Add(GetFactionSaveElement(faction.Value));
            return result;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), nameof(OS.writeSaveGame))]
        internal static bool SaveWriteReplacementPrefix(ref OS __instance, string filename)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true
            };
            var builder = new StringBuilder();
            
            using (var writer = XmlWriter.Create(builder, settings))
            {
                var saveElement = GetHacknetSaveElement(__instance);
                saveElement.Add(GetDLCSaveElement(__instance));
                saveElement.Add(GetFlagsSaveElement(__instance.Flags));
                saveElement.Add(GetNetmapSaveElement(__instance.netMap));
                saveElement.Add(GetMissionSaveElement(__instance.currentMission));

                var branchMissionsTag = new XElement("branchMissions");
                foreach (var branch in __instance.branchMissions)
                    branchMissionsTag.Add(GetMissionSaveElement(branch));

                saveElement.Add(branchMissionsTag);

                saveElement.Add(GetAllFactionsSaveElement(__instance.allFactions));
                
                var otherTag = new XElement("other");
                otherTag.SetAttributeValue("music", MusicManager.currentSongName);
                otherTag.SetAttributeValue("homeNode", __instance.homeNodeID);
                otherTag.SetAttributeValue("homeAssetsNode", __instance.homeAssetServerID);
                saveElement.Add(otherTag);

                EventManager<SaveEvent>.InvokeAll(new SaveEvent(saveElement, filename));
                
                writer.WriteStartDocument();
                
                saveElement.WriteTo(writer);

                writer.WriteEndDocument();
            }
            
            SaveFileManager.WriteSaveData(builder.Replace("\t", "  ").ToString(), filename);

            return false;
        }
    }
}
