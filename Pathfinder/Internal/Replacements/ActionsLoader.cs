using System;
using System.IO;
using Hacknet;
using Pathfinder.Exceptions;
using Pathfinder.Game;
using Pathfinder.ModManager;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Internal.Replacements
{
    public static class ActionsLoader
    {
        public delegate SerializableAction ActionLoader(ElementInfo info);
        public delegate SerializableCondition ConditionLoader(ElementInfo info);

        public static void AddActionLoader(string name, ActionLoader loader, bool overrideable = false)
        { if (overrideable) ActionLoaders.AddOverrideable(name, loader); else ActionLoaders.Add(name, loader); }

        public static bool RemoveActionLoader(string name)
            => ActionLoaders.Remove(name);

        public static void AddConditionLoader(string name, ConditionLoader loader, bool overrideable = false)
        { if (overrideable) ConditionLoaders.AddOverrideable(name, loader); else ConditionLoaders.Add(name, loader); }

        public static bool RemoveConditionLoader(string name)
            => ConditionLoaders.Remove(name);


        private static readonly ModTaggedDict<string, ActionLoader> ActionLoaders = new ModTaggedDict<string, ActionLoader>();
        private static readonly ModTaggedDict<string, ConditionLoader> ConditionLoaders = new ModTaggedDict<string, ConditionLoader>();

        internal static void InitActionLoaders()
        {

            ActionLoaders.AddOverrideable("LoadMission", info =>
            new SALoadMission
                {
                    MissionName = info.Attributes.GetChecked(
                     "MissionName",
                     "Mission File Path",
                     v => File.Exists(Utils.GetFileLoadPrefix() + v),
                     "File does not exist"
                 )
                });
            ActionLoaders.AddOverrideable("RunFunction", info =>
            new SARunFunction
                {
                    FunctionName = info.Attributes.GetNonEmptyString("FunctionName", "Function Name"),
                    FunctionValue = info.Attributes.GetInt("FunctionValue"),
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost")
                });
            ActionLoaders.AddOverrideable("AddAsset", info =>
            new SAAddAsset
                {
                    FileName = info.Attributes.GetValue("FileName"),
                    FileContents = info.Attributes.GetValue("FileContents"),
                    TargetComp = info.Attributes.GetValue("TargetComp"),
                    TargetFolderpath = info.Attributes.GetValue("TargetFolderpath")
                });
            ActionLoaders.AddOverrideable("AddMissionToHubServer", info =>
            new SAAddMissionToHubServer
                {
                    MissionFilepath = info.Attributes.GetNonEmptyString("MissionFilepath", "Mission File Path"),
                    TargetComp = info.Attributes.GetNonEmptyString("TargetComp", "Target Computer"),
                    AssignmentTag = info.Attributes.GetValueOrNull("AssignmentTag")?.BlankToNull(),
                    StartsComplete = info.Attributes.GetBool("StartsComplete")
                });
            ActionLoaders.AddOverrideable("RemoveMissionFromHubServer", info =>
            new SARemoveMissionFromHubServer
                {
                    MissionFilepath = info.Attributes.GetNonEmptyString("MissionFilepath", "Mission File Path"),
                    TargetComp = info.Attributes.GetNonEmptyString("TargetComp", "Target Computer")
                });
            ActionLoaders.AddOverrideable("AddThreadToMissionBoard", info =>
            new SAAddThreadToMissionBoard
                {
                    ThreadFilepath = info.Attributes.GetNonEmptyString("ThreadFilepath", "Thread File Path"),
                    TargetComp = info.Attributes.GetNonEmptyString("TargetComp", "Target Computer")
                });
            ActionLoaders.AddOverrideable("AddIRCMessage", info =>
            {
                var result = new SAAddIRCMessage
                {
                    Author = info.Attributes.GetValue("Author", true),
                    Delay = info.Attributes.GetFloat("Delay"),
                    TargetComp = info.Attributes.GetNonEmptyString("TargetComp", "Target Computer")
                };

                var message = info.Value;
                if(string.IsNullOrWhiteSpace(message))
                    throw new FormatException("Invalid Message (Element Content): Must not be empty.");

                result.Message = message;
                return result;
            });
            ActionLoaders.AddOverrideable("AddConditionalActions", info =>
            new SAAddConditionalActions
                {
                    Filepath = info.Attributes.GetNonEmptyString("Filepath", "CAS File Path"),
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValueOrNull("DelayHost")
                });
            ActionLoaders.AddOverrideable("CopyAsset", info =>
            {
                var result = new SACopyAsset
                {
                    DestFilePath = info.Attributes.GetValue("DestFilePath"),
                    DestComp = info.Attributes.GetValue("DestComp"),
                    SourceComp = info.Attributes.GetValue("SourceComp"),
                    SourceFilePath = info.Attributes.GetValue("SourceFilePath")
                };
		string srcFile = info.Attributes.GetValueOrNull("SourceFileName").BlankToNull();
		string dstFile = info.Attributes.GetValueOrNull("DestFileName").BlankToNull();
		if(srcFile == null && dstFile == null)
			throw new FormatException("Source (\"SourceFileName\") or Destination (\"DestFileName\") file name must be specified.");
		else if(srcFile == null)
			srcFile = dstFile;
		else if(dstFile == null)
			dstFile = srcFile;
		result.SourceFileName = srcFile;
		result.DestFileName = dstFile;
                return result;
            });
            ActionLoaders.AddOverrideable("CrashComputer", info =>
            new SACrashComputer
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    TargetComp = info.Attributes.GetValue("TargetComp"),
                    CrashSource = info.Attributes.GetValue("CrashSource"),
                    DelayHost = info.Attributes.GetValue("DelayHost")
                });
            ActionLoaders.AddOverrideable("DeleteFile", info =>
            new SADeleteFile
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    TargetComp = info.Attributes.GetValue("TargetComp"),
                    FilePath = info.Attributes.GetValue("FilePath"),
                    FileName = info.Attributes.GetValue("FileName"),
                    DelayHost = info.Attributes.GetValue("DelayHost")
                });
            ActionLoaders.AddOverrideable("LaunchHackScript", info =>
            new SALaunchHackScript
                {
                    Filepath = info.Attributes.GetNonEmptyString("Filepath", "Invalid Hacker Script Path"),
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost"),
                    SourceComp = info.Attributes.GetValue("SourceComp"),
                    TargetComp = info.Attributes.GetValue("TargetComp"),
                    RequireLogsOnSource = info.Attributes.GetBool("RequireLogsOnSource"),
                    RequireSourceIntact = info.Attributes.GetBool("RequireSourceIntact")
                });
            ActionLoaders.AddOverrideable("SwitchToTheme", info =>
            new SASwitchToTheme
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    ThemePathOrName = info.Attributes.GetValue("ThemePathOrName"),
                    FlickerInDuration = info.Attributes.GetFloat("FlickerInDuration", 2f),
                    DelayHost = info.Attributes.GetValue("DelayHost")
                });
            ActionLoaders.AddOverrideable("StartScreenBleedEffect", info =>
            new SAStartScreenBleedEffect
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost"),
                    AlertTitle = info.Attributes.GetValue("AlertTitle", true),
                    CompleteAction = info.Attributes.GetValue("CompleteAction"),
                    TotalDurationSeconds = info.Attributes.GetFloat("TotalDurationSeconds", 200f),
                    ContentLines = info.Value.HacknetFilter()
                });
            ActionLoaders.AddOverrideable("CancelScreenBleedEffect", info =>
            new SACancelScreenBleedEffect
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost")
                });
            ActionLoaders.AddOverrideable("AppendToFile", info =>
            new SAAppendToFile
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost"),
                    TargetComp = info.Attributes.GetValue("TargetComp"),
                    TargetFolderpath = info.Attributes.GetValue("TargetFolderpath"),
                    TargetFilename = info.Attributes.GetValue("TargetFilename"),
                    DataToAdd = info.Value.HacknetFilter()
                });
            ActionLoaders.AddOverrideable("KillExe", info =>
            new SAKillExe
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost"),
                    ExeName = info.Attributes.GetValue("ExeName")
                });
            ActionLoaders.AddOverrideable("ChangeAlertIcon", info =>
            new SAChangeAlertIcon
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost"),
                    Type = info.Attributes.GetWithOptions(
                        "Type", 
                        "Type", 
                        new [] { "mail", "irc", "board", "irchub"},
                        toLower: true
                        ),

                    Target = info.Attributes.GetValue("Target")
                });
            ActionLoaders.AddOverrideable("HideNode", info =>
            new SAHideNode
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost"),
                    TargetComp = info.Attributes.GetValue("TargetComp")
                });
            ActionLoaders.AddOverrideable("GivePlayerUserAccount", info =>
            new SAGivePlayerUserAccount
                {
                    Delay = info.Attributes.GetFloat("Delay"),
                    DelayHost = info.Attributes.GetValue("DelayHost"),
                    TargetComp = info.Attributes.GetValue("TargetComp"),
                    Username = info.Attributes.GetValue("Username")
                });
            ActionLoaders.AddOverrideable("ChangeIP", info =>
            new SAChangeIP
            {
                Delay = info.Attributes.GetFloat("Delay"),
                DelayHost = info.Attributes.GetValue("DelayHost"),
                TargetComp = info.Attributes.GetValue("TargetComp"),
                NewIP = info.Attributes.GetValue("NewIP")
            });
            ActionLoaders.AddOverrideable("ChangeNetmapSortMethod", info =>
            new SAChangeNetmapSortMethod
            {
                Delay = info.Attributes.GetFloat("Delay"),
                DelayHost = info.Attributes.GetValue("DelayHost"),
                Method = info.Attributes.GetWithOptions("Method", 
                    "Sorting Method",
                    new [] { "scatter", "grid", "chaos", "scangrid", "seqgrid", "sequencegrid", "sequence grid" },
                    toLower: true
                )
            });
            ActionLoaders.AddOverrideable("SaveGame", info =>
            new SASaveGame
            {
                Delay = info.Attributes.GetFloat("Delay"),
                DelayHost = info.Attributes.GetValue("DelayHost")
            });
            ActionLoaders.AddOverrideable("HideAllNodes", info =>
            new SAHideAllNodes
            {
                Delay = info.Attributes.GetFloat("Delay"),
                DelayHost = info.Attributes.GetValue("DelayHost")
            });
            ActionLoaders.AddOverrideable("ShowNode", info =>
            new SAShowNode
            {
                Delay = info.Attributes.GetFloat("Delay"),
                DelayHost = info.Attributes.GetValue("DelayHost"),
                Target = info.Attributes.GetValue("Target")
            });
            ActionLoaders.AddOverrideable("SetLock", info =>
            new SASetLock
            {
                Delay = info.Attributes.GetFloat("Delay"),
                DelayHost = info.Attributes.GetValue("DelayHost"),
                Module = info.Attributes.GetWithOptions("Module",
                    "Target Module",
                    new [] { "terminal", "netmap", "ram", "display" },
                    toLower: true
                ),
                IsLocked = info.Attributes.GetBool("IsLocked"),
                IsHidden = info.Attributes.GetBool("IsHidden")
            });
        }

        internal static void InitConditionLoaders()
        {
            ConditionLoaders.AddOverrideable("DoesNotHaveFlags", info =>
                new SCDoesNotHaveFlags
                {
                    Flags = info.Attributes.GetValue("Flags")
                });
            ConditionLoaders.AddOverrideable("HasFlags", info =>
                new SCHasFlags
                {
                    requiredFlags = info.Attributes.GetValue("requiredFlags")
                });
            ConditionLoaders.AddOverrideable("Instantly", info =>
                new SCInstantly
                {
                    needsMissionComplete = info.Attributes.GetBool("needsMissionComplete")
                });
            ConditionLoaders.AddOverrideable("OnAdminGained", info =>
                new SCOnAdminGained
                {
                    target = info.Attributes.GetNonEmptyString("target", "Target Computer")
                });
            ConditionLoaders.AddOverrideable("OnConnect", info =>
                new SCOnConnect
                {
                    target = info.Attributes.GetNonEmptyString("target", "Target Computer"),
                    needsMissionComplete =  info.Attributes.GetBool("needsMissionComplete"),
                    requiredFlags = info.Attributes.GetValue("requiredFlags")
                });
            ConditionLoaders.AddOverrideable("OnDisconnect", info =>
                new SCOnDisconnect
                {
                    target = info.Attributes.GetNonEmptyString("target", "Target Computer")
                });
        }
        public static SerializableAction LoadAction(ElementInfo root)
        {
            var actionName = root.Name;
            Console.WriteLine("[ActionLoader] Loading Action: " + actionName);
            if (!ActionLoaders.TryGetValue(actionName, out var loader))
            {
                throw new ActionLoadException(actionName, "No Loader defined.");
            }
            try
            {
                return loader(root);
            } catch(Exception ex)
            {
                throw new ActionLoadException(actionName, ex.Message, ex);
            }
        }
        public static SerializableCondition LoadCondition(ElementInfo root)
        {
            string conditionName = root.Name;
            if(!ConditionLoaders.TryGetValue(conditionName, out var loader))
            {
                throw new ConditionLoadException(conditionName, "No Loader defined.");
            }

            try
            {
                return loader(root);
            } catch(Exception ex)
            {
                throw new ConditionLoadException(conditionName, ex.Message, ex);
            }
        }
        public static SerializableConditionalActionSet LoadConditionalActionSet(ElementInfo root)
        {
            var result = new SerializableConditionalActionSet
            {
                Condition = LoadCondition(root)
            };
            foreach(var info in root.Children)
            {
                result.Actions.Add(LoadAction(info));
            }
            return result;
        }
        public static RunnableConditionalActions LoadConditionalActions(ElementInfo root)
        {
            var result = new RunnableConditionalActions();
            foreach(var info in root.Children)
            {
                result.Actions.Add(LoadConditionalActionSet(info));
            }
            return result;
        }

        public static RunnableConditionalActions LoadConditionalActionsFromFile(string filename)
        {
            var executor = new EventExecutor(filename);
            RunnableConditionalActions result = null;

            executor.AddExecutor("ConditionalActions", (exec, info) =>
            {
                result = LoadConditionalActions(info);
            }, true);

            executor.Parse();

            if (result == null)
                throw new LoadException("Conditional Actions File", "Incorrect root element");

            return result;
        }
    }
}
