using System;
using System.Collections.Generic;
using Pathfinder.Game.ExeModule;
using Pathfinder.Util;

namespace Pathfinder.Game.OS
{
    public static class Extensions
    {
        /// <summary>
        /// Retrieves the List of ExeModule whose Type is exactly instanceType 
        /// </summary>
        /// <param name="instanceType">The exact Type to search for in the Executable List</param>
        public static List<Hacknet.ExeModule> GetExesFor(this Hacknet.OS os, Type instanceType)
        {
            var result = new List<Hacknet.ExeModule>();
            foreach (var e in os.exes)
                if (e.GetType() == instanceType)
                    result.Add(e);
            return result;
        }

        /// <summary>
        /// Retrieves the List of ExeModule List whose Type is or is derived from T 
        /// </summary>
        /// <typeparam name="T">The Type or derivative of the type to search for in the Executable List</typeparam>
        public static List<T> GetExesFor<T>(this Hacknet.OS os) where T : Hacknet.ExeModule
        {
            var result = new List<T>();
            foreach (var e in os.exes)
                if (e is T)
                    result.Add(e as T);
            return result;
        }

        /// <summary>
        /// Retrieves the List of Executable.Instance whose Type is exactly instanceType 
        /// </summary>
        /// <param name="interfaceType">The exact Type to search for in the Executable List</param>
        public static List<Executable.Instance> GetModExeInterfaceFor(this Hacknet.OS os, Type interfaceType)
        {
            var result = new List<Executable.Instance>();
            foreach (var e in os.GetExesFor<Executable.Instance>())
                if (e.Interface.GetType() == interfaceType)
                    result.Add(e);
            return result;
        }

        /// <summary>
        /// Retrieves the List of Executable.Instance List whose Type is or is derived from T 
        /// </summary>
        /// <typeparam name="T">The Type or derivative of the type to search for in the Executable List</typeparam>
        public static List<Executable.Instance> GetModExeInterfaceFor<T>(this Hacknet.OS os) where T : Executable.IInterface
        {
            var result = new List<Executable.Instance>();
            foreach (var e in os.GetExesFor<Executable.Instance>())
                if (e.Interface is T)
                    result.Add(e);
            return result;
        }

        /// <summary>
        /// Kills the ExeModules on the OS.
        /// </summary>
        /// <returns><c>true</c>, if ExeModule was killed, <c>false</c> otherwise.</returns>
        /// <param name="os">The OS.</param>
        /// <param name="module">The ExeModule to kill.</param>
        /// <param name="shouldWrite">If set to <c>true</c> then success will be written to the OS.</param>
        public static bool KillExecutableModule(this Hacknet.OS os, Hacknet.ExeModule module, bool shouldWrite = false)
        {
            if (os.exes.Contains(module))
                return module.Kill(shouldWrite);
            return false;
        }

        /// <summary>
        /// Kills the first ExeModules on the OS that matches the string.
        /// </summary>
        /// <returns><c>true</c>, if ExeModule was killed, <c>false</c> otherwise.</returns>
        /// <param name="os">The OS.</param>
        /// <param name="input">The input string (or string representation of the integer PID) to search against.</param>
        /// <param name="searchName">If set to <c>true</c> then can search by IdentifierName.</param>
        /// <param name="shouldWrite">If set to <c>true</c> will write success and failure to the OS.</param>
        public static bool KillExecutableModule(this Hacknet.OS os, string input, bool searchName = false, bool shouldWrite = false)
        {
            int i;
            Hacknet.ExeModule mod = null;
            input = input.Trim();
            if (int.TryParse(input, out i))
            {
                mod = os.exes.Find((obj) => obj.PID == i);
                if (mod == null && shouldWrite)
                    os.Write(Locale.Get("Invalid PID"));
            }
            else if (searchName && !string.IsNullOrWhiteSpace(input))
            {
                mod = os.exes.Find((obj) => obj.IdentifierName == input);
                if (mod == null && shouldWrite)
                    os.Write(Locale.Get("Invalid Identifier Name"));
            }
            else if(shouldWrite) os.Write(Locale.Get("Error: Invalid PID or Input Format"));
            return mod?.Kill(shouldWrite) ?? false;
        }

        /// <summary>
        /// Kills all ExeModules.
        /// </summary>
        /// <param name="os">The OS.</param>
        /// <param name="shouldWrite">If set to <c>true</c> will write success to the OS.</param>
        public static void KillAllExecutableModules(this Hacknet.OS os, bool shouldWrite = false)
        {
            for (int i = os.exes.Count - 1; i >= 0; i--)
                os.exes[i].Kill(shouldWrite);
        }

        /// <summary>
        /// Retrieves the active network Computer according to the OS
        /// </summary>
        public static Hacknet.Computer GetCurrentComputer(this Hacknet.OS os) => Utility.GetCurrentComputer(os);

        /// <summary>
        /// Writes the formatted string to OS terminal.
        /// </summary>
        /// <param name="os">The OS.</param>
        /// <param name="write">The formatted string to write.</param>
        public static Hacknet.OS Write(this Hacknet.OS os, string write, params object[] args)
        {
            os.write(args == null || args.Length < 1 ? write : string.Format(write, args));
            return os;
        }

        /// <summary>
        /// Writes the formatted string directly to OS terminal. Less safe then <see cref="Write"/>
        /// </summary>
        /// <param name="os">The OS.</param>
        /// <param name="write">The formatted string to write.</param>
        public static Hacknet.OS WriteSingle(this Hacknet.OS os, string write, params object[] args)
        {
            os.writeSingle(args == null || args.Length < 1 ? write : string.Format(write, args));
            return os;
        }
    }
}
