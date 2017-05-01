using Pathfinder.Util;
using Command = Pathfinder.Command;
using Executable = Pathfinder.Executable;
using Port = Pathfinder.Port;

namespace TemplateMod
{
    public class TemplateMod : Pathfinder.IPathfinderMod
    {
        internal static Port.Type p = new Port.Type("TemplateName", 4);

        public string Identifier => "Template Mod";

        public void Load()
        {
            Logger.Verbose("Loading Template Mod");
        }

        public void LoadContent()
        {
            Command.Handler.AddCommand("templateModVersion", Commands.TemplateModVersion, autoComplete: true);
            Executable.Handler.AddExecutable("TempExe", new TempExe());
            if (Port.Handler.AddPort("tempPort", p))
                Logger.Info("added tempPort to game");
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
    }
}
