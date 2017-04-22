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

        /// <summary>
        /// Gets the previous stack frame identity.
        /// </summary>
        /// <returns>The previous stack frame identity.</returns>
        /// <param name="frameSkip">The frames to skip back to.</param>
        public static string GetPreviousStackFrameIdentity(int frameSkip = 2)
        {
            var result = "";
            var asm = new StackFrame(frameSkip).GetMethod().Module.Assembly;
            if (asm == MethodBase.GetCurrentMethod().Module.Assembly)
                result = "Pathfinder";
            else
                result = Pathfinder.GetModByAssembly(asm).Identifier;
            return result;
        }

        /// <summary>
        /// Converts the input to a valid xml attribute name.
        /// </summary>
        /// <returns>A valid xml attribute name.</returns>
        /// <param name="input">Input to convert.</param>
        public static string ConvertToValidXmlAttributeName(string input)
        {
            input = xmlAttribRegex.Replace(input, "_");
            if (Char.IsDigit(input[0]))
                input = '_' + input.Substring(1);
            return input;
        }

        /// <summary>
        /// Retrieves an identifier for the input.
        /// </summary>
        /// <returns>The resulting identifier.</returns>
        /// <param name="inputId">Input identifier.</param>
        /// <param name="ignorePeriod">If set to <c>true</c> ignore period.</param>
        /// <param name="frameSkip">The frames to skip back to.</param>
        /// <param name="ignoreValidXml">If set to <c>true</c> ignore valid xml.</param>
        public static string GetId(string inputId, bool ignorePeriod = false, int frameSkip = 3, bool ignoreValidXml = false)
        {
            inputId = ignoreValidXml ? inputId : ConvertToValidXmlAttributeName(inputId);
            if (!ignorePeriod && inputId.IndexOf('.') == -1)
                inputId = GetPreviousStackFrameIdentity(frameSkip) + "." + inputId;
            return inputId;
        }

        /// <summary>
        /// Gets the primary NetworkMap for the current OS instance.
        /// </summary>
        /// <returns>The primary Network Map.</returns>
        public static Hacknet.NetworkMap GetPrimaryNetMap()
        {
            return Hacknet.OS.currentInstance?.netMap;
        }
    }
}
