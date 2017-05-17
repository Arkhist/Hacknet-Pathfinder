using Pathfinder.Util;
using Command = Pathfinder.Command;
using Executable = Pathfinder.Executable;
using Port = Pathfinder.Port;
using Extension = Pathfinder.Extension;
using Hacknet;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Pathfinder.Computer;
using Daemon = Pathfinder.Daemon;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Pathfinder.Event;
using Pathfinder.Util.Attribute;

namespace TemplateMod
{
    public class TemplateMod : Pathfinder.IPathfinderMod
    {
        internal static Port.Type p = new Port.Type("TemplateName", 4);

        public string Identifier => "Template Mod";

        [EventPriority(2)]
        public static void PriorityTwo(OSLoadContentEvent e)
        {
            Logger.Info("I should run first");
        }

        [EventPriority(1)]
        public static void PriorityOne(OSLoadContentEvent e)
        {
            Logger.Info("I should run second");
        }

        public static void CommandListener(CommandSentEvent e)
        {
            Logger.Info("command {0}", String.Join(" ", e.Arguments));
        }

        public void Load()
        {
            Logger.Verbose("Loading Template Mod");
            EventManager.RegisterListener<OSLoadContentEvent>(PriorityOne);
            EventManager.RegisterListener<OSLoadContentEvent>(PriorityTwo);
            EventManager.RegisterListener<CommandSentEvent>(CommandListener);
            Logger.Info("Loading finished");
        }

        public void LoadContent()
        {
            Logger.Info("command {0} added", Command.Handler.RegisterCommand("templateModVersion", Commands.TemplateModVersion, autoComplete: true));
            Executable.Handler.RegisterExecutable("TempExe", new TempExe());
            if (Port.Handler.RegisterPort("tempPort", p) != null)
                Logger.Info("added tempPort to game");
            Logger.Info("full id {0}", Daemon.Handler.RegisterDaemon("tempdae", new TempDaemon()));
            Logger.Info("full id {0}", Extension.Handler.RegisterExtension("tempext", new TempExtInfo()));
        }

        public void Unload()
        {
            Logger.Verbose("Unloading Template Mod");
        }

        class TempExe : Executable.Interface
        {
            public override string Identifier => "TempExe";

            public override bool? Update(Executable.Instance instance, float time)
            {
                Logger.Verbose("TempExe");
                return true;
            }
        }

        class TempDaemon : Daemon.IInterface
        {
            public string InitialServiceName => "TempDae";

            public void Draw(Daemon.Instance instance, Rectangle bounds, SpriteBatch sb)
            {
                sb.Draw(Utils.white, bounds, Color.Green);
            }

            public void InitFiles(Daemon.Instance instance)
            {
            }

            public void LoadInit(Daemon.Instance instance)
            {
            }

            public void LoadInstance(Daemon.Instance instance, Dictionary<string, string> objects)
            {
            }

            public void OnCreate(Daemon.Instance instance)
            {
            }

            public void OnNavigatedTo(Daemon.Instance instance)
            {
                Logger.Info("was navigated to");
            }

            public void OnUserAdded(Daemon.Instance instance, string name, string pass, byte type)
            {
                Logger.Info("user added {0} {1} {2}", name, pass, type);
            }
        }

        class TempExtInfo : Extension.Info
        {
            public override bool AllowSaves => true;
            public override string Description => "A temporary extension";
            public override string LogoPath => null;
            public override string Name => "Temp Extension";

            public override void OnConstruct(OS os)
            {
                var derp = new Computer("Derpy", "101.101.101.10", new Vector2(10, 10), 0);
                os.netMap.nodes.Add(derp);
                os.thisComputer.AddLink(derp);
            }

            public override void OnLoad(OS os, Stream loadingStream) {}
        }
    }
}
