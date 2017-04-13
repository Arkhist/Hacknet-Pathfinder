using System;
using System.Collections.Generic;
using Hacknet;

namespace Pathfinder.OS
{
    public static class Extensions
    {
        /// <summary>
        /// Only retrieves the OS's exes of the specifed type. Use <see cref="Extensions.GetExesFor(Hacknet.OS)"/> to get inherited types
        /// </summary>
        public static List<ExeModule> GetExesFor(this Hacknet.OS os, Type instanceType)
        {
            var result = new List<ExeModule>();
            foreach (var e in os.exes)
                if (e.GetType() == instanceType)
                    result.Add(e);
            return result;
        }

        /// <summary>
        /// Retrieves all exes derived from T on the OS, Use <see cref="Extensions.GetExesFor(Hacknet.OS, Type)"/> to get only singular types
        /// </summary>
        public static List<T> GetExesFor<T>(this Hacknet.OS os) where T : ExeModule
        {
            var result = new List<T>();
            foreach (var e in os.exes)
                if (e is T)
                    result.Add(e as T);
            return result;
        }

        /// <summary>
        /// Only retrieves the OS's modded exes of the specifed interface. Use <see cref="Extensions.GetModExeInterfaceFor(Hacknet.OS)"/> to get inherited types
        /// </summary>
        public static List<Executable.Instance> GetModExeInterfaceFor(this Hacknet.OS os, Type interfaceType)
        {
            var result = new List<Executable.Instance>();
            foreach (var e in os.GetExesFor<Executable.Instance>())
                if (e.Interface.GetType() == interfaceType)
                    result.Add(e);
            return result;
        }

        /// <summary>
        /// Retrieves all modded exe interfaces derived from T on the OS, Use <see cref="Extensions.GetModExeInterfaceFor(Hacknet.OS, Type)"/> to get only singular types
        /// </summary>
        public static List<Executable.Instance> GetModExeInterfaceFor<T>(this Hacknet.OS os) where T : Executable.IInterface
        {
        	var result = new List<Executable.Instance>();
        	foreach (var e in os.GetExesFor<Executable.Instance>())
        		if (e.Interface is T)
        			result.Add(e);
        	return result;
        }
    }
}
