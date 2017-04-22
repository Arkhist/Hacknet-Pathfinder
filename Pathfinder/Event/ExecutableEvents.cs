using System;
using System.Collections.Generic;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    public class ExecutableEvent : PathfinderEvent
    {
        public Hacknet.Computer Computer { get; private set; }
        public Hacknet.OS OS { get; private set; }
        public string ExecutableName { get; set; }
        public string ExecutableData { get; set; }
        public List<string> Arguments { get; private set; }
        [Obsolete("Use Arguments")]
        public List<string> Parameters => Arguments;
        [Obsolete("Use OS")]
        public Hacknet.OS OsInstance => OS;
        public ExecutableEvent(Hacknet.Computer com, Hacknet.OS os, string[] args, string name = null, string data = null)
        {
            Computer = com;
            OS = os;
            if(name != null)
                ExecutableName = name;
            if(data != null)
                ExecutableData = data;
            Arguments = new List<string>(args ?? Utility.Array<string>.Empty);
        }
    }

    public sealed class ExecutableExecuteEvent : ExecutableEvent
    {
        public Folder Folder { get; private set; }
        public int FileIndex { get; private set; }
        public FileEntry ExecutableFile { get; private set; }
        public Executable.ExecutionResult Result { get; set; } = Executable.ExecutionResult.NotFound;
        public new string ExecutableName
        {
            get { return ExecutableFile.name; }
            set
            {
                ExecutableFile.name = value;
                ((ExecutableEvent)this).ExecutableName = value;
            }
        }
        public new string ExecutableData
        {
            get { return ExecutableFile.data; }
            set
            {
                ExecutableFile.data = value;
                ((ExecutableEvent)this).ExecutableData = value;
            }
        }
        [Obsolete("Does not pertain to normal executables")]
        public Rectangle Location => default(Rectangle);
        [Obsolete("Does not pertain to normal executables")]
        public int TargetPort => -1;
        public ExecutableExecuteEvent(Hacknet.Computer com, Folder fol, int finde, FileEntry file, Hacknet.OS os, string[] args)
            : base(com, os, args)
        {
            Folder = fol;
            FileIndex = finde;
            ExecutableFile = file;
            ExecutableName = file.name;
            ExecutableData = file.data;
        }
    }

    public sealed class ExecutablePortExecuteEvent : ExecutableEvent
    {
        public Rectangle Destination { get; private set; }
        public int TargetPort { get; private set; }
        public ExecutablePortExecuteEvent(Hacknet.OS os, Rectangle dest, string name, string data, int port, string[] args)
            : base(os.thisComputer, os, args, name, data)
        {
            Destination = dest;
            TargetPort = port;
        }
    }
}
