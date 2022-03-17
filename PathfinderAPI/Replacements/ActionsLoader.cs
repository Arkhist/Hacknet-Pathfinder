﻿using System.Reflection;
using System.Xml;
using Hacknet;
using HarmonyLib;
using MonoMod.Cil;
using Pathfinder.Action;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements;

[HarmonyPatch]
public static class ActionsLoader
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RunnableConditionalActions), nameof(RunnableConditionalActions.LoadIntoOS))]
    private static bool LoadActionsIntoOSPrefix(string filepath, object OSobj)
    {
        OS os = (OS)OSobj;
            
        var executor = new EventExecutor(filepath.ContentFilePath(), true);
            
        executor.RegisterExecutor("ConditionalActions", (exec, info) => os.ConditionalActions.Actions.AddRange(LoadActionSets(info).Actions), ParseOption.ParseInterior);

        if (!executor.TryParse(out var ex))
        {
            throw new FormatException($"{filepath}: {ex.Message}", ex);
        }

        if (!os.ConditionalActions.IsUpdating)
            os.ConditionalActions.Update(0f, os);
            
        return false;
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(DelayableActionSystem), nameof(DelayableActionSystem.Update))]
    [HarmonyPatch(typeof(FastDelayableActionSystem), nameof(FastDelayableActionSystem.DeserializeActions))]
    private static void ReplaceDASDeserializeIL(ILContext il, MethodBase method)
    {
        ILCursor c = new ILCursor(il);
            
        c.GotoNext(MoveType.Before,
            x => x.MatchLdloc(7),
            x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(XmlReader), nameof(XmlReader.Create), new Type[] { typeof(Stream) })),
            x => x.MatchStloc(8),
            x => x.MatchLdloc(8),
            x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(SerializableAction), nameof(SerializableAction.Deserialize)))
        );

        c.Index++;
        c.RemoveRange(4);
        c.EmitDelegate<Func<Stream, SerializableAction>>(stream =>
        {
            var executor = new EventExecutor(new StreamReader(stream).ReadToEnd(), false);
            ElementInfo actionInfo = null;
            executor.RegisterExecutor("*", (exec, info) =>
            {
                actionInfo = info;
            }, ParseOption.ParseInterior);
            executor.Parse();
            return ReadAction(actionInfo);
        });

        if (method.Name == nameof(DelayableActionSystem.Update))
        {
            c.GotoNext(MoveType.Before,
                x => x.MatchLdloc(8),
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(XmlReader), nameof(XmlReader.Close)))
            );

            c.RemoveRange(2);
        }
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
                            target = conditionInfo.Attributes.GetOrDefault("target")
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
                
            set.Actions.AddRange(conditionInfo.Children.Select(ReadAction));
                
            ret.Actions.Add(set);
        }
            
        return ret;
    }

    public static SerializableAction ReadAction(ElementInfo actionInfo)
    {
        if (ActionManager.TryLoadCustomAction(actionInfo, out var customAction))
        {
            return customAction;
        }

        switch (actionInfo.Name)
        {
            case "LoadMission":
                return ActionDelayDecorator.Create(actionInfo, new SALoadMission()
                {
                    MissionName = actionInfo.Attributes.GetOrThrow("MissionName", "Invalid mission for LoadMission action", StringExtensions.ContentFileExists)
                });
            case "RunFunction":
                return new SARunFunction()
                {
                    FunctionName = actionInfo.Attributes.GetOrThrow("FunctionName", "No function given for RunFunction action"),
                    FunctionValue = actionInfo.Attributes.GetInt("FunctionValue"),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "AddAsset":
                return ActionDelayDecorator.Create(actionInfo, new SAAddAsset()
                {
                    TargetComp = actionInfo.Attributes.GetString("TargetComp", null),
                    TargetFolderpath = actionInfo.Attributes.GetString("TargetFolderpath", null),
                    FileName = actionInfo.Attributes.GetString("FileName", null),
                    FileContents = actionInfo.Attributes.GetString("FileContents", null)
                });
            case "AddMissionToHubServer":
                var tag = actionInfo.Attributes.GetString("AssignmentTag");
                if (string.IsNullOrWhiteSpace(tag))
                    tag = null;
                return ActionDelayDecorator.Create(actionInfo, new SAAddMissionToHubServer()
                {
                    MissionFilepath = actionInfo.Attributes.GetOrWarn("MissionFilepath", "Invalid mission file path for AddMissionToHubServer", StringExtensions.ContentFileExists),
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for AddMissionToHubServer", StringExtensions.HasContent),
                    AssignmentTag = tag,
                    StartsComplete = actionInfo.Attributes.GetBool("StartsComplete")
                });
            case "RemoveMissionFromHubServer":
                return ActionDelayDecorator.Create(actionInfo, new SARemoveMissionFromHubServer()
                {
                    MissionFilepath = actionInfo.Attributes.GetOrWarn("MissionFilepath", "Invalid mission file path for RemoveMissionFromHubServer", StringExtensions.ContentFileExists),
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for RemoveMissionFromHubServer", StringExtensions.HasContent)
                });
            case "AddThreadToMissionBoard":
                return ActionDelayDecorator.Create(actionInfo, new SAAddThreadToMissionBoard()
                {
                    ThreadFilepath = actionInfo.Attributes.GetOrWarn("ThreadFilepath", "Invalid thread path for AddThreadToMissionBoard", StringExtensions.ContentFileExists),
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for AddThreadToMissionBoard", StringExtensions.HasContent)
                });
            case "AddIRCMessage":
                return new SAAddIRCMessage()
                {
                    Author = ComputerLoader.filter(actionInfo.Attributes.GetOrThrow("Author", "Invalid author for AddIRCMessage", StringExtensions.HasContent)),
                    Message = ComputerLoader.filter(string.IsNullOrEmpty(actionInfo.Content) ? throw new FormatException("Invalid message for AddIRCMessage") : actionInfo.Content),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for AddIRCMessage", StringExtensions.HasContent)
                };
            case "AddConditionalActions":
                return new SAAddConditionalActions()
                {
                    Filepath = actionInfo.Attributes.GetOrWarn("Filepath", "Invalid actions path for AddConditionalActions", StringExtensions.ContentFileExists),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "CopyAsset":
                return ActionDelayDecorator.Create(actionInfo, new SACopyAsset()
                {
                    SourceComp = actionInfo.Attributes.GetOrThrow("SourceComp", "Invalid source computer for CopyAsset", StringExtensions.HasContent),
                    SourceFilePath = actionInfo.Attributes.GetOrThrow("SourceFilePath", "Source file path is required for CopyAsset"),
                    SourceFileName = actionInfo.Attributes.GetOrThrow("SourceFileName", "Invalid source file name for CopyAsset", StringExtensions.HasContent),
                    DestComp = actionInfo.Attributes.GetOrThrow("DestComp", "Invalid dest computer for CopyAsset", StringExtensions.HasContent),
                    DestFilePath = actionInfo.Attributes.GetOrThrow("DestFilePath", "Invalid dest file path for CopyAsset", StringExtensions.HasContent),
                    DestFileName = actionInfo.Attributes.GetString("DestFileName", actionInfo.Attributes.GetString("SourceFileName"))
                });
            case "CrashComputer":
                return new SACrashComputer()
                {
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target comp for CrashComputer", StringExtensions.HasContent),
                    CrashSource = actionInfo.Attributes.GetString("CrashSource", null),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "DeleteFile":
                return new SADeleteFile()
                {
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target comp for DeleteFile", StringExtensions.HasContent),
                    FilePath = actionInfo.Attributes.GetOrThrow("FilePath", "Invalid path for DeleteFile"),
                    FileName = actionInfo.Attributes.GetOrThrow("FileName", "Invalid file name for DeleteFile", StringExtensions.HasContent),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "LaunchHackScript":
                return new SALaunchHackScript()
                {
                    Filepath = actionInfo.Attributes.GetOrThrow("Filepath", "Invalid hackerscript path for LaunchHackScript", StringExtensions.ContentFileExists),
                    TargetComp = actionInfo.Attributes.GetString("TargetComp", null),
                    SourceComp = actionInfo.Attributes.GetString("SourceComp", null),
                    RequireSourceIntact = actionInfo.Attributes.GetBool("RequireSourceIntact"),
                    RequireLogsOnSource = actionInfo.Attributes.GetBool("RequireLogsOnSource"),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "SwitchToTheme":
                return new SASwitchToTheme()
                {
                    ThemePathOrName = actionInfo.Attributes.GetOrThrow("ThemePathOrName", "Invalid theme name or path for SwitchToTheme", StringExtensions.HasContent),
                    FlickerInDuration = actionInfo.Attributes.GetFloat("FlickerInDuration"),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "StartScreenBleedEffect":
                return new SAStartScreenBleedEffect()
                {
                    AlertTitle = ComputerLoader.filter(actionInfo.Attributes.GetString("AlertTitle", null)),
                    ContentLines = ComputerLoader.filter(actionInfo.Content ?? throw new FormatException("StartScreenBleedEffect can't have no content")),
                    TotalDurationSeconds = actionInfo.Attributes.GetFloat("TotalDurationSeconds"),
                    CompleteAction = actionInfo.Attributes.GetString("CompleteAction", null),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "CancelScreenBleedEffect":
                return new SACancelScreenBleedEffect()
                {
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "AppendToFile":
                return new SAAppendToFile()
                {
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for AppendToFile", StringExtensions.HasContent),
                    TargetFolderpath = actionInfo.Attributes.GetOrThrow("TargetFolderpath", "Invalid path for AppendToFile"),
                    TargetFilename = actionInfo.Attributes.GetOrThrow("TargetFilename", "Invalid file name for AppendToFile", StringExtensions.HasContent),
                    DataToAdd = ComputerLoader.filter(actionInfo.Content ?? throw new FormatException("AppendToFile can't have no content")),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "KillExe":
                return new SAKillExe()
                {
                    ExeName = actionInfo.Attributes.GetOrThrow("ExeName", "Invalid exe name for KillExe"),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "ChangeAlertIcon":
                return new SAChangeAlertIcon()
                {
                    Type = actionInfo.Attributes.GetOrThrow("Type", "Invalid icon type, options are mail, irc, and board (only mail if no labyrinths!)", s =>
                    {
                        if (s != "mail" && !DLC1SessionUpgrader.HasDLC1Installed)
                            return false;
                        if (s != "mail" && s != "board" && s != "irc" && s != "irchub")
                            return false;
                        return true;
                    }),
                    Target = actionInfo.Attributes.GetOrThrow("Target", "Invalid target computer for ChangeAlertIcon", StringExtensions.HasContent),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "HideNode":
                return new SAHideNode()
                {
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for HideNode", StringExtensions.HasContent),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "GivePlayerUserAccount":
                return new SAGivePlayerUserAccount()
                {
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for GivePlayerUserAccount", StringExtensions.HasContent),
                    Username = actionInfo.Attributes.GetOrThrow("Username", "Invalid username for GivePlayerUserAccount", StringExtensions.HasContent),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "ChangeIP":
                return new SAChangeIP()
                {
                    TargetComp = actionInfo.Attributes.GetOrThrow("TargetComp", "Invalid target computer for ChangeIP", StringExtensions.HasContent),
                    NewIP = actionInfo.Attributes.GetString("NewIP", null),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "ChangeNetmapSortMethod":
                return new SAChangeNetmapSortMethod()
                {
                    Method = actionInfo.Attributes.GetOrThrow("Method", "Invalid method for ChangeNetmapSortMethod"),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "SaveGame":
                return new SASaveGame()
                {
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "HideAllNodes":
                return new SAHideAllNodes()
                {
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "ShowNode":
                return new SAShowNode()
                {
                    Target = actionInfo.Attributes.GetAnyOrThrow(new[] { "Target", "TargetComp" }, "Invalid target computer for ShowNode", StringExtensions.HasContent),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            case "SetLock":
                return new SASetLock()
                {
                    Module = actionInfo.Attributes.GetOrThrow("Module", "Invalid module for SetLock"),
                    IsHidden = actionInfo.Attributes.GetBool("IsHidden"),
                    IsLocked = actionInfo.Attributes.GetBool("IsLocked"),
                    Delay = actionInfo.Attributes.GetFloat("Delay"),
                    DelayHost = actionInfo.Attributes.GetString("DelayHost", null)
                };
            default:
                throw new KeyNotFoundException($"Unknown action type {actionInfo.Name}");
        }
    }
}