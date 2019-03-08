using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hacknet;
using Hacknet.Extensions;
using Hacknet.Mission;
using Pathfinder.Game;
using Pathfinder.Util;
using Sax.Net;

namespace Pathfinder.Mission
{
    public class Instance : ActiveMission
    {
        private Dictionary<string, Tuple<bool, object>> keyToObject = new Dictionary<string, Tuple<bool, object>>();
        private List<MisisonGoal> dynamicGoals = new List<MisisonGoal>();

        public Interface Interface { get; private set; }
        public string InterfaceId { get; private set; }
        public Computer MissionComputer { get; internal set; }
        public Computer AgentComputer { get; internal set; }

        internal Instance(Interface inter,
                          string next = "NONE",
                          Dictionary<string, object> savedObjects = null,
                          MailServer.EMailData email = default(MailServer.EMailData))
            : base(inter.DefaultGoals ?? new List<MisisonGoal>(), next ?? "NONE", email)
        {
            Interface = inter;
            goals.ForEach((goal) =>
            {
                var g = goal as GoalInstance;
                if (g != null)
                    g.Mission = this;
                Interface.OnGoalAdd(this, goal);
            });
            if (savedObjects == null) savedObjects = new Dictionary<string, object>();
            Interface.OnLoadInstance(this, savedObjects);
            if (Interface.AutoLoadSaveables)
                keyToObject = savedObjects.ToDictionary(p => p.Key, p => new Tuple<bool, object>(true, p.Value));
        }

        public static Instance CreateInstance(string id,
                                              Dictionary<string, object> objects = null,
                                              string next = null,
                                              MailServer.EMailData? mailData = null)
        {
            var inter = Handler.GetMissionById(ref id);
            if (inter == null)
                return null;
            var i = new Instance(inter,
                                 next ?? inter.InitialNextMission ?? "NONE",
                                 objects,
                                 mailData.HasValue ? mailData.Value : inter.IntialEmailData)
            {
                InterfaceId = id
            };
            return i;
        }

        public object this[string key]
        {
            get
            {
                key = Util.Utility.ConvertToValidXmlAttributeName(key);
                Tuple<bool, object> t;
                if (keyToObject.TryGetValue(key, out t))
                    return t.Item2;
                return null;
            }
            set
            {
                key = Util.Utility.ConvertToValidXmlAttributeName(key);
                keyToObject[key] = new Tuple<bool, object>(false, value);
            }
        }

        public object GetInstanceData(string key) => this[key];
        public T GetInstanceData<T>(string key) => (T)GetInstanceData(key);

        public bool? IsInstanceSaveable(string key)
        {
            key = Util.Utility.ConvertToValidXmlAttributeName(key);
            Tuple<bool, object> t;
            if (keyToObject.TryGetValue(key, out t))
                return t.Item1;
            return null;
        }

        public bool SetInstanceData(string key, object val, bool shouldSave = false)
        {
            if (shouldSave && !val.GetType().IsPrimitive && val.GetType() != typeof(string))
                throw new InvalidOperationException("saveable mission instance data must be a primitive object");
            key = Util.Utility.ConvertToValidXmlAttributeName(key);
            var t = new Tuple<bool, object>(shouldSave, val);
            keyToObject[key] = t;
            return keyToObject[key] == t;
        }

        public void SetInstanceDataSaveable(string key, bool shouldSave)
        {
            key = Util.Utility.ConvertToValidXmlAttributeName(key);
            Tuple<bool, object> t;
            if (keyToObject.TryGetValue(key, out t))
            {
                t = new Tuple<bool, object>(shouldSave, t.Item2);
                if (!t.Item2.GetType().IsPrimitive && t.Item2.GetType() != typeof(string))
                    throw new InvalidOperationException("saveable mission instance data must be a primitive object");
                keyToObject[key] = t;
            }
        }

        public Instance AddGoal(MisisonGoal goal)
        {
            if (goal == null) return this;
            Interface.OnGoalAdd(this, goal);
            dynamicGoals.Add(goal);
            var moddedGoal = goal as GoalInstance;
            if (moddedGoal != null)
                moddedGoal.Mission = this;
            return this;
        }

        public Instance AddGoal(string id, Dictionary<string, object> objects = null)
        {
            if (Handler.ContainsMission(id))
                AddGoal(GoalInstance.CreateInstance(id, objects));
            return this;
        }

