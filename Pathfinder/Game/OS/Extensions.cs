using System;
using System.Collections.Generic;
using Hacknet;
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

        public static bool KillExeModule(this Hacknet.OS os, Hacknet.ExeModule module, bool shouldWrite = false)
        {
            if (os.exes.Contains(module))
                return module.Kill(shouldWrite);
            return false;
        }

        public static bool KillExeModule(this Hacknet.OS os, string input, bool shouldWrite = false)
        {
            int i;
            Hacknet.ExeModule mod = null;
            if (int.TryParse(input, out i))
            {
                mod = os.exes.Find((obj) => obj.PID == i);
                if (mod == null && shouldWrite)
                    os.Write(LocaleTerms.Loc("Invalid PID"));
            }
            else if (!String.IsNullOrWhiteSpace(input))
            {
                mod = os.exes.Find((obj) => obj.IdentifierName == input);
                if (mod == null && shouldWrite)
                    os.Write(LocaleTerms.Loc("Invalid Identifier Name"));
            }
            else os.Write(LocaleTerms.Loc("Error: Invalid PID or Input Format"));
            return mod?.Kill(shouldWrite) ?? false;
        }

        public static void KillAllExeModules(this Hacknet.OS os, bool shouldWrite = false)
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
            os.write(args == null || args.Length < 1 ? write : String.Format(write, args));
            return os;
        }

        /// <summary>
        /// Writes the formatted string directly to OS terminal. Less safe then <see cref="Write"/>
        /// </summary>
        /// <param name="os">The OS.</param>
        /// <param name="write">The formatted string to write.</param>
        public static Hacknet.OS WriteSingle(this Hacknet.OS os, string write, params object[] args)
        {
            os.writeSingle(args == null || args.Length < 1 ? write : String.Format(write, args));
            return os;
        }
    }
}
