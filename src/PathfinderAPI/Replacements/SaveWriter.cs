using System.Globalization;
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
using Pathfinder.Event.Saving;
using Pathfinder.Port;
using Pathfinder.Util;

namespace Pathfinder.Replacements;

[HarmonyPatch]
public static class SaveWriter
{
    public static XElement GetHacknetSaveElement(OS os)
    {
        var result = new XElement("HacknetSave");
        result.SetAttributeValue("generatedMissionCount", MissionGenerator.generationCount);
        result.SetAttributeValue("Username", os.username);
        result.SetAttributeValue("Language", os.LanguageCreatedIn);
        result.SetAttributeValue("DLCMode", os.IsInDLCMode);
        result.SetAttributeValue("DisableEmailIcon", os.DisableEmailIcon);
        return result;
    }

    public static XElement GetActionSaveElement(SerializableAction action)
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
                contentValue = string.Format(CultureInfo.InvariantCulture, "{0}", [field.GetValue(action)]);
            }
            else
            {
                var fieldVal = string.Format(CultureInfo.InvariantCulture, "{0}", [field.GetValue(action)]);
                result.SetAttributeValue(field.Name, fieldVal);
            }
        }

        if (contentValue != null)
            result.Value = contentValue;

        return result;
    }

    public static XElement GetConditionSaveElement(SerializableCondition cond)
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
            var fieldVal = string.Format(CultureInfo.InvariantCulture, "{0}", [field.GetValue(cond)]);
            result.SetAttributeValue(field.Name, fieldVal);
        }

        return result;
    }

    public static XElement GetConditionalActionSetSaveElement(SerializableConditionalActionSet set)
    {
        var result = GetConditionSaveElement(set.Condition);
        for (var i = 0; i < set.Actions.Count; i++)
            result.Add(GetActionSaveElement(set.Actions[i]));
        return result;
    }

    public static XElement GetConditionalActionsSaveElement(RunnableConditionalActions actions)
    {
        var result = new XElement("ConditionalActions");
        foreach (var action in actions.Actions)
        {
            result.Add(GetConditionalActionSetSaveElement(action));
        }
        return result;
    }

    public static XElement GetDLCSaveElement(OS os)
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

    public static XElement GetFlagsSaveElement(ProgressionFlags flags)
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

    public static XElement GetNetmapVisibleNodesSaveElement(NetworkMap nmap)
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

    public static XElement GetFirewallSaveElement(Firewall firewall)
    {
        var result = new XElement("firewall");
        result.SetAttributeValue("complexity", firewall.complexity);
        result.SetAttributeValue("solution", firewall.solution);
        result.SetAttributeValue("additionalDelay", firewall.additionalDelay.ToString(CultureInfo.InvariantCulture));
        return result;
    }

    public static XElement GetPortSaveElement(Computer node)
    {
        var result = new XElement("ports");
        foreach (var port in node.GetAllPortStates())
        {
            result.Add(
                new XElement(port.Record.Protocol,
                    new XElement("Original", port.Record.OriginalPortNumber),
                    new XElement("Number", port.PortNumber),
                    new XElement("Display", port.DisplayName)
                ));
        }
        return result;
    }

    public static XElement GetUserDetailSaveElement(UserDetail user)
    {
        var result = new XElement("user");
        result.SetAttributeValue("name", user.name);
        result.SetAttributeValue("pass", user.pass);
        result.SetAttributeValue("type", user.type);
        result.SetAttributeValue("known", user.known);
        return result;
    }

    public static XElement GetMemoryContentsSaveElement(MemoryContents contents)
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

    public static XElement GetFolderSaveElement(Folder folder)
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

    public static XElement GetFilesystemSaveElement(FileSystem fs)
    {
        var result = new XElement("filesystem");
        result.Add(GetFolderSaveElement(fs.root));
        return result;
    }

    public static XElement GetDaemonSaveElement(object daemon)
    {
        XElement CreateDaemonElement(object daemonObj, string name, string[] fields, string[] attribNames)
        {
            var type = daemonObj.GetType();
            fields.ThrowNotSameSizeAs(nameof(fields), attribNames, nameof(attribNames));
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
                    ["name", "themeColor"],
                    ["name", "color"]
                );
                break;
            case MissionListingServer listServer:
                if (listServer.HasCustomColor)
                    result = CreateDaemonElement(daemon, "MissionListingServer",
                        [
                            "name", "groupName", "isPublic", "missionAssigner", "listingTitle", "IconReloadPath",
                            "themeColor", "ArticleFolderPath"
                        ],
                        ["name", "group", "public", "assign", "title", "icon", "color", "articles"]
                    );
                else
                    result = CreateDaemonElement(daemon, "MissionListingServer",
                        ["name", "groupName", "isPublic", "missionAssigner", "listingTitle"],
                        ["name", "group", "public", "assign", "title"]
                    );
                break;
            case AddEmailDaemon _:
                result = CreateDaemonElement(daemon, "AddEmailServer",
                    ["name"],
                    ["name"]
                );
                break;
            case MessageBoardDaemon _:
                result = CreateDaemonElement(daemon, "MessageBoard",
                    ["name", "BoardName"],
                    ["name", "boardName"]
                );
                break;
            case OnlineWebServerDaemon _:
                result = CreateDaemonElement(daemon, "OnlineWebServer",
                    ["name", "webURL"],
                    ["name", "url"]
                );
                break;
            case WebServerDaemon _:
                result = CreateDaemonElement(daemon, "WebServer",
                    ["name", "saveURL"],
                    ["name", "url"]
                );
                break;
            case AcademicDatabaseDaemon _:
                result = CreateDaemonElement(daemon, "AcademicDatabse",
                    ["name"],
                    ["name"]
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
                result = CreateDaemonElement(daemon, "HeartMonitor", ["PatientID"], ["patient"]);
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
                    ["name", "Foldername", "themeColor", "needsAuthentication", "hasReturnViewButton"],
                    ["name", "foldername", "color", "needsAuth", "hasReturnViewButton"]
                );
                break;
            case DLCHubServer _:
                result = CreateDaemonElement(daemon, "DHSDaemon", new string[0], new string[0]);
                break;
            case DatabaseDaemon database:
                result = CreateDaemonElement(daemon, "DatabaseDaemon",
                    ["name", "Permissions", "DataTypeIdentifier", "Foldername"],
                    ["Name", "Permissions", "DataType", "Foldername"]
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
                    ["AuthenticatesItself"],
                    ["SelfAuthenticating"]
                );
                break;
            case IRCDaemon _:
                result = CreateDaemonElement(daemon, "IRCDaemon", new string[0], new string[0]);
                break;
            case MarkovTextDaemon _:
                result = CreateDaemonElement(daemon, "MarkovTextDaemon",
                    ["name", "corpusFolderPath"],
                    ["Name", "SourceFilesContentFolder"]
                );
                break;
            case AircraftDaemon aircraft:
                result = CreateDaemonElement(daemon, "AircraftDaemon",
                    ["name", "FlightProgress"],
                    ["Name", "Progress"]
                );
                result.SetAttributeValue("OriginX", aircraft.mapOrigin.X);
                result.SetAttributeValue("OriginY", aircraft.mapOrigin.Y);
                result.SetAttributeValue("DestX", aircraft.mapDest.X);
                result.SetAttributeValue("DestY", aircraft.mapDest.Y);
                break;
            case LogoCustomConnectDisplayDaemon _:
                result = CreateDaemonElement(daemon, "LogoCustomConnectDisplayDaemon",
                    ["logoImageName", "titleImageName", "LogoShouldClipOverdraw", "buttonAlignmentName"],
                    ["logo", "title", "overdrawLogo", "buttonAlignment"]
                );
                break;
            case CustomConnectDisplayDaemon _:
                result = CreateDaemonElement(daemon, "CustomConnectDisplayDaemon", new string[0], new string[0]);
                break;
            case LogoDaemon _:
                result = CreateDaemonElement(daemon, "LogoDaemon",
                    ["LogoImagePath", "showsTitle", "TextColor", "name"],
                    ["LogoImagePath", "ShowsTitle", "TextColor", "Name"]
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

    public static XElement GetNodeSaveElement(Computer node)
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
        if (node.Equals(node.os.netMap.academicDatabase)) // academicDatabase can be null
        {
            spec = "academic";
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
        securityTag.SetAttributeValue("adminIP", node.adminIP);
        securityTag.SetAttributeValue("portsToCrack", node.portsNeededForCrack);
        result.Add(securityTag);
            
        result.Add(GetPortSaveElement(node));

        XElement adminTag = null;
        string adminType;
        switch (node.admin)
        {
            case null:
                adminType = "none";
                break;
            case BaseAdministrator pfAdmin:
                adminTag = pfAdmin.GetSaveElement();
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
        
        adminTag ??= new XElement("admin");

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

        var eventResult = EventManager<SaveComputerEvent>.InvokeAll(new SaveComputerEvent(node.os, node, result));
        return eventResult.Cancelled ? null : eventResult.Element;
    }

    public static XElement GetNetmapNodesSaveElement(NetworkMap nmap)
    {
        var result = new XElement("network");
        result.Add(nmap.nodes.Select(GetNodeSaveElement).Where(x => x != null).ToArray());
        return result;
    }

    public static XElement GetNetmapSaveElement(NetworkMap nmap)
    {
        var result = new XElement("NetworkMap");
        result.SetAttributeValue("sort", nmap.SortingAlgorithm);
        result.Add(GetNetmapVisibleNodesSaveElement(nmap));
        result.Add(GetNetmapNodesSaveElement(nmap));
        return result;
    }

    public static XElement GetMissionSaveElement(ActiveMission mission)
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

    public static XElement GetFactionSaveElement(Faction faction)
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

    public static XElement GetAllFactionsSaveElement(AllFactions factions)
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

            EventManager<SaveEvent>.InvokeAll(new SaveEvent(__instance, saveElement, filename));
                
            writer.WriteStartDocument();
                
            saveElement.WriteTo(writer);

            writer.WriteEndDocument();
        }
            
        SaveFileManager.WriteSaveData(builder.Replace("\t", "  ").ToString(), filename);

        return false;
    }
}
