#pragma warning disable CS0108 // Un membre masque un membre hérité ; le mot clé new est manquant

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;

namespace Pathfinder.Event
{
    // Called when Hacknet boots up (Program.Main start)
    public class StartUpEvent : PathfinderEvent
    {
        public List<string> MainArguments
        {
            get; private set;
        }

        public StartUpEvent(string[] args)
        {
            MainArguments = new List<string>(args ?? Utility.Array<string>.Empty);
        }
    }

    // Called after Hacknet loads the Game Object (actual game)
    public class LoadContentEvent : PathfinderEvent
    {
        public Game1 GameInstance
        {
            get; private set;
        }

        public LoadContentEvent(Game1 gameInstance)
        {
            GameInstance = gameInstance;
        }
    }

    public class CommandSentEvent : PathfinderEvent
    {
        public bool Disconnects
        {
            get; set;
        }

        public List<string> Arguments
        {
            get; private set;
        }

        [Obsolete("Use Arguments")]
        public string[] Args
        {
            get
            {
                return Arguments.ToArray();
            }
        }

        public Hacknet.OS OsInstance
        {
            get; private set;
        }

        public CommandSentEvent(Hacknet.OS osInstance, string[] args)
        {
            OsInstance = osInstance;
            Arguments = new List<string>(args ?? Utility.Array<string>.Empty);
            Disconnects = false;
        }
    }

    public class LoadSessionEvent : PathfinderEvent
    {
        public Hacknet.OS OsInstance
        {
            get; private set;
        }

        public LoadSessionEvent(Hacknet.OS osInstance)
        {
            OsInstance = osInstance;
        }
    }

    public class PostLoadSessionEvent : PathfinderEvent
    {
        public Hacknet.OS OsInstance
        {
            get; private set;
        }

        public PostLoadSessionEvent(Hacknet.OS osInstance)
        {
            OsInstance = osInstance;
        }
    }

    public class LoadSaveFileEvent : PathfinderEvent
    {
        public Hacknet.OS OsInstance
        {
            get; private set;
        }

        public XmlReader Reader
        {
            get; private set;
        }

        [Obsolete("Use Reader")]
        public XmlReader XmlReader
        {
            get
            {
                return Reader;
            }
        }

        public Stream Stream
        {
            get; private set;
        }

        public LoadSaveFileEvent(Hacknet.OS osInstance, XmlReader xmlReader, Stream stream)
        {
            OsInstance = osInstance;
            Reader = xmlReader;
            Stream = stream;
        }
    }

    public class SaveFileEvent : PathfinderEvent
    {
        public Hacknet.OS OsInstance
        {
            get; private set;
        }

        public string Filename
        {
            get; private set;
        }


        public SaveFileEvent(Hacknet.OS osInstance, string filename)
        {
            OsInstance = osInstance;
            Filename = filename;
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
            SaveString = saveString;
        }
    }

    public class LoadNetmapContentEvent : PathfinderEvent
    {
        public NetworkMap NetMapInstance
        {
            get; private set;
        }

        public LoadNetmapContentEvent(NetworkMap netmapInstance)
        {
            NetMapInstance = netmapInstance;
        }
    }

    public class LoadComputerXmlReadEvent : PathfinderEvent
    {

        public Hacknet.Computer Computer
        {
            get; private set;
        }

        public XmlReader Reader
        {
            get; private set;
        }

        public string Filename
        {
            get; private set;
        }

        public bool PreventNetmapAdd
        {
            get; private set;
        }

        public bool PreventDaemonInit
        {
            get; private set;
        }

        public LoadComputerXmlReadEvent(Hacknet.Computer computer, XmlReader reader, string filename, bool preventNetmapAdd, bool preventDaemonInit)
        {
            Computer = computer;
            Reader = reader;
            Filename = filename;
            PreventNetmapAdd = preventNetmapAdd;
            PreventDaemonInit = preventDaemonInit;
        }
    }

    public class ExecutableExecuteEvent : PathfinderEvent
    {
        public Hacknet.Computer Computer
        {
            get; private set;
        }

        public Folder Folder
        {
            get; private set;
        }

        public int FileIndex
        {
            get; private set;
        }

        public FileEntry ExecutableFile
        {
            get; private set;
        }

        public Hacknet.OS OsInstance
        {
            get; private set;
        }

        public List<string> Arguments
        {
            get; private set;
        }

        public Executable.ExecutionResult Result
        {
            get; set;
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

        [Obsolete("Just directly use ExecutableFile.name")]
        public string ExecutableName
        {
            get
            {
                return ExecutableFile.name;
            }
        }

        [Obsolete("Just directly use Executablefile.data")]
        public string ExecutableData
        {
            get
            {
                return ExecutableFile.data;
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
            Result = Executable.ExecutionResult.NotFound;
            Computer = computer;
            Folder = folder;
            FileIndex = fileIndex;
            ExecutableFile = exeFile;
            OsInstance = os;
            Arguments = new List<string>(argArray ?? Utility.Array<string>.Empty);
        }
    }

    public class PortExecutableExecuteEvent : PathfinderEvent
    {
        public Hacknet.OS OsInstance
        {
            get; private set;
        }

        public Rectangle Destination
        {
            get; private set;
        }

        public string ExecutableName
        {
            get; private set;
        }
        public string ExecutableData
        {
            get; private set;
        }

        public int TargetPort
        {
            get; private set;
        }

        public List<string> Arguments
        {
            get; private set;
        }

        public PortExecutableExecuteEvent(Hacknet.OS os,
                                            Rectangle dest,
                                            string exeName,
                                            string exeFileData,
                                            int targetPort,
                                            string[] argArray)
        {
            OsInstance = os;
            Destination = dest;
            ExecutableName = exeName;
            ExecutableData = exeFileData;
            TargetPort = targetPort;
            Arguments = new List<string>(argArray ?? Utility.Array<string>.Empty);
        }
    }

    public class GameExitEvent : PathfinderEvent
    {
        public Game1 GameInstance
        {
            get; private set;
        }

        public GameExitEvent(Game1 gameInstance)
        {
            GameInstance = gameInstance;
        }
    }
}