        public override void ActivateSuppressedStartFunctionIfPresent()
        {
            Tuple<string, int> t;
            if ((t = Interface.OnStart(this)) != null)
                addStartFunction(t.Item2, t.Item1);
            base.ActivateSuppressedStartFunctionIfPresent();
        }

        public override void finish()
        {
            Tuple<string, int> t;
            if ((t = Interface.OnEnd(this)) != null)
                addEndFunction(t.Item2, t.Item1);

            var os = Util.Utility.ClientOS;
            os.branchMissions.Clear();
            if (nextMission.StartsWith("Pathfinder:", StringComparison.Ordinal))
            {
                var id = nextMission.Substring(nextMission.IndexOf(':') + 1);
                os.currentMission = CreateInstance(id, new Dictionary<string, object>());
                os.currentMission?.sendEmail(os);
            }
            else if (nextMission.Contains("."))
            {
                os.currentMission = CreateInstance(nextMission, new Dictionary<string, object>());
                os.currentMission?.sendEmail(os);
            }
            else if (nextMission != "NONE")
            {
                var str = "Content/Missions";
                if (Settings.IsInExtensionMode)
                    str = ExtensionLoader.ActiveExtensionInfo.FolderPath;
                ComputerLoader.loadMission(str + "/" + nextMission, false);
            }
            else
                os.currentMission = null;

            os.currentMission?.ActivateSuppressedStartFunctionIfPresent();

            if (endFunctionName != null)
                MissionFunctions.runCommand(endFunctionValue, endFunctionName);

            os.saveGame();
            if (os.multiplayer)
                os.endMultiplayerMatch(true);
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            additionalDetails = additionalDetails ?? new List<string>();
            var result = Interface.IsComplete(this, additionalDetails);
            if (result.HasValue)
                return result.Value;
            foreach (var g in dynamicGoals)
                if (!g.isComplete(additionalDetails))
                    return false;
            return base.isComplete(additionalDetails);
        }

        public override void Update(float t)
        {
            Interface.Update(this, t);
            base.Update(t);
        }

        public override string getSaveString()
        {
            var str = new StringBuilder("<mission id=\"" + InterfaceId
                + "\" next=\"" + (nextMission.Contains(".") ? nextMission : "/" + nextMission) + "\"");
            if (wasAutoGenerated)
                str.Append(" genTarget=\"" + genTarget + "\" genFile=\"" + genFile
                    + "\" genPath=\"" + genPath + "\"  genTargetName=\"" + genTargetName + "\" genOther=\"" + genOther + "\"");

            str.Append("activeCheck=\"" + activeCheck + "\">\n\t<storage>");
            foreach (var o in keyToObject) if (o.Value.Item1)
                    str.AppendLine("\t\t<" + o.Key + " type=\"" + o.Value.Item2.GetType() + "\" >" + o.Value.Item2 + "</" + o.Key + ">");
            str.AppendLine("\t<email sender=\"" + Folder.Filter(email.sender)
                       + "\" subject=\"" + Folder.Filter(email.subject) + "\">" + Folder.Filter(email.body) + "</email>");
            str.AppendLine("\t<endFunc val=\"" + endFunctionValue + "\" name=\"" + endFunctionName + "\" />");
            str.AppendLine("\t<posting title=\"" + Folder.Filter(postingTitle) + "\" >" + Folder.Filter(postingBody) + "</posting>");
            str.AppendLine("\t<dynamicGoals>");
            foreach (var g in dynamicGoals)
                str.AppendLine("\t\t" + Utility.GenerateSaveStringFor(g));
            str.AppendLine("\t</dynamicGoals>\n</mission>");
            return str.ToString();
        }

        public override void sendEmail(OS os)
        {
            if (Interface.SendEmail(this, os))
                base.sendEmail(os);
        }

