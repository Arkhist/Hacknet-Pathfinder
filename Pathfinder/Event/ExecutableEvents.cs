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
        public ExecutableEvent(Hacknet.Computer com, Hacknet.OS os, string[] args, string name = null, string data = null)
        {
            Computer = com;
            OS = os;
            ExecutableName = name;
            ExecutableData = data;
            Arguments = new List<string>(args ?? Utility.Array<string>.Empty);
        }

        public string this[int index]
        {
            get
            {
                if (Arguments.Count <= index)
                    return "";
                return Arguments[index];
            }
        }
    }

    public sealed class ExecutableExecuteEvent : ExecutableEvent
    {
        public Folder Folder { get; private set; }
        public int FileIndex { get; private set; }
        public GameFilesystem.File File { get; private set; }
        public FileEntry ExecutableFile => File?.Object;
        public Executable.ExecutionResult Result { get; set; } = Executable.ExecutionResult.NotFound;
        public new string ExecutableName
        {
            get { return ExecutableFile?.name; }
            set
            {
                if (ExecutableFile == null || value == null)
                    return;
                ExecutableFile.name = value;
                ((ExecutableEvent)this).ExecutableName = value;
            }
        }
        public new string ExecutableData
        {
            get { return ExecutableFile?.data; }
            set
            {
                if (ExecutableFile == null || value == null)
                    return;
                ExecutableFile.data = value;
                ((ExecutableEvent)this).ExecutableData = value;
            }
        }
        public ExecutableExecuteEvent(Hacknet.Computer com, Folder fol, int finde, GameFilesystem.File file, Hacknet.OS os, string[] args)
            : base(com, os, args)
        {
            Folder = fol;
            FileIndex = finde;
            File = file;
            ExecutableName = file?.Name;
            ExecutableData = file?.Data;
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
