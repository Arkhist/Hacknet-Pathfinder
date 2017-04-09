#pragma warning disable CS0108 // Un membre masque un membre hérité ; le mot clé new est manquant

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Executable = Pathfinder.Executable;

namespace Pathfinder.Event
{
    // Called when Hacknet boots up (Program.Main start)
    public class StartUpEvent : PathfinderEvent
    {
        private string[] mainArgs;

        public string[] MainArgs
        {
            get
            {
                return mainArgs;
            }
        }

        public StartUpEvent(string[] args)
        {
            mainArgs = args;
        }

    }

    // Called after Hacknet loads the Game Object (actual game)
    public class LoadContentEvent : PathfinderEvent
    {
        private Hacknet.Game1 gameInstance;

        public Hacknet.Game1 GameInstance
        {
            get
            {
                return gameInstance;
            }
        }

        public LoadContentEvent(Hacknet.Game1 gameInstance)
        {
            this.gameInstance = gameInstance;
        }
    }

    public class CommandSentEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;
        private string[] args;
        private bool disconnectFlag = false;

        public bool Disconnects
        {
            get
            {
                return disconnectFlag;
            }
            set
            {
                disconnectFlag = value;
            }
        }

        public string[] Arguments
        {
            get
            {
                return args;
            }
        }

        [Obsolete("Use Arguments")]
        public string[] Args
        {
            get
            {
                return Arguments;
            }
        }

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public CommandSentEvent(Hacknet.OS osInstance, string[] args)
        {
            this.osInstance = osInstance;
            this.args = args;
        }
    }

    public class LoadSessionEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public LoadSessionEvent(Hacknet.OS osInstance)
        {
            this.osInstance = osInstance;
        }
    }

    public class PostLoadSessionEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public PostLoadSessionEvent(Hacknet.OS osInstance)
        {
            this.osInstance = osInstance;
        }
    }

    public class LoadSaveFileEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;
        private XmlReader xmlReader;
        private Stream stream;

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public XmlReader XmlReader
        {
            get
            {
                return xmlReader;
            }
        }

        public Stream Stream
        {
            get
            {
                return stream;
            }
        }

        public LoadSaveFileEvent(Hacknet.OS osInstance, XmlReader xmlReader, Stream stream)
        {
            this.osInstance = osInstance;
            this.xmlReader = xmlReader;
            this.stream = stream;
        }
    }

    public class SaveFileEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;
        private string filename;

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public string Filename
        {
            get
            {
                return filename;
            }
        }


        public SaveFileEvent(Hacknet.OS osInstance, string filename)
        {
            this.osInstance = osInstance;
            this.filename = filename;
        }
    }

    public class LoadNetmapContentEvent : PathfinderEvent
    {
        private Hacknet.NetworkMap netMapInstance;

        public Hacknet.NetworkMap NetMapInstance
        {
            get
            {
                return netMapInstance;
            }
        }

        public LoadNetmapContentEvent(Hacknet.NetworkMap netmapInstance)
        {
            this.netMapInstance = netmapInstance;
        }
    }

    public class ExecutableExecuteEvent : PathfinderEvent
    {
        private Hacknet.Computer computer;
        private Hacknet.Folder folder;
        private int fileIndex;
        private Hacknet.FileEntry exeFile;
        private Hacknet.OS os;
        private List<string> arguments;
        private Executable.ExecutionResult result;

        public Hacknet.Computer Computer
        {
            get
            {
                return computer;
            }
        }

        public Hacknet.Folder Folder
        {
            get
            {
                return folder;
            }
        }

        public int FileIndex
        {
            get
            {
                return fileIndex;
            }
        }

        public Hacknet.FileEntry ExeFile
        {
            get
            {
                return exeFile;
            }
        }

        public Hacknet.OS OsInstance
        {
            get
            {
                return os;
            }
        }

        public List<string> Arguments
        {
            get
            {
                return arguments;
            }
        }

        public Executable.ExecutionResult Result
        {
            get
            {
                return result;
            }
            set
            {
                result = value;
            }
        }

        [Obsolete("Use Arguments")]
        public List<string> Parameters
        {
            get
            {
                return Arguments;
            }
        }

        [Obsolete("Does not pertain to normal executables")]
        public Rectangle Location
        {
            get
            {
                return default(Rectangle);
            }
        }

        [Obsolete("Just directly use ExeFile.name")]
        public string ExecutableName
        {
            get
            {
                return ExeFile.name;
            }
        }

        [Obsolete("Just directly use Exefile.data")]
        public string ExecutableData
        {
            get
            {
                return ExeFile.data;
            }
        }

        [Obsolete("Does not pertain to normal executables")]
        public int TargetPort
        {
            get
            {
                return -1;
            }
        }

        public ExecutableExecuteEvent(Hacknet.Computer computer,
                                      Hacknet.Folder folder,
                                      int fileIndex,
                                      Hacknet.FileEntry exeFile,
                                      Hacknet.OS os,
                                      string[] argArray)
        {
            this.computer = computer;
            this.folder = folder;
            this.fileIndex = fileIndex;
            this.exeFile = exeFile;
            this.os = os;
            this.arguments = new List<string>(argArray);
        }
    }

    public class GameExitEvent : PathfinderEvent
    {
        private Hacknet.Game1 gameInstance;

        public Hacknet.Game1 GameInstance
        {
            get
            {
                return gameInstance;
            }
        }

        public GameExitEvent(Hacknet.Game1 gameInstance)
        {
            this.gameInstance = gameInstance;
        }
    }
}
