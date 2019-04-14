using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hacknet;
using Pathfinder.Command;
using Pathfinder.Game.ExeModule;
using Pathfinder.Game.NetworkMap;
using Pathfinder.Game.OS;
using Pathfinder.GameFilesystem;

namespace KernelUtilities
{
    public static class Commands
    {
        public static bool NetMap(OS os, List<string> args)
        {
            var cmdType = args[1].Trim().ToLower();
            if (args.Count < 2 || new string[] { "set", "remove", "get", "add" }.Contains(cmdType))
                os.write("Usage : netmap [add/remove/set/get]");
            else
            {
                var nodePos = 0;
                var node = os.netMap.nodes.FirstOrDefault(n =>
                {
                    nodePos++;
                    if (cmdType == "set" && args.Count < 4) return n.ip == os.connectedIP;
                    return n.ip == (args.ElementAtOrDefault(2) ?? os.connectedIP);
                });
                if (node == null)
                    os.write("Unable to find node '" + args[2] + "' for netmap command '" + args[1] + "' : Could not resolve IP");
                else switch (cmdType)
                    {
                        case "set":
                            try
                            {
                                node.location.X = float.Parse(node.ip == os.connectedIP ? args[2] : args[3], System.Globalization.CultureInfo.InvariantCulture);
                                node.location.Y = float.Parse(node.ip == os.connectedIP ? args[3] : args[4], System.Globalization.CultureInfo.InvariantCulture);
                                os.write(node.ip + " was moved successfully.");
                            }
                            catch (Exception)
                            {
                                os.write("Position arguments invalid. Eg: 0.55");
                            }
                            break;
                        case "get":
                            os.write(args.ElementAtOrDefault(2) ?? node.ip + " is at : x = " + node.location.X + "; y = " + node.location.Y + ";");
                            break;
                        case "add":
                            if (os.netMap.DiscoverNode(node))
                                os.write("IP " + args.ElementAtOrDefault(2) ?? node.ip + " successfully added.");
                            break;
                        case "remove":
                            if (args.Count == 2)
                                if (os.connectedIP == os.thisComputer.ip)
                                {
                                    os.write("Cannot remove your own node from the netmap.");
                                    break;
                                }
                            os.netMap.visibleNodes.Remove(nodePos);
                            os.write("IP " + args.ElementAtOrDefault(2) ?? os.connectedIP + " successfully removed.");
                            break;
                    }
            }
            return false;
        }

