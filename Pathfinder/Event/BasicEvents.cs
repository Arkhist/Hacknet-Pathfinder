#pragma warning disable CS0108 // Un membre masque un membre hérité ; le mot clé new est manquant

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Hacknet;
using Microsoft.Xna.Framework;

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
        private Game1 gameInstance;

        public Game1 GameInstance
        {
            get
            {
                return gameInstance;
            }
        }

        public LoadContentEvent(Game1 gameInstance)
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

    public class SaveWriteEvent : SaveFileEvent
    {
        public string SaveString
        {
            get; set;
        }

        public SaveWriteEvent(Hacknet.OS osInstance, string filename, string saveString) : base(osInstance, filename)
        {
            this.SaveString = saveString;
        }
    }

    public class LoadNetmapContentEvent : PathfinderEvent
    {
        private NetworkMap netMapInstance;

        public NetworkMap NetMapInstance
        {
            get
            {
                return netMapInstance;
            }
        }

        public LoadNetmapContentEvent(NetworkMap netmapInstance)
        {
            this.netMapInstance = netmapInstance;
        }
    }

    public class LoadComputerXmlReadEvent : PathfinderEvent
    {
        private Hacknet.Computer computer;
        private XmlReader reader;
        private string filename;
        private bool preventNetmapAdd;
        private bool preventDaemonInit;

        public Hacknet.Computer Computer
        {
            get
            {
                return computer;
            }
        }

        public XmlReader Reader
        {
            get
            {
                return reader;
            }
        }

        public string Filename
        {
            get
            {
                return filename;
            }
        }

        public bool PreventNetmapAdd
        {
            get
            {
                return preventNetmapAdd;
            }
        }

        public bool PreventDaemonInit
        {
            get
            {
                return preventDaemonInit;
            }
        }

        public LoadComputerXmlReadEvent(Hacknet.Computer computer, XmlReader reader, string filename, bool preventNetmapAdd, bool preventDaemonInit)
        {
            this.computer = computer;
            this.reader = reader;
            this.filename = filename;
            this.preventNetmapAdd = preventNetmapAdd;
            this.preventDaemonInit = preventDaemonInit;
        }
    }

    public class ExecutableExecuteEvent : PathfinderEvent
    {
        private Hacknet.Computer computer;
        private Folder folder;
        private int fileIndex;
        private FileEntry exeFile;
        private Hacknet.OS os;
        private List<string> arguments;
        private Executable.ExecutionResult result = Executable.ExecutionResult.NotFound;

        public Hacknet.Computer Computer
        {
            get
            {
                return computer;
            }
        }

        public Folder Folder
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

        public FileEntry ExeFile
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
                                      Folder folder,
                                      int fileIndex,
                                      FileEntry exeFile,
                                      Hacknet.OS os,
                                      string[] argArray)
        {
            this.computer = computer;
            this.folder = folder;
            this.fileIndex = fileIndex;
            this.exeFile = exeFile;
            this.os = os;
            this.arguments = new List<string>(argArray ?? new string[0]);
        }
    }

    public class PortExecutableExecuteEvent : PathfinderEvent
    {
        private Hacknet.OS osInstance;
        private Rectangle destination;
        private string exeName;
        private string exeData;
        private int targetPort;
        private List<string> arguments;

        public Hacknet.OS OsInstance
        {
            get
            {
                return osInstance;
            }
        }

        public Rectangle Destination
        {
            get
            {
                return destination;
            }
        }

        public string ExecutableName
        {
            get
            {
                return exeName;
            }
        }
        public string ExecutableData
        {
            get
            {
                return exeData;
            }
        }

        public int TargetPort
        {
            get
            {
                return targetPort;
            }
        }

        public List<string> Arguments
        {
            get
            {
                return arguments;
            }
        }

        public PortExecutableExecuteEvent(Hacknet.OS os,
                                            Rectangle dest,
                                            string exeName,
                                            string exeFileData,
                                            int targetPort,
                                            string[] argArray)
        {
            this.osInstance = os;
            this.destination = dest;
            this.exeName = exeName;
            this.exeData = exeFileData;
            this.targetPort = targetPort;
            this.arguments = new List<string>(argArray ?? new string[0]);
        }
    }

    public class GameExitEvent : PathfinderEvent
    {
        private Game1 gameInstance;

        public Game1 GameInstance
        {
            get
            {
                return gameInstance;
            }
        }

        public GameExitEvent(Game1 gameInstance)
        {
            this.gameInstance = gameInstance;
        }
    }
}
