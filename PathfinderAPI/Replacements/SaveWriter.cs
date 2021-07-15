using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Hacknet;
using Hacknet.Factions;
using Hacknet.Localization;
using Hacknet.PlatformAPI.Storage;
using Hacknet.Security;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.Event.Loading;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class SaveWriter
    {
        internal static XElement GetHacknetSaveElement(OS os)
        {
            XElement result = new XElement("HacknetSave");
            result.SetAttributeValue("generatedMissionCount", MissionGenerator.generationCount);
            result.SetAttributeValue("Username", os.username);
            result.SetAttributeValue("Language", os.LanguageCreatedIn);
            result.SetAttributeValue("DLCMode", os.IsInDLCMode);
            result.SetAttributeValue("DisableEmailIcon", os.DisableEmailIcon);
            return result;
        }

        internal static XElement GetActionSaveElement(SerializableAction action)
        {
            Type actionType = action.GetType();
            string saveTag = actionType.Name;
            if (saveTag.StartsWith("Hacknet."))
            {
                saveTag = saveTag.Substring("Hacknet.".Length);
            }
            if (saveTag.StartsWith("SA"))
            {
                saveTag = saveTag.Substring("SA".Length);
            }
            XElement result = new XElement(saveTag);
            string contentValue = null;
            FieldInfo[] fields = actionType.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (Utils.FieldContainsAttributeOfType(fields[i], typeof(XMLContentAttribute)))
                {
                    if (contentValue != null)
                    {
                        throw new InvalidOperationException("More than one field in object " + action.ToString() + " is a content serializable type");
                    }
                    contentValue = string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { fields[i].GetValue(action) });
                }
                else
                {
                    string fieldVal = string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { fields[i].GetValue(action) });
                    result.SetAttributeValue(fields[i].Name, fieldVal);
                }
            }

            return result;
        }

        internal static XElement GetConditionSaveElement(SerializableCondition cond)
        {
            Type condType = cond.GetType();
            string saveTag = condType.Name;
            if (saveTag.StartsWith("Hacknet."))
            {
                saveTag = saveTag.Substring("Hacknet.".Length);
            }
            if (saveTag.StartsWith("SC"))
            {
                saveTag = saveTag.Substring("SC".Length);
            }
            XElement result = new XElement(saveTag);
            FieldInfo[] fields = condType.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                string fieldVal = string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { fields[i].GetValue(cond) });
                result.SetAttributeValue(fields[i].Name, fieldVal);
            }

            return result;
        }

        internal static XElement GetConditionalActionSetSaveElement(SerializableConditionalActionSet set)
        {
            XElement result = GetConditionSaveElement(set.Condition);
            for (int i = 0; i < set.Actions.Count; i++)
                result.Add(GetActionSaveElement(set.Actions[i]));
            return result;
        }

        internal static XElement GetConditionalActionsSaveElement(RunnableConditionalActions actions)
        {
            XElement result = new XElement("ConditionalActions");
            for (int i = 0; i < actions.Actions.Count; i++)
            {
                result.Add(GetConditionalActionSetSaveElement(actions.Actions[i]));
            }
            return result;
        }

        internal static XElement GetDLCSaveElement(OS os)
        {
            XElement result = new XElement("DLC");
            result.SetAttributeValue("Active", os.IsInDLCMode.ToString());
            result.SetAttributeValue("LoadedContent", os.HasLoadedDLCContent.ToString());
            XElement DLCFlags = new XElement("Flags");
            DLCFlags.SetAttributeValue("OriginalFaction", os.PreDLCFaction);
            result.Add(DLCFlags);
            XElement OriginalNodes = new XElement("OriginalVisibleNodes");
            OriginalNodes.SetValue(os.PreDLCVisibleNodesCache);
            result.Add(OriginalNodes);
            result.Add(GetConditionalActionsSaveElement(os.ConditionalActions));
            return result;
        }

        internal static XElement GetFlagsSaveElement(ProgressionFlags flags)
        {
            XElement result = new XElement("Flags");
            StringBuilder flagsBuild = new StringBuilder();
            for (int i = 0; i < flags.Flags.Count; i++)
            {
                flagsBuild.Append(flags.Flags[i].Replace(",", "[%%COMMAREPLACED%%]"));
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
            XElement result = new XElement("visible");
            StringBuilder visible = new StringBuilder();
            for (int i = 0; i < nmap.visibleNodes.Count; i++)
            {
                visible.Append(nmap.visibleNodes[i] + ((i != nmap.visibleNodes.Count - 1) ? " " : ""));
            }
            result.SetValue(visible.ToString());
            return result;
        }

        internal static XElement GetFirewallSaveElement(Firewall firewall)
        {
            XElement result = new XElement("firewall");
            result.SetAttributeValue("complexity", firewall.complexity);
            result.SetAttributeValue("solution", firewall.solution);
            result.SetAttributeValue("additionalDelay", firewall.additionalDelay.ToString(CultureInfo.InvariantCulture));
            return result;
        }

        internal static XElement GetPortRemappingSaveElement(Dictionary<int, int> input)
        {
            if (input == null || input.Count == 0)
                return null;
            
            XElement result = new XElement("portRemap");
            StringBuilder remaps = new StringBuilder();
            foreach (KeyValuePair<int, int> item in input)
                remaps.Append(item.Key.ToString() + "=" + item.Value.ToString() + ",");
            remaps.Length--;
            result.SetValue(remaps.ToString());
            return result;
        }

        internal static XElement GetUserDetailSaveElement(UserDetail user)
        {
            XElement result = new XElement("user");
            result.SetAttributeValue("name", user.name);
            result.SetAttributeValue("pass", user.pass);
            result.SetAttributeValue("type", user.type);
            result.SetAttributeValue("known", user.known);
            return result;
        }

        internal static XElement GetMemoryContentsSaveElement(MemoryContents contents)
        {
            XElement result = new XElement("Memory");
            if (contents.DataBlocks != null && contents.DataBlocks.Count > 0)
            {
                XElement dataTag = new XElement("Data");
                for (int i = 0; i < contents.DataBlocks.Count; i++)
                {
                    XElement blockTag = new XElement("Block");
                    blockTag.SetValue(Folder.Filter(contents.DataBlocks[i]));
                    dataTag.Add(blockTag);
                }
                result.Add(dataTag);
            }
            if (contents.CommandsRun != null && contents.CommandsRun.Count > 0)
            {
                XElement dataTag = new XElement("Commands");
                for (int i = 0; i < contents.CommandsRun.Count; i++)
                {
                    XElement blockTag = new XElement("Command");
                    blockTag.SetValue(Folder.Filter(contents.CommandsRun[i]));
                    dataTag.Add(blockTag);
                }
                result.Add(dataTag);
            }
            if (contents.FileFragments != null && contents.FileFragments.Count > 0)
            {
                XElement dataTag = new XElement("FileFragments");
                for (int i = 0; i < contents.FileFragments.Count; i++)
                {
                    XElement blockTag = new XElement("File");
                    blockTag.SetAttributeValue("name", Folder.Filter(contents.FileFragments[i].Key));
                    blockTag.SetValue(Folder.Filter(contents.FileFragments[i].Value));
                    dataTag.Add(blockTag);
                }
                result.Add(dataTag);
            }
            if (contents.Images != null && contents.Images.Count > 0)
            {
                XElement dataTag = new XElement("Images");
                for (int i = 0; i < contents.Images.Count; i++)
                {
                    XElement blockTag = new XElement("Image");
                    blockTag.SetValue(Folder.Filter(contents.Images[i]));
                    dataTag.Add(blockTag);
                }
                result.Add(dataTag);
            }
            return result;
        }

        internal static XElement GetFolderSaveElement(Folder folder)
        {
            XElement result = new XElement("result");
            result.SetAttributeValue("name", Folder.Filter(folder.name));

            for (int i = 0; i < folder.folders.Count; i++)
                result.Add(GetFolderSaveElement(folder.folders[i]));
            
            for (int i = 0; i < folder.files.Count; i++)
            {
                XElement fileTag = new XElement("file");
                fileTag.SetAttributeValue("name", Folder.Filter(folder.files[i].name));
                fileTag.SetValue(Folder.Filter(folder.files[i].data));
                result.Add(fileTag);
            }

            return result;
        }

        internal static XElement GetFilesystemSaveElement(FileSystem fs)
        {
            XElement result = new XElement("filesystem");
            result.Add(GetFolderSaveElement(fs.root));
            return result;
        }

        internal static XElement GetNodeSaveElement(Computer node)
        {
            XElement result = new XElement("computer");
            result.SetAttributeValue("name", node.name);
            result.SetAttributeValue("ip", node.name);
            result.SetAttributeValue("type", node.type);
            string spec = "none";
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

            XElement locationTag = new XElement("location");
            locationTag.SetAttributeValue("x", node.location.X.ToString(CultureInfo.InvariantCulture));
            locationTag.SetAttributeValue("y", node.location.Y.ToString(CultureInfo.InvariantCulture));
            result.Add(locationTag);

            XElement securityTag = new XElement("security");
            securityTag.SetAttributeValue("level", node.securityLevel);
            securityTag.SetAttributeValue("traceTime", node.traceTime.ToString(CultureInfo.InvariantCulture));
            if (node.startingOverloadTicks > 0f)
            {
                securityTag.SetAttributeValue("proxyTime", (node.hasProxy ? node.startingOverloadTicks.ToString(CultureInfo.InvariantCulture) : "-1"));
            }
            securityTag.SetAttributeValue("portsToCrack", node.portsNeededForCrack);
            securityTag.SetAttributeValue("adminIP", node.adminIP);
            result.Add(securityTag);

            XElement adminTag = new XElement("admin");
            string adminType;
            if (node.admin == null)
                adminType = "none";
            else if (node.admin is FastBasicAdministrator)
                adminType = "fast";
            else if (node.admin is FastProgressOnlyAdministrator)
                adminType = "progress";
            else if (node.admin is BasicAdministrator)
                adminType = "basic";
            else
                adminType = node.admin.GetType().Name;

            adminTag.SetAttributeValue("type", adminType);
            adminTag.SetAttributeValue("resetPass", node.admin != null && node.admin.ResetsPassword);
            adminTag.SetAttributeValue("isSuper", node.admin != null && node.admin.IsSuper);
            result.Add(adminTag);


            XElement linksTag = new XElement("links");
            StringBuilder links = new StringBuilder();
            for (int i = 0; i < node.links.Count; i++)
                links.Append(" " + node.links[i]);
            linksTag.SetValue(links.ToString());
            result.Add(linksTag);

            if (node.firewall != null)
                result.Add(GetFirewallSaveElement(node.firewall));
            
            XElement portsOpenTag = new XElement("portsOpen");
            StringBuilder ports = new StringBuilder();
            for (int i = 0; i < node.portsOpen.Count; i++)
                ports.Append(" " + node.ports[i]);
            portsOpenTag.SetValue(ports);
            result.Add(portsOpenTag);

            XElement portRemaps = GetPortRemappingSaveElement(node.PortRemapping);
            if (portRemaps != null)
                result.Add(portRemaps);

            XElement usersTag = new XElement("users");
            for (int i = 0; i < node.users.Count; i++)
                usersTag.Add(GetUserDetailSaveElement(node.users[i]));
            result.Add(usersTag);

            if (node.Memory != null)
                result.Add(GetMemoryContentsSaveElement(node.Memory));

            
            XElement daemonsTag = new XElement("daemons");
            daemonsTag.SetValue("");
            for (int i = 0; i < node.daemons.Count; i++)
            {
                daemonsTag.SetValue(daemonsTag.Value + node.daemons[i].getSaveString() + "\n");
            }
            result.Add(daemonsTag);

            result.Add(GetFilesystemSaveElement(node.files));

            return result;
        }

        internal static XElement GetNetmapNodesSaveElement(NetworkMap nmap)
        {
            XElement result = new XElement("network");
            for (int i = 0; i < nmap.nodes.Count; i++)
            {
                result.Add(GetNodeSaveElement(nmap.nodes[i]));
            }
            return result;
        }

        internal static XElement GetNetmapSaveElement(NetworkMap nmap)
        {
            XElement result = new XElement("NetworkMap");
            result.SetAttributeValue("sort", nmap.SortingAlgorithm);
            result.Add(GetNetmapVisibleNodesSaveElement(nmap));
            result.Add(GetNetmapNodesSaveElement(nmap));
            return result;
        }

        internal static XElement GetMissionSaveElement(ActiveMission mission)
        {
            XElement result = new XElement("mission");
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

            XElement email = new XElement("email");
            email.SetAttributeValue("sender", Folder.Filter(mission.email.sender));
            email.SetAttributeValue("subject", Folder.Filter(mission.email.subject));
            email.SetValue(Folder.Filter(mission.email.body));
            result.Add(email);

            XElement endFunctionVal = new XElement("endFunc");
            endFunctionVal.SetAttributeValue("val", mission.endFunctionValue);
            endFunctionVal.SetAttributeValue("name", mission.endFunctionName);
            result.Add(endFunctionVal);

            XElement postingTag = new XElement("posting");
            postingTag.SetAttributeValue("title", Folder.Filter(mission.postingTitle));
            postingTag.SetValue(Folder.Filter(mission.postingBody));
            result.Add(postingTag);
            return result;
        }

        internal static XElement GetFactionSaveElement(Faction faction)
        {
            string tagName = "Faction";
            if (faction is EntropyFaction)
                tagName = "EntropyFaction";
            else if (faction is HubFaction)
                tagName = "HubFaction";
            else if (faction is CustomFaction)
                tagName = "CustomFaction";
            XElement result = new XElement(tagName);
            result.SetAttributeValue("name", faction.name);
            result.SetAttributeValue("id", faction.idName);
            result.SetAttributeValue("neededVal", faction.neededValue);
            result.SetAttributeValue("playerVal", faction.playerValue);
            result.SetAttributeValue("playerHasPassed", faction.playerHasPassedValue);
            return result;
        }

        internal static XElement GetAllFactionsSaveElement(AllFactions factions)
        {
            XElement result = new XElement("AllFactions");
            result.SetAttributeValue("current", factions.currentFaction);
            foreach (KeyValuePair<string, Faction> faction in factions.factions)
                result.Add(GetFactionSaveElement(faction.Value));
            return result;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OS), nameof(OS.writeSaveGame))]
        internal static bool SaveWriteReplacementPrefix(ref OS __instance, string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartDocument();

                GetHacknetSaveElement(__instance).WriteTo(writer);
                GetDLCSaveElement(__instance).WriteTo(writer);
                GetFlagsSaveElement(__instance.Flags).WriteTo(writer);
                GetNetmapSaveElement(__instance.netMap).WriteTo(writer);
                GetMissionSaveElement(__instance.currentMission).WriteTo(writer);

                XElement branchMissionsTag = new XElement("branchMissions");
                for (int i = 0; i < __instance.branchMissions.Count; i++)
                    branchMissionsTag.Add(GetMissionSaveElement(__instance.branchMissions[i]));
                branchMissionsTag.WriteTo(writer);

                GetAllFactionsSaveElement(__instance.allFactions).WriteTo(writer);
                XElement otherTag = new XElement("other");
                otherTag.SetAttributeValue("music", MusicManager.currentSongName);
                otherTag.SetAttributeValue("homeNode", __instance.homeNodeID);
                otherTag.SetAttributeValue("homeAssetsNode", __instance.homeAssetServerID);
                otherTag.WriteTo(writer);

                writer.WriteEndDocument();
            }
            SaveFileManager.WriteSaveData(builder.ToString(), filename);
            return false;
        }
    }
}
