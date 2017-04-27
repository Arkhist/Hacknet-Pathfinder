#pragma warning disable RECS0137 // Method with optional parameter is hidden by overload
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
            var xmlString = inputId.IndexOf('.') != -1 ? inputId.Substring(inputId.LastIndexOf('.') + 1) : inputId;

            xmlString = ignoreValidXml ? xmlString : ConvertToValidXmlAttributeName(xmlString);
            if (!ignorePeriod && inputId.IndexOf('.') == -1)
                xmlString = GetPreviousStackFrameIdentity(frameSkip) + "." + xmlString;
            return inputId.IndexOf('.') != -1 ? inputId.Remove(inputId.LastIndexOf('.')+1) + xmlString : inputId;
        }

        /// <summary>
        /// Gets the current client OS.
        /// </summary>
        /// <returns>The client's current OS.</returns>
        public static Hacknet.OS GetClientOS()
        {
            return Hacknet.OS.currentInstance;
        }

        /// <summary>
        /// Gets the current client's Computer.
        /// </summary>
        /// <returns>The client's Current Computer.</returns>
        public static Hacknet.Computer GetClientComputer()
        {
            return GetClientOS().thisComputer;
        }

        /// <summary>
        /// Gets the current client's NetworkMap for the current client OS.
        /// </summary>
        /// <returns>The client's current Network Map.</returns>
        public static Hacknet.NetworkMap GetClientNetMap()
        {
            return GetClientOS()?.netMap;
        }

        /// <summary>
        /// Gets the current Computer the client is connected to.
        /// </summary>
        /// <returns>The currently connected to Computer.</returns>
        public static Hacknet.Computer GetCurrentComputer(Hacknet.OS os = null)
        {
            if (os == null)
                os = GetClientOS();
            return os?.connectedComp ?? os?.thisComputer;
        }

        public static Hacknet.Computer GetCurrentComputer()
        {
            return GetCurrentComputer(null);
        }
    }
}
