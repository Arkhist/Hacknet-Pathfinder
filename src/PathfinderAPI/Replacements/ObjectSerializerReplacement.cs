using System.Reflection;
using System.Xml;
using Hacknet;
using HarmonyLib;
using MonoMod.Cil;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements;

[HarmonyPatch]
public static class ObjectSerializerReplacement
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DatabaseDaemon), nameof(DatabaseDaemon.CleanXMLForFile))]
    [HarmonyPatch(typeof(DatabaseDaemon), nameof(DatabaseDaemon.DeCleanXMLForFile))]
    internal static bool DontCleanForXML(string data, out string __result)
    {
        __result = data.Replace("\t", "  ");
        return false;
    }
        
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectSerializer), nameof(ObjectSerializer.SerializeObject), [typeof(object), typeof(bool)])]
    public static bool SerializeObject(object o, bool preventOuterTag, ref string __result)
    {
        if (o == null)
        {
            __result = "";
            return false;
        }

        if (o is ElementInfo info)
        {
            __result = info.ToString();
            return false;
        }

        return true;
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(ObjectSerializer), nameof(ObjectSerializer.SerializeObject), [typeof(object), typeof(bool)])]
    private static void DontSerializeStaticFields(ILContext il)
    {
        ILCursor c = new ILCursor(il);
            
        c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(Type), nameof(Type.GetFields), new Type[0])));

        c.EmitDelegate<Func<FieldInfo[], FieldInfo[]>>(fields => fields.Where(x => !x.IsStatic).ToArray());
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectSerializer), nameof(ObjectSerializer.DeserializeObject), [typeof(XmlReader), typeof(Type)])]
    public static bool DeserializeObject(XmlReader rdr, Type t, out object __result)
    {
        var executor = new EventExecutor(rdr);

        ElementInfo result = null;
            
        executor.RegisterExecutor("*", (exec, info) =>
        {
            result = info;
        }, ParseOption.ParseInterior);
            
        executor.Parse();

        __result = result;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectSerializer), nameof(ObjectSerializer.GetValueFromObject))]
    internal static bool GetValueFromElementInfo(object o, string FieldName, ref object __result)
    {
        if (o is ElementInfo info)
        {
            __result = info.Children.TryGetElement(FieldName, out var field) ? field.Content : null;
            return false;
        }

        return true;
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(DatabaseDaemon), nameof(DatabaseDaemon.DrawEntry))]
    internal static void DatabaseDaemonDirectDeserialize(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0)
        );

        c.Index += 2;
        c.RemoveRange(8);

        c.EmitDelegate<Func<DatabaseDaemon, object>>(database =>
        {
            var executor = new EventExecutor(database.ActiveFile.data, false);

            ElementInfo result = null;
            
            executor.RegisterExecutor("*", (exec, info) =>
            {
                result = info;
            }, ParseOption.ParseInterior);
            
            executor.Parse();

            return result;
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DatabaseDaemon), nameof(DatabaseDaemon.GetFilenameForObject))]
    internal static bool GetCorrectFileName(DatabaseDaemon __instance, object obj, ref string __result)
    {
        if (obj is ElementInfo info)
        {
            switch (info.Name)
            {
                case "GitCommitEntry":
                    __result =
                        $"Commit#{int.Parse(info.Children.GetElement("EntryNumber").Content):000}{(info.Children.GetElement("SourceIP").Content.StartsWith("192.168.1.1") ? "" : "*")}";
                    break;
                case "Person":
                    __result = __instance.GetFilenameForPersonName(info.Children.GetElement("firstName").Content, info.Children.GetElement("lastName").Content);
                    return false;
                case "TextRecord":
                    __result = info.Children.GetElement("Title").Content;
                    break;
                case "OnlineAccount":
                    __result = info.Children.GetElement("Username").Content + "#" + info.Children.GetElement("ID").Content;
                    break;
                case "CAROData":
                    __result = info.Children.GetElement("UserID").Content;
                    break;
                case "Account":
                    __result = info.Children.GetElement("ID").Content;
                    break;
                case "SurveillanceProfile":
                    __result = info.Children.GetElement("Name").Content;
                    break;
                case "AgentDetails":
                    __result = info.Children.GetElement("Codename").Content;
                    break;
                default:
                    __result = info.Name;
                    break;
            }

            __result = Utils.GetNonRepeatingFilename(__result.Replace(" ", "_").ToLower(), ".rec", __instance.DatasetFolder);
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ReflectiveRenderer), nameof(ReflectiveRenderer.GetRenderablesFromType))]
    public static bool GetRenderablesFromTypePrefix(Type type, object o, int indentLevel, ref List<ReflectiveRenderer.RenderableField> __result)
    {
        if (o is ElementInfo info)
        {
            __result = GetRenderables(info, indentLevel);
            return false;
        }

        throw new NotSupportedException("Pathfinder ReflectiveRenderer only supports ElementInfo!");
    }
        
    private static List<ReflectiveRenderer.RenderableField> GetRenderables(ElementInfo info, int indent) 
    {
        if (info.Content != null)
        {
            var type = ObjectSerializer.GetTypeForName(info.Name);
            if (Enum.TryParse(info.Content, out Neopal.PetType _))
            {
                type = typeof(Neopal.PetType);
            }

            return
            [
                new ReflectiveRenderer.RenderableField()
                {
                    IndentLevel = indent,
                    IsTitle = false,
                    RenderedValue = info.Content,
                    t = type,
                    VariableName = info.Name
                }
            ];
        }
            
        var list = new List<ReflectiveRenderer.RenderableField>()
        {
            new ReflectiveRenderer.RenderableField()
            {
                IndentLevel = indent,
                IsTitle = true,
                RenderedValue = info.Name
            }
        };
        foreach (var child in info.Children)
        {
            list.AddRange(GetRenderables(child, indent + 1));
        }

        return list;
    }
}
