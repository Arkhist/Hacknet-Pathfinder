using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.ModManager;
using Sax.Net;

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

        public static Random Random => pathfinderRng;

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
        public static Computer ClientComputer => ClientOS?.thisComputer;

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
            return os?.connectedComp ?? os?.thisComputer;
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

        public static string GenerateNumberString(byte radix, int size, Random rand = null)
        {
            string str = "";
            while (str.Length >= size)
                if (radix == 2 || radix == 8 || radix == 10 || radix == 16)
                    str += Convert.ToString((rand ?? pathfinderRng).Next(255), radix);
                else str += DecimalToArbitrarySystem((rand ?? pathfinderRng).Next(255), radix);
            return str.Substring(0, size);
        }

        public enum Radix : byte
        {
            Binary = 2,
            Ternary = 3,
            Quaternary = 4,
            Quinary = 5,
            Senary = 6,
            Septenary = 7,
            Octal = 8,
            Nonary = 9,
            Decimal = 10,
            Undecimal = 11,
            Duodecimal = 12,
            Hexidecimal = 16,
            Hexatrigesimal = 36,
            Max = Hexatrigesimal
        }

        public static string GenerateNumberString(Radix radix, int size, Random rand = null)
            => GenerateNumberString((byte)radix, size, rand);

        public static string GenerateBinString(int size = 500, Random rand = null)
            => GenerateNumberString(2, size, rand);

        public static string GenerateOctString(int size, Random rand = null)
            => GenerateNumberString(8, size, rand);

        public static string GenerateDecString(int size, Random rand = null)
            => GenerateNumberString(8, size, rand);

        public static string GenerateHexString(int size, Random rand = null)
            => GenerateNumberString(16, size, rand);

        /// <summary>
        /// Converts the given decimal number to the numeral system with the
        /// specified radix (in the range [2, 36]).
        /// </summary>
        /// <param name="decimalNumber">The number to convert.</param>
        /// <param name="radix">The radix of the destination numeral system (in the range [2, 36]).</param>
        /// <returns></returns>
        public static string DecimalToArbitrarySystem(long decimalNumber, int radix)
        {
            const int BitsInLong = 64;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > Digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length.ToString());

            if (decimalNumber == 0)
                return "0";

            int index = BitsInLong - 1;
            long currentNumber = Math.Abs(decimalNumber);
            char[] charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % radix);
                charArray[index--] = Digits[remainder];
                currentNumber = currentNumber / radix;
            }

            string result = new String(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }

            return result;
        }

        /// <summary>
        /// Converts the given number from the numeral system with the specified
        /// radix (in the range [2, 36]) to decimal numeral system.
        /// </summary>
        /// <param name="number">The arbitrary numeral system number to convert.</param>
        /// <param name="radix">The radix of the numeral system the given number
        /// is in (in the range [2, 36]).</param>
        /// <returns></returns>
        public static long ArbitraryToDecimalSystem(string number, int radix)
        {
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > Digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " +
                    Digits.Length.ToString());

            if (String.IsNullOrEmpty(number))
                return 0;

            // Make sure the arbitrary numeral system number is in upper case
            number = number.ToUpperInvariant();

            long result = 0;
            long multiplier = 1;
            for (int i = number.Length - 1; i >= 0; i--)
            {
                char c = number[i];
                if (i == 0 && c == '-')
                {
                    // This is the negative sign symbol
                    result = -result;
                    break;
                }

                int digit = Digits.IndexOf(c);
                if (digit == -1)
                    throw new ArgumentException(
                        "Invalid character in the arbitrary numeral system number",
                        nameof(number));

                result += digit * multiplier;
                multiplier *= radix;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
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
            var builder = new StringBuilder();
            foreach (var n in fromConvert.Split(' '))
                if (!ignoreNewline || !n.Contains("\n"))
                    builder.Append(Convert.ToChar(Convert.ToInt32(n, 16)));
            return builder.ToString();
        }

        public static Color GetColorFromString(string input, Color? defaultColor = null) =>
            GetColorFromString(input, false, defaultColor).Value;

        public static Color? GetColorFromString(string input, bool includeNull, Color? defaultColor = null)
        {
            if (input == null) return defaultColor.HasValue || includeNull ? defaultColor : Color.White;
            var colorArr = input.Split(new char[] { ',', ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var rgba = new int[4];
            if (colorArr.Length >= 3)
                for (var i = 0; i < rgba.Length; i++)
                    rgba[i] = colorArr.Length < i ? 255 : Convert.ToInt32(colorArr[i]);
            else return defaultColor.HasValue || includeNull ? defaultColor : Color.White;
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
        }

        public static Administrator GetAdminFromString(string input, bool resetsPass = true, bool isSuper = true)
        {
            Administrator basicAdmin = null;
            switch (input)
            {
                case "fast":
                    basicAdmin = new FastBasicAdministrator();
                    break;
                case "basic":
                    basicAdmin = new BasicAdministrator();
                    break;
                case "progress":
                    basicAdmin = new FastProgressOnlyAdministrator();
                    break;
            }
            if (basicAdmin != null)
            {
                basicAdmin.ResetsPassword = resetsPass;
                basicAdmin.IsSuper = isSuper;
            }
            return basicAdmin;
        }

        public static T Create<T>(params object[] args)
        {
            var typeArr = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                typeArr[i] = args[i].GetType();
            var ctor = typeof(T).GetConstructor(typeArr);
            if (ctor == null) return default(T);
            return GetActivator<T>(ctor)(args);
        }

        private static Dictionary<ConstructorInfo, Delegate> ctorToActivator = new Dictionary<ConstructorInfo, Delegate>();
        public delegate T ObjectActivator<T>(params object[] args);
        public static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
        {
            if (ctorToActivator.ContainsKey(ctor))
                return (ObjectActivator<T>)ctorToActivator[ctor];
            Type type = ctor.DeclaringType;
            var paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            var param = Expression.Parameter(typeof(object[]), "args");

            var argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                var index = Expression.Constant(i);
                var paramType = paramsInfo[i].ParameterType;

                var paramAccessorExp = Expression.ArrayIndex(param, index);

                var paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            var newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            var lambda = Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

            //compile it
            var compiled = (ObjectActivator<T>)lambda.Compile();
            ctorToActivator[ctor] = compiled;
            return compiled;
        }

        /*public static List<string> smartSplit(string text, char delim, char esc)
        {
            List<string> tokens = new List<string>();
            bool normal = true;
            string frag = "";

            foreach (var c in text)
            {
                if (normal)
                {
                    if (c == delim)
                    {
                        if (string.IsNullOrEmpty(frag)) tokens.Add(frag);
                        frag = "";
                    }
                    else if (c == esc) normal = false;
                    else frag += c;
                }
                else
                {
                    frag += c;
                    normal = true;
                }
            }
            if (!string.IsNullOrEmpty(frag))
                tokens.Add(frag);
            return tokens;
        }*/

        public static StringBuilder AddXml(this StringBuilder b, string key, string val)
           => b.Append("<" + key + ">" + val + "</" + key + ">");

        public static StringBuilder AddXml<T1, T2>(this StringBuilder b, KeyValuePair<T1, T2> kv)
            => b.AddXml(kv.Key?.ToString(), kv.Value?.ToString());

        public static string ToXml(this string key, string value, params string[] attributes)
        {
            var attributeStr = new StringBuilder(attributes.Length * 25);
            bool attribName = true;
            for (int i = 0; i < attributes.Length; i++)
            {
                attributeStr.Append(attributes[i]);
                if (attribName && i + 1 < attributes.Length) attributeStr.Append("=\"");
                if (!attribName) attributeStr.Append("\" ");
                attribName = !attribName;
            }
            if (value == null) return "<" + key + (attributeStr.Length > 0 ? " " + attributeStr : "") + " />";
            return "<" + key + ">" + value + "</" + key + (attributeStr.Length > 0 ? " " + attributeStr : "") + ">";

        }

        public static string ToXml(this string key, string value, IAttributes attributes)
        {
            var attributeStr = new StringBuilder(attributes.Length * 25);
            for (int i = 0; i < attributes.Length; i++)
            {
                attributeStr.Append(attributes.GetQName(i));
                var val = attributes.GetValue(i);
                if (val != null)
                    attributeStr.Append("=\"" + val + "\" ");
            }
            if (value == null) return "<" + key + (attributeStr.Length > 0 ? " " + attributeStr : "") + " />";
            return "<" + key + ">" + value + "</" + key + (attributeStr.Length > 0 ? " " + attributeStr : "") + ">";

        }

        public static string ToXml(this Dictionary<string, Internal.DatabaseDaemonHandler.DataInfo> input, string objName, bool excludeOuter = false)
        {
            if (input == null || input.Count < 1 && excludeOuter) return "<" + objName + " />";
            var builder = new StringBuilder();
            if (!excludeOuter)
                builder.Append("<" + objName + ">");
            foreach (var set in input)
                builder.AddXml(set);
            if (excludeOuter)
                builder.Append("\n");
            else builder.Append("\n</" + objName + ">");
            return builder.ToString();
        }

        public static string ToXml(this ICollection<string> input, string objName, bool excludeOuter = false)
        {
            if (input == null || input.Count < 1 && excludeOuter) return "<" + objName + " />";
            var builder = new StringBuilder();
            if (!excludeOuter)
                builder.Append("<" + objName + ">");
            foreach (var s in input)
                builder.AppendLine(s);
            if (excludeOuter)
                builder.Append("\n");
            else builder.Append("\n</" + objName + ">");
            return builder.ToString();
        }

        public static string ToXml(this VehicleRegistration input, bool excludeOuter = false)
        {
            if (input == null && excludeOuter) return "<VehicleRegistration />";
            var builder = new StringBuilder();
            if (!excludeOuter) builder.Append("<VehicleRegistration>");
            builder.AddXml("maker", input.vehicle.maker)
                   .AddXml("model", input.vehicle.model)
                   .AddXml("licensePlate", input.licencePlate)
                   .AddXml("licenseNumber", input.licenceNumber);
            if (excludeOuter)
                builder.Append("\n");
            else builder.Append("\n</VehicleRegistration>");
            return builder.ToString();
        }

        public static string ToXml(this NeopalsAccount input, bool excludeOuter = false)
        {
            if (input == null && excludeOuter) return "<NeopalsAccount />";
            var builder = new StringBuilder();
            if (!excludeOuter) builder.Append("<NeopalsAccount>");
            builder.AddXml("AccountName", input.AccountName)
                   .AddXml("NeoPoints", input.NeoPoints.ToString())
                   .AddXml("BankedPoints", input.BankedPoints.ToString())
                   .AddXml("InventoryID", input.InventoryID)
                   .AddXml("Pets", string.Join("\n", input.Pets.Select(
                       (p) => "<Type>" + p.Type.ToString() + "</Type>" + "<Name>" + p.Name + "</Name>" + "<DaysSinceFed>" + p.DaysSinceFed + "</DaysSinceFed>" + "<Happiness>" + p.Happiness + "</Happiness>" + "<Identifier>" + p.Identifier + "</Identifier>"
                      )));
            if (excludeOuter)
                builder.Append("\n");
            else builder.Append("\n</NeopalsAccount>");
            return builder.ToString();
        }

        public static string ToXml(this Person input, bool excludeOuter = false)
        {
            if (input == null && excludeOuter) return "<Person />";
            var builder = new StringBuilder();
            if (excludeOuter) builder.Append("<Person>");
            builder.AddXml("DateOfBirth", input.DateOfBirth.ToString("", CultureInfo.InvariantCulture))
                   .AddXml("isMale", input.isMale.ToString())
                   .AddXml("isHacker", input.isHacker.ToString())
                   .AddXml("firstName", input.firstName)
                   .AddXml("lastName", input.lastName)
                   .AddXml("handle", input.handle)
                   .AddXml("degrees", string.Join("\n", input.degrees.Select(
                       (d) => "<name>" + d.name + "</name>" + "<uni>" + d.uni + "</uni>" + "<GPA>" + d.GPA + "</GPA>"))
                          )
                   .AddXml("vehicles", string.Join("\n", input.vehicles.Select((v) => v.ToXml(true))))
                   .Append("<birthplace>")
                   .AddXml("country", input.birthplace.country)
                   .AddXml("name", input.birthplace.name)
                   .AddXml("educationLevel", input.birthplace.educationLevel.ToString())
                   .AddXml("lifeLevel", input.birthplace.lifeLevel.ToString())
                   .AddXml("employerLevel", input.birthplace.employerLevel.ToString())
                   .AddXml("affordabilityLevel", input.birthplace.affordabilityLevel.ToString())
                   .Append("</birthplace>")
                   .Append("<medicalRecord>")
                   .Append(input.medicalRecord.Visits.ToXml("Visits"))
                   .Append(input.medicalRecord.Perscriptions.ToXml("Perscriptions"))
                   .Append(input.medicalRecord.Allergies.ToXml("Allergies"))
                   .AddXml("Height", input.medicalRecord.Height.ToString())
                   .AddXml("Notes", input.medicalRecord.Notes)
                   .AddXml("BloodType", input.medicalRecord.BloodType)
                   .AddXml("DateofBirth", input.medicalRecord.DateofBirth.ToString("", CultureInfo.InvariantCulture))
                   .Append("</medicalRecord>")
                   .Append(input.NeopalsAccount.ToXml())
                   .Append("</Person>");
            return builder.ToString();

            /*
            public DateTime DateOfBirth = DateTime.Now;
            public bool isMale = true;
            public bool isHacker = false;
            public string firstName;
            public string lastName;
            public string handle;
            public List<Degree> degrees;
            public List<VehicleRegistration> vehicles;
            public WorldLocation birthplace;
            public MedicalRecord medicalRecord;
            public NeopalsAccount Neopals   */
        }
    }
}
