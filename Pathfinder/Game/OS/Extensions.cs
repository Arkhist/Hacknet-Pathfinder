using System;
using System.Collections.Generic;
using Hacknet;
using Pathfinder.Util;

namespace Pathfinder.Game.OS
{
    public static class Extensions
    {
        /// <summary>
        /// Retrieves the List of ExeModule whose Type is exactly instanceType 
        /// </summary>
        /// <param name="instanceType">The exact Type to search for in the Executable List</param>
        public static List<ExeModule> GetExesFor(this Hacknet.OS os, Type instanceType)
        {
            var result = new List<ExeModule>();
            foreach (var e in os.exes)
                if (e.GetType() == instanceType)
                    result.Add(e);
            return result;
        }

        /// <summary>
        /// Retrieves the List of ExeModule List whose Type is or is derived from T 
        /// </summary>
        /// <typeparam name="T">The Type or derivative of the type to search for in the Executable List</typeparam>
        public static List<T> GetExesFor<T>(this Hacknet.OS os) where T : ExeModule
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
