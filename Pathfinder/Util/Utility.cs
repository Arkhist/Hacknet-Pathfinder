#pragma warning disable RECS0137 // Method with optional parameter is hidden by overload
using System;
using System.Text.RegularExpressions;

namespace Pathfinder.Util
{
    public static class Utility
    {
        /// <summary>
        /// Type specific Array Utilities
        /// </summary>
        public static class Array<T>
        {
            /// <summary>
            /// An Empty Array of T
            /// </summary>
            public static readonly T[] Empty = new T[0];
        }

        private static Regex xmlAttribRegex = new Regex("[^a-zA-Z0-9_.]");

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

        public static string ActiveModId => Pathfinder.CurrentMod?.Identifier ?? "Pathfinder";

        /// <summary>
        /// Retrieves an identifier for the input.
        /// </summary>
        /// <returns>The resulting identifier.</returns>
        /// <param name="inputId">Input identifier.</param>
        /// <param name="ignorePeriod">If set to <c>true</c> ignore period.</param>
        /// <param name="ignoreValidXml">If set to <c>true</c> ignore valid xml.</param>
        public static string GetId(string inputId, bool ignorePeriod = false, bool ignoreValidXml = false, bool throwFindingPeriod = false)
        {
            if (throwFindingPeriod && inputId.IndexOf('.') != -1)
                throw new ArgumentException("inputId can't have a period in it", nameof(inputId));
            var xmlString = inputId.IndexOf('.') != -1 ? inputId.Substring(inputId.LastIndexOf('.') + 1) : inputId;

            xmlString = ignoreValidXml ? xmlString : ConvertToValidXmlAttributeName(xmlString);
            if (!ignorePeriod && inputId.IndexOf('.') == -1)
                xmlString = ActiveModId + "." + xmlString;
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
        /// Gets the current Computer the OS is active in.
        /// </summary>
        /// <returns>The currently active Computer.</returns>
        /// <param name="os">The OS to get the current Computer, or equal to <see cref="Utility.GetClientOS"/> if <c>null</c>.</param>
        public static Hacknet.Computer GetCurrentComputer(Hacknet.OS os = null)
        {
            if (os == null)
                os = GetClientOS();
            return os?.connectedComp ?? os?.thisComputer;
        }

        public static Hacknet.Computer GetCurrentComputer() => GetCurrentComputer(null);
    }
}
