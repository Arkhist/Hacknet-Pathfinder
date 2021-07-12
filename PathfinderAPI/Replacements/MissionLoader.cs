using System;
using System.Collections.Generic;
using Hacknet;
using Hacknet.Extensions;
using Hacknet.Mission;
using HarmonyLib;
using Pathfinder.Mission;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    public static class MissionLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ComputerLoader), nameof(ComputerLoader.readMission))]
        private static bool ReadMissionPrefix(string filename, out object __result)
        {
            __result = LoadContentMission(filename);
            return false;
        }

        public static ActiveMission LoadContentMission(string filename)
        {
            if (ComputerLoader.MissionPreLoadComplete != null)
                ComputerLoader.MissionPreLoadComplete();
            
            var mission = new ActiveMission(new List<MisisonGoal>(), null, default)
            {
                willSendEmail = true
            };
            var branches = new List<ActiveMission>();
            mission.email.attachments = new List<string>();
            
            var executor = new EventExecutor(LocalizedFileLoader.GetLocalizedFilepath(filename), true);
            
            executor.RegisterExecutor("mission", (exec, info) =>
            {
                mission.activeCheck = info.Attributes.GetBool("activeCheck");
                mission.ShouldIgnoreSenderVerification = info.Attributes.GetBool("shouldIgnoreSenderVerification");
            });
            executor.RegisterExecutor("mission.goals.goal", (exec, info) => mission.goals.Add(LoadGoal(info)), ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.missionStart", (exec, info) =>
            {
                var function = info.Content ?? throw new FormatException("missionStart with no mission function!");
                var val = info.Attributes.GetInt("val", 1);
                if (info.Attributes.GetBool("suppress", Settings.IsInExtensionMode))
                {
                    mission.addStartFunction(val, function);
                }
                else
                {
                    MissionFunctions.runCommand(val, function);
                }
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.missionEnd", (exec, info) =>
            {
                mission.addEndFunction(info.Attributes.GetInt("val", 1), info.Content ?? throw new FormatException("missionEnd with no mission function!"));
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.nextMission", (exec, info) =>
            {
                mission.willSendEmail = !info.Attributes.GetBool("IsSilent", false);
                mission.nextMission = info.Content ?? "NONE";
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.branchMissions.branch", (exec, info) =>
            {
                var filePrefix = Settings.IsInExtensionMode ? ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" : "Content/Missions/";
                branches.Add(LoadContentMission(filePrefix + info.Content));
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.branchMissions", (exec, info) => OS.currentInstance.branchMissions = branches, ParseOption.FireOnEnd);
            executor.RegisterExecutor("mission.posting", (exec, info) =>
            {
                mission.postingTitle = info.Attributes.GetString("title", "UNKNOWN");
                mission.postingBody = info.Content ?? "UNKNOWN";
                mission.postingAcceptFlagRequirements = info.Attributes.GetString("reqs").Split(Utils.commaDelim, StringSplitOptions.RemoveEmptyEntries);
                mission.requiredRank = info.Attributes.GetInt("requiredRank");
                mission.difficulty = info.Attributes.GetInt("difficulty");
                mission.client = info.Attributes.GetString("client");
                mission.target = info.Attributes.GetString("target");
            }, ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.email.sender", (exec, info) => mission.email.sender = info.Content.Filter(), ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.email.subject", (exec, info) => mission.email.subject = info.Content.Filter(), ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.email.body", (exec, info) => mission.email.body = info.Content.Filter(), ParseOption.ParseInterior);
            executor.RegisterExecutor("mission.email.attachments.link", (exec, info) =>
            {
                var comp = info.Attributes.GetComp("comp");
                mission.email.attachments.Add($"link#%#{comp.name}#%#{comp.ip}");
            });
            executor.RegisterExecutor("mission.email.attachments.account", (exec, info) =>
            {
                var comp = info.Attributes.GetComp("comp");
                mission.email.attachments.Add(
                    $"account#%#{comp.name}#%#{comp.ip}#%#{info.Attributes.GetString("user", "UNKNOWN")}#%#{info.Attributes.GetString("pass", "UNKNOWN")}"
                );
            });
            executor.RegisterExecutor("mission.email.attachments.note", (exec, info) =>
            {
                mission.email.attachments.Add($"note#%#{info.Attributes.GetString("title", "Data")}#%#{info.Content ?? "ERROR LOADING NOTE"}");
            }, ParseOption.ParseInterior);
            
            if (!executor.TryParse(out var ex))
            {
                throw new FormatException($"{filename}: {ex.Message}", ex);
            }

            return mission;
        }

        private static MisisonGoal LoadGoal(ElementInfo info)
        {
            var type = info.Attributes.GetString("type").ToLower();

            var os = OS.currentInstance;

            if (GoalManager.TryLoadCustomGoal(type, out var customGoal))
            {
                if (customGoal is PathfinderGoal pfGoal)
                    pfGoal.LoadFromXML(info);
                return customGoal;
            }

            switch (type)
            {
                case "filedeletion":
                    return new FileDeletionMission(
                        info.Attributes.GetString("path").Filter(),
                        info.Attributes.GetString("file").Filter(),
                        info.Attributes.GetComp("target").ip,
                        os
                    );
                case "clearfolder":
                    return new FileDeleteAllMission(
                        info.Attributes.GetString("path").Filter(),
                        info.Attributes.GetComp("target").ip,
                        os
                    );
                case "filedownload":
                    return new FileDownloadMission(
                        info.Attributes.GetString("path").Filter(),
                        info.Attributes.GetString("file").Filter(),
                        info.Attributes.GetComp("target").ip,
                        os
                    );
                case "filechange":
                    return new FileChangeMission(
                        info.Attributes.GetString("path").Filter(),
                        info.Attributes.GetString("file").Filter(),
                        info.Attributes.GetComp("target").ip,
                        info.Attributes.GetString("keyword").Filter(),
                        os,
                        info.Attributes.GetBool("removal")
                    )
                    {
                        caseSensitive = info.Attributes.GetBool("filechange")
                    };
                case "getadmin":
                    return new GetAdminMission(info.Attributes.GetComp("target").ip, os);
                case "getstring":
                    return new GetStringMission(info.Attributes.GetString("target").Filter());
                case "delay":
                    return new DelayMission(info.Attributes.GetFloat("time", 1f));
                case "hasflag":
                    return new CheckFlagSetMission(info.Attributes.GetString("target").Filter(), os);
                case "fileupload":
                    return new FileUploadMission(
                        info.Attributes.GetString("path").Filter(),
                        info.Attributes.GetString("file").Filter(),
                        info.Attributes.GetComp("target").ip,
                        info.Attributes.GetComp("destTarget").ip,
                        info.Attributes.GetString("destPath").Filter(),
                        os,
                        info.Attributes.GetBool("decrypt"),
                        info.Attributes.GetString("decryptPass").Filter()
                    );
                case "adddegree":
                    return new AddDegreeMission(
                        info.Attributes.GetString("owner").Filter(),
                        info.Attributes.GetString("degree").Filter(),
                        info.Attributes.GetString("uni").Filter(),
                        info.Attributes.GetFloat("gpa", -1f),
                        os
                    );
                case "wipedegrees":
                    return new WipeDegreesMission(info.Attributes.GetString("owner").Filter(), os);
                case "removedeathrowrecord":
                    return new DeathRowRecordRemovalMission(
                        info.Attributes.GetString("fname", info.Attributes.GetString("name").Split(' ')[0]),
                        info.Attributes.GetString("lname", info.Attributes.GetString("name").Split(' ')[1]),
                        os
                    );
                case "modifydeathrowrecord":
                    return new DeathRowRecordModifyMission(
                        info.Attributes.GetString("fname", info.Attributes.GetString("name").Split(' ')[0]),
                        info.Attributes.GetString("lname", info.Attributes.GetString("name").Split(' ')[1]),
                        info.Content,
                        os
                    );
                case "sendemail":
                    return new SendEmailMission(
                        info.Attributes.GetString("mailServer", "jmail").Filter(),
                        info.Attributes.GetString("recipient").Filter(),
                        info.Attributes.GetString("subject").Filter(),
                        os
                    );
                case "databaseentrychange":
                    return new DatabaseEntryChangeMission(
                        info.Attributes.GetComp("comp").ip,
                        os,
                        info.Attributes.GetString("operation").Filter(),
                        info.Attributes.GetString("fieldName").Filter(),
                        info.Attributes.GetString("targetValue").Filter(),
                        info.Attributes.GetString("recordName").Filter()
                    );
                case "getadminpasswordstring":
                    return new GetAdminPasswordStringMission(info.Attributes.GetComp("target").ip, os);
                default:
                    throw new KeyNotFoundException("Unknown goal type");
            }
        }
    }
}
