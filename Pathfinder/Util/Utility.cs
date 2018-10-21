using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Hacknet;
using Pathfinder.ModManager;

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

        private static Random pathfinderRng = new Random();
        private static Regex xmlAttribRegex = new Regex("[^a-zA-Z0-9_.]");

        public static int? HaltOffset = 0;

        /// <summary>
        /// Converts the input to a valid xml attribute name.
        /// </summary>
        /// <returns>A valid xml attribute name.</returns>
        /// <param name="input">Input to convert.</param>
        public static string ConvertToValidXmlAttributeName(string input)
        {
            input = xmlAttribRegex.Replace(input, "_");
            if (char.IsDigit(input[0]))
                input = '_' + input.Substring(1);
            return input;
        }

        /// <summary>
        /// Gets the active mod's identifier.
        /// </summary>
        /// <value>The active mod identifier or Pathfinder if there is no active mod.</value>
        public static string ActiveModId => Pathfinder.CurrentMod?.GetCleanId() ?? "Pathfinder";

        public static string GetCleanId(this string id) => id.Trim();
        public static string GetCleanId(this IMod mod) => mod.Identifier.GetCleanId();

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
                throw new ArgumentException("Can't have a period in it", nameof(inputId));
            var xmlString = inputId.IndexOf('.') != -1 ? inputId.Substring(inputId.LastIndexOf('.') + 1) : inputId;

            xmlString = ignoreValidXml ? xmlString : ConvertToValidXmlAttributeName(xmlString);
            if (!ignorePeriod && inputId.IndexOf('.') == -1)
                return ActiveModId + "." + xmlString;
            return inputId.IndexOf('.') != -1 ? inputId.Remove(inputId.LastIndexOf('.') + 1) + xmlString : inputId;
        }

        /// <summary>
        /// Gets the current client OS.
        /// </summary>
        /// <returns>The client's current OS.</returns>
        public static OS ClientOS => OS.currentInstance;

        /// <summary>
        /// Gets the current client's Computer.
        /// </summary>
        /// <returns>The client's Current Computer.</returns>
        public static Computer ClientComputer => ClientOS.thisComputer;

        /// <summary>
        /// Gets the current client's NetworkMap for the current client OS.
        /// </summary>
        /// <returns>The client's current Network Map.</returns>
        public static NetworkMap ClientNetworkMap => ClientOS?.netMap;

        /// <summary>
        /// Gets the current Computer the OS is active in.
        /// </summary>
        /// <returns>The currently active Computer.</returns>
        /// <param name="os">The OS to get the current Computer, or equal to <see cref="ClientOS"/> if <c>null</c>.</param>
        public static Computer GetCurrentComputer(OS os = null)
        {
            if (os == null)
                os = ClientOS;
            return os.connectedComp ?? os.thisComputer;
        }

        /// <summary>
        /// Gets the current Computer the client is connected to.
        /// </summary>
        /// <value>The current Computer.</value>
        public static Computer CurrentComputer => GetCurrentComputer(null);

        public static int GenerateRandomIPSection(Random rand = null) => (rand ?? pathfinderRng).Next(254) + 1;
        public static string GenerateRandomIP(Random rand = null) =>
            GenerateRandomIPSection(rand) + "." + GenerateRandomIPSection(rand)
                + "." + GenerateRandomIPSection(rand) + "." + GenerateRandomIPSection(rand);

        public static void HaltThread(int ms, bool ignoreDebugGoFast = false)
        {
            if ((!HaltOffset.HasValue && HaltOffset < ms) || (!ignoreDebugGoFast && Utils.DebugGoFast()))
                Thread.Sleep(ms + (HaltOffset ?? 0));
        }

        internal static string GetPreviousStackFrameIdentity(int frameSkip = 2)
        {
            var result = "";
            var asm = new StackFrame(frameSkip).GetMethod().Module.Assembly;
            if (asm == MethodBase.GetCurrentMethod().Module.Assembly)
                result = "Pathfinder";
            else if (asm == typeof(Program).Assembly)
                result = "Hacknet";
            else
                result = asm.GetFirstMod()?.GetCleanId();
            return result;
        }

        public static T GetFirstAttribute<T>(this MethodInfo info, bool inherit = false) where T : System.Attribute
        {
            if (info.GetCustomAttributes(inherit).Length > 0)
                return info.GetCustomAttributes(typeof(T), inherit)[0] as T;
            return null;
        }

        public static T GetFirstAttribute<T>(this Type type, bool inherit = false) where T : System.Attribute
        {
            if (type.GetCustomAttributes(inherit).Length > 0)
                return type.GetCustomAttributes(typeof(T), inherit)[0] as T;
            return null;
        }

        public static string ConvertToHexBlocks(string toConvert, bool keepNewline = true)
        {
            string result = "";
            bool firstRound = true;
            foreach (var c in toConvert)
            {
                if (keepNewline && c == '\n') continue;
                result += (firstRound ? "" : " ") + Convert.ToByte(c).ToString("X2");
                if (firstRound) firstRound = false;
            }
            return result;
        }

        public static string ConvertFromHexBlocks(string fromConvert, bool ignoreNewline = true)
        {
            string result = "";
            foreach (var n in fromConvert.Split(' '))
                result += ignoreNewline && n.Contains("\n") ? 0 : Convert.ToChar(Convert.ToInt32(n, 16));
            return result;
        }
    }
}