        public static bool Copy(OS os, List<string> args)
        {
            Action<List<File>, Directory, string> lamb = (files, folder, data) =>
            {
                var isCorrect = false;
                var hasBeenChanged = false;
                var passes = 0;
                var newFileName = args[1];
                while (!isCorrect)
                {
                    hasBeenChanged = false;
                    foreach (var file in files)
                        if (newFileName == file.Name)
                        {
                            newFileName += "-c";
                            hasBeenChanged = true;
                        }
                    if (!hasBeenChanged)
                        isCorrect = true;
                    else if (passes > 5)
                        break;
                    else
                        passes++;
                }
                if (!isCorrect)
                {
                    os.write("Could not copy file " + args[1] + " : Failed to fix name conflict.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(newFileName)) return;
                folder.CreateFile(newFileName, data);
                os.write("File successfully copied.");
            };

            if (os.connectedIP != os.thisComputer.ip)
                os.write("Cannot copy files on remote host.");
            else if (args.Count < 2)
                os.write("Usage : cp [FileName] (destination)");
            else
            {
                var sourceFolder = os.GetCurrentDirectory();
                var sourceFile = sourceFolder.SearchForFile(args[1]);
                if (sourceFile == null)
                { //Find Source File
                    int length = args[1].LastIndexOf('/');
                    if (length <= 0)
                    {
                        os.write("File " + args[1] + " not found.");
                        return false;
                    }
                    var path = args[1].Substring(0, length);
                    sourceFolder = ((Filesystem)os).SearchForDirectory(path, nullOut: true);
                    if (sourceFolder == null)
                    {
                        os.write("Local Folder " + path + " not found.");
                        return false;
                    }
                    path = args[1].Substring(length + 1);
                    sourceFile = sourceFolder.SearchForFile(path);
                    if (sourceFile == null)
                        os.write("File " + path + " not found at specified filepath.");
                }
                else if (args.Count == 2)
                    lamb(sourceFolder.Files, sourceFolder, sourceFile.Data);
                else
                {
                    int length = args[2].LastIndexOf('/');
                    if (length <= 0)
                        lamb(sourceFolder.Files, sourceFolder, sourceFile.Data);
                    else
                        lamb(sourceFolder.Files,
                             ((Filesystem)os).SearchForDirectory(args[2].Substring(0, length), nullOut: true),
                             sourceFile.Data);
                }
            }
            return false;
        }


        public static bool Kill(OS os, List<string> args)
        {
            args.RemoveAt(0);
            for (var i = 0; i < args.Count; i++)
            {
                if (args[0] == "-l")
                {
                    if (i == 0) i++;
                    if (args[i] == "0")
                    {
                        os.Write("T");
                        continue;
                    }
                    var val = Utility.GetSignal(args[i], isNullable: true);
                    if (val == null) os.Write("{0}: invalid signal specification", args[i]);
                    else os.Write(char.IsDigit(args[i][0]) ? ((int)val).ToString() : val.ToString());
                }
                else if (i == 0 && args[0].In("-s", "-n") || (args[0][0] == '-' && args[0][1] != '-'))
                {
                    Utility.Signal? signal = null;
                    if (args[0].StartsWith("-")) signal = Utility.GetSignal(args[0].Substring(1), true, true);
                    else if (args.Count > 2) signal = Utility.GetSignal(args[1], true, true);
                    else if (args.Count <= 2) os.Write(Help.ActiveHelp); // usage print
                    else
                    {
                        os.Write("{0}: option requires an argument", args[0]);
                        return false;
                    }

                    if (signal == null)
                    {
                        os.Write("{0}: invalid signal specification", args[1]);
                        return false;
                    }
                    Utility.CurrentSignal = signal.Value;
                }
                else
                {
                    var doubleDash = args[i].StartsWith("--");
                    if (args[i].StartsWith("-") || doubleDash && char.IsDigit(args[i][doubleDash ? 2 : 1]) && char.IsDigit(args[i].Last()))
                    {
                        var killList = new Stack<ExeModule>(os.exes.Count);
                        var number = Convert.ToInt32(args[i].Substring(doubleDash ? 2 : 1));
                        for (var innerI = os.exes.Count - 1; i > -1; i--)
                            if (os.exes[innerI].PID > number && Utility.CurrentSignal != 0)
                                killList.Push(os.exes[innerI]);
                        while (killList.Count > 0) killList.Pop().Kill(true);
                    }
                    else
                    {
                        var number = Convert.ToInt32(args[i]);
                        var exe = os.exes.First(e => e.PID == number);
                        if (exe == null)
                            os.Write("({0}) - No such process", number);
                        else if (Utility.CurrentSignal != 0)
                            exe.Kill(true);
                    }
                }
            }
            return false;
        }

        private static List<string> ReadArgs(this List<string> arguments, params Tuple<string, char>[] longToShort)
        {
            longToShort = longToShort ?? new Tuple<string, char>[0];
            var end = false;
            return arguments.Aggregate(new List<string>(arguments.Count), (res, str) =>
            {
                if (end) return res;
                if (str.IndexOf('-') == 0 && str.IndexOf('-', 1) == -1)
                {
                    res.AddRange(str.Remove(0).Select(c => '-'+char.ToString(c)));
                    return res;
                }
                var check = str.StartsWith("--") ? str.Substring(2) : null;
                if (check != null)
                {
                    var split = check.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    var shortStr = longToShort.FirstOrDefault(t => split[0] != null && t.Item1 == split[0]);
                    if (shortStr.Item1 != null && shortStr.Item2 != 0) res.Add(shortStr.Item2.ToString());
                    if (split.Length > 1) res.Add(split[1]);
                }
                else res.AddRange(str.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries));
                return res;
            });
        }

        public static bool MakeDirectory(OS os, List<string> args)
        {
            if (args.Count < 2) os.Write(Help.ActiveHelp); // print usage
            else
            {
                var createParents = false;
                var verbose = false;
                foreach (var arg in args.ReadArgs(
                    new Tuple<string, char>("parents", 'p'),
                    new Tuple<string, char>("verbose", 'v')
                ))
                {
                    switch (arg)
                    {
                        case "-p":
                            createParents = true;
                            continue;
                        case "-v":
                            verbose = true;
                            continue;
                        case "--help":
                            os.Write(Help.ActiveHelp);
                            return false;
                        case "--version":
                            os.Write("mkdir (HacknetKernelUtilities) 0.1 (GNU emulated command)");
                            return false;
                        default:
                            if (arg.StartsWith("-"))
                            {
                                os.Write("invalid option -- '{0}'\nTry 'mkdir --help' for more information.", arg.Substring(1));
                                return false;
                            }
                            goto breakLoop;
                    }
                    breakLoop: break;
                }

                var index = args.FindLastIndex(a => a.Contains("-")) + 1;
                if (index == 0) index++;
                var store = "";
                var storing = false;
                for (; index < args.Count; index++)
                {
                    if (args[index].StartsWith("\""))
                    {
                        store = args[index].Substring(1);
                        storing = true;
                        continue;
                    }
                    if (storing)
                    {
                        store += args[index];
                        if (store.EndsWith("\""))
                        {
                            store = store.Remove(store.Length - 1, 1);
                            storing = false;
                        }
                        else continue;
                    }
                    var directory = os.GetCurrentDirectory().SearchForDirectory(store = store == string.Empty ? args[index] : store);
                    if (store.Length <= directory.Path.Length)
                    {
                        if (!createParents)
                            os.Write("cannot create directory ‘{0}’: File exists", store);
                        continue;
                    }
                    var dirPath = store.Substring(directory.Path.Length + 1);
                    var paths = dirPath.Split(new string[] { FilePath.SEPERATOR }, StringSplitOptions.RemoveEmptyEntries);
                    if (paths.Length != 0)
                    {
                        if (createParents)
                        {
                            var dir = directory;
                            foreach (var p in paths)
                            {
                                dir = dir.CreateDirectory(p);
                                if (verbose) os.Write("created directory '{0}'", dir.Path);
                            }
                        }
                        else
                        {
                            os.Write("cannot create directory ‘{0}’: No such file or directory", store);
                            continue;
                        }
                    }
                    else
                    {
                        directory = directory.CreateDirectory(paths[0]);
                        if (verbose) os.Write("created directory '{0}'", directory.Path);
                    }
                    store = "";
                }
            }
            return false;
        }

        public static bool RemoveDirectory(OS os, List<string> args)
        {
            if (args.Count < 2) os.Write(Help.ActiveHelp); // print usage
            else
            {
                var deleteParents = false;
                var ignoreFilled = false;
                var verbose = false;
                foreach (var arg in args.ReadArgs(
                    new Tuple<string, char>("parents", 'p'),
                    new Tuple<string, char>("verbose", 'v')
                ))
                {
                    switch (arg)
                    {
                        case "-p":
                            deleteParents = true;
                            continue;
                        case "-v":
                            verbose = true;
                            continue;
                        case "--help":
                            os.Write(Help.ActiveHelp);
                            return false;
                        case "--version":
                            os.Write("rmdir (HacknetKernelUtilities) 0.1 (GNU emulated command)");
                            return false;
                        default:
                            if (arg.StartsWith("-"))
                            {
                                os.Write("invalid option -- '{0}'\nTry 'rmdir --help' for more information.", arg.Substring(1));
                                return false;
                            }
                            goto breakLoop;
                    }
                    breakLoop: break;
                }

                var index = args.FindLastIndex(a => a.Contains("-")) + 1;
                if (index == 0) index++;
                var store = "";
                var storing = false;
                for (; index < args.Count; index++)
                {
                    if (args[index].StartsWith("\""))
                    {
                        store = args[index].Substring(1);
                        storing = true;
                        continue;
                    }
                    if (storing)
                    {
                        store += args[index];
                        if (store.EndsWith("\""))
                        {
                            store = store.Remove(store.Length - 1, 1);
                            storing = false;
                        }
                        else continue;
                    }
                    var directory = os.GetCurrentDirectory().SearchForDirectory(store = store == string.Empty ? args[index] : store);
                    if (store.Length != directory.Path.Length)
                    {
                        os.Write("failed to remove '{0}': No such file or directory", store);
                        continue;
                    }
                    var parent = directory.ParentDirectory;
                    if (deleteParents)
                    {
                        var storeSplit = store.Split(new string[] { FilePath.SEPERATOR }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = storeSplit.Length - 1; i >= 0; i--)
                        {
                            store = store.Remove(store.LastIndexOf(FilePath.SEPERATOR));
                            if (verbose) os.Write("removing directory, '{0}'", store);
                            if (directory.HasContents)
                            {
                                if (ignoreFilled) break;
                                os.Write("failed to remove '{0}': Directory not empty", store);
                            }
                            if (directory.Name == FilePath.SEPERATOR)
                            {
                                os.Write("failed to remove directory '{0}': Device or resource busy", FilePath.SEPERATOR);
                                break;
                            }
                            parent.RemoveDirectory(directory);
                            directory = parent;
                            parent = directory.ParentDirectory;
                        }
                        continue;
                    }
                    if (verbose) os.Write("remove directory, '{0}'", store);
                    if (directory.HasContents)
                    {
                        if (ignoreFilled) continue;
                        os.Write("failed to remove '{0}': Directory not empty", store);
                    }
                    if (directory.Name == FilePath.SEPERATOR)
                    {
                        os.Write("failed to remove directory '{0}': Device or resource busy", FilePath.SEPERATOR);
                        continue;
                    }
                    parent.RemoveDirectory(directory);
                    directory = parent;
                    parent = directory.ParentDirectory;
                }
            }
            return false;
        }

        private enum TimeChanges : byte
        {
            Access = 1,
            Modify = 2,
            Change = 3 // actually irrelevant
        }

        private static Tuple<string, string> ToTuple(this string[] source) =>
            new Tuple<string, string>(source[0], source[1]);

        private static Tuple<string, string> ToTuple(this string source, string s2) =>
                new Tuple<string, string>(source, s2);

        private static string[] formats = {
            "MMddhhmm",
            "MMddhhmm.ss",
            "yyMMddhhmm",
            "yyMMddhhmm.ss",
            "yyyyMMddhhmm",
            "yyyyMMddhhmm.ss",
        };

        public static bool TouchFile(OS os, List<string> args)
        {
            if (args.Count < 2) os.Write(Help.ActiveHelp); // print usage
            else
            {
                var date = default(DateTime?);
                var properties = default(FileProperties?);
                var noCreate = false;
                var affectSymlink = false;
                byte changeTimes = 0;
                Tuple<string, string> arguments;
                var index = 0;
                foreach (var arg in args = args.ReadArgs(
                    new Tuple<string, char>("no-create", 'c'),
                    new Tuple<string, char>("date", 'd'),
                    new Tuple<string, char>("no-dereference", 'h'),
                    new Tuple<string, char>("reference", 'r')
                ))
                {
                    index++;
                    if (index < args.Count && !args[index].StartsWith("-"))
                        arguments = arg.ToTuple(args[index]);
                    else arguments = arg.ToTuple(null);
                    switch (arguments.Item1)
                    {
                        case "-a":
                            changeTimes |= 1 << (byte)TimeChanges.Access;
                            continue;
                        case "-c":
                            noCreate = true;
                            continue;
                        case "-d":
                            DateTime d;
                            if (DateTime.TryParse(arguments.Item2, out d))
                                date = d;
                            continue;
                        case "-f": continue;

                        case "-h":
                            affectSymlink = true;
                            continue;
                        case "-m":
                            changeTimes |= 1 << (byte)TimeChanges.Modify;
                            continue;
                        case "-r":
                            properties = Filesystem.CurrentDirectory.SearchForFile(arguments.Item2).Properties;
                            continue;
                        case "-t":
                            if (DateTime.TryParseExact(arguments.Item2, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out d))
                                date = d;
                            continue;
                        case "--time":
                            switch (arguments.Item2.ToLower())
                            {
                                case "access":
                                case "atime":
                                case "use":
                                    changeTimes |= 1 << (byte)TimeChanges.Access;
                                    break;
                                case "modify":
                                case "mtime":
                                    changeTimes |= 1 << (byte)TimeChanges.Modify;
                                    break;
                            }
                            continue;
                        case "--help":
                            os.Write(Help.ActiveHelp);
                            return false;
                        case "--version":
                            os.Write("rmdir (HacknetKernelUtilities) 0.1 (GNU emulated command)");
                            return false;
                        default:
                            if (arg.StartsWith("-"))
                            {
                                os.Write("invalid option -- '{0}'\nTry 'rmdir --help' for more information.", arg.Substring(1));
                                return false;
                            }
                            goto breakLoop;
                    }
                    breakLoop: break;
                }

                index--;
                var storing = false;
                var store = "";
                for (; index < args.Count; index++)
                {
                    if (args[index].StartsWith("\""))
                    {
                        store = args[index].Substring(1);
                        storing = true;
                        continue;
                    }
                    if (storing)
                    {
                        store += args[index];
                        if (store.EndsWith("\""))
                        {
                            store = store.Remove(store.Length - 1, 1);
                            storing = false;
                        }
                        else continue;
                    }
                    store = store == string.Empty ? args[index] : store;
                    var directoryPath = store.Remove(store.LastIndexOf(FilePath.SEPERATOR));
                    var fileName = store.Substring(store.LastIndexOf(FilePath.SEPERATOR));
                    var parentDirectory = os.GetCurrentDirectory().SearchForDirectory(directoryPath, false, true);
                    if (parentDirectory == null)
                    {
                        // could not find directory
                        continue;
                    }
                    if (affectSymlink)
                    {
                        if (!parentDirectory.ContainsFile(fileName))
                            if (!noCreate)
                                //warn lack of file
                                ;
                        // modify symlink
                        continue;
                    }
                    var f = parentDirectory.FindFile(fileName);
                    if (!noCreate && f == null)
                        continue;
                    if (f == null) f = parentDirectory.CreateFile(fileName);
                    date = date ?? new DateTime((long)OS.currentElapsedTime);
                    f.Properties = properties ?? new FileProperties
                    {
                        AccessedTime = (changeTimes >> (int)TimeChanges.Access & 1) == 1 ? date.Value.Ticks : f.Properties.AccessedTime,
                        ModifiedTime = (changeTimes >> (int)TimeChanges.Modify & 1) == 1 ? date.Value.Ticks : f.Properties.ModifiedTime,
                        ChangedTime = (long)OS.currentElapsedTime,
                        PrivilegeMask = f.Properties.PrivilegeMask
                    };
                }
            }
            return false;
        }
    }
}