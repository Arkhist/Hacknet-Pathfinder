using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pathfinder.Util
{
    public static class Utility
    {
        public static class Array<T>
        {
            public static readonly T[] Empty = new T[0];
        }

        private static Regex xmlAttribRegex = new Regex("[^a-zA-Z0-9_.]");

        public static string GetPreviousStackFrameIdentity(int frameSkip = 2)
        {
            var result = "";
            var asm = new StackFrame(frameSkip).GetMethod().Module.Assembly;
            if (asm == MethodBase.GetCurrentMethod().Module.Assembly)
                result = "Pathfinder";
            else
                result = Pathfinder.GetModByAssembly(asm).GetIdentifier();
            return result;
        }

        public static string ConvertToValidXmlAttributeName(string input)
        {
            input = xmlAttribRegex.Replace(input, "_");
            if (Char.IsDigit(input[0]))
                input = '_' + input.Substring(1);
            return input;
        }

        public static string GetId(string inputId, bool ignorePeriod = false, int frameSkip = 3, bool ignoreValidXml = false)
        {
            inputId = ignoreValidXml ? inputId : ConvertToValidXmlAttributeName(inputId);
            if (!ignorePeriod && inputId.IndexOf('.') == -1)
                inputId = GetPreviousStackFrameIdentity(frameSkip) + "." + inputId;
            return inputId;
        }

        public static Hacknet.NetworkMap GetPrimaryNetMap()
        {
            return Hacknet.OS.currentInstance?.netMap;
        }
    }
}
