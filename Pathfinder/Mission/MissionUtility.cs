#pragma warning disable CS0162 // Unreachable code detected
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hacknet;
using Hacknet.Mission;
using Pathfinder.Game;
using Pathfinder.Util;
using Sax.Net;

namespace Pathfinder.Mission
{
    public static class Utility
    {
        private static Dictionary<string, bool> missionTagOrderCheck = new Dictionary<string, bool>
        {
            {"generationKeys", false},
            {"goals", true},
            {"missionStart", false},
            {"missionEnd", false},
            {"nextMission", true},
            {"branchMissions", false},
            {"posting", false},
            {"email", true}
        };

        public static ActiveMission LoadContentMission(SaxProcessor.ElementInfo info, OS os)
        {
            var missionInfo = info.Elements.FirstOrDefault(i => i.Name == "mission");
            if (missionInfo == null) return null;
            var missionGoals = new List<MisisonGoal>();

            var activeCheck = missionInfo.Attributes.GetBool("activeCheck");
            var shouldIgnoreSenderVerification = missionInfo.Attributes.GetBool("shouldIgnoreSenderVerification");
            var missionType = missionInfo.Attributes.GetValueOrDefault("id", "Hacknet.ActiveMission");

            WarnExtensionContentOrder(missionInfo);

            var goalsOrGenKeyInfo = missionInfo.Elements.FirstOrDefault(i => i.Name == "goals" || i.Name == "generationKeys");
            switch (goalsOrGenKeyInfo?.Name)
            {
                case "generationKeys":
                    Dictionary<string, string> keys =
                        goalsOrGenKeyInfo.Attributes.ToDictionary(a => a.QName, a => a.Value);
                    var data = goalsOrGenKeyInfo.Value;
                    if (!string.IsNullOrEmpty(data))
                        keys.Add("Data", data);
                    MissionGenerator.setMissionGenerationKeys(keys);
                    goalsOrGenKeyInfo = missionInfo.Elements.FirstOrDefault(i => i.Name == "goals");
                    if (goalsOrGenKeyInfo == null) goto default;
                    goto case "goals";
                case "goals":
                    LoadMissionGoals(goalsOrGenKeyInfo, os);
                    break;
                default:
                    ExtensionsError(goalsOrGenKeyInfo, "Vanilla Hacknet, lack of <goals> will crash the game.");
                    break;
            }

            int startFuncVal = 0, endFuncVal = 0, postingRank = 0, postingDifficulty = 0;
            string startFuncStr = null, endFuncStr = null, nextMissionStr = null,
            postingTitle = null, postingReqs = null, postingClient = null, postingTarget = null, postingValue = null,
            emailSender = null, emailSubject = null, emailBody = null;
            var sendEmail = true;
            var attachments = new List<string>();
            foreach (var missionPartsInfo in missionInfo.Elements)
            {
                switch (missionPartsInfo.Name)
                {
                    case "nextMission":
                        sendEmail = missionPartsInfo.Attributes.GetBool("IsSilent", true);
                        nextMissionStr = missionPartsInfo.Value ?? "NONE";
                        os.branchMissions?.Clear();
                        break;
                    case "missionStart":
                        startFuncVal = missionPartsInfo.Attributes.GetInt("val", 1);
                        startFuncStr = missionPartsInfo.Value;
                        if (!missionPartsInfo.Attributes.GetBool("suppress", Settings.IsInExtensionMode))
                        {
                            try
                            {
                                MissionFunctions.runCommand(startFuncVal, startFuncStr);
                            }
                            catch (Exception e)
                            {
                                Utils.AppendToErrorFile("Mission Start Function Exception:\n" + Utils.GenerateReportFromException(e));
                            }
                            startFuncVal = 0;
                            startFuncStr = "";
                        }

                        break;
                    case "missionEnd":
                        endFuncVal = missionPartsInfo.Attributes.GetInt("val", 1);
                        endFuncStr = missionPartsInfo.Value;
                        break;
                    case "branchMissions":
                        var activeMissions = new List<ActiveMission>();
                        foreach (var branchInfo in missionPartsInfo.Elements.Where(i => i.Name == "branch"))
                        {
                            var path = "Content/Missions/";
                            if (Settings.IsInExtensionMode)
                                path = Extension.Handler.ActiveExtension.FolderPath + "/";
                            activeMissions.Add((ActiveMission)ComputerLoader.readMission(path + branchInfo.Value));
                        }
                        os.branchMissions = activeMissions;
                        break;
                    case "posting":
                        postingTitle = missionPartsInfo.Attributes.GetValueOrDefault("title", "UNKNOWN", true);
                        postingReqs = missionPartsInfo.Attributes.GetValueOrDefault("reqs", "UNKNOWN");
                        postingRank = missionPartsInfo.Attributes.GetInt("requiredRank");
                        postingDifficulty = missionPartsInfo.Attributes.GetInt("difficulty");
                        postingClient = missionPartsInfo.Attributes.GetValueOrDefault("client", "UNKNOWN");
                        postingTarget = missionPartsInfo.Attributes.GetValueOrDefault("target", "UNKNOWN");
                        postingValue = missionPartsInfo.Value?.HacknetFilter() ?? "UNKNOWN";
                        break;
                    case "email":
                        foreach (var emailPartsInfo in missionPartsInfo.Elements)
                        {
                            switch (emailPartsInfo.Name)
                            {
                                case "sender":
                                    emailSender = emailPartsInfo.Value.HacknetFilter().Trim();
                                    break;
                                case "subject":
                                    emailSubject = emailPartsInfo.Value.HacknetFilter().Trim();
                                    break;
                                case "body":
                                    emailBody = emailPartsInfo.Value.HacknetFilter().Trim();
                                    break;
                                case "attachments":
                                    foreach (var attachmentPartsInfo in emailPartsInfo.Elements)
                                    {
                                        switch (attachmentPartsInfo.Name)
                                        {
                                            case "link":
                                                var compId = attachmentPartsInfo.Attributes.GetValueOrDefault("comp", "", true);
                                                var comp = ComputerLoader.findComp(compId);
                                                if (comp == null)
                                                    ComputerLoader.postAllLoadedActions += () =>
                                                    {
                                                        comp = ComputerLoader.findComp(compId);
                                                        if (comp != null)
                                                            attachments.Add("link#%#" + comp.name + "#%#" + comp.ip);
                                                    };
                                                else
                                                    attachments.Add("link#%#" + comp.name + "#%#" + comp.ip);
                                                break;
                                            case "account":
                                                string accountUser = attachmentPartsInfo.Attributes.GetValueOrDefault("user", "UNKNOWN", true),
                                                accountPass = attachmentPartsInfo.Attributes.GetValueOrDefault("pass", "UNKNOWN", true);
                                                compId = attachmentPartsInfo.Attributes.GetValueOrDefault("comp", "", true);
                                                comp = ComputerLoader.findComp(compId);
                                                if (comp == null)
                                                    ComputerLoader.postAllLoadedActions += () =>
                                                    {
                                                        comp = ComputerLoader.findComp(compId);
                                                        if (comp != null)
                                                            attachments.Add("account#%#" + comp.name + "#%#" + comp.ip + "#%#" + accountUser + "#%#" + accountPass);
                                                    };
                                                else
                                                    attachments.Add("account#%#" + comp.name + "#%#" + comp.ip + "#%#" + accountUser + "#%#" + accountPass);
                                                break;
                                            case "note":
                                                attachments.Add("note#%#" + attachmentPartsInfo.Attributes.GetValueOrDefault("title", "Data", true) + "#%#" + attachmentPartsInfo.Value?.HacknetFilter() ?? "ERROR LOADING NOTE");
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        if (emailSender == null)
                            ExtensionsError(missionInfo, "Vanilla Hackent, missing <email> <sender> will crash the game");
                        if (emailSubject == null)
                            ExtensionsError(missionInfo, "Vanilla Hackent, missing <email> <subject> will crash the game");
                        if (emailBody == null)
                            ExtensionsError(missionInfo, "Vanilla Hackent, missing <email> <body> will crash the game");
                        break;
                    default:
                        /*if (missionInfo.Elements.FirstOrDefault(i => i.Name == "nextMission") == null)
                            throw new FormatException("Could not find required <nextMission> in mission file \"" + missionPartsInfo.Locator.SystemId + "\"");
*/
                        break;
                }
            }

            if (nextMissionStr == null)
            {
                nextMissionStr = "NONE";
                ExtensionsWarn(missionInfo, "Vanilla Hacknet, lack of <nextMission> will crash the game.");
            }

            if (missionGoals.Count == 0)
                ExtensionsError(missionInfo, "Vanilla Hacknet, missing <goals> and any goals inside of it will crash the game.");

            if (emailSender == null)
                ExtensionsError(missionInfo, "Vanilla Hacknet, missing <email> will crash the game.");

            var result = new ActiveMission(missionGoals, nextMissionStr, new MailServer.EMailData(emailSender, emailBody, emailSubject, attachments))
            {
                activeCheck = activeCheck,
                ShouldIgnoreSenderVerification = shouldIgnoreSenderVerification,
                postingBody = postingValue,
                postingTitle = postingTitle,
                requiredRank = postingRank,
                difficulty = postingDifficulty,
                client = postingClient,
                target = postingTarget,
                reloadGoalsSourceFile = missionInfo.Locator.SystemId,
                willSendEmail = sendEmail
            };
            if (!string.IsNullOrWhiteSpace(postingReqs))
                result.postingAcceptFlagRequirements = postingReqs.Split(Utils.commaDelim, StringSplitOptions.RemoveEmptyEntries);
            if (!string.IsNullOrWhiteSpace(startFuncStr))
                result.addStartFunction(startFuncVal, startFuncStr);
            if (!string.IsNullOrWhiteSpace(endFuncStr))
                result.addEndFunction(endFuncVal, endFuncStr);

            return result;
        }

        public static MisisonGoal LoadMissionGoal(SaxProcessor.ElementInfo goalInfo, OS os)
        {
            var targetStr = goalInfo.Attributes.GetValue("target", true);
            var target = ComputerLoader.findComp(targetStr);
            var file = goalInfo.Attributes.GetValue("file", true);
            var path = goalInfo.Attributes.GetValue("path", true);
            var type = goalInfo.Attributes.GetValueOrDefault("type", "unknown");
            try
            {
                switch (type.ToLower())
                {
                    case "filedeletion": return new FileDeletionMission(path, file, target?.ip ?? targetStr, os);

                    case "filedeleteall":
                    case "clearfolder":
                        return new FileDeleteAllMission(path, target?.ip ?? targetStr, os);
                    case "filedownload": return new FileDownloadMission(path, file, target?.ip ?? targetStr, os);

                    case "filechange":
                        if (target != null)
                            return new FileChangeMission(path,
                                                         file,
                                                         target?.ip ?? targetStr,
                                                         goalInfo.Attributes.GetValue("keyword", true),
                                                         os,
                                                         goalInfo.Attributes.GetBool("removal"))
                            {
                                caseSensitive = goalInfo.Attributes.GetBool("caseSensitive")
                            };
                        return null;
                    case "getadmin": return new GetAdminMission(target?.ip ?? targetStr, os);
                    case "getstring": return new GetStringMission(targetStr);
                    case "delay": return new DelayMission(goalInfo.Attributes.GetFloat("time", 1));

                    case "checkflagset":
                    case "hasflag":
                        return new CheckFlagSetMission(targetStr, os);
                    case "fileupload":
                        return new FileUploadMission(path,
                                                     file,
                                                     targetStr,
                                                     goalInfo.Attributes.GetValue("destTarget", true),
                                                     goalInfo.Attributes.GetValue("destPath", true),
                                                     os,
                                                     goalInfo.Attributes.GetBool("decrypt"),
                                                     goalInfo.Attributes.GetValue("decryptPass", true));
                    case "adddegree":
                        return new AddDegreeMission(goalInfo.Attributes.GetValue("owner", true),
                                                    goalInfo.Attributes.GetValue("degree", true),
                                                    goalInfo.Attributes.GetValue("uni", true),
                                                    goalInfo.Attributes.GetFloat("gpa", -1),
                                                    os);
                    case "wipedegrees": return new WipeDegreesMission(goalInfo.Attributes.GetValue("owner", true), os);

                    case "deathrowrecordremovalmission":
                    case "removedeathrowrecord":
                        var names = new string[]
                        {
                            goalInfo.Attributes.GetValue("fname", true),
                            goalInfo.Attributes.GetValue("lname", true)
                        };
                        if (string.IsNullOrWhiteSpace(names[0]) || string.IsNullOrWhiteSpace(names[1]))
                            names = goalInfo.Attributes.GetValue("name", true).Split(' ');
                        return new DeathRowRecordRemovalMission(names[0] ?? "UNKNOWN", names[1] ?? "UNKNOWN", os);

                    case "deathrowrecordmodifymission":
                    case "modifydeathrowrecord":
                        names = new string[]
                        {
                            goalInfo.Attributes.GetValue("fname", true),
                            goalInfo.Attributes.GetValue("lname", true)
                        };
                        if (string.IsNullOrWhiteSpace(names[0]) || string.IsNullOrWhiteSpace(names[1]))
                            names = goalInfo.Attributes.GetValue("name", true).Split(' ');
                        return new DeathRowRecordModifyMission(names[0] ?? "UNKNOWN", names[1] ?? "UNKNOWN", goalInfo.Value, os);
                    case "sendemail":
                        return new SendEmailMission(goalInfo.Attributes.GetValueOrDefault("mailServer", "jmail", true),
                                                    goalInfo.Attributes.GetValue("recipient", true),
                                                    goalInfo.Attributes.GetValue("subject", true),
                                                    os);
                    case "databaseentrychange":
                        return new DatabaseEntryChangeMission(goalInfo.Attributes.GetValue("comp", true),
                                                              os,
                                                              goalInfo.Attributes.GetValue("operation", true),
                                                              goalInfo.Attributes.GetValue("fieldName", true),
                                                              goalInfo.Attributes.GetValue("targetValue", true),
                                                              goalInfo.Attributes.GetValue("recordName", true));
                    case "getadminpasswordstring": return new GetAdminPasswordStringMission(target?.ip ?? targetStr, os);
                    default:
                        var goalInterface = Handler.GetMissionGoalById(type);
                        if (goalInterface == null)
                        {
                            var modId = type.Split('.')[0];
                            if (modId != type && ModManager.Manager.GetLoadedMod(modId) == null)

                                ExtensionsError(goalInfo, "Vanilla Hacknet, <goal> failed to load, mod \"" + modId + "\" not found for goal \"" + type + "\", perhaps it has not been loaded or installed.");


                            ExtensionsError(goalInfo, "Vanilla Hacknet, <goal> failed to load, expected vanilla mission goal, no vanilla type id \"" + type + "\" found.");
                        }
                        return new GoalInstance(goalInterface);
                }
            }
            catch (NullReferenceException e)
            {
                ExtensionsError(goalInfo, "Vanilla Hacknet, <goals> failed to load \"" + type + "\", likely means something referenced by an ID (probably a computer) or a filename (missions/scripts etc) was not found.\nNullReferenceException\n--------------------------------------\n" + e.Message + "\n--------------------------------------\n");
            }
            return null;
        }

        public static List<MisisonGoal> LoadMissionGoals(SaxProcessor.ElementInfo info, OS os, bool ignorePreload = false)
        {
            var result = new List<MisisonGoal>();
            if (!ignorePreload)
                ComputerLoader.MissionPreLoadComplete?.Invoke();
            foreach (var goalInfo in info.Elements.Where(i => i.Name == "goal"))
                result.Add(LoadMissionGoal(goalInfo, os));
            return result;
        }

        public static string GenerateSaveStringFor(MisisonGoal goal)
        {
            var modGoal = goal as GoalInstance;
            if (modGoal != null)
                return modGoal.SaveString;
            var name = goal.GetType().Name;
            var result = "<goal id=\"Hacknet." + name.Remove(name.LastIndexOf("Mission")) + "\" ";
            foreach (var f in goal.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (f.FieldType == typeof(Hacknet.OS) || f.FieldType == typeof(Hacknet.Folder)) continue;
                const string t = "target";
                var val = f.GetValue(goal);
                var computer = val as Hacknet.Computer;
                name = f.Name != t && f.Name.Contains(t) ? f.Name.Substring(f.Name.IndexOf(t) + t.Length) : f.Name;
                result += name + "=\"" + computer?.ip ?? val;
            }
            return result + "/>";
        }

        public static MisisonGoal LoadFromSaveString(SaxProcessor.ElementInfo info)
        {

            var mission = info.Elements.FirstOrDefault(i => i.Name == "mission");


            /*if (str.Contains("vanillaMissionGoal"))
            {
                const string id = "interfaceId=\"";
                var name = str.Substring(str.IndexOf(id) + id.Length) + "Mission";
                var type = Type.GetType("Hacknet.Mission." + name + ",HacknetPathfinder");
                var ctor = type.GetConstructors(BindingFlags.Public)[0];
                string[] strs = new string[ctor.GetParameters().Length];
                int i = 0;
                foreach (var p in ctor.GetParameters())
                {
                    strs[i] =
                }
                Activator.CreateInstance(type, type.GetConstructors
            }*/
#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar
            throw new NotImplementedException();
#pragma warning restore RECS0083 // Shows NotImplementedException throws in the quick task bar
        }

        private static void ExtensionsWarn(SaxProcessor.ElementInfo i, params object[] input)
        {
            var include = Logger.IncludeModId;
            Logger.IncludeModId = false;
            Logger.Warn(new object[] { i.Locator.SystemId + ": " + input[0] }.Concat(input.Skip(1)).ToArray());
            Logger.IncludeModId = include;
        }

        private static void ExtensionsError(SaxProcessor.ElementInfo i, params object[] input)
        {
            var include = Logger.IncludeModId;
            Logger.IncludeModId = false;
            Logger.Error(new object[] { i.Locator.SystemId + ": " + input[0] }.Concat(input.Skip(1)).ToArray());
            Logger.IncludeModId = include;
        }

        private static void WarnExtensionContentOrder(SaxProcessor.ElementInfo missionInfo)
        {
            var prevRequiredElement = default(KeyValuePair<string, bool>);
            for (var i = 0; i < missionTagOrderCheck.Count; i++)
            {
                var nameNRequired = missionTagOrderCheck.ElementAt(i);
                var prevElement = i > 0 ? missionTagOrderCheck.ElementAt(i - 1) : default(KeyValuePair<string, bool>);

                if (nameNRequired.Value)
                {
                    ExtensionsWarn(missionInfo, "Vanilla Hacknet, <{0}> is required and {1}{2}{3}.",
                                   nameNRequired.Key,
                                   prevRequiredElement.Key != null && i != 0 ? " must come after <" + prevRequiredElement.Key + "> and " : "",
                                   prevElement.Value || prevElement.Key == null ? " must" : " may",
                                   i == 0 ? " come first" : " come after <" + prevElement.Key + ">");
                    prevRequiredElement = nameNRequired;
                }
                else
                    ExtensionsWarn(missionInfo, "Vanilla Hacknet, <{0}> is optional but must come {1}.",
                                   nameNRequired.Key,
                                   i == 0 ? "first" : "after <" + (prevRequiredElement.Key ?? prevElement.Key) + ">");
            }
        }

        public static object GetPrimitive(string type, string value)
        {
            switch (type.ToLower())
            {
                case "boolean": return value == "true";
                case "byte": return Byte.Parse(value);
                case "sbyte": return SByte.Parse(value);
                case "int16": return Int16.Parse(value);
                case "uint16": return UInt16.Parse(value);
                case "int32": return Int32.Parse(value);
                case "uint32": return UInt32.Parse(value);
                case "int64": return Int64.Parse(value);
                case "uint64": return UInt64.Parse(value);
                case "char": return Char.Parse(value);
                case "double": return Double.Parse(value);
                case "single": return Single.Parse(value);
            }
            return value;
        }
    }
}
