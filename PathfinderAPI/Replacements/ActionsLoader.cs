using System;
using System.IO;
using Hacknet;
using HarmonyLib;
using Pathfinder.Action;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class ActionsLoader
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunnableConditionalActions), nameof(RunnableConditionalActions.LoadIntoOS))]
        private static bool LoadActionsIntoOSPrefix(string filepath, object OSobj)
        {
            OS os = (OS)OSobj;
            
            var executor = new EventExecutor(LocalizedFileLoader.GetLocalizedFilepath(Utils.GetFileLoadPrefix() + filepath), true);
            
            executor.RegisterExecutor("ConditionalActions", (exec, info) => os.ConditionalActions.Actions.AddRange(LoadActionSets(info).Actions), ParseOption.ParseInterior);

            if (!executor.TryParse(out var ex))
            {
                throw new FormatException($"{filepath}: {ex.Message}", ex);
            }
            
            return false;
        }

        public static RunnableConditionalActions LoadActionSets(ElementInfo root)
        {
            var ret = new RunnableConditionalActions();
            
            if (root.Name != "ConditionalActions")
                throw new FormatException("Condtional actions root element wasn't named \"ConditionalActions\"!");

            foreach (var conditionInfo in root.Children)
            {
                var set = new SerializableConditionalActionSet();
                
                #region Conditions
                
                if (ConditionManager.TryLoadCustomCondition(conditionInfo, out var customCondition))
                {
                    set.Condition = customCondition;
                }
                else
                {
                    switch (conditionInfo.Name)
                    {
                        case "OnAdminGained":
                            set.Condition = new SCOnAdminGained()
                            {
                                target = conditionInfo.Attributes.GetOrThrow("target", "No target found for OnAdminGained condition!")
                            };
                            break;
                        case "OnConnect":
                            set.Condition = new SCOnConnect()
                            {
                                target = conditionInfo.Attributes.GetOrThrow("target", "No target found for OnConnect condition!"),
                                requiredFlags = conditionInfo.Attributes.GetString("requiredFlags", null),
                                needsMissionComplete = conditionInfo.Attributes.GetBool("needsMissionComplete")
                            };
                            break;
                        case "HasFlags":
                            set.Condition = new SCHasFlags()
                            {
                                requiredFlags = conditionInfo.Attributes.GetString("requiredFlags", null)
                            };
                            break;
                        case "Instantly":
                            set.Condition = new SCInstantly()
                            {
                                needsMissionComplete = conditionInfo.Attributes.GetBool("needsMissionComplete")
                            };
                            break;
                        case "OnDisconnect":
                            set.Condition = new SCOnDisconnect()
                            {
                                target = conditionInfo.Attributes.GetOrThrow("target", "No target found for OnDisconnect condition!")
                            };
                            break;
                        case "DoesNotHaveFlags":
                            set.Condition = new SCDoesNotHaveFlags()
                            {
                                Flags = conditionInfo.Attributes.GetString("Flags", null)
                            };
                            break;
                    }
                }
                
                if (set.Condition == null)
                    throw new FormatException($"Condition {conditionInfo.Name} could not be found!");
                
                #endregion
                #region Actions
                
                foreach (var actionInfo in conditionInfo.Children)
                {
                    if (ActionManager.TryLoadCustomAction(actionInfo, out var customAction))
                    {
                        set.Actions.Add(customAction);
                        continue;
                    }

                    switch (actionInfo.Name)
                    {
                        case "LoadMission":
                            set.Actions.Add(new SALoadMission()
                            {
                                MissionName = actionInfo.Attributes.GetOrThrow("MissionName", "Invalid mission for LoadMission action", ExtFileExists)
                            });
                            break;
                        case "RunFunction":
                            set.Actions.Add(new SARunFunction()
                            {
                                FunctionName = actionInfo.Attributes.GetOrThrow("FunctionName", "No function given for RunFunction action"),
                                FunctionValue = actionInfo.Attributes.GetInt("FunctionValue"),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "AddAsset":
                            set.Actions.Add(new SAAddAsset()
                            {
                                TargetComp = actionInfo.Attributes.GetString("TargetComp", null),
                                TargetFolderpath = actionInfo.Attributes.GetString("TargetFolderpath", null),
                                FileName = actionInfo.Attributes.GetString("FileName", null),
                                FileContents = actionInfo.Attributes.GetString("FileContents", null)
                            });
                            break;
                        case "AddMissionToHubServer":
                            set.Actions.Add(new SAAddMissionToHubServer()
                            {
                                MissionFilepath = actionInfo.Attributes.GetOrThrow("MissionFilepath", "Invalid mission file path for AddMissionToHubServer", ExtFileExists),
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for AddMissionToHubServer", StringExtensions.HasContent),
                                AssignmentTag = actionInfo.Attributes.GetString("AssignmentTag", null),
                                StartsComplete = actionInfo.Attributes.GetBool("StartsComplete")
                            });
                            break;
                        case "RemoveMissionFromHubServer":
                            set.Actions.Add(new SARemoveMissionFromHubServer()
                            {
                                MissionFilepath = actionInfo.Attributes.GetOrThrow("MissionFilepath", "Invalid mission file path for RemoveMissionFromHubServer", ExtFileExists),
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for RemoveMissionFromHubServer", StringExtensions.HasContent)
                            });
                            break;
                        case "AddThreadToMissionBoard":
                            set.Actions.Add(new SAAddThreadToMissionBoard()
                            {
                                ThreadFilepath = actionInfo.Attributes.GetOrThrow("ThreadFilepath", "Invalid thread path for AddThreadToMissionBoard", ExtFileExists),
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for AddThreadToMissionBoard", StringExtensions.HasContent)
                            });
                            break;
                        case "AddIRCMessage":
                            set.Actions.Add(new SAAddIRCMessage()
                            {
                                Author = ComputerLoader.filter(actionInfo.Attributes.GetOrThrow("Author", "Invalid author for AddIRCMessage", StringExtensions.HasContent)),
                                Message = ComputerLoader.filter(string.IsNullOrEmpty(actionInfo.Content) ? throw new FormatException("Invalid message for AddIRCMessage") : actionInfo.Content),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for AddIRCMessage", StringExtensions.HasContent)
                            });
                            break;
                        case "AddConditionalActions":
                            set.Actions.Add(new SAAddConditionalActions()
                            {
                                Filepath = actionInfo.Attributes.GetOrThrow("Filepath", "Invalid actions path for AddConditionalActions", ExtFileExists),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "CopyAsset":
                            set.Actions.Add(new SACopyAsset()
                            {
                                SourceComp = actionInfo.Attributes.GetOrThrow("SourceComp", "Invalid source computer for CopyAsset", StringExtensions.HasContent),
                                SourceFilePath = actionInfo.Attributes.GetOrThrow("SourceFilePath", "Source file path is required for CopyAsset"),
                                SourceFileName = actionInfo.Attributes.GetOrThrow("SourceFileName", "Invalid source file name for CopyAsset", StringExtensions.HasContent),
                                DestComp = actionInfo.Attributes.GetOrThrow("DestComp", "Invalid dest computer for CopyAsset", StringExtensions.HasContent),
                                DestFilePath = actionInfo.Attributes.GetOrThrow("DestFilePath", "Invalid dest file path for CopyAsset", StringExtensions.HasContent),
                                DestFileName = actionInfo.Attributes.GetString("DestFileName", actionInfo.Attributes.GetString("SourceFileName"))
                            });
                            break;
                        case "CrashComputer":
                            set.Actions.Add(new SACrashComputer()
                            {
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target comp for CrashComputer", StringExtensions.HasContent),
                                CrashSource = actionInfo.Attributes.GetString("CrashSource", null),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "DeleteFile":
                            set.Actions.Add(new SADeleteFile()
                            {
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target comp for DeleteFile", StringExtensions.HasContent),
                                FilePath = actionInfo.Attributes.GetOrThrow("FilePath", "Invalid path for DeleteFile"),
                                FileName = actionInfo.Attributes.GetOrThrow("FileName", "Invalid file name for DeleteFile", StringExtensions.HasContent),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "LaunchHackScript":
                            set.Actions.Add(new SALaunchHackScript()
                            {
                                Filepath = actionInfo.Attributes.GetOrThrow("Filepath", "Invalid hackerscript path for LaunchHackScript", ExtFileExists),
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target comp for LaunchHackScript", StringExtensions.HasContent),
                                SourceComp = actionInfo.Attributes.GetOrThrow("SourceComp", "Invalid source computer for LaunchHackScript", StringExtensions.HasContent),
                                RequireSourceIntact = actionInfo.Attributes.GetBool("RequireSourceIntact"),
                                RequireLogsOnSource = actionInfo.Attributes.GetBool("RequireLogsOnSource"),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "SwitchToTheme":
                            set.Actions.Add(new SASwitchToTheme()
                            {
                                ThemePathOrName = actionInfo.Attributes.GetOrThrow("ThemePathOrName", "Invalid theme name or path for SwitchToTheme", StringExtensions.HasContent),
                                FlickerInDuration = actionInfo.Attributes.GetFloat("FlickerInDuration"),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "StartScreenBleedEffect":
                            set.Actions.Add(new SAStartScreenBleedEffect()
                            {
                                AlertTitle = ComputerLoader.filter(actionInfo.Attributes.GetString("AlertTitle", null)),
                                ContentLines = ComputerLoader.filter(actionInfo.Content ?? throw new FormatException("StartScreenBleedEffect can't have no content")),
                                TotalDurationSeconds = actionInfo.Attributes.GetFloat("TotalDurationSeconds"),
                                CompleteAction = actionInfo.Attributes.GetString("CompleteAction", null),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "CancelScreenBleedEffect":
                            set.Actions.Add(new SACancelScreenBleedEffect()
                            {
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "AppendToFile":
                            set.Actions.Add(new SAAppendToFile()
                            {
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for AppendToFile", StringExtensions.HasContent),
                                TargetFolderpath = actionInfo.Attributes.GetOrThrow("TargetFolderpath", "Invalid path for AppendToFile"),
                                TargetFilename = actionInfo.Attributes.GetOrThrow("TargetFilename", "Invalid file name for AppendToFile", StringExtensions.HasContent),
                                DataToAdd = ComputerLoader.filter(actionInfo.Content ?? throw new FormatException("AppendToFile can't have no content")),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "KillExe":
                            set.Actions.Add(new SAKillExe()
                            {
                                ExeName = actionInfo.Attributes.GetOrThrow("ExeName", "Invalid exe name for KillExe"),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "ChangeAlertIcon":
                            set.Actions.Add(new SAChangeAlertIcon()
                            {
                                Type = actionInfo.Attributes.GetOrThrow("Type", "Invalid icon type, options are mail, irc, and board (only mail if no labyrinths!)", s =>
                                    {
                                        if (s != "mail" && !DLC1SessionUpgrader.HasDLC1Installed)
                                            return false;
                                        if (s != "mail" && s != "board" && s != "irc")
                                            return false;
                                        return true;
                                    }),
                                Target = actionInfo.Attributes.GetOrThrow("Target", "Invalid target computer for ChangeAlertIcon", StringExtensions.HasContent),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "HideNode":
                            set.Actions.Add(new SAHideNode()
                            {
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for HideNode", StringExtensions.HasContent),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "GivePlayerUserAccount":
                            set.Actions.Add(new SAGivePlayerUserAccount()
                            {
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for GivePlayerUserAccount", StringExtensions.HasContent),
                                Username = actionInfo.Attributes.GetOrThrow("Username", "Invalid username for GivePlayerUserAccount", StringExtensions.HasContent),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "ChangeIP":
                            set.Actions.Add(new SAChangeIP()
                            {
                                TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for ChangeIP", StringExtensions.HasContent),
                                NewIP = actionInfo.Attributes.GetString("NewIP", null),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "ChangeNetmapSortMethod":
                            set.Actions.Add(new SAChangeNetmapSortMethod()
                            {
                                Method = actionInfo.Attributes.GetOrThrow("Method", "Invalid method for ChangeNetmapSortMethod"),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "SaveGame":
                            set.Actions.Add(new SASaveGame()
                            {
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "HideAllNodes":
                            set.Actions.Add(new SAHideAllNodes()
                            {
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "ShowNode":
                            set.Actions.Add(new SAShowNode()
                            {
                                Target = actionInfo.Attributes.GetOrThrow("Target", "Invalid target computer for ShowNode", StringExtensions.HasContent),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                        case "SetLock":
                            set.Actions.Add(new SASetLock()
                            {
                                Module = actionInfo.Attributes.GetOrThrow("Module", "Invalid module for SetLock"),
                                IsHidden = actionInfo.Attributes.GetBool("IsHidden"),
                                IsLocked = actionInfo.Attributes.GetBool("IsLocked"),
                                Delay = actionInfo.Attributes.GetFloat("Delay"),
                                DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                            });
                            break;
                    }
                }
                
                #endregion
                
                ret.Actions.Add(set);
            }
            
            return ret;
        }

        private static bool ExtFileExists(string filename) => File.Exists(LocalizedFileLoader.GetLocalizedFilepath(Utils.GetFileLoadPrefix() + filename));
    }
}