        public static ActiveMission Load(SaxProcessor.ElementInfo info, OS os)
        {
            var missionInfo = info.Elements.FirstOrDefault(i => i.Name == "mission");
            if (missionInfo == null) return null;

            var missionId = missionInfo.Attributes.GetValueOrDefault("id", "Vanilla");
            var nextMission = missionInfo.Attributes.GetValueOrDefault("next", "NONE").Replace('\\', '/');
            var missionContent = missionInfo.Attributes.GetValue("goals")?.Replace('\\', '/');
            var missionGoals = new List<MisisonGoal>();
            MissionGenerationParser.Comp = missionInfo.Attributes.GetValue("genTarget");
            if (MissionGenerationParser.Comp != null)
            {
                MissionGenerationParser.File = missionInfo.Attributes.GetValue("genFile");
                MissionGenerationParser.Path = missionInfo.Attributes.GetValue("genPath");
                MissionGenerationParser.Target = missionInfo.Attributes.GetValue("genTargetName");
                MissionGenerationParser.Other = missionInfo.Attributes.GetValue("genOther");
            }
            var activeCheck = missionInfo.Attributes.GetBool("activeCheck");

            var storage = new Dictionary<string, object>();
            if (missionId.ToLower() != "vanilla")
                foreach (var storageInfo in missionInfo.Elements.FirstOrDefault(i => i.Name == "storage")?.Elements)
                    storage.Add(storageInfo.Name, Utility.GetPrimitive(storageInfo.Attributes.GetValue("type", "string"), storageInfo.Value));
            ActiveMission result = null;
            if (missionContent != "NULL_MISSION")
            {
                var attachments = new List<string>();
                if (missionContent.IndexOf('/') != -1)
                {
                    if (!Settings.IsInExtensionMode && !missionContent.StartsWith("Content", StringComparison.Ordinal))
                        missionContent = "Content/" + missionContent;
                    ActiveMission mission;
                    try
                    {
                        mission = ComputerLoader.readMission(nextMission) as ActiveMission;
                        if (mission != null)
                        {
                            activeCheck = activeCheck || mission.activeCheck;
                            attachments = mission.email.attachments;
                            missionGoals = mission.goals;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Extension/Vanilla Mission '" + nextMission + "' errored out: " + ex);
                    }
                }
                else
                {
                    if(missionContent.Contains(":"))
                        missionContent = nextMission.Substring(missionContent.IndexOf(':') + 1);
                    var mission = CreateInstance(missionContent);
                    if (mission != null)
                    {
                        activeCheck = activeCheck || mission.activeCheck;
                        attachments = mission.email.attachments;
                        missionGoals = mission.goals;
                    }
                }
                var emailInfo = missionInfo.Elements.FirstOrDefault(i => i.Name == "email");
                var sender = (emailInfo?.Attributes).GetValueOrDefault("sender", "ERRORBOT").XmlDefilter();
                var subject = (emailInfo?.Attributes).GetValueOrDefault("subject", "ERROR").XmlDefilter();
                var body = emailInfo?.Value.XmlDefilter() ?? "ERROR :: MAIL LOAD FAILED";
                var goalsSource = missionInfo.Attributes.GetValue("goals");
                var reqRank = missionInfo.Attributes.GetInt("reqRank");
                var endFuncInfo = missionInfo.Elements.FirstOrDefault(i => i.Name == "endFunc");
                var funcVal = (endFuncInfo?.Attributes).GetInt("value");
                var funName = (endFuncInfo?.Attributes).GetValueOrDefault("name", "");
                var postingInfo = missionInfo.Elements.FirstOrDefault(i => i.Name == "posting");
                var postingBody = postingInfo?.Value.XmlDefilter() ?? "";
                var postingTitle = (postingInfo?.Attributes).GetValueOrDefault("title", "").XmlDefilter();

                if (missionId.ToLower() != "vanilla")
                {
                    result = CreateInstance(missionId, storage, nextMission, new MailServer.EMailData(sender, body, subject, attachments));
                    result.activeCheck = activeCheck;
                    result.reloadGoalsSourceFile = goalsSource;
                    result.requiredRank = reqRank;
                    result.endFunctionName = funName;
                    result.endFunctionValue = funcVal;
                    result.postingBody = postingBody;
                    result.postingTitle = postingTitle;

                    foreach (var goalInfo in missionInfo.Elements.FirstOrDefault(i => i.Name == "dynamicGoals")?.Elements.Where(i => i.Name == "goal"))
                        ((Instance)result).AddGoal(Utility.LoadMissionGoal(goalInfo, os));
                }
                else
                    result = new ActiveMission(missionGoals, nextMission, new MailServer.EMailData(sender, body, subject, attachments))
                    {
                        activeCheck = activeCheck,
                        reloadGoalsSourceFile = goalsSource,
                        requiredRank = reqRank,
                        endFunctionName = funName,
                        endFunctionValue = funcVal,
                        postingBody = postingBody,
                        postingTitle = postingTitle
                    };
            }
            return result;
        }
    }
}
