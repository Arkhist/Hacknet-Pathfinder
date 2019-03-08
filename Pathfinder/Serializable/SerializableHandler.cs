using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Serializable
{
    public static class Handler
    {
        internal static Dictionary<string, Action.Interface> actions =
                                                     new Dictionary<string, Action.Interface>
                                                     {

                                                     };

        internal static Dictionary<string, Condition.Interface> conditions =
                                                        new Dictionary<string, Condition.Interface>();

        internal static Dictionary<string, ConvertedCondition> VanillaConditions = new Dictionary<string, ConvertedCondition>
        {
            {"OnAdminGained", ConvertConditionFunc(SCOnAdminGained.DeserializeFromReader)},
            {"OnConnect", ConvertConditionFunc(SCOnConnect.DeserializeFromReader)},
            {"HasFlags", ConvertConditionFunc(SCHasFlags.DeserializeFromReader)},
            {"Instantly", ConvertConditionFunc(SCInstantly.DeserializeFromReader)},
            {"DoesNotHaveFlags", ConvertConditionFunc(SCDoesNotHaveFlags.DeserializeFromReader)}
        };

        internal static Dictionary<string, ConvertedAction> VanillaActions = new Dictionary<string, ConvertedAction>
        {
            {"LoadMission", ConvertActionFunc(SALoadMission.DeserializeFromReader)},
            {"RunFunction", ConvertActionFunc(SARunFunction.DeserializeFromReader)},
            {"AddAsset", ConvertActionFunc(SAAddAsset.DeserializeFromReader)},
            {"AddMissionToHubServer", ConvertActionFunc(SAAddMissionToHubServer.DeserializeFromReader)},
            {"RemoveMissionFromHubServer", ConvertActionFunc(SARemoveMissionFromHubServer.DeserializeFromReader)},
            {"AddThreadToMissionBoard", ConvertActionFunc(SAAddThreadToMissionBoard.DeserializeFromReader)},
            {"AddIRCMessage", ConvertActionFunc(SAAddIRCMessage.DeserializeFromReader)},
            {"AddConditionalActions", ConvertActionFunc(SAAddConditionalActions.DeserializeFromReader)},
            {"CopyAsset", ConvertActionFunc(SACopyAsset.DeserializeFromReader)},
            {"CrashComputer", ConvertActionFunc(SACrashComputer.DeserializeFromReader)},
            {"DeleteFile", ConvertActionFunc(SADeleteFile.DeserializeFromReader)},
            {"LaunchHackScript", ConvertActionFunc(SALaunchHackScript.DeserializeFromReader)},
            {"SwitchToTheme", ConvertActionFunc(SASwitchToTheme.DeserializeFromReader)},
            {"StartScreenBleedEffect", ConvertActionFunc(SAStartScreenBleedEffect.DeserializeFromReader)},
            {"CancelScreenBleedEffect", ConvertActionFunc(SACancelScreenBleedEffect.DeserializeFromReader)},
            {"AppendToFile", ConvertActionFunc(SAAppendToFile.DeserializeFromReader)},
            {"KillExe", ConvertActionFunc(SAKillExe.DeserializeFromReader)},
            {"ChangeAlertIcon", ConvertActionFunc(SAChangeAlertIcon.DeserializeFromReader)},
            {"HideNode", ConvertActionFunc(SAHideNode.DeserializeFromReader)},
            {"GivePlayerUserAccount", ConvertActionFunc(SAGivePlayerUserAccount.DeserializeFromReader)},
            {"ChangeIP", ConvertActionFunc(SAChangeIP.DeserializeFromReader)},
            {"ChangeNetmapSortMethod", ConvertActionFunc(SAChangeNetmapSortMethod.DeserializeFromReader)},
            {"SaveGame", ConvertActionFunc(SASaveGame.DeserializeFromReader)},
            {"HideAllNodes", ConvertActionFunc(SAHideAllNodes.DeserializeFromReader)},
            {"ShowNode", ConvertActionFunc(SAShowNode.DeserializeFromReader)},
        };

        public delegate SerializableCondition ConvertedCondition(SaxProcessor.ElementInfo info);
        public delegate SerializableAction ConvertedAction(SaxProcessor.ElementInfo info);

        private static XmlReader ConvertFromElementInfo(SaxProcessor.ElementInfo info) =>
            XmlReader.Create(new StringReader(info.Name.ToXml(info.Value, info.Attributes)),
                             new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment },
                             new XmlParserContext(null, new XmlNamespaceManager(new NameTable()), null, XmlSpace.None)
                            );

        private static ConvertedCondition ConvertConditionFunc(Func<XmlReader, SerializableCondition> func) =>
            info => func(ConvertFromElementInfo(info));

        private static ConvertedAction ConvertActionFunc(Func<XmlReader, SerializableAction> func) =>
                info => func(ConvertFromElementInfo(info));

        /// <summary>
        /// Adds a serializable action to Hacknet
        /// </summary>
        /// <returns><c>true</c> if added to the game, <c>false</c> otherwise</returns>
        /// <param name="condition">The serializable action used in the XML.</param>
        /// <param name="deserializer">The function run when deserializing the action.</param>
        public static bool RegisterAction(string action,
                                          Action.Interface serializableAction)
        {
            if (Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException("RegisterAction can not be called outside of mod or extension loading.");
            var id = Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id;
            Logger.Verbose("{0} {1} is attempting to add action {2}",
                           Pathfinder.CurrentMod != null ? "Mod" : "Extension", id, action);
            if (actions.ContainsKey(action))
                return false;
            actions.Add(action, serializableAction);

            return true;
        }

        public static SerializableAction LoadAction(SaxProcessor.ElementInfo info)
        {
            var result = Action.Instance.CreateInstance(info.Name);
            if (result == null && info.Name.ToLower() == "moddedaction")
            {
                var id = info.Attributes.GetValue("id");
                if (!id.Contains("."))
                    Logger.Error("Can't identify owning mod of serializable action \"{0}\"", id);
                else
                    result = Action.Instance.CreateInstance(id);
            }
            if (result == null)
            {
                ConvertedAction a;
                VanillaActions.TryGetValue(info.Name, out a);
                return a(info);
            }
            if (result == null) return null;
            result.OnRead(info);
            return result;
        }

        public static Action.Interface GetActionById(string id) => GetActionById(ref id);
        public static Action.Interface GetActionById(ref string id)
        {
            id = Utility.GetId(id);
            Action.Interface i;
            if (actions.TryGetValue(id, out i))
                return i;
            return null;
        }

        /// <summary>
        /// Adds a serializable action to Hacknet
        /// </summary>
        /// <returns><c>true</c> if added to the game, <c>false</c> otherwise</returns>
        /// <param name="condition">The serializable action used in the XML.</param>
        /// <param name="deserializer">The function run when deserializing the action.</param>
        public static bool RegisterCondition(string condition,
                                             Condition.Interface serializableCondition)
        {
            if (Pathfinder.CurrentMod == null && !Extension.Handler.CanRegister)
                throw new InvalidOperationException("RegisterCondition can not be called outside of mod or extension loading.");
            var id = Pathfinder.CurrentMod?.GetCleanId() ?? Extension.Handler.ActiveInfo.Id;
            Logger.Verbose("{0} {1} is attempting to add condition {2}",
                           Pathfinder.CurrentMod != null ? "Mod" : "Extension", id, condition);
            if (conditions.ContainsKey(condition))
                return false;
            conditions.Add(condition, serializableCondition);

            return true;
        }

        public static SerializableCondition LoadCondition(SaxProcessor.ElementInfo info)
        {
            var result = Condition.Instance.CreateInstance(info.Name);
            if (result.Interface == null && info.Name.ToLower() == "moddedcondition")
            {
                var id = info.Attributes.GetValue("id");
                if (!id.Contains("."))
                    Logger.Error("Can't identify owning mod of serializable condition \"{0}\"", id);
                else
                    result = Condition.Instance.CreateInstance(id);
            }
            if (result == null)
            {
                ConvertedCondition c;
                VanillaConditions.TryGetValue(info.Name, out c);
                return c(info);
            }
            if (result.Interface == null) return null;
            result.OnRead(info);
            return result;
        }

        public static Condition.Interface GetConditionById(string id) => GetConditionById(ref id);
        public static Condition.Interface GetConditionById(ref string id)
        {
            id = Utility.GetId(id);
            Condition.Interface i;
            if (conditions.TryGetValue(id, out i))
                return i;
            return null;
        }
    }
}
