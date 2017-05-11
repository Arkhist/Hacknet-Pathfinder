using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Event;
using Pathfinder.OS;

namespace Pathfinder.Internal
{
    static class HandlerListener
    {
        public static void CommandListener(CommandSentEvent e)
        {
            Command.Handler.CommandFunc f;
            if (Command.Handler.commands.TryGetValue(e.Arguments[0], out f))
            {
                e.IsCancelled = true;
                try
                {
                    e.Disconnects = f(e.OS, e.Arguments);
                }
                catch (Exception ex)
                {
                    e.OS.WriteF("Command {0} threw Exception:\n    {1}('{2}')", e.Arguments[0], ex.GetType().FullName, ex.Message);
                    throw ex;
                }
            }
            else if (e.Arguments[0].ToLower() == "help" || e.Arguments[0].ToLower() == "man" || e.Arguments[0] == "?")
            {
                e.IsCancelled = true;
                int page = 0;
                if (e.Arguments.Count > 1)
                {
                    try
                    {
                        page = Convert.ToInt32(e.Arguments[1]);
                        if (page > Command.Help.PageCount)
                        {
                            e.OS.Write("Invalid Page Number - Displaying First Page");
                            page = 0;
                        }
                    }
                    catch (FormatException)
                    {
                        e.OS.Write("Invalid Page Number");
                    }
                    catch (OverflowException)
                    {
                        e.OS.Write("Invalid Page Number");
                    }
                }
                e.OS.Write(Command.Help.GetPageString(page));
                e.Disconnects = false;
            }
        }

        public static void DaemonLoadListener(LoadComputerXmlReadEvent e)
        {
            Daemon.IInterface i;
            var id = e.Reader.GetAttribute("interfaceId");
            if (id != null && Daemon.Handler.idToInterface.TryGetValue(id, out i))
            {
                var objs = new Dictionary<string, string>();
                var storedObjects = e.Reader.GetAttribute("storedObjects")?.Split(' ');
                if (storedObjects != null)
                    foreach (var s in storedObjects)
                        objs[s.Remove(s.IndexOf('|'))] = s.Substring(s.IndexOf('|') + 1);
                e.Computer.daemons.Add(Daemon.Instance.CreateInstance(id, e.Computer, objs));
            }
        }

        public static void ExecutableListener(ExecutableExecuteEvent e)
        {
            Executable.IInterface i;
            if (Executable.Handler.IsFileDataForModExe(e.ExecutableFile.data)
                && Executable.Handler.idToInterface.TryGetValue(e.ExecutableFile.data.Split('\n')[0], out i))
            {
                int num = e.OS.ram.bounds.Y + RamModule.contentStartOffset;
                foreach (var exe in e.OS.exes)
                    num += exe.bounds.Height;
                var location = new Rectangle(e.OS.ram.bounds.X, num, RamModule.MODULE_WIDTH, (int)Hacknet.OS.EXE_MODULE_HEIGHT);
                e.OS.addExe(Executable.Instance.CreateInstance(i, e.ExecutableFile, e.OS, e.Arguments, location));
                e.Result = Executable.ExecutionResult.StartupSuccess;
            }
        }

        public static void ExecutableListInsertListener(CommandSentEvent e)
        {
            if (e.Arguments[0].Equals("exe"))
            {
                e.IsCancelled = true;
                e.Disconnects = false;
                var os = e.OS;
                var folder = os.thisComputer.files.root.searchForFolder("bin");
                os.write("Available Executables:\n");
                os.write("PortHack");
                os.write("ForkBomb");
                os.write("Shell");
                os.write("Tutorial");
                foreach (var file in folder.files)
                {
                    bool alreadyHandled = false;
                    var name = file.name.Contains(".") ? file.name.Remove(file.name.LastIndexOf('.')) : file.name;
                    foreach (var num in PortExploits.exeNums)
                        if (file.data == PortExploits.crackExeData[num]
                            || file.data == PortExploits.crackExeDataLocalRNG[num])
                        {
                            os.write(name);
                            alreadyHandled = true;
                            break;
                        }
                    if (!alreadyHandled && Executable.Handler.IsFileDataForModExe(file.data))
                        os.write(name);
                }
                os.write(" ");
            }
        }
    }
}
