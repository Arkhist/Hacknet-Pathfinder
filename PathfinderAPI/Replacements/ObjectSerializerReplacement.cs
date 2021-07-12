using System;
using System.Collections.Generic;
using System.Xml;
using Hacknet;
using HarmonyLib;
using Pathfinder.Util.XML;

namespace Pathfinder.Replacements
{
    [HarmonyPatch]
    public static class ObjectSerializerReplacement
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ObjectSerializer), nameof(ObjectSerializer.SerializeObject), new Type[] { typeof(object), typeof(bool) })]
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ObjectSerializer), nameof(ObjectSerializer.DeserializeObject), new Type[] { typeof(XmlReader), typeof(Type) })]
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

                return new List<ReflectiveRenderer.RenderableField>()
                {
                    new ReflectiveRenderer.RenderableField()
                    {
                        IndentLevel = indent,
                        IsTitle = false,
                        RenderedValue = info.Content,
                        t = type,
                        VariableName = info.Name
                    }
                };
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
}